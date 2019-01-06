namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// These values correspond to the THUNK_ORDINAL enumeration.
    /// </summary>
    public enum ThunkOrdinal : byte
    {
        /// <summary>
        /// THUNK_ORDINAL_NOTYPE - Standard thunk.
        /// </summary>
        Standard,

        /// <summary>
        /// THUNK_ORDINAL_ADJUSTOR - A <c>this</c> adjustor thunk.
        /// </summary>
        ThisAdjustor,

        /// <summary>
        /// THUNK_ORDINAL_VCALL - Virtual call thunk.
        /// </summary>
        VirtualCall,

        /// <summary>
        /// THUNK_ORDINAL_PCODE - P-code thunk.
        /// </summary>
        PCode,

        /// <summary>
        /// THUNK_ORDINAL_LOAD - Delay load thunk.
        /// </summary>
        DelayLoad,

        /// <summary>
        /// THUNK_ORDINAL_TRAMP_INCREMENTAL - Incremental trampoline thunk (a trampoline thunk is used to bounce calls from one memory space to another).
        /// </summary>
        TrampolineIncremental,

        /// <summary>
        /// THUNK_ORDINAL_TRAMP_BRANCHISLAND - Branch point trampoline thunk.
        /// </summary>
        TrampolineBranchIsland,
    }
}
