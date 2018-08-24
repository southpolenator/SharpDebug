namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents CV_ptrmode_e.
    /// </summary>
    public enum PointerMode : byte
    {
        /// <summary>
        /// "normal" pointer
        /// </summary>
        Pointer = 0x00,

        /// <summary>
        /// "old" reference
        /// </summary>
        LValueReference = 0x01,

        /// <summary>
        /// pointer to data member
        /// </summary>
        PointerToDataMember = 0x02,

        /// <summary>
        /// pointer to member function
        /// </summary>
        PointerToMemberFunction = 0x03,

        /// <summary>
        /// r-value reference
        /// </summary>
        RValueReference = 0x04,
    }
}
