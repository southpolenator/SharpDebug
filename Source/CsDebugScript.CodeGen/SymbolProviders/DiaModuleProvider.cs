namespace CsDebugScript.CodeGen.SymbolProviders
{
    internal class DiaModuleProvider : IModuleProvider
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
