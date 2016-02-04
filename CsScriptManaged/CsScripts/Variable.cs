using CsScriptManaged;
using CsScriptManaged.Utility;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace CsScripts
{
    /// <summary>
    /// Ultimate class for working with variables from process being debugged.
    /// </summary>
    public class Variable : DynamicObject, IConvertible
    {
        /// <summary>
        /// The name of variable when its value is computed
        /// </summary>
        public const string ComputedName = "<computed>";

        /// <summary>
        /// The name
        /// </summary>
        private string name;

        /// <summary>
        /// The code type
        /// </summary>
        private CodeType codeType;

        /// <summary>
        /// The data
        /// </summary>
        private SimpleCache<ulong> data;

        /// <summary>
        /// The runtime type
        /// </summary>
        private SimpleCache<CodeType> runtimeType;

        /// <summary>
        /// The fields
        /// </summary>
        private SimpleCache<Variable[]> fields;

        /// <summary>
        /// The fields that are casted to user type if metadata is provided
        /// </summary>
        private SimpleCache<Variable[]> userTypeCastedFields;

        /// <summary>
        /// The fields indexed by name
        /// </summary>
        private DictionaryCache<string, Variable> fieldsByName;

        /// <summary>
        /// The fields that are casted to user type if metadata is provided, indexed by name
        /// </summary>
        private DictionaryCache<string, Variable> userTypeCastedFieldsByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public Variable(Variable variable)
        {
            name = variable.name;
            Address = variable.Address;
            codeType = variable.codeType;
            runtimeType = variable.runtimeType;
            fields = variable.fields;
            userTypeCastedFields = variable.userTypeCastedFields;
            fieldsByName = variable.fieldsByName;
            userTypeCastedFieldsByName = variable.userTypeCastedFieldsByName;
            data = variable.data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        internal Variable(CodeType codeType, ulong address, string name)
        {
            this.codeType = codeType;
            this.name = name;
            Address = address;

            // Initialize caches
            data = SimpleCache.Create(ReadData);
            runtimeType = SimpleCache.Create(FindRuntimeType);
            fields = SimpleCache.Create(FindFields);
            userTypeCastedFields = SimpleCache.Create(FindUserTypeCastedFields);
            fieldsByName = new DictionaryCache<string, Variable>(GetOriginalField);
            userTypeCastedFieldsByName = new DictionaryCache<string, Variable>(GetUserTypeCastedFieldByName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        /// <param name="data">The loaded data value (this can be used only with pointers).</param>
        private Variable(CodeType codeType, ulong address, string name, ulong data)
            : this(codeType, address, name)
        {
            if (!codeType.IsPointer)
            {
                throw new Exception("You cannot assign data to non-pointer type variable. Type was " + codeType);
            }

            this.data.Value = data;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        public static Variable Create(CodeType codeType, ulong address, string name = ComputedName)
        {
            Variable variable = CreateNoCast(codeType, address, name);

            return codeType.Module.Process.UserTypeCastedVariables[variable];
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable"/> class and doesn't cast it to user code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        internal static Variable CreateNoCast(CodeType codeType, ulong address, string name)
        {
            return codeType.Module.Process.Variables[Tuple.Create(codeType, address, name)];
        }

        /// <summary>
        /// The address where this variable value is stored
        /// </summary>
        internal ulong Address { get; private set; }

        /// <summary>
        /// Gets the loaded data value.
        /// </summary>
        internal ulong Data
        {
            get
            {
                return data.Value;
            }
        }

        #region Simple casts
        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator bool (Variable v)
        {
            if (!v.codeType.IsSimple && !v.codeType.IsPointer)
            {
                return bool.Parse(v.ToString());
            }

            return v.Data != 0;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Byte"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator byte (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return byte.Parse(v.ToString());
            }

            return (byte)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.SByte"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator sbyte (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return sbyte.Parse(v.ToString());
            }

            return (sbyte)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Char"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator char (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return char.Parse(v.ToString());
            }

            return (char)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Int16"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator short (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return short.Parse(v.ToString());
            }

            return (short)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.UInt16"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator ushort (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return ushort.Parse(v.ToString());
            }

            return (ushort)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator int (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return int.Parse(v.ToString());
            }

            return (int)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.UInt32"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator uint (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return uint.Parse(v.ToString());
            }

            return (uint)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator long (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return long.Parse(v.ToString());
            }

            return (long)v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator ulong (Variable v)
        {
            if (!v.codeType.IsSimple)
            {
                return ulong.Parse(v.ToString());
            }

            return v.Data;
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <exception cref="System.ArgumentException">Incorrect data size</exception>
        public override string ToString()
        {
            // Check if it is null
            if (IsNullPointer())
            {
                return "(null)";
            }

            // ANSI string
            if (codeType.IsAnsiString)
            {
                uint stringLength;
                ulong address = codeType.IsPointer ? Data : Address;
                StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                Context.DataSpaces.ReadMultiByteStringVirtual(address, Constants.MaxStringReadLength, sb, (uint)sb.Capacity, out stringLength);
                return sb.ToString();
            }

            // Unicode string
            if (codeType.IsWideString)
            {
                uint stringLength;
                ulong address = codeType.IsPointer ? Data : Address;
                StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                Context.DataSpaces.ReadUnicodeStringVirtualWide(address, Constants.MaxStringReadLength * 2, sb, (uint)sb.Capacity, out stringLength);
                return sb.ToString();
            }

            // Simple type
            if (codeType.IsSimple)
            {
                if (codeType.Name == "bool" || codeType.Name == "BOOL")
                {
                    return (Data != 0).ToString();
                }

                switch (codeType.Size)
                {
                    case 1:
                        return ((byte)Data).ToString();
                    case 2:
                        return ((short)Data).ToString();
                    case 4:
                        return ((int)Data).ToString();
                    case 8:
                        return ((long)Data).ToString();
                    default:
                        throw new ArgumentException("Incorrect data size " + codeType.Size);
                }
            }

            // TODO: Call custom caster (e.g. std::string, std::wstring)

            // Check if it is pointer
            if (codeType.IsPointer)
            {
                if (codeType.Size == 4)
                {
                    return string.Format("0x{0:X4}", Data);
                }
                else
                {
                    return string.Format("0x{0:X8}", Data);
                }
            }

            return "{" + codeType.Name + "}";
        }

        /// <summary>
        /// Determines whether this variable is null pointer.
        /// </summary>
        /// <returns></returns>
        public bool IsNullPointer()
        {
            return codeType.IsPointer && GetPointerAddress() == 0;
        }

        /// <summary>
        /// Gets the pointer address.
        /// </summary>
        /// <exception cref="System.ArgumentException">Variable is not a pointer type, but ...</exception>
        public ulong GetPointerAddress()
        {
            return codeType.IsPointer ? Data : Address;
        }

        /// <summary>
        /// Gets the memory address where value of this Variable is stored.
        /// </summary>
        public ulong GetAddress()
        {
            return Address;
        }

        /// <summary>
        /// Gets the name of variable.
        /// </summary>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Gets the code type.
        /// </summary>
        public CodeType GetCodeType()
        {
            return codeType;
        }

        /// <summary>
        /// Gets the runtime type.
        /// </summary>
        public CodeType GetRuntimeType()
        {
            return runtimeType.Value;
        }

        /// <summary>
        /// Gets the field names (including base classes).
        /// </summary>
        public string[] GetFieldNames()
        {
            return codeType.FieldNames;
        }

        /// <summary>
        /// Gets the field names (it doesn't include base classes).
        /// </summary>
        public string[] GetClassFieldNames()
        {
            return codeType.ClassFieldNames;
        }

        /// <summary>
        /// Gets the fields (including base classes).
        /// </summary>
        public Variable[] GetFields()
        {
            return userTypeCastedFields.Value;
        }

        /// <summary>
        /// Gets the not user type casted fields (original ones).
        /// </summary>
        internal Variable[] GetOriginalFields()
        {
            return fields.Value;
        }

        /// <summary>
        /// Gets the array element.
        /// </summary>
        /// <param name="index">The index.</param>
        public Variable GetArrayElement(int index)
        {
            if (!codeType.IsArray && !codeType.IsPointer)
            {
                throw new ArgumentException("Variable is not a array or pointer type, but " + codeType);
            }

            CodeType elementType = codeType.ElementType;
            ulong baseAddress = codeType.IsPointer ? Data : Address;
            ulong address = baseAddress + (ulong)(index * elementType.Size);

            return Create(elementType, address, ComputedName);
        }

        /// <summary>
        /// Dereferences the pointer.
        /// </summary>
        /// <exception cref="System.ArgumentException">Variable is not a pointer type, but ...</exception>
        public Variable DereferencePointer()
        {
            if (!codeType.IsPointer)
            {
                throw new ArgumentException("Variable is not a pointer type, but " + codeType);
            }

            return GetArrayElement(0);
        }

        /// <summary>
        /// Adjusts the pointer.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <remarks>Returned variable is not casted to the user type. Use CastAs function to cast to suitable type.</remarks>
        /// <returns>Computed variable that points to new address</returns>
        /// <exception cref="System.ArgumentException">Variable is not a pointer type, but ...</exception>
        public Variable AdjustPointer(int offset)
        {
            if (codeType.IsPointer)
            {
                return new Variable(codeType, Address, name, Data + (ulong)offset);
            }
            else if (Address != 0)
            {
                return CreateNoCast(codeType, Address + (ulong)offset, name);
            }

            throw new ArgumentException("Variable is not a pointer type, but " + codeType + " and its address is 0");
        }

        /// <summary>
        /// Casts variable to new type.
        /// </summary>
        /// <param name="newType">The type.</param>
        /// <returns>Computed variable that is of new type.</returns>
        public Variable CastAs(CodeType newType)
        {
            Variable variable;

            if (newType == codeType)
            {
                return this;
            }
            else if (codeType.IsPointer && newType.IsPointer)
            {
                variable = new Variable(newType, Address, name, Data);
            }
            else if (newType.IsPointer)
            {
                variable = new Variable(newType, 0, name, Address);
            }
            else if (codeType.IsPointer)
            {
                variable = new Variable(newType, Data, name);
            }
            else if (Address != 0)
            {
                return Create(newType, Address, name);
            }
            else
            {
                throw new ArgumentException("Variable is not a pointer type, but " + codeType + " and its address is 0");
            }

            return newType.Module.Process.UserTypeCastedVariables[variable];
        }

        /// <summary>
        /// Casts variable to the new type.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <returns>Computed variable that is of new type.</returns>
        public T CastAs<T>()
        {
            return (T)CastAs(typeof(T));
        }

        /// <summary>
        /// Casts variable to the new type.
        /// </summary>
        /// <param name="conversionType">The new type.</param>
        public object CastAs(Type conversionType)
        {
            // If we are converting Variable to Variable, just return us
            if (conversionType == typeof(Variable) || conversionType == GetType())
                return this;

            // Check if it is basic type
            else if (conversionType == typeof(bool))
                return (bool)this;
            else if (conversionType == typeof(char))
                return (char)this;
            else if (conversionType == typeof(byte))
                return (byte)this;
            else if (conversionType == typeof(sbyte))
                return (sbyte)this;
            else if (conversionType == typeof(short))
                return (short)this;
            else if (conversionType == typeof(ushort))
                return (ushort)this;
            else if (conversionType == typeof(int))
                return (int)this;
            else if (conversionType == typeof(uint))
                return (uint)this;
            else if (conversionType == typeof(long))
                return (long)this;
            else if (conversionType == typeof(ulong))
                return (ulong)this;

            // Check if we should do CastAs
            if (conversionType.IsSubclassOf(typeof(Variable)))
            {
                ulong address = GetPointerAddress();
                UserTypeMetadata metadata = UserTypeMetadata.ReadFromType(conversionType);
                UserTypeDescription description = metadata.ConvertToDescription();
                CodeType newType = description.UserType;

                // TODO: Fix this in the future
                // We are lazy loading user types if they haven't been loaded (for example in non-scripting mode)
                if (!newType.Module.Process.UserTypes.Contains(description))
                {
                    newType.Module.Process.UserTypes.Add(description);
                }

                return CastAs(newType);
            }

            // Check if type has constructor with one argument and that argument is inherited from Variable
            var constructors = conversionType.GetConstructors();

            foreach (var constructor in constructors)
            {
                if (!constructor.IsPublic)
                {
                    continue;
                }

                var parameters = constructor.GetParameters();

                if (parameters.Length < 1 || parameters.Count(p => !p.HasDefaultValue) > 1)
                {
                    continue;
                }

                if (parameters[0].ParameterType == typeof(Variable))
                {
                    return Activator.CreateInstance(conversionType, this);
                }
            }

            throw new InvalidCastException("Cannot cast Variable to " + conversionType);
        }

        /// <summary>
        /// Casts variable to new type.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <returns>Computed variable that is of new type.</returns>
        public Variable CastAs(string newType)
        {
            Module module = codeType.Module;
            int moduleIndex = newType.IndexOf('!');

            if (moduleIndex > 0)
            {
                string moduleName = newType.Substring(moduleIndex);

                module = module.Process.ModulesByName[moduleName];
                newType = newType.Substring(moduleIndex + 1);
            }

            CodeType newCodeType = module.TypesByName[newType];

            return CastAs(newCodeType);
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by name.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <remarks>This is not casted to user type</remarks>
        /// <param name="className">The class name.</param>
        public Variable GetBaseClass(string className)
        {
            var tuple = codeType.BaseClasses[className];

            return CreateNoCast(tuple.Item1, GetPointerAddress() + (uint)tuple.Item2, className);
        }

        /// <summary>
        /// Gets the class field (it doesn't go through base classes).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        public Variable GetClassField(string fieldName)
        {
            var tuple = codeType.ClassFields[fieldName];
            CodeType fieldType = tuple.Item1;
            ulong fieldAddress = GetPointerAddress() + (ulong)tuple.Item2;

            return Create(fieldType, fieldAddress, fieldName);
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>Field variable if the specified field exists.</returns>
        public Variable GetField(string name)
        {
            return userTypeCastedFieldsByName[name];
        }

        /// <summary>
        /// Gets the original field value (not user type casted field).
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>Field variable if the specified field exists.</returns>
        private Variable GetOriginalField(string name)
        {
            CodeType fieldType = codeType.GetFieldType(name);
            ulong fieldAddress = GetPointerAddress() + (ulong)codeType.GetFieldOffset(name);

            return CreateNoCast(fieldType, fieldAddress, name);
        }

        /// <summary>
        /// Gets the user type casted field.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>Field variable casted to user type if the specified field exists.</returns>
        private Variable GetUserTypeCastedFieldByName(string name)
        {
            Variable field = codeType.Module.Process.UserTypeCastedVariables[fieldsByName[name]];

            if (userTypeCastedFieldsByName.Count == 0)
            {
                GlobalCache.VariablesUserTypeCastedFieldsByName.Add(userTypeCastedFieldsByName);
            }

            return field;
        }

        /// <summary>
        /// Casts the variable to user type.
        /// </summary>
        /// <param name="originalVariable">The original variable.</param>
        internal static Variable CastVariableToUserType(Variable originalVariable)
        {
            // Get user type descriptions to be used by this process
            var userTypes = originalVariable.codeType.Module.Process.UserTypes;

            if (userTypes.Count == 0)
            {
                return originalVariable;
            }

            // Look at the type and see if it should be converted to user type
            var typesBasedOnModule = userTypes.Where(t => t.Module == originalVariable.codeType.Module);
            var typesBasedOnName = typesBasedOnModule.Where(t => t.UserType.TypeId == (originalVariable.codeType.IsPointer ? originalVariable.codeType.ElementType.TypeId : originalVariable.codeType.TypeId));

            var types = typesBasedOnName.ToArray();

            if (types.Length > 1)
            {
                throw new Exception(string.Format("Multiple types ({0}) are targeting same type {1}", string.Join(", ", types.Select(t => t.Type.FullName)), originalVariable.codeType.Name));
            }

            if (types.Length == 0)
            {
                return originalVariable;
            }

            // Check if it is null
            if (originalVariable.IsNullPointer())
            {
                return null;
            }

            // Create new instance of user defined type
            return (Variable)Activator.CreateInstance(types[0].Type, originalVariable);
        }

        /// <summary>
        /// Casts the variable collection to user type.
        /// </summary>
        /// <param name="originalCollection">The original variable collection.</param>
        internal static VariableCollection CastVariableCollectionToUserType(VariableCollection originalCollection)
        {
            Variable[] variables = new Variable[originalCollection.Count];
            Dictionary<string, Variable> variablesByName = new Dictionary<string, Variable>();

            for (int i = 0; i < variables.Length; i++)
            {
                variables[i] = originalCollection[i].codeType.Module.Process.UserTypeCastedVariables[originalCollection[i]];
                variablesByName.Add(originalCollection[i].name, variables[i]);
            }

            return new VariableCollection(variables, variablesByName);
        }

        /// <summary>
        /// Gets the length of the array represented with this variable.
        /// </summary>
        /// <exception cref="System.ArgumentException">Variable is not an array, but  + type.Name</exception>
        public int GetArrayLength()
        {
            if (!codeType.IsArray)
            {
                throw new ArgumentException("Variable is not an array, but " + codeType.Name);
            }

            return (int)(codeType.Size / codeType.ElementType.Size);
        }

        /// <summary>
        /// Tries to convert the variable to the specified type.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if conversion succeeds, <c>false</c> otherwise</returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // TODO: Implement
            return base.TryConvert(binder, out result);
        }

        /// <summary>
        /// Tries apply binary operation on the variable.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="arg">The argument.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if operation succeeds, <c>false</c> otherwise</returns>
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            // TODO: Implement
            return base.TryBinaryOperation(binder, arg, out result);
        }

        /// <summary>
        /// Tries to apply unary operation on the variable.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if operation succeeds, <c>false</c> otherwise</returns>
        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            // TODO: Implement
            return base.TryUnaryOperation(binder, out result);
        }

        /// <summary>
        /// Gets the dynamic member names.
        /// </summary>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetFieldNames();
        }

        /// <summary>
        /// Tries to get the member.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if member exists, <c>false</c> otherwise</returns>
        private bool TryGetMember(string name, out object result)
        {
            try
            {
                result = userTypeCastedFieldsByName[name];
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if member exists, <c>false</c> otherwise</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }

        /// <summary>
        /// Tries to get the element at specified index.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="indexes">The indexes.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if index exists, <c>false</c> otherwise</returns>
        /// <exception cref="System.ArgumentException">Multidimensional arrays are not supported</exception>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length > 1)
            {
                throw new ArgumentException("Multidimensional arrays are not supported");
            }

            try
            {
                int index = Convert.ToInt32(indexes[0]);

                if (codeType.IsPointer || codeType.IsArray)
                {
                    result = GetArrayElement(index);
                    return true;
                }
            }
            catch (Exception)
            {
                // Index is not a number, fall back to getting the member
            }

            return TryGetMember(indexes[0].ToString(), out result);
        }

        #region Forbidden setters/deleters
        /// <summary>
        /// Tries to delete the member - it is forbidden.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            throw new UnauthorizedAccessException();
        }

        /// <summary>
        /// Tries to delete the index - it is forbidden.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="indexes">The indexes.</param>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            throw new UnauthorizedAccessException();
        }

        /// <summary>
        /// Tries to set the member - it is forbidden.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new UnauthorizedAccessException();
        }

        /// <summary>
        /// Tries to set the index - it is forbidden.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="indexes">The indexes.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            throw new UnauthorizedAccessException();
        }
        #endregion

        /// <summary>
        /// Finds the runtime type.
        /// </summary>
        private CodeType FindRuntimeType()
        {
            // TODO: See if it is complex type and try to get VTable
            return null;
        }

        /// <summary>
        /// Gets field offset.
        /// </summary>
        public int GetFieldOffset(string fieldName)
        {
            return codeType.GetFieldOffset(fieldName);
        }

        /// <summary>
        /// Finds the fields.
        /// </summary>
        private Variable[] FindFields()
        {
            if (codeType.IsArray)
            {
                return new Variable[0];
            }

            string[] fieldNames = GetFieldNames();
            Variable[] fields = new Variable[fieldNames.Length];

            for (int i = 0; i < fieldNames.Length; i++)
            {
                fields[i] = fieldsByName[fieldNames[i]];
            }

            return fields;
        }

        /// <summary>
        /// Finds the user type casted fields.
        /// </summary>
        private Variable[] FindUserTypeCastedFields()
        {
            Variable[] originalFields = GetOriginalFields();
            Variable[] fields = new Variable[originalFields.Length];

            for (int i = 0; i < originalFields.Length; i++)
            {
                string name = originalFields[i].name;
                Variable field;

                if (userTypeCastedFieldsByName.TryGetExistingValue(name, out field))
                {
                    fields[i] = field;
                }
                else
                {
                    fields[i] = userTypeCastedFieldsByName[name] = codeType.Module.Process.UserTypeCastedVariables[originalFields[i]];
                }
            }

            GlobalCache.VariablesUserTypeCastedFields.Add(userTypeCastedFields);
            return fields;
        }

        /// <summary>
        /// Reads the data value of this variable.
        /// </summary>
        private ulong ReadData()
        {
            return Context.SymbolProvider.ReadSimpleData(codeType, Address);
        }

        #region IConvertible
        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return (bool)this;
        }

        public char ToChar(IFormatProvider provider)
        {
            return (char)this;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return (sbyte)this;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return (byte)this;
        }

        public short ToInt16(IFormatProvider provider)
        {
            return (short)this;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return (ushort)this;
        }

        public int ToInt32(IFormatProvider provider)
        {
            return (int)this;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return (uint)this;
        }

        public long ToInt64(IFormatProvider provider)
        {
            return (long)this;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return (ulong)this;
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return CastAs(conversionType);
        }
        #endregion
    }
}
