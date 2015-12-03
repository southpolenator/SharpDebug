using CsScriptManaged;
using DbgEngManaged;
using System;
using System.Text;

namespace CsScripts
{
    /// <summary>
    /// Debugging type of variables
    /// </summary>
    public class DType
    {
        /// <summary>
        /// The typed data
        /// </summary>
        private DEBUG_TYPED_DATA typedData;

        /// <summary>
        /// The base type
        /// </summary>
        private SimpleCache<DType> baseType;

        /// <summary>
        /// The element type
        /// </summary>
        private SimpleCache<DType> elementType;

        /// <summary>
        /// The name
        /// </summary>
        private SimpleCache<string> name;

        /// <summary>
        /// The size
        /// </summary>
        private SimpleCache<uint> size;

        /// <summary>
        /// Initializes a new instance of the <see cref="DType"/> class.
        /// </summary>
        /// <param name="typedData">The typed data.</param>
        internal DType(DEBUG_TYPED_DATA typedData)
        {
            this.typedData = typedData;
            InitializeCache();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DType"/> class.
        /// </summary>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="tag">The tag.</param>
        internal DType(ulong moduleId, uint typeId, ulong offset = 0, SymTag tag = SymTag.Null)
        {
            try
            {
                typedData = GlobalCache.TypedData[Tuple.Create(moduleId, typeId, offset)];
            }
            catch (Exception)
            {
                typedData.ModBase = moduleId;
                typedData.TypeId = typeId;
                typedData.Offset = offset;
                typedData.Tag = tag;
            }

            InitializeCache();
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        private void InitializeCache()
        {
            baseType = SimpleCache.Create(GetBaseType);
            elementType = SimpleCache.Create(GetElementType);
            name = SimpleCache.Create(GetName);
            size = SimpleCache.Create(() => Context.Symbols.GetTypeSize(ModuleId, TypeId));
        }

        /// <summary>
        /// Gets the module identifier.
        /// </summary>
        public ulong ModuleId
        {
            get
            {
                return typedData.ModBase;
            }
        }

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        public uint TypeId
        {
            get
            {
                return typedData.TypeId;
            }
        }

        /// <summary>
        /// Gets the base type.
        /// </summary>
        public DType BaseType
        {
            get
            {
                return baseType.Value;
            }
        }

        /// <summary>
        /// Gets the type of the element if type is array or pointer.
        /// </summary>
        public DType ElementType
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
        internal SymTag Tag
        {
            get
            {
                return typedData.Tag;
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
        /// Gets the base type.
        /// </summary>
        private DType GetBaseType()
        {
            if (typedData.BaseTypeId < Constants.MaxBaseTypeId && typedData.BaseTypeId != TypeId)
            {
                return new DType(ModuleId, typedData.BaseTypeId, typedData.Offset);
            }

            return this;
        }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        private DType GetElementType()
        {
            if (IsPointer || IsArray)
            {
                try
                {
                    return new DType(Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                    {
                        Operation = ExtTdop.GetDereference,
                        InData = typedData,
                    }).OutData);
                }
                catch (Exception)
                {
                }
            }

            return this;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        private string GetName()
        {
            uint nameSize;
            StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

            Context.Symbols.GetTypeName(ModuleId, TypeId, sb, (uint)sb.Capacity, out nameSize);
            return sb.ToString();
        }
    }
}
