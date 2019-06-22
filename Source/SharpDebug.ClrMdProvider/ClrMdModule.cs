using SharpDebug.CLR;
using SharpPdb.Managed;
using SharpUtilities;
using System;

namespace SharpDebug.ClrMdProvider
{
    /// <summary>
    /// ClrMD implementation of <see cref="IClrModule"/>.
    /// </summary>
    internal class ClrMdModule : IClrModule, IDisposable
    {
        /// <summary>
        /// The managed PDB reader.
        /// </summary>
        private SimpleCache<IPdbFile> clrPdbReader;

        /// <summary>
        /// The native module
        /// </summary>
        private SimpleCache<Module> module;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdModule"/> class.
        /// </summary>
        /// <param name="provider">The CLR provider.</param>
        /// <param name="clrModule">The CLR module.</param>
        public ClrMdModule(CLR.ClrMdProvider provider, Microsoft.Diagnostics.Runtime.ClrModule clrModule)
        {
            Provider = provider;
            ClrModule = clrModule;
            clrPdbReader = SimpleCache.Create(() =>
            {
                try
                {
                    string pdbPath = ClrModule.Runtime.DataTarget.SymbolLocator.FindPdb(ClrModule.Pdb);

                    if (!string.IsNullOrEmpty(pdbPath))
                    {
                        return Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader.OpenPdb(pdbPath);
                    }
                }
                catch (Exception)
                {
                }

                return null;
            });
            module = SimpleCache.Create(() =>
            {
                return Provider.GetProcess(ClrModule.Runtime)?.ClrModuleCache[this];
            });
        }

        /// <summary>
        /// Gets the base of the image loaded into memory. This may be 0 if there is not a physical file backing it.
        /// </summary>
        public ulong ImageBase => ClrModule.ImageBase;

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string Name => ClrModule.Name;

        /// <summary>
        /// Gets the name of the PDB file.
        /// </summary>
        public string PdbFileName => ClrModule.Pdb?.FileName;

        /// <summary>
        /// Gets the size of the image in memory.
        /// </summary>
        public ulong Size => ClrModule.Size;

        /// <summary>
        /// Gets the native module.
        /// </summary>
        public Module Module => module.Value;

        /// <summary>
        /// Gets the CLR provider.
        /// </summary>
        internal CLR.ClrMdProvider Provider { get; private set; }

        /// <summary>
        /// Gets the CLR module.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrModule ClrModule { get; private set; }

        /// <summary>
        /// Gets the CLR PDB reader.
        /// </summary>
        internal IPdbFile ClrPdbReader => clrPdbReader.Value;

        /// <summary>
        /// Attempts to obtain a <see cref="T:SharpDebug.CLR.IClrType" /> based on the name of the type.
        /// Note this is a "best effort" due to the way that the DAC handles types.
        /// This function will fail for Generics, and types which have never been constructed in the target process.
        /// Please be sure to null-check the return value of this function.
        /// </summary>
        /// <param name="typeName">The name of the type. (This would be the EXACT value returned by <see cref="P:SharpDebug.CLR.IClrType.Name" />).</param>
        public IClrType GetTypeByName(string typeName)
        {
            return Provider.FromClrType(ClrModule.GetTypeByName(typeName));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            clrPdbReader.Dispose();
        }
    }
}
