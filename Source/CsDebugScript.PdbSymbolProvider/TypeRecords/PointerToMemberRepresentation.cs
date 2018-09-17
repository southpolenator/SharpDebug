namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents CV_pmtype_e.
    /// </summary>
    public enum PointerToMemberRepresentation : ushort
    {
        /// <summary>
        /// not specified (pre VC8)
        /// </summary>
        Unknown = 0x00,

        /// <summary>
        /// member data, single inheritance
        /// </summary>
        SingleInheritanceData = 0x01,

        /// <summary>
        /// member data, multiple inheritance
        /// </summary>
        MultipleInheritanceData = 0x02,

        /// <summary>
        /// member data, virtual inheritance
        /// </summary>
        VirtualInheritanceData = 0x03,

        /// <summary>
        /// member data, most general
        /// </summary>
        GeneralData = 0x04,

        /// <summary>
        /// member function, single inheritance
        /// </summary>
        SingleInheritanceFunction = 0x05,

        /// <summary>
        /// member function, multiple inheritance
        /// </summary>
        MultipleInheritanceFunction = 0x06,

        /// <summary>
        /// member function, virtual inheritance
        /// </summary>
        VirtualInheritanceFunction = 0x07,

        /// <summary>
        /// member function, most general
        /// </summary>
        GeneralFunction = 0x08,
    }
}
