using Dia2Lib;

namespace CsDebugScript.Engine.SymbolProviders
{
    /// <summary>
    /// Interface that provides existing DIA sessions to avoid reopening sessions in the same process.
    /// </summary>
    public interface IDiaSessionProvider
    {
        /// <summary>
        /// Gets the DIA session for the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>
        ///   <see cref="IDiaSession" /> if available, null otherwise.
        /// </returns>
        IDiaSession GetModuleDiaSession(Module module);
    }
}
