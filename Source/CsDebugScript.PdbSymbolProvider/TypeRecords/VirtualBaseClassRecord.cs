using CsDebugScript.PdbSymbolProvider.Utility;
using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies directly inherited virtual base class. If a class directly inherits virtual base
    /// classes, the corresponding Direct Virtual BaseClass records will follow all Real Base Class
    /// member records and precede all other member records in the field list of that class. Direct
    /// Virtual Base class records are emitted in bottommost left-to-right inheritance order for directly
    /// inherited virtual bases.
    /// </summary>
    public class VirtualBaseClassRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_VBCLASS, TypeLeafKind.LF_IVBCLASS
        };

        /// <summary>
        /// Gets the member attributes.
        /// </summary>
        public MemberAttributes Attributes { get; private set; }

        /// <summary>
        /// Gets the index to type record of the direct or indirect virtual base class. The
        /// class name can be obtained from this record.
        /// </summary>
        public TypeIndex BaseType { get; private set; }

        /// <summary>
        /// Gets the type index of the virtual base pointer for this base.
        /// </summary>
        public TypeIndex VirtualBasePointerType { get; private set; }

        /// <summary>
        /// Gets the offset of the virtual base pointer from the
        /// address point of the class for this virtual base.
        /// </summary>
        public ulong VirtualBasePointerOffset { get; private set; }

        /// <summary>
        /// Gets the index into the virtual base displacement
        /// table of the entry that contains the displacement of the virtual base.
        /// The displacement is relative to the address point of the class plus <see cref="VirtualBasePointerOffset"/>.
        /// </summary>
        public ulong VirtualTableIndex { get; private set; }

        /// <summary>
        /// Reads <see cref="VirtualBaseClassRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static VirtualBaseClassRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new VirtualBaseClassRecord
            {
                Kind = kind,
                Attributes = MemberAttributes.Read(reader),
                BaseType = TypeIndex.Read(reader),
                VirtualBasePointerType = TypeIndex.Read(reader),
                VirtualBasePointerOffset = Convert.ToUInt64(reader.ReadEncodedInteger()),
                VirtualTableIndex = Convert.ToUInt64(reader.ReadEncodedInteger()),
            };
        }
    }
}
