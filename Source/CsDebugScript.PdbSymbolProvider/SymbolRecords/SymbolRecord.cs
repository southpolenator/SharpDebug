namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Base class for all symbol record classes.
    /// </summary>
    public class SymbolRecord
    {
        /// <summary>
        /// Type of the symbol record.
        /// </summary>
        public SymbolRecordKind Kind { get; protected set; }
    }
}
