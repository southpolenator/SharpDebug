using CsDebugScript.CLR;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using CsDebugScript.Exceptions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace CsDebugScript
{
    /// <summary>
    /// Ultimate class for working with variables from process being debugged.
    /// </summary>
    public class Variable : DynamicObject, IConvertible, IEquatable<Variable>
    {
        /// <summary>
        /// The name of variable when its value is computed
        /// </summary>
        public const string ComputedName = "<computed>";

        /// <summary>
        /// The unknown path
        /// </summary>
        public const string UnknownPath = "<unknown>";

        /// <summary>
        /// The untracked path
        /// </summary>
        public const string UntrackedPath = "<untracked>";

        /// <summary>
        /// The name
        /// </summary>
        private string name;

        /// <summary>
        /// The path
        /// </summary>
        private string path;

        /// <summary>
        /// The code type
        /// </summary>
        private CodeType codeType;

        /// <summary>
        /// Runtime code type and offset to original type start.
        /// </summary>
        internal SimpleCacheStruct<Tuple<CodeType, int>> runtimeCodeTypeAndOffset;

        /// <summary>
        /// The data
        /// </summary>
        private SimpleCacheStruct<ulong> data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public Variable(Variable variable)
        {
            name = variable.name;
            path = variable.path;
            Address = variable.Address;
            codeType = variable.codeType;
            data = variable.data;
            runtimeCodeTypeAndOffset = variable.runtimeCodeTypeAndOffset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable" /> class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        internal Variable(CodeType codeType, ulong address, string name, string path)
        {
            this.codeType = codeType;
            this.name = name;
            this.path = path;
            Address = address;

            // Initialize caches
            data = SimpleCache.CreateStruct(ReadData);
            runtimeCodeTypeAndOffset = SimpleCache.CreateStruct(FindRuntimeCodeTypeAndOffset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable" /> class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="data">The loaded data value (this can be used only with pointers).</param>
        internal Variable(CodeType codeType, ulong address, string name, string path, ulong data)
            : this(codeType, address, name, path)
        {
            if (!codeType.IsPointer)
            {
                throw new WrongCodeTypeException(codeType, nameof(codeType), "pointer");
            }

            this.data.Value = data;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public static Variable Create(CodeType codeType, ulong address, string name = ComputedName, string path = UnknownPath)
        {
            Variable variable = CreateNoCast(codeType, address, name, path);

            return codeType.Module.Process.CastVariableToUserType(variable);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class and doesn't cast it to user code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public static Variable CreateNoCast(CodeType codeType, ulong address, string name = ComputedName, string path = UnknownPath)
        {
            if (Context.EnableVariableCaching)
            {
                return codeType.Module.Process.Variables[Tuple.Create(codeType, address, name, path)];
            }

            return new Variable(codeType, address, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class of pointer type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public static Variable CreatePointer(CodeType codeType, ulong address, string name = ComputedName, string path = UnknownPath)
        {
            Variable variable = CreatePointerNoCast(codeType, address, name, path);

            return codeType.Module.Process.CastVariableToUserType(variable);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class of pointer type and doesn't cast it to user code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="address">The address.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public static Variable CreatePointerNoCast(CodeType codeType, ulong address, string name = ComputedName, string path = UnknownPath)
        {
            return new Variable(codeType, 0, name, path, address);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <param name="value">Constant stored as ulong.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public static Variable CreateConstant(CodeType codeType, ulong value, string name = ComputedName, string path = UnknownPath)
        {
            Variable variable = CreateNoCast(codeType, 0, name, path);

            variable.Data = value;
            return variable;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(bool value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<bool>(module), value ? 1U : 0U, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(byte value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<byte>(module), value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(sbyte value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<sbyte>(module), (ulong)value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(short value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<short>(module), (ulong)value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(ushort value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<ushort>(module), value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(int value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<int>(module), (ulong)value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(uint value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<uint>(module), value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(long value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<long>(module), (ulong)value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static Variable CreateConstant(ulong value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<ulong>(module), value, name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static unsafe Variable CreateConstant(float value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<float>(module), *((uint*)&value), name, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Variable" /> class as constant and doesn't cast it to user code type.
        /// Variable will have address set to 0, so it probably won't be usable in all scenarios...
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <param name="module">The module where type should be created.</param>
        public static unsafe Variable CreateConstant(double value, string name = ComputedName, string path = UnknownPath, Module module = null)
        {
            if (module == null)
                module = Process.Current.Modules[0];
            return CreateConstant(BuiltinCodeTypes.GetCodeType<double>(module), *((ulong*)&value), name, path);
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

            set
            {
                data.Value = value;
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
            if (!v.codeType.IsSimple && !v.codeType.IsPointer && !v.codeType.IsEnum)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 1)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 1)
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
            if ((v.codeType.IsSimple || v.codeType.IsEnum) && v.codeType.Size == 1)
            {
                return (char)(byte)v.Data;
            }

            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 2)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 2)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 2)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 4)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 4)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 8)
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
            if ((!v.codeType.IsSimple && !v.codeType.IsEnum) || v.codeType.Size != 8)
            {
                return ulong.Parse(v.ToString());
            }

            return v.Data;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Single"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator float (Variable v)
        {
            if (v.codeType.IsDouble)
            {
                return (float)(double)v;
            }

            if (v.codeType.IsFloat)
            {
                return BitConverter.ToSingle(BitConverter.GetBytes((uint)v.Data), 0);
            }

            return float.Parse(v.ToString());
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Variable"/> to <see cref="System.Double"/>.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator double (Variable v)
        {
            if (v.codeType.IsDouble)
            {
                return BitConverter.Int64BitsToDouble((long)v.Data);
            }

            if (v.codeType.IsFloat)
            {
                return (float)v;
            }

            return double.Parse(v.ToString());
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
                return UserType.ReadString(codeType.Module.Process, GetPointerAddress(), 1);
            }

            // Unicode string
            if (codeType.IsWideString)
            {
                return UserType.ReadString(codeType.Module.Process, GetPointerAddress(), (int)codeType.ElementType.Size);
            }

            // Check float/double
            if (codeType.IsFloat)
            {
                return ((float)this).ToString();
            }

            if (codeType.IsDouble)
            {
                return ((double)this).ToString();
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

            // Enumeration
            if (codeType.IsEnum)
            {
                NativeCodeType nativeCodeType = codeType as NativeCodeType;

                if (nativeCodeType != null)
                {
                    return Context.SymbolProvider.GetEnumName(codeType.Module, nativeCodeType.TypeId, Data);
                }
            }

            // TODO: Call custom caster (e.g. std::string, std::wstring)

            // Check pointer size
            if (codeType.Module.Process.GetPointerSize() == 4)
            {
                return string.Format("0x{0:X4} ({1})", GetPointerAddress(), codeType.Name);
            }
            else
            {
                return string.Format("0x{0:X8} ({1})", GetPointerAddress(), codeType.Name);
            }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public Variable this[int index]
        {
            get
            {
                return GetArrayElement(index);
            }
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
        /// Gets the pointer address. If this variable is pointer, it will return its value, otherwise it will return variable memory address.
        /// </summary>
        public ulong GetPointerAddress()
        {
            return codeType.IsPointer ? Data : Address;
        }

        /// <summary>
        /// Gets the memory address where value of this Variable is stored.
        /// </summary>
        public ulong GetMemoryAddress()
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
        /// Gets the path of variable.
        /// </summary>
        public string GetPath()
        {
            return path;
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
            return runtimeCodeTypeAndOffset.Value.Item1;
        }

        /// <summary>
        /// Finds the runtime code type (and offset to original code type) by looking at the v-table.
        /// </summary>
        private Tuple<CodeType, int> FindRuntimeCodeTypeAndOffset()
        {
            try
            {
                if (!codeType.IsSimple)
                {
                    Process process = codeType.Module.Process;
                    MemoryBuffer memoryBuffer = Debugger.ReadMemory(process, GetPointerAddress(), process.GetPointerSize());
                    ulong vtableAddress = UserType.ReadPointer(memoryBuffer, 0, (int)process.GetPointerSize());

                    Tuple<CodeType, int> runtimeCodeType = Context.SymbolProvider.GetRuntimeCodeTypeAndOffset(codeType.Module.Process, vtableAddress);

                    if (runtimeCodeType != null && runtimeCodeType.Item1 != null)
                    {
                        return runtimeCodeType;
                    }
                }
            }
            catch (Exception)
            {
                // Fall back to original code type
            }

            return Tuple.Create(codeType, 0);
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
            return FindUserTypeCastedFields();
        }

        /// <summary>
        /// Gets the not user type casted fields (original ones).
        /// </summary>
        internal Variable[] GetOriginalFields()
        {
            return FindFields();
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
            ulong baseAddress = GetPointerAddress();
            ulong address = baseAddress + (ulong)(index * elementType.Size);

            return Create(elementType, address, ComputedName, GenerateNewPath("[{0}]", index));
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
                return CreatePointerNoCast(codeType, GetPointerAddress() + (ulong)offset, name, path);
            }
            else if (Address != 0)
            {
                return CreateNoCast(codeType, Address + (ulong)offset, name, path);
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
                variable = new Variable(newType, Address, name, path, Data);
            }
            else if (newType.IsPointer)
            {
                variable = CreatePointerNoCast(newType, Address, name, path);
            }
            else if (codeType.IsPointer)
            {
                variable = CreateNoCast(newType, Data, name, path);
            }
            else if (Address != 0)
            {
                return Create(newType, Address, name, path);
            }
            else
            {
                throw new ArgumentException("Variable is not a pointer type, but " + codeType + " and its address is 0");
            }

            return newType.Module.Process.CastVariableToUserType(variable);
        }

        /// <summary>
        /// Casts the specified variable to the new type.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>Computed variable that is of new type.</returns>
        public static T CastAs<T>(Variable variable)
        {
            if (variable == null)
            {
                return default(T);
            }

            return variable.CastAs<T>();
        }

        /// <summary>
        /// Casts variable to the new type.
        /// </summary>
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
            else if (conversionType.GetTypeInfo().IsPrimitive)
            {
                if (conversionType == typeof(bool))
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
                else if (conversionType == typeof(float))
                    return (float)this;
                else if (conversionType == typeof(double))
                    return (double)this;
            }

            // Check if it is null
            if (GetPointerAddress() == 0)
            {
                return null;
            }

            // Check if type has constructor with one argument and that argument is inherited from Variable
            IUserTypeDelegates delegates = UserTypeDelegates.Delegates[conversionType];

            if (delegates.SymbolicConstructor == null)
            {
                throw new InvalidCastException("Cannot cast Variable to " + conversionType);
            }

            return delegates.SymbolicConstructor(this);
        }

        /// <summary>
        /// Casts variable to new type.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <returns>Computed variable that is of new type.</returns>
        public Variable CastAs(string newType)
        {
            CodeType newCodeType;

            if (newType.IndexOf('!') > 0)
            {
                newCodeType = CodeType.Create(newType);
            }
            else
            {
                newCodeType = CodeType.Create(newType, codeType.Module);
            }

            return CastAs(newCodeType);
        }

        /// <summary>
        /// Gets the variable that is casted to base class. <see cref="CodeType"/> of this variable must have single base class.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <remarks>This is not casted to user type</remarks>
        public Variable GetBaseClass()
        {
            if (codeType.InheritedClasses.Count == 0)
            {
                throw new Exception("CodeType doesn't have any base class.");
            }

            if (codeType.InheritedClasses.Count > 1)
            {
                throw new Exception("There is more than one base class.");
            }

            return GetBaseClass(codeType.InheritedClassesSorted[0]);
        }

        /// <summary>
        /// Gets the variable that is casted to base class. <see cref="CodeType" /> of this variable must have single base class.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <typeparam name="T">Type to cast returned variable into.</typeparam>
        public T GetBaseClass<T>()
        {
            return GetBaseClass().CastAs<T>();
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by index.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <remarks>This is not casted to user type</remarks>
        /// <param name="baseClassIndex">Index of the base class by looking at the offset.</param>
        public Variable GetBaseClass(int baseClassIndex)
        {
            if (baseClassIndex < 0 || baseClassIndex >= codeType.InheritedClasses.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(baseClassIndex));
            }

            var tuple = codeType.InheritedClassesSorted[baseClassIndex];

            return GetBaseClass(tuple);
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by index.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <typeparam name="T">Type to cast returned variable into.</typeparam>
        /// <param name="baseClassIndex">Index of the base class by looking at the offset.</param>
        public T GetBaseClass<T>(int baseClassIndex)
        {
            return GetBaseClass(baseClassIndex).CastAs<T>();
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by index.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <remarks>This is not casted to user type</remarks>
        /// <typeparam name="TParent">The type of the multi-class-inheritance "parent".</typeparam>
        /// <param name="baseClassIndex">Index of the base class by looking at the offset.</param>
        /// <param name="userType">The user type.</param>
        public Variable GetBaseClass<TParent>(int baseClassIndex, TParent userType)
            where TParent : UserType
        {
            return GetBaseClass<Variable, TParent>(baseClassIndex, userType);
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by index.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <typeparam name="T">Type to cast returned variable into.</typeparam>
        /// <typeparam name="TParent">The type of the multi-class-inheritance "parent".</typeparam>
        /// <param name="baseClassIndex">Index of the base class by looking at the offset.</param>
        /// <param name="userType">The user type.</param>
        public T GetBaseClass<T, TParent>(int baseClassIndex, TParent userType)
            where TParent : UserType
        {
            object instance = GetBaseClass(baseClassIndex).CastAs(UserTypeDelegates<TParent>.Instance.GetMciAuxiliaryClassType<T>());

            ((IMultiClassInheritance)instance).DowncastParent = userType;
            return (T)instance;
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by name.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <remarks>This is not casted to user type</remarks>
        /// <param name="className">The class name.</param>
        public Variable GetBaseClass(string className)
        {
            if (codeType.Name == className)
            {
                return this;
            }

            var tuple = codeType.BaseClasses[className];

            return GetBaseClass(tuple);
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by the mighty tuple.
        /// </summary>
        /// <remarks>This is not casted to user type</remarks>
        /// <param name="baseClassCodeTypeAndOffset">The base class code type and offset tuple.</param>
        internal Variable GetBaseClass(Tuple<CodeType, int> baseClassCodeTypeAndOffset)
        {
            var newCodeType = baseClassCodeTypeAndOffset.Item1;

            if (newCodeType == codeType)
            {
                return this;
            }

            if (!newCodeType.IsPointer && codeType.IsPointer)
            {
                newCodeType = newCodeType.PointerToType;
            }

            if (newCodeType == codeType)
            {
                return this;
            }

            // Check if it virtual inheritance
            ulong newAddress;

            if (baseClassCodeTypeAndOffset.Item2 < 0)
            {
                newAddress = Context.SymbolProvider.GetVirtualClassBaseAddress(codeType, GetPointerAddress(), baseClassCodeTypeAndOffset.Item1);
            }
            else
            {
                newAddress = GetPointerAddress() + (uint)baseClassCodeTypeAndOffset.Item2;
            }

            if (newCodeType.IsPointer)
            {
                return CreatePointerNoCast(newCodeType, newAddress, name, path);
            }
            else
            {
                return CreateNoCast(newCodeType, newAddress, name, path);
            }
        }

        /// <summary>
        /// Gets the variable that is casted to base class given by name.
        /// This is mostly used by auto generated code (exported from PDB) or to access multi inheritance base classes.
        /// </summary>
        /// <typeparam name="T">Type to cast returned variable into.</typeparam>
        /// <param name="className">The class name.</param>
        public T GetBaseClass<T>(string className)
        {
            return GetBaseClass(className).CastAs<T>();
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

            return Create(fieldType, fieldAddress, fieldName, GenerateNewPath(".{0}", fieldName));
        }

        /// <summary>
        /// Gets the class field as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public T GetClassField<T>(string fieldName)
        {
            Variable field = GetClassField(fieldName);

            if (field == null)
            {
                return default(T);
            }

            return field.CastAs<T>();
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>Field variable if the specified field exists.</returns>
        public Variable GetField(string name)
        {
            return GetUserTypeCastedFieldByName(name);
        }

        /// <summary>
        /// Gets the original field value (not user type casted field).
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>Field variable if the specified field exists.</returns>
        private Variable GetOriginalField(string name)
        {
            CodeType fieldType = codeType.GetFieldType(name);
            int fieldOffset = codeType.GetFieldOffset(name);

            // Check if it is field comming from virtual inheritance class
            if (fieldOffset < 0)
            {
                // We need to first get base class and then get field from it.
                return GetBaseClass(Tuple.Create(fieldType, fieldOffset)).GetOriginalField(name);
            }

            ulong fieldAddress = GetPointerAddress() + (ulong)fieldOffset;
            Variable field = CreateNoCast(fieldType, fieldAddress, name, GenerateNewPath(".{0}", path));

            return UpcastClrVariable(field);
        }

        /// <summary>
        /// Upcasts the CLR variable to a runtime type.
        /// </summary>
        /// <param name="variable">The variable.</param>
        internal static Variable UpcastClrVariable(Variable variable)
        {
            // Check if it is CLR variable
            ClrCodeType clrCodeType = variable.GetCodeType() as ClrCodeType;

            if (clrCodeType != null)
            {
                // Check if CLR code type can be upcasted.
                if (clrCodeType.BuiltinType != BuiltinType.NoType)
                    return variable;
                if (!clrCodeType.IsPointer)
                    return variable;

                // Get runtime type
                IClrType newClrType = clrCodeType.ClrType.Heap.GetObjectType(variable.GetPointerAddress());
                ClrCodeType clrCodeTypeSpecialization = clrCodeType.GetSpecializedCodeType(variable.GetPointerAddress());

                if ((newClrType != clrCodeType.ClrType && newClrType.Module != null) || clrCodeTypeSpecialization != clrCodeType)
                {
                    // New code type is correct, use it
                    ClrCodeType newCodeType = clrCodeTypeSpecialization != clrCodeType ? clrCodeTypeSpecialization : Process.Current.FromClrType(newClrType) as ClrCodeType;
                    ulong address = variable.GetPointerAddress();

                    // If original filed type was System.Object (pointer) and resulting type is struct or simple type (not pointer)
                    // Then, we need to move address for a vtable.
                    if (clrCodeType.IsPointer && !newCodeType.IsPointer)
                    {
                        address = variable.Data + clrCodeType.Module.Process.GetPointerSize();
                    }

                    // Find code type specialization
                    newCodeType = newCodeType.GetSpecializedCodeType(address);

                    // If it is array, take pointer of the first element
                    ClrArrayCodeType clrArrayCodeType = newCodeType as ClrArrayCodeType;

                    if (clrArrayCodeType != null)
                    {
                        address = clrArrayCodeType.ClrType.GetArrayElementAddress(address, 0);
                    }

                    // Create new variable
                    if (newCodeType.IsPointer)
                    {
                        return CreatePointerNoCast(newCodeType, address, variable.name, variable.path);
                    }
                    else
                    {
                        return CreateNoCast(newCodeType, address, variable.name, variable.path);
                    }
                }
            }

            return variable;
        }

        /// <summary>
        /// Gets the user type casted field.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>Field variable casted to user type if the specified field exists.</returns>
        private Variable GetUserTypeCastedFieldByName(string name)
        {
            return codeType.Module.Process.CastVariableToUserType(GetOriginalField(name));
        }

        /// <summary>
        /// Casts the variable to user type.
        /// </summary>
        /// <param name="originalVariable">The original variable.</param>
        internal static Variable CastVariableToUserType(Variable originalVariable)
        {
            if (Context.UserTypeMetadata == null || Context.UserTypeMetadata.Length == 0)
            {
                return originalVariable;
            }

            CodeType codeType = originalVariable.GetCodeType();

            foreach (Type type in codeType.UserTypes)
            {
                // Create new instance of user defined type
                IUserTypeDelegates delegates = UserTypeDelegates.Delegates[type];

                if (delegates.SymbolicConstructor != null)
                {
                    return (Variable)delegates.SymbolicConstructor(originalVariable);
                }
            }

            // Check if it is null
            if (originalVariable.GetPointerAddress() == 0)
            {
                return null;
            }

            return originalVariable;
        }

        /// <summary>
        /// Casts the variable collection to user type.
        /// </summary>
        /// <param name="originalCollection">The original variable collection.</param>
        internal static VariableCollection CastVariableCollectionToUserType(VariableCollection originalCollection)
        {
            Variable[] variables = new Variable[originalCollection?.Count ?? 0];
            string[] names = originalCollection?.Names ?? new string[0];

            for (int i = variables.Length - 1; i >= 0; i--)
            {
                variables[i] = originalCollection[i].codeType.Module.Process.CastVariableToUserType(originalCollection[i]);
            }

            return new VariableCollection(variables, names);
        }

        /// <summary>
        /// Gets the pointer to this variable.
        /// </summary>
        public Variable GetPointer()
        {
            return CreatePointerNoCast(codeType.PointerToType, GetPointerAddress(), name, path);
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
                result = GetUserTypeCastedFieldByName(name);
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
                fields[i] = GetOriginalField(fieldNames[i]);
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
                fields[i] = codeType.Module.Process.CastVariableToUserType(originalFields[i]);
            }

            return fields;
        }

        /// <summary>
        /// Reads the data value of this variable.
        /// </summary>
        private ulong ReadData()
        {
            if (codeType is ClrCodeType)
            {
                uint size = codeType.Size;

                switch (size)
                {
                    case 1:
                        return codeType.Module.Process.ReadByte(Address);
                    case 2:
                        return codeType.Module.Process.ReadUshort(Address);
                    case 4:
                        return codeType.Module.Process.ReadUint(Address);
                    case 8:
                        return codeType.Module.Process.ReadUlong(Address);
                }
            }

            return Context.SymbolProvider.ReadSimpleData(codeType, Address);
        }

        /// <summary>
        /// Generates the new variable path.
        /// If existing name is computed, it will remain like that. If not, new format will be appended to existing name.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        private string GenerateNewPath(string format, params object[] args)
        {
            if (!Context.EnableVariablePathTracking)
            {
                return UntrackedPath;
            }

            if (name == ComputedName)
            {
                return name;
            }

            return name + string.Format(format, args);
        }

        #region IConvertible
        /// <summary>
        /// Returns the <see cref="T:System.TypeCode" /> for this instance.
        /// </summary>
        /// <returns>
        /// The enumerated constant that is the <see cref="T:System.TypeCode" /> of the class or value type that implements this interface.
        /// </returns>
        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Boolean value using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A Boolean value equivalent to the value of this instance.</returns>
        public bool ToBoolean(IFormatProvider provider)
        {
            return (bool)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A Unicode character equivalent to the value of this instance.</returns>
        public char ToChar(IFormatProvider provider)
        {
            return (char)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit signed integer equivalent to the value of this instance.</returns>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return (sbyte)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit unsigned integer equivalent to the value of this instance.</returns>
        public byte ToByte(IFormatProvider provider)
        {
            return (byte)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 16-bit signed integer equivalent to the value of this instance.</returns>
        public short ToInt16(IFormatProvider provider)
        {
            return (short)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 16-bit unsigned integer equivalent to the value of this instance.</returns>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return (ushort)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 32-bit signed integer equivalent to the value of this instance.</returns>
        public int ToInt32(IFormatProvider provider)
        {
            return (int)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 32-bit unsigned integer equivalent to the value of this instance.</returns>
        public uint ToUInt32(IFormatProvider provider)
        {
            return (uint)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 64-bit signed integer equivalent to the value of this instance.</returns>
        public long ToInt64(IFormatProvider provider)
        {
            return (long)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An 64-bit unsigned integer equivalent to the value of this instance.</returns>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return (ulong)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A single-precision floating-point number equivalent to the value of this instance.</returns>
        public float ToSingle(IFormatProvider provider)
        {
            return (float)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A double-precision floating-point number equivalent to the value of this instance.</returns>
        public double ToDouble(IFormatProvider provider)
        {
            return (double)this;
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.Decimal" /> number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="T:System.Decimal" /> number equivalent to the value of this instance.</returns>
        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.DateTime" /> using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="T:System.DateTime" /> instance equivalent to the value of this instance.</returns>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="T:System.String" /> using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="T:System.String" /> instance equivalent to the value of this instance.</returns>
        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        /// <summary>
        /// Converts the value of this instance to an <see cref="T:System.Object" /> of the specified <see cref="T:System.Type" /> that has an equivalent value, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="conversionType">The <see cref="T:System.Type" /> to which the value of this instance is converted.</param>
        /// <param name="provider">An <see cref="T:System.IFormatProvider" /> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An <see cref="T:System.Object" /> instance of type conversionType whose value is equivalent to the value of this instance.</returns>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return CastAs(conversionType);
        }
        #endregion

        #region IEquatable<Variable>
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Variable other)
        {
            if (other == null && IsNullPointer())
            {
                return true;
            }

            if (other == null || GetPointerAddress() != other.GetPointerAddress())
            {
                return false;
            }

            if (codeType == other.codeType)
            {
                return true;
            }

            if (codeType.IsPointer == other.codeType.IsPointer)
            {
                return false;
            }

            if ((codeType.IsPointer && codeType.ElementType == other.codeType)
                || (other.codeType.IsPointer && other.codeType.ElementType == codeType))
            {
                return true;
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null && IsNullPointer())
            {
                return true;
            }

            Variable other = obj as Variable;

            if (other != null)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="first">First object.</param>
        /// <param name="second">Second object.</param>
        /// <returns><c>true</c> if objects are equal.</returns>
        public static bool operator ==(Variable first, Variable second)
        {
            if (ReferenceEquals(first, second))
                return true;
            if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
                return false;
            return first != null && first.Equals(second);
        }

        /// <summary>
        /// Determines whether the specified objects are not equal.
        /// </summary>
        /// <param name="first">First object.</param>
        /// <param name="second">Second object.</param>
        /// <returns><c>true</c> if objects are not equal.</returns>
        public static bool operator !=(Variable first, Variable second)
        {
            return !(first == second);
        }
    }
}
