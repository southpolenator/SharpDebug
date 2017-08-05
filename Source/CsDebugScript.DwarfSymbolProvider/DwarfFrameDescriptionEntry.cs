namespace CsDebugScript.DwarfSymbolProvider
{
    internal class DwarfFrameDescriptionEntry
    {
        public DwarfFrameDescriptionEntry(DwarfMemoryReader data, DwarfCommonInformationEntry commonInformationEntry, int endPosition)
        {
            CommonInformationEntry = commonInformationEntry;
            ParseData(data, endPosition);
        }

        public ulong InitialLocation { get; set; }

        public ulong AddressRange { get; set; }

        public byte[] Instructions { get; set; }

        public DwarfCommonInformationEntry CommonInformationEntry { get; set; }

        private void ParseData(DwarfMemoryReader data, int endPosition)
        {
            InitialLocation = data.ReadUlong(CommonInformationEntry.AddressSize);
            AddressRange = data.ReadUlong(CommonInformationEntry.AddressSize);
            Instructions = data.ReadBlock((uint)(endPosition - data.Position));
        }
    }
}
