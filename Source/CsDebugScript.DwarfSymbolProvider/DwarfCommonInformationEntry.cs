using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// Common information entry shared across frame description entries.
    /// </summary>
    internal class DwarfCommonInformationEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DwarfCommonInformationEntry"/> class.
        /// </summary>
        /// <param name="data">The data memory reader.</param>
        /// <param name="defaultAddressSize">Default size of the address.</param>
        /// <param name="endPosition">The end position in the memory stream.</param>
        private DwarfCommonInformationEntry(DwarfMemoryReader data, byte defaultAddressSize, int endPosition)
        {
            ParseData(data, defaultAddressSize, endPosition);
        }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// Gets or sets the augmentation string.
        /// </summary>
        public string Augmentation { get; set; }

        /// <summary>
        /// Gets or sets the size of the address.
        /// </summary>
        public byte AddressSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the segment selector.
        /// </summary>
        public byte SegmentSelectorSize { get; set; }

        /// <summary>
        /// Gets or sets the code alignment factor.
        /// </summary>
        public ulong CodeAlignmentFactor { get; set; }

        /// <summary>
        /// Gets or sets the data alignment factor.
        /// </summary>
        public ulong DataAlignmentFactor { get; set; }

        /// <summary>
        /// Gets or sets the return address register.
        /// </summary>
        public ulong ReturnAddressRegister { get; set; }

        /// <summary>
        /// Gets or sets the initial instructions stream to be executed before instructions in frame description entry.
        /// </summary>
        public byte[] InitialInstructions { get; set; }

        /// <summary>
        /// Gets or sets the list of frame description entries that share this common information entry.
        /// </summary>
        public List<DwarfFrameDescriptionEntry> FrameDescriptionEntries { get; set; } = new List<DwarfFrameDescriptionEntry>();

        /// <summary>
        /// Parses the specified data for all common information entries and frame description entries.
        /// </summary>
        /// <param name="data">The data memory reader.</param>
        /// <param name="defaultAddressSize">Default size of the address.</param>
        /// <returns>All the parsed common information entries</returns>
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

        /// <summary>
        /// Parses the single entry from the specified data.
        /// </summary>
        /// <param name="data">The data memory reader.</param>
        /// <param name="defaultAddressSize">Default size of the address.</param>
        /// <param name="startPosition">The start position.</param>
        /// <returns>Parsed common information entry.</returns>
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

        /// <summary>
        /// Parses the data for this instance.
        /// </summary>
        /// <param name="data">The data memory reader.</param>
        /// <param name="defaultAddressSize">Default size of the address.</param>
        /// <param name="endPosition">The end position.</param>
        private void ParseData(DwarfMemoryReader data, byte defaultAddressSize, int endPosition)
        {
            Version = data.ReadByte();
            Augmentation = data.ReadString();
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
