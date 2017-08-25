namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Interface for opening modules
    /// </summary>
    public interface IModuleProvider
    {
        /// <summary>
        /// Opens the module for the specified XML module description.
        /// </summary>
        /// <param name="module">The XML module description.</param>
        Module Open(XmlModule module);
    }
}
