﻿using SharpDebug.CLR;
using SharpDebug.Engine;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpDebug
{
    /// <summary>
    /// Debugging type of variables
    /// </summary>
    public abstract class CodeType
    {
        /// <summary>
        /// The naked pointer code type name
        /// </summary>
        internal const string NakedPointerCodeTypeName = "~NakedPointerCodeTypeName~";

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
        /// The direct base classes and offsets sorted by offset
        /// </summary>
        protected internal SimpleCache<Tuple<CodeType, int>[]> directBaseClassesAndOffsetsArraySorted;

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
        /// Array of user types associated with this code type.
        /// </summary>
        private SimpleCache<Type[]> userTypes;

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
            directBaseClassesAndOffsetsArraySorted = SimpleCache.Create(() => InheritedClasses.Values.OrderBy(t => t.Item2).ThenBy(t => t.Item1.Name).ToArray());
            allFieldNames = SimpleCache.Create(GetTypeAllFieldNames);
            fieldNames = SimpleCache.Create(GetTypeFieldNames);
            allFieldTypesAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>(GetAllFieldTypeAndOffset);
            fieldTypeAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>(GetFieldTypeAndOffset);
            baseClassesAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>(GetBaseClassAndOffset);
            templateArgumentsStrings = SimpleCache.Create(GetTemplateArgumentsStrings);
            templateArguments = SimpleCache.Create(GetTemplateArguments);
            userTypes = Context.UserTypeMetadataCaches.CreateSimpleCache(GetUserTypes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType"/> class.
        /// </summary>
        /// <param name="originalCodeType">The original CodeType.</param>
        protected CodeType(CodeType originalCodeType)
        {
            Module = originalCodeType.Module;
            elementType = originalCodeType.elementType;
            pointerToType = originalCodeType.pointerToType;
            name = originalCodeType.name;
            size = originalCodeType.size;
            directBaseClassesAndOffsets = originalCodeType.directBaseClassesAndOffsets;
            directBaseClassesAndOffsetsArraySorted = originalCodeType.directBaseClassesAndOffsetsArraySorted;
            allFieldNames = originalCodeType.allFieldNames;
            fieldNames = originalCodeType.fieldNames;
            allFieldTypesAndOffsets = originalCodeType.allFieldTypesAndOffsets;
            fieldTypeAndOffsets = originalCodeType.fieldTypeAndOffsets;
            baseClassesAndOffsets = originalCodeType.baseClassesAndOffsets;
            templateArgumentsStrings = originalCodeType.templateArgumentsStrings;
            templateArguments = originalCodeType.templateArguments;
            userTypes = originalCodeType.userTypes;
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
        /// <para>It will return: <code>new object[] { CodeType.Create("Arg1", Module), 2, CodeType.Create("Arg3&lt;5&gt;", Module) }</code></para>
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
        /// Gets the base classes - classes that this class inherits from. Array is sorted by offset.
        /// </summary>
        internal Tuple<CodeType, int>[] InheritedClassesSorted
        {
            get
            {
                return directBaseClassesAndOffsetsArraySorted.Value;
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
        /// Gets the built-in type.
        /// </summary>
        public abstract BuiltinType BuiltinType { get; }

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
        /// Gets the array of user types associated with this code type.
        /// </summary>
        internal Type[] UserTypes
        {
            get
            {
                return userTypes.Value;
            }
        }

        /// <summary>
        /// Removes pointer from the code type.
        /// </summary>
        /// <returns>Element type of if this code type is a pointer or self if it isn't a pointer.</returns>
        public virtual CodeType RemovePointer()
        {
            return IsPointer ? ElementType : this;
        }

        /// <summary>
        /// Gets the type of the class field.
        /// </summary>
        /// <param name="classFieldName">Name of the class field.</param>
        /// <returns>The type of the class field.</returns>
        public CodeType GetClassFieldType(string classFieldName)
        {
            return ClassFields[classFieldName].Item1;
        }

        /// <summary>
        /// Gets the field offset.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The field offset.</returns>
        public int GetFieldOffset(string fieldName)
        {
            return allFieldTypesAndOffsets[fieldName].Item2;
        }

        /// <summary>
        /// Gets the offset of all fields.
        /// </summary>
        /// <returns>The offset of all fields.</returns>
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
        /// Gets the field code type.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The field code type.</returns>
        public CodeType GetFieldType(string fieldName)
        {
            return allFieldTypesAndOffsets[fieldName].Item1;
        }

        /// <summary>
        /// Gets the code type of all fields.
        /// </summary>
        /// <returns>The code type of all fields.</returns>
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
        /// <returns>CodeType from specified name and module</returns>
        public static CodeType Create(string codeTypeName, Module module = null)
        {
            return Create(module != null ? module.Process : Process.Current, codeTypeName, module);
        }

        /// <summary>
        /// Creates the code type from specified name and module. If name contains module!, module can be omitted.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="codeTypeName">The code type name.</param>
        /// <param name="module">The module.</param>
        /// <returns>CodeType from specified name and module</returns>
        public static CodeType Create(Process process, string codeTypeName, Module module = null)
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
                    module = process.ModulesByName[moduleName];
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
        /// Creates the code type from the list containing specified module name and type.
        /// First successful created code type is returned.
        /// </summary>
        /// <param name="codeTypeNames">The list of code type names in form "module!codetype".</param>
        /// <returns>CodeType from specified name and module</returns>
        public static CodeType Create(params string[] codeTypeNames)
        {
            return Create(Process.Current, codeTypeNames);
        }

        /// <summary>
        /// Creates the code type from the list containing specified module name and type.
        /// First successful created code type is returned.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="codeTypeNames">The list of code type names in form "module!codetype".</param>
        /// <returns>CodeType from specified name and module</returns>
        public static CodeType Create(Process process, params string[] codeTypeNames)
        {
            CodeType lastDeclaration = null;

            foreach (string codeTypeName in codeTypeNames)
            {
                try
                {
                    CodeType codeType = Create(process, codeTypeName);

                    if (codeType.Size > 0)
                    {
                        return codeType;
                    }
                    else
                    {
                        lastDeclaration = codeType;
                    }
                }
                catch
                {
                    // ignore this module and try another one
                }
            }

            if (lastDeclaration != null)
            {
                return lastDeclaration;
            }

            throw new Exception("Unable to create requested code type");
        }

        /// <summary>
        /// Checks if this instance inherits the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if this instance inherits the specified code type; otherwise <c>false</c></returns>
        public bool Inherits(CodeType codeType)
        {
            if (this == codeType)
            {
                return true;
            }

            foreach (var inheritedClass in InheritedClassesSorted)
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
        /// <returns><c>true</c> if this instance inherits the specified type name; otherwise <c>false</c></returns>
        public bool Inherits(string typeName)
        {
            if (TypeNameMatches(Name, typeName))
            {
                return true;
            }

            foreach (var inheritedClass in InheritedClassesSorted)
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
        /// <returns><c>true</c> if this instance inherits the specified type; otherwise <c>false</c></returns>
        public bool Inherits<T>()
            where T : UserType
        {
            return Inherits(typeof(T));
        }

        /// <summary>
        /// Checks if this instance inherits the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if this instance inherits the specified type; otherwise <c>false</c></returns>
        public bool Inherits(Type type)
        {
            if (type.GetTypeInfo().IsSubclassOf(typeof(Variable)))
            {
                // Check types associated with this code type and all of its inherited code types.
                if (InheritsPrivate(type))
                    return true;

                // Check metadata that wasn't extracted from the debugger context.
                foreach (var metadata in UserTypeMetadata.ReadFromType(type))
                    if (Inherits(metadata.TypeName))
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if this instance inherits the specified type by scaning array of user types and recursive for all inherited classes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if this instance inherits the specified type; otherwise <c>false</c></returns>
        private bool InheritsPrivate(Type type)
        {
            // Check user types
            foreach (Type ut in UserTypes)
            {
                if (ut == type)
                {
                    return true;
                }
            }

            // Check inherited classes
            foreach (var tuple in InheritedClasses.Values)
            {
                if (tuple.Item1.InheritsPrivate(type))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type can be used for this code type.
        /// </summary>
        /// <param name="type">The user type.</param>
        /// <returns><c>true</c> if the specified type can be used for this code type.</returns>
        public bool IsFor(Type type)
        {
            if (type.IsSubclassOf(typeof(Variable)))
            {
                foreach (Type ut in UserTypes)
                    if (ut == type)
                        return true;
                foreach (var metadata in UserTypeMetadata.ReadFromType(type))
                    if (TypeNameMatches(Name, metadata.TypeName))
                        return true;
            }

            return false;
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
        /// Determines whether the specified type name can be used for this code type.
        /// </summary>
        /// <param name="typeName">The type name written as attribute on user type.</param>
        /// <returns><c>true</c> if the specified type name can be used for this code type.</returns>
        internal bool IsFor(string typeName)
        {
            return TypeNameMatches(Name, typeName);
        }

        /// <summary>
        /// Checks whether types matches by comparing names.
        /// </summary>
        /// <param name="name">The type name.</param>
        /// <param name="className">The string read from the user type.</param>
        internal static bool TypeNameMatches(string name, string className)
        {
            if (name == className)
                return true;

            // TODO: Do better matching of generics type
            if (className.Contains("<>"))
            {
                string[] partials = className.Split(new string[] { "<>" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string p in partials)
                    if (!name.Contains(p))
                        return false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the static field from this type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        public abstract Variable GetStaticField(string staticFieldName);

        /// <summary>
        /// Gets the static field from CLR type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        /// <param name="appDomain">The CLR application domain.</param>
        public abstract Variable GetClrStaticField(string staticFieldName, CLR.IClrAppDomain appDomain);

        /// <summary>
        /// Gets the element type.
        /// </summary>
        /// <returns>The element type.</returns>
        protected abstract CodeType GetElementType();

        /// <summary>
        /// Gets the pointer to type.
        /// </summary>
        /// <returns>The pointer to type.</returns>
        protected abstract CodeType GetPointerToType();

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <returns>The name of the type.</returns>
        protected abstract string GetTypeName();

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        /// <returns>The size of the type.</returns>
        protected abstract uint GetTypeSize();

        /// <summary>
        /// Gets the direct base classes and offsets.
        /// </summary>
        /// <returns>The direct base classes and offsets.</returns>
        protected abstract Dictionary<string, Tuple<CodeType, int>> GetDirectBaseClassesAndOffsets();

        /// <summary>
        /// Gets the type all field names (including all base classes).
        /// </summary>
        /// <returns>The type all field names (including all base classes).</returns>
        protected abstract string[] GetTypeAllFieldNames();

        /// <summary>
        /// Gets the type field names.
        /// </summary>
        /// <returns>The type field names.</returns>
        protected abstract string[] GetTypeFieldNames();

        /// <summary>
        /// Gets field type and offset from all fields (including all base classes).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Field type and offset from all fields (including all base classes).</returns>
        protected abstract Tuple<CodeType, int> GetAllFieldTypeAndOffset(string fieldName);

        /// <summary>
        /// Gets the field type and offset.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The field type and offset.</returns>
        protected abstract Tuple<CodeType, int> GetFieldTypeAndOffset(string fieldName);

        /// <summary>
        /// Gets the base class and offset.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>The base class and offset.</returns>
        protected abstract Tuple<CodeType, int> GetBaseClassAndOffset(string className);

        /// <summary>
        /// Gets the template arguments.
        /// </summary>
        /// <returns>The template arguments.</returns>
        protected abstract object[] GetTemplateArguments();

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        /// <returns>The template arguments strings.</returns>
        protected abstract string[] GetTemplateArgumentsStrings();

        /// <summary>
        /// Finds all user types that can be created/converted from this code type.
        /// </summary>
        private Type[] GetUserTypes()
        {
            // Check if it pointer, but element is not pointer
            if (IsPointer && !ElementType.IsPointer)
                return ElementType.UserTypes;

            // If we didn't set user type metadata, then we don't know about any user type, so return empty array.
            if (Context.UserTypeMetadata == null)
                return Array.Empty<Type>();

            // Search Context.UserTypeMetadata for this CodeType
            // TODO: Speed this up with search caches
            List<Type> types = new List<Type>();

            foreach (var metadata in Context.UserTypeMetadata)
            {
                // Check module name
                if (metadata.ModuleName != null && metadata.ModuleName != Module.Name)
                    continue;

                // Check type name
                if (!CodeType.TypeNameMatches(Name, metadata.TypeName))
                    continue;

                // Check using verification function
                if (!metadata.VerifyCodeType(this))
                    continue;

                // Add type to the list
                types.Add(metadata.Type);
            }

            return types.ToArray();
        }
    }

    /// <summary>
    /// Debugging type of native variables
    /// </summary>
    internal class NativeCodeType : CodeType
    {
        /// <summary>
        /// The build-in type.
        /// </summary>
        private BuiltinType builtinType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType"/> class.
        /// </summary>
        /// <remarks>This should not be used directly, but through Module.TypesById[typeId]</remarks>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="tag">The code type tag.</param>
        /// <param name="builtinType">Built-in type.</param>
        internal NativeCodeType(Module module, uint typeId, CodeTypeTag tag, BuiltinType builtinType)
            : base(module)
        {
            TypeId = typeId;
            Tag = tag;
            this.builtinType = builtinType;

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
        /// Gets the code type tag.
        /// </summary>
        internal CodeTypeTag Tag { get; private set; }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => builtinType;

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
                return Tag == CodeTypeTag.Enum;
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
                return Tag == CodeTypeTag.Array;
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
                return Tag == CodeTypeTag.Pointer;
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
                return (IsArray || IsPointer) && ((ElementType.Size == 2 && ElementType.IsSimple) || (((NativeCodeType)ElementType).BuiltinType == BuiltinType.Char32));
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
                return BuiltinType == BuiltinType.Float32 || BuiltinType == BuiltinType.Float64 || BuiltinType == BuiltinType.Float80;
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
                return BuiltinType == BuiltinType.Float32;
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
                return BuiltinType == BuiltinType.Float64;
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
                return Tag == CodeTypeTag.BuiltinType;
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
                return Tag == CodeTypeTag.Function;
            }
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

                    if (!IsArray)
                    {
                        elementType.pointerToType.Value = this;
                    }

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
            if (!Module.IsFakeCodeTypeId(TypeId))
            {
                try
                {
                    uint pointerTypeId = Context.SymbolProvider.GetTypePointerToTypeId(Module, TypeId);

                    if (pointerTypeId != int.MaxValue)
                        return Module.TypesById[pointerTypeId];
                }
                catch
                {
                }
            }
            return CreateFakePointerType(this);
        }

        /// <summary>
        /// Creates fake pointer code type from the specified code type.
        /// </summary>
        /// <param name="originalCodeType">Element code type for generated code type.</param>
        /// <returns>Pointer code type to the specified code type.</returns>
        internal static CodeType CreateFakePointerType(CodeType originalCodeType)
        {
            NativeCodeType codeType = new NativeCodeType(originalCodeType.Module, originalCodeType.Module.GetNextFakeCodeTypeId(), CodeTypeTag.Pointer, BuiltinType.NoType);

            codeType.elementType.Value = originalCodeType;
            codeType.name.Value = originalCodeType.name.Value + "*";
            codeType.size.Value = originalCodeType.Module.Process.GetPointerSize();
            if (!originalCodeType.IsPointer)
            {
                codeType.allFieldNames = originalCodeType.allFieldNames;
                codeType.allFieldTypesAndOffsets = originalCodeType.allFieldTypesAndOffsets;
                codeType.fieldNames = originalCodeType.fieldNames;
                codeType.fieldTypeAndOffsets = originalCodeType.fieldTypeAndOffsets;
                codeType.baseClassesAndOffsets = originalCodeType.baseClassesAndOffsets;
                codeType.directBaseClassesAndOffsets = originalCodeType.directBaseClassesAndOffsets;
                codeType.templateArguments = originalCodeType.templateArguments;
                codeType.templateArgumentsStrings = originalCodeType.templateArgumentsStrings;
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
            if (IsPointer)
                return ElementType.TemplateArguments;

            object[] result = Context.SymbolProvider.GetTemplateArguments(Module, TypeId);

            if (result != null)
                return result;

            string[] arguments = TemplateArgumentsStrings;

            result = new object[arguments.Length];
            for (int i = 0; i < result.Length; i++)
            {
                int intValue;

                if (int.TryParse(arguments[i], out intValue))
                {
                    result[i] = intValue;
                }
                else
                {
                    try
                    {
                        result[i] = CodeType.Create(arguments[i], Module);
                    }
                    catch
                    {
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        protected override string[] GetTemplateArgumentsStrings()
        {
            if (IsPointer)
                return ElementType.TemplateArgumentsStrings;
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

        /// <summary>
        /// Gets the static field from this type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        public override Variable GetStaticField(string staticFieldName)
        {
            return Module.GetVariable($"{Name}::{staticFieldName}");
        }

        /// <summary>
        /// Gets the static field from CLR type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        /// <param name="appDomain">The CLR application domain.</param>
        /// <exception cref="System.NotImplementedException">You cannot get CLR variable from Native code type</exception>
        public override Variable GetClrStaticField(string staticFieldName, CLR.IClrAppDomain appDomain)
        {
            throw new NotImplementedException("You cannot get CLR variable from Native code type");
        }
    }

    /// <summary>
    /// Helper base class for all builtin code type that can be used directly in .NET (sbyte, byte, int, long, etc.).
    /// </summary>
    /// <seealso cref="SharpDebug.CodeType" />
    internal abstract class NativeBuiltinCodeType : CodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeBuiltinCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public NativeBuiltinCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is ANSI string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsAnsiString { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is array.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is array; otherwise, <c>false</c>.
        /// </value>
        public override bool IsArray { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this instance is double.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is double; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDouble { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is enum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is enum; otherwise, <c>false</c>.
        /// </value>
        public override bool IsEnum { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this instance is float.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is float; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFloat { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is function type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is function type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFunction { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is pointer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is pointer; otherwise, <c>false</c>.
        /// </value>
        public override bool IsPointer { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this instance is floating point number (float or double).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is floating point number; otherwise, <c>false</c>.
        /// </value>
        public override bool IsReal { get { return IsFloat || IsDouble; } }

        /// <summary>
        /// Gets a value indicating whether this type is simple type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is simple type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSimple { get { return true; } }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI or wide string.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsString { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is wide string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is wide string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsWideString { get { return false; } }

        /// <summary>
        /// Gets the static field from this type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Variable GetStaticField(string staticFieldName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the static field from CLR type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        /// <param name="appDomain">The CLR application domain.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Variable GetClrStaticField(string staticFieldName, CLR.IClrAppDomain appDomain)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets field type and offset from all fields (including all base classes).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Tuple<CodeType, int> GetAllFieldTypeAndOffset(string fieldName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the base class and offset.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Tuple<CodeType, int> GetBaseClassAndOffset(string className)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the direct base classes and offsets.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Dictionary<string, Tuple<CodeType, int>> GetDirectBaseClassesAndOffsets()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override CodeType GetElementType()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the field type and offset.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Tuple<CodeType, int> GetFieldTypeAndOffset(string fieldName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the pointer to type.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override CodeType GetPointerToType()
        {
            return NativeCodeType.CreateFakePointerType(this);
        }

        /// <summary>
        /// Gets the template arguments.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override object[] GetTemplateArguments()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override string[] GetTemplateArgumentsStrings()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the type all field names (including all base classes).
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override string[] GetTypeAllFieldNames()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the type field names.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override string[] GetTypeFieldNames()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Helper class that represents C# bool type as native code type.
    /// </summary>
    internal class BoolCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoolCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public BoolCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.Bool;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "bool";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 1;
        }
    }

    /// <summary>
    /// Helper class that represents C# byte type as native code type.
    /// </summary>
    internal class ByteCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public ByteCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.UInt8;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "unsigned char";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 1;
        }
    }

    /// <summary>
    /// Helper class that represents C# sbyte type as native code type.
    /// </summary>
    internal class SbyteCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SbyteCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public SbyteCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.Int8;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "char";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 1;
        }
    }

    /// <summary>
    /// Helper class that represents C# short type as native code type.
    /// </summary>
    internal class ShortCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public ShortCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.Int16;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "short";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 2;
        }
    }

    /// <summary>
    /// Helper class that represents C# ushort type as native code type.
    /// </summary>
    internal class UshortCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UshortCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public UshortCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.UInt16;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "unsigned short";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 2;
        }
    }

    /// <summary>
    /// Helper class that represents C# int type as native code type.
    /// </summary>
    internal class IntCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public IntCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.Int32;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "int";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 4;
        }
    }

    /// <summary>
    /// Helper class that represents C# uint type as native code type.
    /// </summary>
    internal class UintCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UintCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public UintCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.UInt32;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "unsigned int";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 4;
        }
    }

    /// <summary>
    /// Helper class that represents C# long type as native code type.
    /// </summary>
    internal class LongCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LongCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public LongCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.Int64;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "long long";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 8;
        }
    }

    /// <summary>
    /// Helper class that represents C# ulong type as native code type.
    /// </summary>
    internal class UlongCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UlongCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public UlongCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.UInt64;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "unsigned long long";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 8;
        }
    }

    /// <summary>
    /// Helper class that represents C# float type as native code type.
    /// </summary>
    internal class FloatCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public FloatCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance is float.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is float; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFloat => true;

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.Float32;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "float";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 4;
        }
    }

    /// <summary>
    /// Helper class that represents C# double type as native code type.
    /// </summary>
    internal class DoubleCodeType : NativeBuiltinCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public DoubleCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance is double.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is double; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDouble => true;

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.Float64;

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "double";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return 8;
        }
    }

    /// <summary>
    /// Helper class that creates built-in code types.
    /// </summary>
    internal static class BuiltinCodeTypes
    {
        /// <summary>
        /// String that represents code type being built by this class.
        /// </summary>
        public const string FakeNameStart = "~BuiltinCodeTypes~";

        /// <summary>
        /// Gets built-in code type from the specified module.
        /// </summary>
        /// <typeparam name="T">Type that should be represented by newly created code type.</typeparam>
        /// <param name="module">The module.</param>
        /// <returns>Built-in code type.</returns>
        public static CodeType GetCodeType<T>(Module module)
        {
            return module.TypesByName[$"{FakeNameStart}{typeof(T).Name}"];
        }

        /// <summary>
        /// Creates built-in code type from the specified .NET type name. It only works for built-in .NET types.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="name">Name of the .NET type.</param>
        /// <returns>Built-in code type.</returns>
        internal static CodeType CreateCodeType(Module module, string name)
        {
            switch (name)
            {
                case "Boolean":
                    return new BoolCodeType(module);
                case "Byte":
                    return new ByteCodeType(module);
                case "SByte":
                    return new SbyteCodeType(module);
                case "Int16":
                    return new ShortCodeType(module);
                case "UInt16":
                    return new UshortCodeType(module);
                case "Int32":
                    return new IntCodeType(module);
                case "UInt32":
                    return new UintCodeType(module);
                case "Int64":
                    return new LongCodeType(module);
                case "UInt64":
                    return new UlongCodeType(module);
                case "Single":
                    return new FloatCodeType(module);
                case "Double":
                    return new DoubleCodeType(module);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Helper type to aid NakedPointer class.
    /// </summary>
    /// <seealso cref="SharpDebug.CodeType" />
    internal class NakedPointerCodeType : CodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NakedPointerCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public NakedPointerCodeType(Module module)
            : base(module)
        {
        }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType => BuiltinType.NoType;

        /// <summary>
        /// Gets a value indicating whether this type is ANSI string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is ANSI string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsAnsiString { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is array.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is array; otherwise, <c>false</c>.
        /// </value>
        public override bool IsArray { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this instance is double.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is double; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDouble { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is enum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is enum; otherwise, <c>false</c>.
        /// </value>
        public override bool IsEnum { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this instance is float.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is float; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFloat { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is function type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is function type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFunction { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is pointer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is pointer; otherwise, <c>false</c>.
        /// </value>
        public override bool IsPointer { get { return true; } }

        /// <summary>
        /// Gets a value indicating whether this instance is floating point number (float or double).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is floating point number; otherwise, <c>false</c>.
        /// </value>
        public override bool IsReal { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is simple type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is simple type; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSimple { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is ANSI or wide string.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsString { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this type is wide string.
        /// </summary>
        /// <value>
        /// <c>true</c> if this type is wide string; otherwise, <c>false</c>.
        /// </value>
        public override bool IsWideString { get { return false; } }

        /// <summary>
        /// Gets the static field from this type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Variable GetStaticField(string staticFieldName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the static field from CLR type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        /// <param name="appDomain">The CLR application domain.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Variable GetClrStaticField(string staticFieldName, CLR.IClrAppDomain appDomain)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets field type and offset from all fields (including all base classes).
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Tuple<CodeType, int> GetAllFieldTypeAndOffset(string fieldName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the base class and offset.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Tuple<CodeType, int> GetBaseClassAndOffset(string className)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the direct base classes and offsets.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Dictionary<string, Tuple<CodeType, int>> GetDirectBaseClassesAndOffsets()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override CodeType GetElementType()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the field type and offset.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override Tuple<CodeType, int> GetFieldTypeAndOffset(string fieldName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the pointer to type.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override CodeType GetPointerToType()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the template arguments.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override object[] GetTemplateArguments()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the template arguments strings.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override string[] GetTemplateArgumentsStrings()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the type all field names (including all base classes).
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override string[] GetTypeAllFieldNames()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the type field names.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override string[] GetTypeFieldNames()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        protected override string GetTypeName()
        {
            return "void*";
        }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return Module.Process.GetPointerSize();
        }
    }

    /// <summary>
    /// Debugging type of CLR variables
    /// </summary>
    internal class ClrCodeType : CodeType
    {
        /// <summary>
        /// The array code type specializations
        /// </summary>
        private DictionaryCache<int, ClrArrayCodeType> arraySpecializations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrCodeType"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="clrType">The CLR type.</param>
        public ClrCodeType(Module module, IClrType clrType)
            : base(module)
        {
            ClrType = clrType;
            arraySpecializations = new DictionaryCache<int, ClrArrayCodeType>((elementsCount) => new ClrArrayCodeType(this, elementsCount));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrCodeType"/> class.
        /// </summary>
        /// <param name="originalClrCodeType">The original CLR code type.</param>
        public ClrCodeType(ClrCodeType originalClrCodeType)
            : base(originalClrCodeType)
        {
            ClrType = originalClrCodeType.ClrType;
            arraySpecializations = originalClrCodeType.arraySpecializations;
        }

        /// <summary>
        /// Gets the specialized code type.
        /// </summary>
        /// <param name="address">The variable address.</param>
        internal ClrCodeType GetSpecializedCodeType(ulong address)
        {
            if (IsArray)
            {
                return arraySpecializations[ClrType.GetArrayLength(address)];
            }

            return this;
        }

        /// <summary>
        /// Gets the CLR type.
        /// </summary>
        internal IClrType ClrType { get; private set; }

        /// <summary>
        /// Gets the built-in type.
        /// </summary>
        public override BuiltinType BuiltinType
        {
            get
            {
                if (ClrType.HasSimpleValue)
                {
                    switch (ClrType.ElementType)
                    {
                        case ClrElementType.Boolean:
                            return BuiltinType.Bool;
                        case ClrElementType.Char:
                            return BuiltinType.Char16;
                        case ClrElementType.Float:
                            return BuiltinType.Float32;
                        case ClrElementType.Double:
                            return BuiltinType.Float64;
                        case ClrElementType.Int8:
                            return BuiltinType.Int8;
                        case ClrElementType.UInt8:
                            return BuiltinType.UInt8;
                        case ClrElementType.Int16:
                            return BuiltinType.Int16;
                        case ClrElementType.UInt16:
                            return BuiltinType.UInt16;
                        case ClrElementType.NativeInt:
                        case ClrElementType.Int32:
                            return BuiltinType.Int32;
                        case ClrElementType.NativeUInt:
                        case ClrElementType.UInt32:
                            return BuiltinType.UInt32;
                        case ClrElementType.Int64:
                            return BuiltinType.Int64;
                        case ClrElementType.UInt64:
                            return BuiltinType.UInt64;
                    }
                }
                return BuiltinType.NoType;
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
                return ClrType.ElementType == ClrElementType.Double;
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
                return ClrType.ElementType == ClrElementType.Float;
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
                return ClrType.ElementType == ClrElementType.FunctionPointer;
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
                return ClrType.IsPointer || ClrType.IsObjectReference;
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
                return ClrType.IsPointer && ClrType.ElementType == ClrElementType.Char;
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
            var field = ClrType.Fields.FirstOrDefault(f => f.Name == fieldName);

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
        private Tuple<CodeType, int> GetFieldTypeAndOffset(IClrInstanceField field)
        {
            return Tuple.Create(Module.FromClrType(field.Type), field.GetOffset(ClrType.IsValueClass));
        }

        /// <summary>
        /// Gets the base class and offset.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        protected override Tuple<CodeType, int> GetBaseClassAndOffset(string className)
        {
            if (Name == className)
            {
                return new Tuple<CodeType, int>(this, 0);
            }
            for (var clrType = ClrType; clrType != null; clrType = clrType.BaseType)
            {
                if (clrType.Name == className)
                {
                    return Tuple.Create(Module.FromClrType(clrType), 0);
                }
            }
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
            {
                baseClassesAndOffsets.Add(baseType.Name, Tuple.Create(Module.FromClrType(baseType), 0));
            }
            return baseClassesAndOffsets;
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        protected override CodeType GetElementType()
        {
            if (ClrType.ComponentType != null)
            {
                return Module.FromClrType(ClrType.ComponentType);
            }
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
            IEnumerable<IClrInstanceField> fields = ClrType.Fields;

            for (var baseType = ClrType.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                fields = fields.Concat(baseType.Fields);
            }
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
            if (ClrType.HasSimpleValue)
            {
                switch (ClrType.ElementType)
                {
                    case ClrElementType.Boolean:
                        return 1;
                    case ClrElementType.Char:
                        return 2;
                    case ClrElementType.String:
                    case ClrElementType.Object:
                    case ClrElementType.Pointer:
                        return Module.Process.GetPointerSize();
                    case ClrElementType.Float:
                        return 4;
                    case ClrElementType.Double:
                        return 8;
                    case ClrElementType.Int8:
                    case ClrElementType.UInt8:
                        return 1;
                    case ClrElementType.Int16:
                    case ClrElementType.UInt16:
                        return 2;
                    case ClrElementType.Int32:
                    case ClrElementType.UInt32:
                    case ClrElementType.NativeInt:
                    case ClrElementType.NativeUInt:
                        return 4;
                    case ClrElementType.Int64:
                    case ClrElementType.UInt64:
                        return 8;
                }
            }

            return (uint)ClrType.BaseSize;
        }

        /// <summary>
        /// Gets the static field from this type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        public override Variable GetStaticField(string staticFieldName)
        {
            return Module.GetVariable($"{Name}.{staticFieldName}");
        }

        /// <summary>
        /// Gets the static field from CLR type.
        /// </summary>
        /// <param name="staticFieldName">Name of the static field.</param>
        /// <param name="appDomain">The CLR application domain.</param>
        public override Variable GetClrStaticField(string staticFieldName, CLR.IClrAppDomain appDomain)
        {
            return Module.GetClrVariable($"{Name}.{staticFieldName}", appDomain);
        }
    }

    /// <summary>
    /// Specialization code type for CLR array
    /// </summary>
    internal class ClrArrayCodeType : ClrCodeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrArrayCodeType"/> class.
        /// </summary>
        /// <param name="arrayCodeType">Type of the array code.</param>
        /// <param name="length">The length.</param>
        internal ClrArrayCodeType(ClrCodeType arrayCodeType, int length)
            : base(arrayCodeType)
        {
            size = SimpleCache.Create(GetTypeSize);
            Length = length;
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        internal int Length { get; private set; }

        /// <summary>
        /// Gets the size of the type.
        /// </summary>
        protected override uint GetTypeSize()
        {
            return (uint)(ClrType.ElementSize * Length);
        }
    }
}
