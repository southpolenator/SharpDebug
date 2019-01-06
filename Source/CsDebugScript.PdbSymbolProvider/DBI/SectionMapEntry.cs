using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// DBI stream section map entry.
    /// </summary>
    public struct SectionMapEntry
    {
        /// <summary>
        /// Gets the description flags.
        /// </summary>
        public OMFSegmentDescriptionFlags Flags { get; private set; }

        /// <summary>
        /// Logical overlay number.
        /// </summary>
        public ushort Overlay { get; private set; }

        /// <summary>
        /// Group index into descriptor array.
        /// </summary>
        public ushort GroupIndex { get; private set; }

        /// <summary>
        /// Frame number
        /// </summary>
        public ushort Frame { get; private set; }

        /// <summary>
        /// Byte index of the segment or group name in the sstSegName table, or 0xFFFF.
        /// </summary>
        public ushort SectionNameIndex { get; private set; }

        /// <summary>
        /// Byte index of the class name in the sstSegName table, or 0xFFFF.
        /// </summary>
        public ushort ClassNameIndex { get; private set; }

        /// <summary>
        /// Byte offset of the logical segment within the specified physical segment.
        /// If group is set in flags, offset is the offset of the group.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Byte count of the segment or group.
        /// </summary>
        public uint Size { get; private set; }

        /// <summary>
        /// Reads <see cref="SectionMapEntry"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static SectionMapEntry Read(IBinaryReader reader)
        {
            return new SectionMapEntry
            {
                Flags = (OMFSegmentDescriptionFlags)reader.ReadUshort(),
                Overlay = reader.ReadUshort(),
                GroupIndex = reader.ReadUshort(),
                Frame = reader.ReadUshort(),
                SectionNameIndex = reader.ReadUshort(),
                ClassNameIndex = reader.ReadUshort(),
                Offset = reader.ReadUint(),
                Size = reader.ReadUint(),
            };
        }
    }
}
