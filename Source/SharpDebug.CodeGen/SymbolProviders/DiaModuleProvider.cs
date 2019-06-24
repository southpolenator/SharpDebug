namespace SharpDebug.CodeGen.SymbolProviders
{
    /// <summary>
    /// Implementation of <see cref="IModuleProvider"/> that uses DIA for opening modules.
    /// </summary>
    /// <seealso cref="SharpDebug.CodeGen.SymbolProviders.IModuleProvider" />
    public class DiaModuleProvider : IModuleProvider
    {
        /// <summary>
        /// Opens the module for the specified XML module description.
        /// </summary>
        /// <param name="module">The XML module description.</param>
        public Module Open(XmlModule module)
        {
            return DiaModule.Open(module);
        }
    }
}
