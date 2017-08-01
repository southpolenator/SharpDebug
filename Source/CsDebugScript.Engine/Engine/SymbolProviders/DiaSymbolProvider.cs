using Dia2Lib;
using System.IO;

namespace CsDebugScript.Engine.SymbolProviders
{
    /// <summary>
    /// Symbol provider that is being implemented over DIA library.
    /// </summary>
    internal class DiaSymbolProvider : PerModuleSymbolProvider
    {
        /// <summary>
        /// Loads symbol provider module from the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>Interface for symbol provider module</returns>
        public override ISymbolProviderModule LoadModule(Module module)
        {
            // Try to get debugger DIA session
            IDiaSession diaSession = Context.Debugger.GetModuleDiaSession(module);

            if (diaSession != null)
            {
                return new DiaModule(diaSession, module);
            }

            // Try to load PDB file into our own DIA session
            string pdb = module.SymbolFileName;

            if (!string.IsNullOrEmpty(pdb) && Path.GetExtension(pdb).ToLower() == ".pdb")
            {
                try
                {
                    return new DiaModule(pdb, module);
                }
                catch
                {
                }
            }

            return null;
        }
    }
}
