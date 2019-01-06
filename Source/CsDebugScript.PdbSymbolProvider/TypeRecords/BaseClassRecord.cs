using CsDebugScript.PdbSymbolProvider.Utility;
using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies a real base class. If a class inherits real base classes, the corresponding Real
    /// Base Class records will precede all other member records in the field list of that class. Base
    /// class records are emitted in left-to-right declaration order for real bases.
    /// </summary>
    public class BaseClassRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_BCLASS, TypeLeafKind.LF_BINTERFACE
        };

        /// <summary>
        /// Gets the member attributes.
        /// </summary>
        public MemberAttributes Attributes { get; private set; }

        /// <summary>
        /// Gets the index to type record of the class. The class name can be obtained from this record.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the offset of subobject that represents the base class within the structure.
        /// </summary>
        public ulong Offset { get; private set; }

        /// <summary>
        /// Reads <see cref="BaseClassRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static BaseClassRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new BaseClassRecord
            {
                Kind = kind,
                Attributes = MemberAttributes.Read(reader),
                Type = TypeIndex.Read(reader),
                Offset = Convert.ToUInt64(reader.ReadEncodedInteger()),
            };
        }
    }
}
