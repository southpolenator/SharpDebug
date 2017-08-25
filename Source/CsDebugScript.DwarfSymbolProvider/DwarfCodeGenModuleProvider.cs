using CsDebugScript.CodeGen;
using CsDebugScript.CodeGen.SymbolProviders;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// Implementation of <see cref="IModuleProvider"/> that uses DWARF for opening modules.
    /// </summary>
    /// <seealso cref="CsDebugScript.CodeGen.SymbolProviders.IModuleProvider" />
    public class DwarfCodeGenModuleProvider : IModuleProvider
    {
        /// <summary>
        /// DWARF symbol provider
        /// </summary>
        private DwarfSymbolProvider symbolProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DwarfCodeGenModuleProvider"/> class.
        /// </summary>
        public DwarfCodeGenModuleProvider()
        {
            symbolProvider = new DwarfSymbolProvider();
        }

        /// <summary>
        /// Opens the module for the specified XML module description.
        /// </summary>
        /// <param name="xmlModule">The XML module description.</param>
        public CsDebugScript.CodeGen.SymbolProviders.Module Open(XmlModule xmlModule)
        {
            Module module = new Module(default(Process), 0)
            {
                Name = xmlModule.Name,
                SymbolFileName = xmlModule.PdbPath,
                MappedImageName = xmlModule.PdbPath,
                ImageName = xmlModule.PdbPath,
            };
            var result = new EngineSymbolProviderModule(module, xmlModule, symbolProvider);
            DwarfSymbolProviderModule provider = (DwarfSymbolProviderModule)result.EngineModuleProvider;

            module.PointerSize = provider.Is64bit ? 8U : 4U;
            return result;
        }
    }
}
