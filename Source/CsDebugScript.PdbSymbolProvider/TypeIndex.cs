using CsDebugScript.PdbSymbolProvider.Utility;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// It can represent simple built-in type or index in type info array.
    /// </summary>
    public struct TypeIndex
    {
        /// <summary>
        /// Size of <see cref="TypeIndex"/> structure in bytes.
        /// </summary>
        public const uint Size = 4;

        /// <summary>
        /// First non simple type index value.
        /// </summary>
        public const uint FirstNonSimpleIndex = 0x1000;

        /// <summary>
        /// Mask for extracting <see cref="SimpleTypeKind"/> out of <see cref="Index"/>.
        /// </summary>
        public const uint SimpleKindMask = 0x000000ff;

        /// <summary>
        /// Mask for extracting <see cref="SimpleTypeMode"/> out of <see cref="Index"/>.
        /// </summary>
        public const uint SimpleModeMask = 0x00000700;

        #region Predefined common built-in types
        /// <summary>
        /// Uncharacterized type (no type)
        /// </summary>
        public static readonly TypeIndex None = new TypeIndex(SimpleTypeKind.None);

        /// <summary>
        /// Void type.
        /// </summary>
        public static readonly TypeIndex Void = new TypeIndex(SimpleTypeKind.Void);

        /// <summary>
        /// 32-bit pointer to void type.
        /// </summary>
        public static readonly TypeIndex VoidPointer32 = new TypeIndex(SimpleTypeKind.Void, SimpleTypeMode.NearPointer32);

        /// <summary>
        /// 64-bit pointer to void type.
        /// </summary>
        public static readonly TypeIndex VoidPointer64 = new TypeIndex(SimpleTypeKind.Void, SimpleTypeMode.NearPointer64);

        /// <summary>
        /// 8 bit signed.
        /// </summary>
        public static readonly TypeIndex SignedCharacter = new TypeIndex(SimpleTypeKind.SignedCharacter);

        /// <summary>
        /// 8 bit unsigned.
        /// </summary>
        public static readonly TypeIndex UnsignedCharacter = new TypeIndex(SimpleTypeKind.UnsignedCharacter);

        /// <summary>
        /// Really a <c>char</c>.
        /// </summary>
        public static readonly TypeIndex NarrowCharacter = new TypeIndex(SimpleTypeKind.NarrowCharacter);

        /// <summary>
        /// Wide char (<c>wchar_t</c>).
        /// </summary>
        public static readonly TypeIndex WideCharacter = new TypeIndex(SimpleTypeKind.WideCharacter);

        /// <summary>
        /// 16 bit signed
        /// </summary>
        public static readonly TypeIndex Int16Short = new TypeIndex(SimpleTypeKind.Int16Short);

        /// <summary>
        /// 16 bit unsigned
        /// </summary>
        public static readonly TypeIndex UInt16Short = new TypeIndex(SimpleTypeKind.UInt16Short);

        /// <summary>
        /// 32 bit signed int
        /// </summary>
        public static readonly TypeIndex Int32 = new TypeIndex(SimpleTypeKind.Int32);

        /// <summary>
        /// 32 bit unsigned int
        /// </summary>
        public static readonly TypeIndex UInt32 = new TypeIndex(SimpleTypeKind.UInt32);

        /// <summary>
        /// 32 bit signed
        /// </summary>
        public static readonly TypeIndex Int32Long = new TypeIndex(SimpleTypeKind.Int32Long);

        /// <summary>
        /// 32 bit unsigned
        /// </summary>
        public static readonly TypeIndex UInt32Long = new TypeIndex(SimpleTypeKind.UInt32Long);

        /// <summary>
        /// 64 bit signed int
        /// </summary>
        public static readonly TypeIndex Int64 = new TypeIndex(SimpleTypeKind.Int64);

        /// <summary>
        /// 64 bit unsigned int
        /// </summary>
        public static readonly TypeIndex UInt64 = new TypeIndex(SimpleTypeKind.UInt64);

        /// <summary>
        /// 64 bit signed
        /// </summary>
        public static readonly TypeIndex Int64Quad = new TypeIndex(SimpleTypeKind.Int64Quad);

        /// <summary>
        /// 64 bit unsigned
        /// </summary>
        public static readonly TypeIndex UInt64Quad = new TypeIndex(SimpleTypeKind.UInt64Quad);

        /// <summary>
        /// 32 bit real
        /// </summary>
        public static readonly TypeIndex Float32 = new TypeIndex(SimpleTypeKind.Float32);

        /// <summary>
        /// 32 bit PP real
        /// </summary>
        public static readonly TypeIndex Float64 = new TypeIndex(SimpleTypeKind.Float64);
        #endregion

        /// <summary>
        /// Predefined dictionary of built-in type names.
        /// </summary>
        private static readonly Dictionary<SimpleTypeKind, string> SimpleTypeNames = new Dictionary<SimpleTypeKind, string>()
        {
            { SimpleTypeKind.Void, "void" },
            { SimpleTypeKind.NotTranslated, "<not translated>" },
            { SimpleTypeKind.HResult, "HRESULT" },
            { SimpleTypeKind.SignedCharacter, "signed char" },
            { SimpleTypeKind.UnsignedCharacter, "unsigned char" },
            { SimpleTypeKind.NarrowCharacter, "char" },
            { SimpleTypeKind.WideCharacter, "wchar_t" },
            { SimpleTypeKind.Character16, "char16_t" },
            { SimpleTypeKind.Character32, "char32_t" },
            { SimpleTypeKind.SByte, "__int8" },
            { SimpleTypeKind.Byte, "unsigned __int8" },
            { SimpleTypeKind.Int16Short, "short" },
            { SimpleTypeKind.UInt16Short, "unsigned short" },
            { SimpleTypeKind.Int16, "__int16" },
            { SimpleTypeKind.UInt16, "unsigned __int16" },
            { SimpleTypeKind.Int32Long, "long" },
            { SimpleTypeKind.UInt32Long, "unsigned long" },
            { SimpleTypeKind.Int32, "int" },
            { SimpleTypeKind.UInt32, "unsigned" },
            { SimpleTypeKind.Int64Quad, "__int64" },
            { SimpleTypeKind.UInt64Quad, "unsigned __int64" },
            { SimpleTypeKind.Int64, "__int64" },
            { SimpleTypeKind.UInt64, "unsigned __int64" },
            { SimpleTypeKind.Int128, "__int128" },
            { SimpleTypeKind.UInt128, "unsigned __int128" },
            { SimpleTypeKind.Float16, "__half" },
            { SimpleTypeKind.Float32, "float" },
            { SimpleTypeKind.Float32PartialPrecision, "float" },
            { SimpleTypeKind.Float48, "__float48" },
            { SimpleTypeKind.Float64, "double" },
            { SimpleTypeKind.Float80, "long double" },
            { SimpleTypeKind.Float128, "__float128" },
            { SimpleTypeKind.Complex32, "_Complex float" },
            { SimpleTypeKind.Complex64, "_Complex double" },
            { SimpleTypeKind.Complex80, "_Complex long double" },
            { SimpleTypeKind.Complex128, "_Complex __float128" },
            { SimpleTypeKind.Boolean8, "bool" },
            { SimpleTypeKind.Boolean16, "__bool16" },
            { SimpleTypeKind.Boolean32, "__bool32" },
            { SimpleTypeKind.Boolean64, "__bool64" },
        };

        /// <summary>
        /// Predefined dictionary of built-in pointer type names.
        /// </summary>
        private static Dictionary<SimpleTypeKind, string> SimpleTypeNamesPointer;

        /// <summary>
        /// Initializes the <see cref="TypeIndex"/> class.
        /// </summary>
        static TypeIndex()
        {
            SimpleTypeNamesPointer = new Dictionary<SimpleTypeKind, string>();
            foreach (var kvp in SimpleTypeNames)
                SimpleTypeNamesPointer.Add(kvp.Key, kvp.Value + "*");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeIndex"/> class.
        /// </summary>
        /// <param name="index">Index value.</param>
        public TypeIndex(uint index)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeIndex"/> class.
        /// </summary>
        /// <param name="kind">Type of the simple built-in type.</param>
        public TypeIndex(SimpleTypeKind kind)
            : this((uint)kind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeIndex"/> class.
        /// </summary>
        /// <param name="kind">Type of the simple built-in type.</param>
        /// <param name="mode">Pointer type of the simple built-in type.</param>
        public TypeIndex(SimpleTypeKind kind, SimpleTypeMode mode)
            : this((uint)kind | (uint)mode)
        {
        }

        /// <summary>
        /// Gets list of all known built-in types (types that we know how to return name).
        /// </summary>
        public static TypeIndex[] BuiltinTypes
        {
            get
            {
                TypeIndex[] builtinTypes = new TypeIndex[SimpleTypeNames.Count];
                int index = 0;

                foreach (SimpleTypeKind kind in SimpleTypeNames.Keys)
                    builtinTypes[index++] = new TypeIndex(kind);
                return builtinTypes;
            }
        }

        /// <summary>
        /// Gets the index value.
        /// </summary>
        public uint Index { get; private set; }

        /// <summary>
        /// Gets the flag if this instance is simple built-in type.
        /// </summary>
        public bool IsSimple => Index < FirstNonSimpleIndex;

        /// <summary>
        /// Checks wheter this instance is same as <see cref="None"/>.
        /// </summary>
        public bool IsNoneType => this == None;

        /// <summary>
        /// Gets array index. It is invalid value if <see cref="Index"/> points to simple built-in type (<see cref="IsSimple"/>).
        /// </summary>
        public uint ArrayIndex => Index - FirstNonSimpleIndex;

        /// <summary>
        /// Gets the type of the simple built-in type. It is valid only if it is simple built-in type (<see cref="IsSimple"/>).
        /// </summary>
        public SimpleTypeKind SimpleKind => (SimpleTypeKind)(Index & SimpleKindMask);

        /// <summary>
        /// Gets the pointer type of the simple built-in type. It is valid only if it is simple built-in type (<see cref="IsSimple"/>).
        /// </summary>
        public SimpleTypeMode SimpleMode => (SimpleTypeMode)(Index & SimpleModeMask);

        /// <summary>
        /// Gets the name of the simple built-in type. It is valid only if it is simple built-in type (<see cref="IsSimple"/>).
        /// </summary>
        public string SimpleTypeName
        {
            get
            {
                if (IsNoneType)
                    return "<no type>";

                // This is a simple type.
                string name;
                Dictionary<SimpleTypeKind, string> typeNames = SimpleMode == SimpleTypeMode.Direct ? SimpleTypeNames : SimpleTypeNamesPointer;

                if (typeNames.TryGetValue(SimpleKind, out name))
                    return name;

                // We don't know type name
                return "<unknown simple type>";
            }
        }

        /// <summary>
        /// Compares two type indexes for equality.
        /// </summary>
        /// <param name="index1">The first type index.</param>
        /// <param name="index2">The second type index.</param>
        /// <returns><c>true</c> if type indexes are equal.</returns>
        public static bool operator ==(TypeIndex index1, TypeIndex index2)
        {
            return index1.Index == index2.Index;
        }

        /// <summary>
        /// Compares two type indexes for inequality.
        /// </summary>
        /// <param name="index1">The first type index.</param>
        /// <param name="index2">The second type index.</param>
        /// <returns><c>true</c> if type indexes are not equal.</returns>
        public static bool operator !=(TypeIndex index1, TypeIndex index2)
        {
            return index1.Index != index2.Index;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(TypeIndex))
                return false;

            TypeIndex ti = (TypeIndex)obj;

            return Index == ti.Index;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return (int)Index;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsNoneType || IsSimple)
                return $"{SimpleTypeName} ({SimpleKind} | {SimpleMode})";
            return $"{ArrayIndex} (0x{Index:X})";
        }

        /// <summary>
        /// Creates new <see cref="TypeIndex"/> instance that represents array index.
        /// </summary>
        /// <param name="arrayIndex">The array index.</param>
        public static TypeIndex FromArrayIndex(int arrayIndex)
        {
            return new TypeIndex(FirstNonSimpleIndex + (uint)arrayIndex);
        }

        /// <summary>
        /// Reads <see cref="TypeIndex"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static TypeIndex Read(IBinaryReader reader)
        {
            return new TypeIndex(reader.ReadUint());
        }
    }
}
