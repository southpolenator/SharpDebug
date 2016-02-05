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
    public class CodeType
    {
        /// <summary>
        /// The element type
        /// </summary>
        private SimpleCache<CodeType> elementType;

        /// <summary>
        /// The name
        /// </summary>
        private SimpleCache<string> name;

        /// <summary>
        /// The size
        /// </summary>
        private SimpleCache<uint> size;

        /// <summary>
        /// The all field names (including all base classes)
        /// </summary>
        private SimpleCache<string[]> allFieldNames;

        /// <summary>
        /// All field types and offsets
        /// </summary>
        private DictionaryCache<string, Tuple<CodeType, int>> allFieldTypesAndOffsets;

        /// <summary>
        /// The field names
        /// </summary>
        private SimpleCache<string[]> fieldNames;

        /// <summary>
        /// The field type and offsets
        /// </summary>
        private DictionaryCache<string, Tuple<CodeType, int>> fieldTypeAndOffsets;

        /// <summary>
        /// The base classes and offsets
        /// </summary>
        private DictionaryCache<string, Tuple<CodeType, int>> baseClassesAndOffsets;

        /// <summary>
        /// The direct base classes and offsets
        /// </summary>
        private SimpleCache<Dictionary<string, Tuple<CodeType, int>>> directBaseClassesAndOffsets;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType"/> class.
        /// </summary>
        /// <remarks>This should not be used directly, but through Module.TypesById[typeId]</remarks>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="tag">The symbol tag.</param>
        /// <param name="basicType">Type of the basic type.</param>
        internal CodeType(Module module, uint typeId, SymTag tag, Dia2Lib.BasicType basicType)
        {
            Module = module;
            TypeId = typeId;
            Tag = tag;
            BasicType = basicType;
            InitializeCache();
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        private void InitializeCache()
        {
            elementType = SimpleCache.Create(GetElementType);
            name = SimpleCache.Create(() => Context.SymbolProvider.GetTypeName(Module, TypeId));
            size = SimpleCache.Create(() => Context.SymbolProvider.GetTypeSize(Module, TypeId));
            allFieldNames = SimpleCache.Create(() => Context.SymbolProvider.GetTypeAllFieldNames(Module, TypeId));
            allFieldTypesAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>((fieldName) =>
            {
                var field = Context.SymbolProvider.GetTypeAllFieldTypeAndOffset(Module, TypeId, fieldName);

                return Tuple.Create(Module.TypesById[field.Item1], field.Item2);
            });
            fieldNames = SimpleCache.Create(() => Context.SymbolProvider.GetTypeFieldNames(Module, TypeId));
            fieldTypeAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>((fieldName) =>
            {
                var field = Context.SymbolProvider.GetTypeFieldTypeAndOffset(Module, TypeId, fieldName);

                return Tuple.Create(Module.TypesById[field.Item1], field.Item2);
            });
            baseClassesAndOffsets = new DictionaryCache<string, Tuple<CodeType, int>>((className) =>
            {
                var baseClass = Context.SymbolProvider.GetTypeBaseClass(Module, TypeId, className);

                return Tuple.Create(Module.TypesById[baseClass.Item1], baseClass.Item2);
            });
            directBaseClassesAndOffsets = SimpleCache.Create(() =>
            {
                var baseClasses = Context.SymbolProvider.GetTypeDirectBaseClasses(Module, TypeId);
                var result = new Dictionary<string, Tuple<CodeType, int>>();

                foreach (var baseClass in baseClasses)
                {
                    result.Add(baseClass.Key, Tuple.Create(Module.TypesById[baseClass.Value.Item1], baseClass.Value.Item2));
                }

                return result;
            });
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
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        public uint TypeId { get; private set; }

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
        /// Gets a value indicating whether this type is enum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is enum; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnum
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
        public bool IsArray
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
        public bool IsPointer
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
        public bool IsString
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
        public bool IsAnsiString
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
        public bool IsWideString
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
        public bool IsReal
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
        public bool IsFloat
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
        public bool IsDouble
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
        public bool IsSimple
        {
            get
            {
                return Tag == SymTag.BaseType;
            }
        }

        /// <summary>
        /// Gets the symbol tag.
        /// </summary>
        internal SymTag Tag { get; private set; }

        /// <summary>
        /// Gets the type of the basic type.
        /// </summary>
        internal Dia2Lib.BasicType BasicType { get; private set; }

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
        /// Gets the element type.
        /// </summary>
        private CodeType GetElementType()
        {
            if (IsPointer || IsArray)
            {
                try
                {
                    uint elementTypeId = Context.SymbolProvider.GetTypeElementTypeId(Module, TypeId);
                    return Module.TypesById[elementTypeId];
                }
                catch (Exception)
                {
                }
            }

            return this;
        }
    }
}
