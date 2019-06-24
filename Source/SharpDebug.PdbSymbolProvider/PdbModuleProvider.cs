using SharpDebug.CodeGen;
using SharpDebug.CodeGen.SymbolProviders;

namespace SharpDebug.PdbSymbolProvider
{
    using Module = SharpDebug.CodeGen.SymbolProviders.Module;

    /// <summary>
    /// CodeGen module provider based on PDB reader.
    /// </summary>
    public class PdbModuleProvider : IModuleProvider
    {
        /// <summary>
        /// Opens the module for the specified XML module description.
        /// </summary>
        /// <param name="module">The XML module description.</param>
        public Module Open(XmlModule module)
        {
            return new PdbModule(module);
        }
    }
}
