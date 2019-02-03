using CsDebugScript.ClrMdProvider;
using CsDebugScript.Engine;
using SharpUtilities;
using System.Linq;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// ClrMD implementation of <see cref="IClrProvider"/>.
    /// </summary>
    public class ClrMdProvider : IClrProvider
    {
        /// <summary>
        /// The cache of data targets per process
        /// </summary>
        private DictionaryCache<uint, Microsoft.Diagnostics.Runtime.DataTarget> dataTargetsPerProcess;

        /// <summary>
        /// The cache of CLR types.
        /// </summary>
        private DictionaryCache<Microsoft.Diagnostics.Runtime.ClrType, ClrMdType> clrTypes;

        /// <summary>
        /// The cache of CLR modules.
        /// </summary>
        private DictionaryCache<Microsoft.Diagnostics.Runtime.ClrModule, ClrMdModule> clrModules;

        /// <summary>
        /// The cache of CLR heaps.
        /// </summary>
        private DictionaryCache<Microsoft.Diagnostics.Runtime.ClrHeap, ClrMdHeap> clrHeaps;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdProvider"/> class.
        /// </summary>
        public ClrMdProvider()
        {
            dataTargetsPerProcess = new DictionaryCache<uint, Microsoft.Diagnostics.Runtime.DataTarget>(GetDataTarget);
            clrTypes = new DictionaryCache<Microsoft.Diagnostics.Runtime.ClrType, ClrMdType>(type => new ClrMdType(this, type));
            clrModules = new DictionaryCache<Microsoft.Diagnostics.Runtime.ClrModule, ClrMdModule>(module => new ClrMdModule(this, module));
            clrHeaps = new DictionaryCache<Microsoft.Diagnostics.Runtime.ClrHeap, ClrMdHeap>(clrHeap =>
            {
                IClrRuntime runtime = Process.All.SelectMany(p => p.ClrRuntimes).Where(r => ((ClrMdRuntime)r).ClrRuntime == clrHeap.Runtime).FirstOrDefault();

                return (ClrMdHeap)(runtime?.Heap);
            });
        }

        /// <summary>
        /// Gets the CLR runtimes running in the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public IClrRuntime[] GetClrRuntimes(Process process)
        {
            try
            {
                var dataTarget = dataTargetsPerProcess[process.Id];
                return dataTarget.ClrVersions.Select(clrInfo => new ClrMdRuntime(this, process, clrInfo.CreateRuntime())).ToArray();
            }
            catch
            {
                return new ClrMdRuntime[0];
            }
        }

        /// <summary>
        /// Converts CLR type to Engine interface.
        /// </summary>
        /// <param name="clrType">The CLR type.</param>
        internal ClrMdType FromClrType(Microsoft.Diagnostics.Runtime.ClrType clrType)
        {
            if (clrType != null)
            {
                return clrTypes[clrType];
            }

            return null;
        }

        /// <summary>
        /// Converts CLR module to Engine interface.
        /// </summary>
        /// <param name="clrModule">The CLR module.</param>
        internal ClrMdModule FromClrModule(Microsoft.Diagnostics.Runtime.ClrModule clrModule)
        {
            if (clrModule != null)
            {
                return clrModules[clrModule];
            }

            return null;
        }

        /// <summary>
        /// Converts CLR heap to Engine interface.
        /// </summary>
        /// <param name="clrHeap">The CLR heap.</param>
        internal IClrHeap FromClrHeap(Microsoft.Diagnostics.Runtime.ClrHeap clrHeap)
        {
            if (clrHeap != null)
            {
                return clrHeaps[clrHeap];
            }

            return null;
        }

        /// <summary>
        /// Gets the process for the specified CLR runtime.
        /// </summary>
        /// <param name="runtime">The CLR runtime.</param>
        internal Process GetProcess(Microsoft.Diagnostics.Runtime.ClrRuntime runtime)
        {
            return Process.All.FirstOrDefault(p => p.ClrRuntimes.Cast<ClrMdRuntime>().Any(r => r.ClrRuntime == runtime));
        }

        /// <summary>
        /// Gets the data target for the specified process ID.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        private Microsoft.Diagnostics.Runtime.DataTarget GetDataTarget(uint processId)
        {
            Process process = GlobalCache.Processes[processId];
            var dataTarget = Microsoft.Diagnostics.Runtime.DataTarget.CreateFromDataReader(new DataReader(process));

            dataTarget.SymbolLocator = new ClrMdSymbolLocator(process, dataTarget.SymbolLocator);
            return dataTarget;
        }
    }
}
