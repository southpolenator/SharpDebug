using System;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Implementation of <see cref="IModuleProvider"/> that uses engine ISymbolProvider.
    /// </summary>
    /// <seealso cref="CsDebugScript.CodeGen.SymbolProviders.IModuleProvider" />
    public class EngineSymbolProviderModuleProvider : IModuleProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineSymbolProviderModuleProvider"/> class.
        /// </summary>
        /// <param name="process">The process that we will do CodeGen for.</param>
        public EngineSymbolProviderModuleProvider(Process process)
        {
            Process = process;
        }

        /// <summary>
        /// Gets the process that contains modules to be used for CodeGen.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Opens the module for the specified XML module description.
        /// </summary>
        /// <param name="xmlModule">The XML module description.</param>
        public virtual Module Open(XmlModule xmlModule)
        {
            foreach (var module in Process.Modules)
                if (module.SymbolFileName == xmlModule.SymbolsPath || module.MappedImageName == xmlModule.SymbolsPath)
                    return new EngineSymbolProviderModule(module, xmlModule);
            throw new ArgumentException("Couldn't find XML module in current process modules list.", nameof(xmlModule));
        }
    }
}
