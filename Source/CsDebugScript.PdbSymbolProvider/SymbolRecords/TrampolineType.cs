namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Trampoline type used in <see cref="TrampolineSymbol"/>.
    /// </summary>
    public enum TrampolineType : ushort
    {
        /// <summary>
        /// Incremental trampoline thunk (a trampoline thunk is used to bounce calls from one memory space to another).
        /// </summary>
        Incremental,

        /// <summary>
        /// Branch point trampoline thunk.
        /// </summary>
        BranchIsland,
    }
}
