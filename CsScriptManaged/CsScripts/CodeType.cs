using CsScriptManaged;
using DbgEngManaged;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// The field names
        /// </summary>
        private SimpleCache<string[]> fieldNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType"/> class.
        /// </summary>
        /// <remarks>This should not be used directly, but through Module.TypesById[typeId]</remarks>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        internal CodeType(Module module, uint typeId, SymTag tag)
        {
            Module = module;
            TypeId = typeId;
            Tag = tag;
            InitializeCache();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType"/> class.
        /// </summary>
        /// <param name="typedData">The typed data.</param>
        internal CodeType(DEBUG_TYPED_DATA typedData)
        {
            // TODO: Deprecate this constructor
            Module = Process.Current.ModulesById[typedData.ModBase];
            TypeId = typedData.TypeId;
            Tag = typedData.Tag;
            InitializeCache();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType"/> class.
        /// </summary>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="tag">The tag.</param>
        internal CodeType(ulong moduleId, uint typeId, SymTag tag = SymTag.Null)
        {
            // TODO: Deprecate this constructor
            Module = Process.Current.ModulesById[moduleId];
            TypeId = typeId;
            Tag = tag;
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
            fieldNames = SimpleCache.Create(() => Context.SymbolProvider.GetTypeFieldNames(Module, TypeId));
        }

        /// <summary>
        /// Gets the type field names.
        /// </summary>
        public string[] FieldNames
        {
            get
            {
                return fieldNames.Value;
            }
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// Gets the module identifier.
        /// </summary>
        public ulong ModuleId
        {
            get
            {
                return Module.Id;
            }
        }

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
        /// Gets the tag.
        /// </summary>
        internal SymTag Tag { get; private set; }

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
