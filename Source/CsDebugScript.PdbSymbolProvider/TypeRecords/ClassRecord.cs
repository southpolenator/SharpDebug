using CsDebugScript.PdbSymbolProvider.Utility;
using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents class, structure or interface type record.
    /// </summary>
    public class ClassRecord : TagRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_CLASS, TypeLeafKind.LF_STRUCTURE, TypeLeafKind.LF_INTERFACE
        };

        /// <summary>
        /// Gets the type index of the derivation list. This is output by the compiler as
        /// 0x0000 and is filled in by the CVPACK utility to a LF_DERIVED
        /// record containing the type indices of those classes which immediately
        /// inherit the current class. A zero index indicates that no derivation
        /// information is available. An LF_NULL index indicates that the class
        /// is not inherited by other classes.
        /// </summary>
        public TypeIndex DerivationList { get; private set; }

        /// <summary>
        /// Gets the type index of the virtual function table shape descriptor.
        /// </summary>
        public TypeIndex VirtualTableShape { get; private set; }

        /// <summary>
        /// Gets the size in bytes of the structure.
        /// </summary>
        public ulong Size { get; private set; }

        /// <summary>
        /// Reads <see cref="ClassRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static ClassRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            ClassRecord record = new ClassRecord
            {
                Kind = kind,
                MemberCount = reader.ReadUshort(),
                Options = (ClassOptions)reader.ReadUshort(),
                FieldList = TypeIndex.Read(reader),
                DerivationList = TypeIndex.Read(reader),
                VirtualTableShape = TypeIndex.Read(reader),
                Size = Convert.ToUInt64(reader.ReadEncodedInteger()),
                Name = reader.ReadCString(),
            };

            if (record.HasUniqueName)
                record.UniqueName = reader.ReadCString();
            return record;
        }
    }
}
