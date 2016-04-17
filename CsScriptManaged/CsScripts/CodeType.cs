using CsScriptManaged;
using CsScriptManaged.Native;
using CsScriptManaged.Utility;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CsScripts
{
    /// <summary>
    /// Debugging type of variables
    /// </summary>
    public abstract class CodeType
    {
        /// <summary>
        /// The element type
        /// </summary>
        protected internal SimpleCache<CodeType> elementType;

        /// <summary>
        /// The pointer to type
        /// </summary>
        protected internal SimpleCache<CodeType> pointerToType;

        /// <summary>
        /// The name
        /// </summary>
        protected internal SimpleCache<string> name;

        /// <summary>
        /// The size
        /// </summary>
        protected internal SimpleCache<uint> size;

        /// <summary>
        /// The all field names (including all base classes)
        /// </summary>
        protected internal SimpleCache<string[]> allFieldNames;

        /// <summary>
        /// The field names
        /// </summary>
        protected internal SimpleCache<string[]> fieldNames;

        /// <summary>
        /// The direct base classes and offsets
        /// </summary>
        protected internal SimpleCache<Dictionary<string, Tuple<CodeType, int>>> directBaseClassesAndOffsets;

        /// <summary>
        /// All field types and offsets
        /// </summary>
        protected internal DictionaryCache<string, Tuple<CodeType, int>> allFieldTypesAndOffsets;

        /// <summary>
        /// The field type and offsets
        /// </summary>
        protected internal DictionaryCache<string, Tuple<CodeType, int>> fieldTypeAndOffsets;

        /// <summary>
        /// The base classes and offsets
        /// </summary>
        protected internal DictionaryCache<string, Tuple<CodeType, int>> baseClassesAndOffsets;

        /// <summary>
        /// The template arguments
        /// </summary>
        protected internal SimpleCache<object[]> templateArguments;

        /// <summary>
        /// The template arguments
        /// </summary>
        protected internal SimpleCache<string[]> templateArgumentsStrings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType" /> class.
        /// </summary>
        /// <param name="module">The module.</param>
        protected CodeType(Module module)
        {
            Module = module;

            elementType = SimpleCache.Create(GetElementType);
            pointerToType = SimpleCache.Create(GetPointerToType);
            name = SimpleCache.Create(GetTypeName);
            size = SimpleCache.Create(GetTypeSize);
            directBaseClassesAndOffsets = SimpleCache.Create(GetDirectBaseClassesAndOffsets);
            allFieldNames = SimpleCache.Create(GetTypeAllFieldNames);
            fieldNames = SimpleCache.Create(GetTypeFieldNames);
            allFieldTypesAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>(GetAllFieldTypeAndOffset);
            fieldTypeAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>(GetFieldTypeAndOffset);
            baseClassesAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>(GetBaseClassAndOffset);
            templateArgumentsStrings = SimpleCache.Create(GetTemplateArgumentsStrings);
            templateArguments = SimpleCache.Create(GetTemplateArguments);
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Gets the type of the element if type is array or pointer.
        /// </summary>
        public CodeType ElementType
        {
            get
            {
                return elementType.Value;
            }
        }

        /// <summary>
        /// Gets the type of the pointer to this type.
        /// </summary>
        public CodeType PointerToType
        {
            get
            {
                return pointerToType.Value;
            }
        }

        /// <summary>
        /// Gets the type name.
        /// </summary>
        public string Name
        {
            get
            {
                return name.Value;
            }
        }

        /// <summary>
        /// Gets the type size in bytes.
        /// </summary>
        public uint Size
        {
            get
            {
                return size.Value;
            }
        }

        /// <summary>
        /// Gets the type field names (including base classes).
        /// </summary>
        public string[] FieldNames
        {
            get
            {
                return allFieldNames.Value;
            }
        }

        /// <summary>
        /// Gets the class field names (it doesn't include base classes).
        /// </summary>
        public string[] ClassFieldNames
        {
            get
            {
                return fieldNames.Value;
            }
        }

        /// <summary>
        /// Gets the template arguments strings.
        /// <para>For given type: MyType&lt;Arg1, 2, Arg3&lt;5&gt;&gt;</para>
        /// <para>It will return: <code>new string[] { "Arg1", "2", "Arg3&lt;5&gt;" }</code></para>
        /// </summary>
        public string[] TemplateArgumentsStrings
        {
            get
            {
                return templateArgumentsStrings.Value;
            }
        }

        /// <summary>
        /// Gets the template arguments.
        /// <para>For given type: MyType&lt;Arg1, 2, Arg3&lt;5&gt;&gt;</para>
        /// <para>It will return: <code>new string[] { CodeType.Create("Arg1", Module), 2, CodeType.Create("Arg3&lt;5&gt;", Module) }</code></para>
        /// </summary>
        public object[] TemplateArguments
        {
            get
            {
                return templateArguments.Value;
            }
        }

        /// <summary>
        /// Gets the base classes - classes that this class inherits from.
        /// </summary>
        public Dictionary<string, Tuple<CodeType, int>> InheritedClasses
        {
            get
            {
                return directBaseClassesAndOffsets.Value;
            }
        }

        /// <summary>
        /// Gets the inherited class.
        /// </summary>
        public CodeType InheritedClass
        {
            get
            {
                if (InheritedClasses.Count > 1)
                {
                    throw new Exception("Multiple inheritance is not supported");
                }

                if (InheritedClasses.Count == 0)
                {
                    throw new Exception("This type doesn't inherit any class");
                }

                return InheritedClasses.Values.First().Item1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is enum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is enum; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsEnum { get; }

        /// <summary>
        /// Gets a value indicating whether this type is array.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is array; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsArray { get; }

        /// <summary>
        /// Gets a value indicating whether this type is pointer.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is pointer; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsPointer { get; }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI or wide string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is string; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsString { get; }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is ANSI string; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsAnsiString { get; }

        /// <summary>
        /// Gets a value indicating whether this type is wide string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is wide string; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsWideString { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is floating point number (float or double).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is floating point number; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsReal { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is float.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is float; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsFloat { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is double.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is double; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsDouble { get; }

        /// <summary>
        /// Gets a value indicating whether this type is simple type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is simple type; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsSimple { get; }

        /// <summary>
        /// Gets a value indicating whether this type is function type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is function type; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsFunction { get; }

        /// <summary>
        /// Gets the class fields.
        /// </summary>
        internal DictionaryCache<string, Tuple<CodeType, int>> ClassFields
        {
            get
            {
                return fieldTypeAndOffsets;
            }
        }

        /// <summary>
        /// Gets the base classes.
        /// </summary>
        internal DictionaryCache<string, Tuple<CodeType, int>> BaseClasses
        {
            get
            {
                return baseClassesAndOffsets;
            }
        }

        /// <summary>
        /// Gets the type of the class field.
        /// </summary>
        /// <param name="classFieldName">Name of the class field.</param>
        public CodeType GetClassFieldType(string classFieldName)
        {
            return ClassFields[classFieldName].Item1;
        }

        /// <summary>
        /// Gets field offset.
        /// </summary>
        public int GetFieldOffset(string fieldName)
        {
            return allFieldTypesAndOffsets[fieldName].Item2;
        }

        /// <summary>
        /// Gets offset of all fields.
        /// </summary>
        public Dictionary<string, int> GetFieldOffsets()
        {
            Dictionary<string, int> offsets = new Dictionary<string, int>();

            foreach (var field in FieldNames)
            {
                offsets[field] = GetFieldOffset(field);
            }

            return offsets;
        }

        /// <summary>
        /// Gets field code type.
        /// </summary>
        public CodeType GetFieldType(string fieldName)
        {
            return allFieldTypesAndOffsets[fieldName].Item1;
        }

        /// <summary>
        /// Gets code type of all fields.
        /// </summary>
        public Dictionary<string, CodeType> GetFieldTypes()
        {
            Dictionary<string, CodeType> offsets = new Dictionary<string, CodeType>();

            foreach (var field in FieldNames)
            {
                offsets[field] = GetFieldType(field);
            }

            return offsets;
        }

        /// <summary>
        /// Creates the code type from specified name and module. If name contains module!, module can be omitted.
        /// </summary>
        /// <param name="codeTypeName">The code type name.</param>
        /// <param name="module">The module.</param>
        public static CodeType Create(string codeTypeName, Module module = null)
        {
            int moduleIndex = codeTypeName.IndexOf('!');

            if (moduleIndex > 0)
            {
                string moduleName = codeTypeName.Substring(0, moduleIndex);

                if (module != null && moduleName.ToLowerInvariant() != module.Name.ToLowerInvariant())
                {
                    throw new Exception(string.Format("Module specified inside codeTypeName doesn't match specified module parameter.\ncodeTypeName: {0}\nmodule.Name = {1}", codeTypeName, module.Name));
                }

                if (module == null && string.IsNullOrEmpty(moduleName) == false)
                {
                    module = Process.Current.ModulesByName[moduleName];
                }
                else
                {
                    module = module.Process.ModulesByName[moduleName];
                }
                codeTypeName = codeTypeName.Substring(moduleIndex + 1);
            }
            else if (module == null)
            {
                throw new Exception("Module must be specified either using module parameter or using codeTypeName with 'module!'");
            }

            return module.TypesByName[codeTypeName];
        }

        /// <summary>
        /// Checks if this instance inherits the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        public bool Inherits(CodeType codeType)
        {
            if (this == codeType)
            {
                return true;
            }

            foreach (var inheritedClass in InheritedClasses.Values)
            {
                if (inheritedClass.Item1.Inherits(codeType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if this instance inherits the specified type name.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        public bool Inherits(string typeName)
        {
            if (Name == typeName)
            {
                return true;
            }

            foreach (var inheritedClass in InheritedClasses.Values)
            {
                if (inheritedClass.Item1.Inherits(typeName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if this instance inherits the specified type.
        /// </summary>
        /// <typeparam name="T">Type to check if this instance inherits it</typeparam>
        public bool Inherits<T>()
            where T : UserType
        {
            return Inherits(typeof(T));
        }

        /// <summary>
        /// Checks if this instance inherits the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        public bool Inherits(Type type)
        {
            if (type.IsSubclassOf(typeof(Variable)))
            {
                UserTypeDescription[] descriptions = Module.Process.TypeToUserTypeDescription[type];

                foreach (var description in descriptions)
                {
                    CodeType newType = description.UserType;

                    // Check if it was non-unique generics type
                    if (newType != null && Inherits(newType))
                        return true;
                }

                // TODO: Do better with generics
                UserTypeMetadata[] metadatas = UserTypeMetadata.ReadFromType(type);

                foreach (var metadata in metadatas)
                    if (Inherits(metadata.TypeName))
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        protected abstract CodeType GetElementType();

        /// <summary>
        /// Gets the pointer to type.
        /// </summary>
        protected abstract CodeType GetPointerToType();

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected abstract string GetTypeName();

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected abstract uint GetTypeSize();

        /// <summary>
        /// Gets the direct base classes and offsets.
        /// </summary>
        protected abstract Dictionary<string, Tuple<CodeType, int>> GetDirectBaseClassesAndOffsets();

        /// <summary>
        /// Gets the type all field names (including all base classes).
        /// </summary>
        protected abstract string[] GetTypeAllFieldNames();

        /// <summary>
        /// Gets the type field names.
        /// </summary>
        protected abstract string[] GetTypeFieldNames();

        /// <summary>
        /// Gets field type and offset from all fields (including all base classes).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        protected abstract Tuple<CodeType, int> GetAllFieldTypeAndOffset(string fieldName);

        /// <summary>
        /// Gets the field type and offset.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        protected abstract Tuple<CodeType, int> GetFieldTypeAndOffset(string fieldName);

        /// <summary>
        /// Gets the base class and offset.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        protected abstract Tuple<CodeType, int> GetBaseClassAndOffset(string className);

        /// <summary>
        /// Gets the template arguments.
        /// </summary>
        protected abstract object[] GetTemplateArguments();

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        protected abstract string[] GetTemplateArgumentsStrings();
    }

    /// <summary>
    /// Debugging type of native variables
    /// </summary>
    internal class NativeCodeType : CodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType"/> class.
        /// </summary>
        /// <remarks>This should not be used directly, but through Module.TypesById[typeId]</remarks>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="tag">The symbol tag.</param>
        /// <param name="basicType">Type of the basic type.</param>
        internal NativeCodeType(Module module, uint typeId, SymTag tag, Dia2Lib.BasicType basicType)
            : base(module)
        {
            TypeId = typeId;
            Tag = tag;
            BasicType = basicType;

            if (IsPointer && module.IsFakeCodeTypeId(typeId))
            {
                if (!ElementType.pointerToType.Cached)
                {
                    ElementType.pointerToType.Value = this;
                }
            }
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        public uint TypeId { get; private set; }

        /// <summary>
        /// Gets the symbol tag.
        /// </summary>
        internal SymTag Tag { get; private set; }

        /// <summary>
        /// Gets the type of the basic type.
        /// </summary>
        internal Dia2Lib.BasicType BasicType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this type is enum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is enum; otherwise, <c>false</c>.
        /// </value>
        public override bool IsEnum
        {
            get
            {
                return Tag == SymTag.Enum;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is array.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is array; otherwise, <c>false</c>.
        /// </value>
        public override bool IsArray
        {
            get
            {
                return Tag == SymTag.ArrayType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is pointer.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is pointer; otherwise, <c>false</c>.
        /// </value>
        public override bool IsPointer
        {
            get
            {
                return Tag == SymTag.PointerType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI or wide string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsString
        {
            get
            {
                return IsAnsiString || IsWideString;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is ANSI string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsAnsiString
        {
            get
            {
                return (IsArray || IsPointer) && ElementType.Size == 1 && ElementType.IsSimple;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is wide string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is wide string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsWideString
        {
            get
            {
                return (IsArray || IsPointer) && ElementType.Size == 2 && ElementType.IsSimple;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is floating point number (float or double).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is floating point number; otherwise, <c>false</c>.
        /// </value>
        public override bool IsReal
        {
            get
            {
                return BasicType == Dia2Lib.BasicType.Float;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is float.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is float; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFloat
        {
            get
            {
                return IsReal && Size == 4;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is double.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is double; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDouble
        {
            get
            {
                return IsReal && Size == 8;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is simple type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is simple type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSimple
        {
            get
            {
                return Tag == SymTag.BaseType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is function type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is function type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFunction
        {
            get
            {
                return Tag == SymTag.FunctionType;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return Context.SymbolProvider.GetTypeName(Module, TypeId);
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return Context.SymbolProvider.GetTypeSize(Module, TypeId);
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        protected override CodeType GetElementType()
        {
            if (IsPointer || IsArray)
            {
                try
                {
                    uint elementTypeId = Context.SymbolProvider.GetTypeElementTypeId(Module, TypeId);
                    CodeType elementType = Module.TypesById[elementTypeId];

                    elementType.pointerToType.Value = this;
                    return elementType;
                }
                catch (Exception)
                {
                }
            }

            return this;
        }

        /// <summary>
        /// Gets the pointer to type.
        /// </summary>
        protected override CodeType GetPointerToType()
        {
            try
            {
                uint elementTypeId = Context.SymbolProvider.GetTypePointerToTypeId(Module, TypeId);
                return Module.TypesById[elementTypeId];
            }
            catch (Exception)
            {
                NativeCodeType codeType = new NativeCodeType(Module, Module.GetNextFakeCodeTypeId(), SymTag.PointerType, Dia2Lib.BasicType.NoType);

                codeType.elementType.Value = this;
                codeType.name.Value = name.Value + "*";
                codeType.size.Value = Module.Process.GetPointerSize();
                if (!IsPointer)
                {
                    codeType.allFieldNames = allFieldNames;
                    codeType.allFieldTypesAndOffsets = allFieldTypesAndOffsets;
                    codeType.fieldNames = fieldNames;
                    codeType.fieldTypeAndOffsets = fieldTypeAndOffsets;
                    codeType.baseClassesAndOffsets = baseClassesAndOffsets;
                    codeType.directBaseClassesAndOffsets = directBaseClassesAndOffsets;
                    codeType.templateArguments = templateArguments;
                    codeType.templateArgumentsStrings = templateArgumentsStrings;
                }
                else
                {
                    codeType.allFieldNames.Value = new string[0];
                    codeType.fieldNames.Value = new string[0];
                    codeType.directBaseClassesAndOffsets.Value = new Dictionary<string, Tuple<CodeType, int>>();
                    codeType.templateArguments.Value = new object[0];
                    codeType.templateArgumentsStrings.Value = new string[0];
                }

                return codeType;
            }
        }

        /// <summary>
        /// Gets the direct base classes and offsets.
        /// </summary>
        protected override Dictionary<string, Tuple<CodeType, int>> GetDirectBaseClassesAndOffsets()
        {
            var baseClasses = Context.SymbolProvider.GetTypeDirectBaseClasses(Module, TypeId);
            var result = new Dictionary<string, Tuple<CodeType, int>>();

            foreach (var baseClass in baseClasses)
            {
                result.Add(baseClass.Key, Tuple.Create(Module.TypesById[baseClass.Value.Item1], baseClass.Value.Item2));
            }

            return result;
        }

        /// <summary>
        /// Gets the type all field names (including all base classes).
        /// </summary>
        protected override string[] GetTypeAllFieldNames()
        {
            return Context.SymbolProvider.GetTypeAllFieldNames(Module, TypeId);
        }

        /// <summary>
        /// Gets the type field names.
        /// </summary>
        protected override string[] GetTypeFieldNames()
        {
            return Context.SymbolProvider.GetTypeFieldNames(Module, TypeId);
        }

        /// <summary>
        /// Gets field type and offset from all fields (including all base classes).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        protected override Tuple<CodeType, int> GetAllFieldTypeAndOffset(string fieldName)
        {
            var field = Context.SymbolProvider.GetTypeAllFieldTypeAndOffset(Module, TypeId, fieldName);

            return Tuple.Create(Module.TypesById[field.Item1], field.Item2);
        }

        /// <summary>
        /// Gets the field type and offset.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        protected override Tuple<CodeType, int> GetFieldTypeAndOffset(string fieldName)
        {
            var field = Context.SymbolProvider.GetTypeFieldTypeAndOffset(Module, TypeId, fieldName);

            return Tuple.Create(Module.TypesById[field.Item1], field.Item2);
        }

        /// <summary>
        /// Gets the base class and offset.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        protected override Tuple<CodeType, int> GetBaseClassAndOffset(string className)
        {
            var baseClass = Context.SymbolProvider.GetTypeBaseClass(Module, TypeId, className);

            return Tuple.Create(Module.TypesById[baseClass.Item1], baseClass.Item2);
        }

        /// <summary>
        /// Gets the template arguments.
        /// </summary>
        protected override object[] GetTemplateArguments()
        {
            string[] arguments = TemplateArgumentsStrings;
            object[] result = new object[arguments.Length];

            for (int i = 0; i < result.Length; i++)
            {
                int intValue;

                if (int.TryParse(arguments[i], out intValue))
                {
                    result[i] = intValue;
                }
                else
                {
                    result[i] = CodeType.Create(arguments[i], Module);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        protected override string[] GetTemplateArgumentsStrings()
        {
            return GetTemplateArgumentsStrings(Name);
        }

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        public static string[] GetTemplateArgumentsStrings(string name)
        {
            var arguments = new List<string>();
            int templateStart = name.IndexOf('<');

            if (templateStart > 0)
            {
                for (int i = templateStart + 1; i < name.Length; i++)
                {
                    var extractedType = ExtractType(name, i);

                    arguments.Add(extractedType.Trim());
                    i += extractedType.Length;
                }
            }

            return arguments.ToArray();
        }

        /// <summary>
        /// Extracts the type from template arguments list.
        /// </summary>
        /// <param name="inputType">The input type name.</param>
        /// <param name="indexStart">The index start.</param>
        private static string ExtractType(string inputType, int indexStart)
        {
            int i = indexStart;
            int openedTypes = 0;

            while (i < inputType.Length && (openedTypes != 0 || (inputType[i] != ',' && inputType[i] != '>')))
            {
                switch (inputType[i])
                {
                    case '<':
                        openedTypes++;
                        break;
                    case '>':
                        openedTypes--;
                        break;
                }

                i++;
            }

            return inputType.Substring(indexStart, i - indexStart);
        }
    }

    /// <summary>
    /// Debugging type of CLR variables
    /// </summary>
    internal class ClrCodeType : CodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="clrType">The CLR type.</param>
        public ClrCodeType(Module module, Microsoft.Diagnostics.Runtime.ClrType clrType)
            : base(module)
        {
            ClrType = clrType;
        }

        /// <summary>
        /// Gets the CLR type.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrType ClrType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is ANSI string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsAnsiString
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is array.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is array; otherwise, <c>false</c>.
        /// </value>
        public override bool IsArray
        {
            get
            {
                return ClrType.IsArray;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is double.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is double; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDouble
        {
            get
            {
                return ClrType.ElementType == Microsoft.Diagnostics.Runtime.ClrElementType.Double;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is enum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is enum; otherwise, <c>false</c>.
        /// </value>
        public override bool IsEnum
        {
            get
            {
                return ClrType.IsEnum;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is float.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is float; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFloat
        {
            get
            {
                return ClrType.ElementType == Microsoft.Diagnostics.Runtime.ClrElementType.Float;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is function type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is function type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFunction
        {
            get
            {
                return ClrType.ElementType == Microsoft.Diagnostics.Runtime.ClrElementType.FunctionPointer;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is pointer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is pointer; otherwise, <c>false</c>.
        /// </value>
        public override bool IsPointer
        {
            get
            {
                return ClrType.IsPointer;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is floating point number (float or double).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is floating point number; otherwise, <c>false</c>.
        /// </value>
        public override bool IsReal
        {
            get
            {
                return IsDouble || IsFloat;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is simple type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is simple type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSimple
        {
            get
            {
                return ClrType.IsPrimitive;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI or wide string.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsString
        {
            get
            {
                return IsWideString || IsAnsiString;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this type is wide string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is wide string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsWideString
        {
            get
            {
                return ClrType.IsString;
            }
        }

        /// <summary>
        /// Gets field type and offset from all fields (including all base classes).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        protected override Tuple<CodeType, int> GetAllFieldTypeAndOffset(string fieldName)
        {
            for (var clrType = ClrType; clrType != null; clrType = clrType.BaseType)
            {
                var field = clrType.Fields.Where(f => f.Name == fieldName).FirstOrDefault();

                if (field != null && field.Type != null)
                {
                    return GetFieldTypeAndOffset(field);
                }
            }

            throw new EntryPointNotFoundException(fieldName);
        }

        /// <summary>
        /// Gets the field type and offset.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        protected override Tuple<CodeType, int> GetFieldTypeAndOffset(string fieldName)
        {
            var field = ClrType.Fields.Where(f => f.Name == fieldName).FirstOrDefault();

            if (field != null && field.Type != null)
            {
                return GetFieldTypeAndOffset(field);
            }

            throw new EntryPointNotFoundException(fieldName);
        }

        /// <summary>
        /// Gets the field type and offset.
        /// </summary>
        /// <param name="field">The CLR field.</param>
        private Tuple<CodeType, int> GetFieldTypeAndOffset(Microsoft.Diagnostics.Runtime.ClrInstanceField field)
        {
            return Tuple.Create(Module.FromClrType(field.Type), (int)field.GetAddress(0));
        }

        /// <summary>
        /// Gets the base class and offset.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        protected override Tuple<CodeType, int> GetBaseClassAndOffset(string className)
        {
            if (Name == className)
                return new Tuple<CodeType, int>(this, 0);
            for (var clrType = ClrType; clrType != null; clrType = clrType.BaseType)
                if (clrType.Name == className)
                    return Tuple.Create(Module.FromClrType(clrType), 0);
            throw new EntryPointNotFoundException(className);
        }

        /// <summary>
        /// Gets the direct base classes and offsets.
        /// </summary>
        protected override Dictionary<string, Tuple<CodeType, int>> GetDirectBaseClassesAndOffsets()
        {
            Dictionary<string, Tuple<CodeType, int>> baseClassesAndOffsets = new Dictionary<string, Tuple<CodeType, int>>();
            var baseType = ClrType.BaseType;

            if (baseType != null)
                baseClassesAndOffsets.Add(baseType.Name, Tuple.Create(Module.FromClrType(baseType), 0));
            return baseClassesAndOffsets;
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        protected override CodeType GetElementType()
        {
            if (ClrType.ComponentType != null)
                return Module.FromClrType(ClrType.ComponentType);
            return this;
        }

        /// <summary>
        /// Gets the pointer to type.
        /// </summary>
        protected override CodeType GetPointerToType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the template arguments.
        /// </summary>
        protected override object[] GetTemplateArguments()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        protected override string[] GetTemplateArgumentsStrings()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type all field names (including all base classes).
        /// </summary>
        protected override string[] GetTypeAllFieldNames()
        {
            IEnumerable<Microsoft.Diagnostics.Runtime.ClrInstanceField> fields = ClrType.Fields;

            for (var baseType = ClrType.BaseType; baseType != null; baseType = baseType.BaseType)
                fields = fields.Union(baseType.Fields);
            return fields.Select(f => f.Name).ToArray();
        }

        /// <summary>
        /// Gets the type field names.
        /// </summary>
        protected override string[] GetTypeFieldNames()
        {
            return ClrType.Fields.Select(f => f.Name).ToArray();
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return ClrType.Name;
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return (uint)ClrType.BaseSize;
        }
    }
}
