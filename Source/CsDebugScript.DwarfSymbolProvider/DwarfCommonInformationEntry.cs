using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.DwarfSymbolProvider
{
    internal class DwarfCommonInformationEntry
    {
        private DwarfCommonInformationEntry(DwarfMemoryReader data, byte defaultAddressSize, int endPosition)
        {
            ParseData(data, defaultAddressSize, endPosition);
        }

        public byte Version { get; set; }
        public string Augmentation { get; set; }
        public byte AddressSize { get; set; }
        public byte SegmentSelectorSize { get; set; }
        public ulong CodeAlignmentFactor { get; set; }
        public ulong DataAlignmentFactor { get; set; }
        public ulong ReturnAddressRegister { get; set; }
        public byte[] InitialInstructions { get; set; }
        public List<DwarfFrameDescriptionEntry> FrameDescriptionEntries { get; set; } = new List<DwarfFrameDescriptionEntry>();

        public static DwarfCommonInformationEntry[] ParseAll(DwarfMemoryReader data, byte defaultAddressSize)
        {
            Dictionary<int, DwarfCommonInformationEntry> entries = new Dictionary<int, DwarfCommonInformationEntry>();

            while (!data.IsEnd)
            {
                bool is64bit;
                int startPosition = data.Position;
                ulong length = data.ReadLength(out is64bit);
                int endPosition = data.Position + (int)length;
                int offset = data.ReadOffset(is64bit);

                if (offset == -1)
                {
                    DwarfCommonInformationEntry entry = new DwarfCommonInformationEntry(data, defaultAddressSize, endPosition);

                    entries.Add(startPosition, entry);
                }
                else
                {
                    DwarfCommonInformationEntry entry;

                    if (!entries.TryGetValue(offset, out entry))
                    {
                        entry = ParseEntry(data, defaultAddressSize, offset);
                        entries.Add(offset, entry);
                    }

                    DwarfFrameDescriptionEntry description = new DwarfFrameDescriptionEntry(data, entry, endPosition);

                    entry.FrameDescriptionEntries.Add(description);
                }
            }

            return entries.Values.ToArray();
        }

        private static DwarfCommonInformationEntry ParseEntry(DwarfMemoryReader data, byte defaultAddressSize, int startPosition)
        {
            int position = data.Position;

            data.Position = startPosition;

            bool is64bit;
            ulong length = data.ReadLength(out is64bit);
            int endPosition = data.Position + (int)length;
            int offset = data.ReadOffset(is64bit);

            if (offset != -1)
            {
                throw new Exception("Expected CommonInformationEntry");
            }

            DwarfCommonInformationEntry entry = new DwarfCommonInformationEntry(data, defaultAddressSize, endPosition);

            data.Position = position;
            return entry;
        }

        private void ParseData(DwarfMemoryReader data, byte defaultAddressSize, int endPosition)
        {
            Version = data.ReadByte();
            Augmentation = data.ReadAnsiString();
            if (!string.IsNullOrEmpty(Augmentation))
            {
                AddressSize = 4;
                SegmentSelectorSize = 0;
                CodeAlignmentFactor = 0;
                DataAlignmentFactor = 0;
                ReturnAddressRegister = 0;
            }
            else
            {
                if (Version >= 4)
                {
                    AddressSize = data.ReadByte();
                    SegmentSelectorSize = data.ReadByte();
                }
                else
                {
                    AddressSize = defaultAddressSize;
                    SegmentSelectorSize = 0;
                }
                CodeAlignmentFactor = data.LEB128();
                DataAlignmentFactor = data.LEB128();
                ReturnAddressRegister = data.LEB128();
            }
            InitialInstructions = data.ReadBlock((uint)(endPosition - data.Position));
        }
    }
}
