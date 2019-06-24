using SharpDebug.CLR;
using SharpUtilities;
using System.Linq;

namespace SharpDebug.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of <see cref="IClrModule"/>.
    /// </summary>
    internal class VSClrModule : IClrModule
    {
        /// <summary>
        /// The native module cache.
        /// </summary>
        private SimpleCache<Module> moduleCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSClrModule"/> class.
        /// </summary>
        /// <param name="runtime">The owning runtime.</param>
        /// <param name="imageBase">The module image base.</param>
        public VSClrModule(VSClrRuntime runtime, ulong imageBase)
        {
            Runtime = runtime;
            ImageBase = imageBase;
            moduleCache = SimpleCache.Create(() => Runtime.Process.OriginalModules.FirstOrDefault(m => m.Address == ImageBase));
        }

        /// <summary>
        /// Gets the Visual Studio implementation of the runtime.
        /// </summary>
        public VSClrRuntime Runtime { get; private set; }

        /// <summary>
        /// Gets the base of the image loaded into memory. This may be 0 if there is not a physical file backing it.
        /// </summary>
        public ulong ImageBase { get; private set; }

        /// <summary>
        /// Gets the native module.
        /// </summary>
        public Module Module => moduleCache.Value;

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string Name => Module.Name;

        /// <summary>
        /// Gets the name of the PDB file.
        /// </summary>
        public string PdbFileName => Module.SymbolFileName;

        /// <summary>
        /// Gets the size of the image in memory.
        /// </summary>
        public ulong Size => Module.Size;

        /// <summary>
        /// Attempts to obtain a <see cref="IClrType"/> based on the name of the type.
        /// Note this is a "best effort" due to the way that the DAC handles types.
        /// This function will fail for Generics, and types which have never been constructed in the target process.
        /// Please be sure to null-check the return value of this function.
        /// </summary>
        /// <param name="typeName">The name of the type. (This would be the EXACT value returned by <see cref="IClrType.Name"/>).</param>
        public IClrType GetTypeByName(string typeName)
        {
            return Runtime.GetClrType(Runtime.Proxy.GetClrModuleTypeByName(Runtime.Process.Id, Runtime.Id, ImageBase, typeName));
        }
    }
}
