namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Base class for all type record read from type info stream.
    /// </summary>
    public class TypeRecord
    {
        /// <summary>
        /// Gets the type record kind.
        /// </summary>
        public TypeLeafKind Kind { get; protected set; }
    }
}
