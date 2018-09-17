namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents CV_LABEL_TYPE_e.
    /// </summary>
    public enum LabelType : ushort
    {
        /// <summary>
        /// Near label.
        /// </summary>
        Near = 0x0,

        /// <summary>
        /// Far label.
        /// </summary>
        Far = 0x4,
    }
}
