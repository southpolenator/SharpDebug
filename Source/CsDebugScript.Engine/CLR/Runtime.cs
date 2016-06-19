using CsDebugScript.Engine.Utility;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// CLR code Runtime. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public class Runtime
    {
        /// <summary>
        /// The application domains
        /// </summary>
        private SimpleCache<AppDomain[]> appDomains;

        /// <summary>
        /// The modules
        /// </summary>
        private SimpleCache<Module[]> modules;

        /// <summary>
        /// The shared domain
        /// </summary>
        private SimpleCache<AppDomain> sharedDomain;

        /// <summary>
        /// The system domain
        /// </summary>
        private SimpleCache<AppDomain> systemDomain;

        /// <summary>
        /// The threads
        /// </summary>
        private SimpleCache<ClrThread[]> threads;

        /// <summary>
        /// The GC threads
        /// </summary>
        private SimpleCache<ClrThread[]> gcThreads;

        /// <summary>
        /// The cache of CLR heap
        /// </summary>
        private SimpleCache<Heap> heap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Runtime" /> class.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="clrRuntime">The CLR runtime.</param>
        internal Runtime(Process process, Microsoft.Diagnostics.Runtime.ClrRuntime clrRuntime)
        {
            Process = process;
            ClrRuntime = clrRuntime;
            appDomains = SimpleCache.Create(() => ClrRuntime.AppDomains.Select(ad => new AppDomain(this, ad)).ToArray());
            modules = SimpleCache.Create(() => ClrRuntime.Modules.Select(mm => Process.ClrModuleCache[mm]).ToArray());
            sharedDomain = SimpleCache.Create(() => ClrRuntime.SharedDomain != null ? new AppDomain(this, ClrRuntime.SharedDomain) : null);
            systemDomain = SimpleCache.Create(() => ClrRuntime.SystemDomain != null ? new AppDomain(this, ClrRuntime.SystemDomain) : null);
            threads = SimpleCache.Create(() => ClrRuntime.Threads.Select(tt => new ClrThread(Process.Threads.Where(t => t.SystemId == tt.OSThreadId).FirstOrDefault(), tt, Process)).ToArray());
            gcThreads = SimpleCache.Create(() => ClrRuntime.EnumerateGCThreads().Select(tt => new ClrThread(Process.Threads.Where(t => t.SystemId == tt).FirstOrDefault(), ClrRuntime.Threads.First(ct => ct.OSThreadId == tt), Process)).ToArray());
            heap = SimpleCache.Create(() => new Heap(this, ClrRuntime.GetHeap()));

            var version = ClrRuntime.ClrInfo.Version;

            Version = new ModuleVersion()
            {
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Patch,
                Revision = version.Revision,
            };
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the CLR version.
        /// </summary>
        public ModuleVersion Version { get; private set; }

        /// <summary>
        /// Gets the array of application domains in the process. Note that System AppDomain and Shared AppDomain are omitted.
        /// </summary>
        public AppDomain[] AppDomains
        {
            get
            {
                return appDomains.Value;
            }
        }

        /// <summary>
        /// Gets the enumeration of all application domains in the process including System and Shared AppDomain.
        /// </summary>
        public IEnumerable<AppDomain> AllAppDomains
        {
            get
            {
                foreach (AppDomain appDomain in AppDomains)
                    yield return appDomain;
                yield return SharedDomain;
                yield return SystemDomain;
            }
        }

        /// <summary>
        /// Gets the Shared AppDomain.
        /// </summary>
        public AppDomain SharedDomain
        {
            get
            {
                return sharedDomain.Value;
            }
        }

        /// <summary>
        /// Gets the System AppDomain.
        /// </summary>
        public AppDomain SystemDomain
        {
            get
            {
                return systemDomain.Value;
            }
        }

        /// <summary>
        /// Gets the array of all modules loaded in the runtime.
        /// </summary>
        public Module[] Modules
        {
            get
            {
                return modules.Value;
            }
        }

        /// <summary>
        /// Gets the array of all managed threads in the process. Only threads which have previously run managed code will be enumerated.
        /// </summary>
        public ClrThread[] Threads
        {
            get
            {
                return threads.Value;
            }
        }

        /// <summary>
        /// Gets the array of GC threads in the runtime.
        /// </summary>
        public ClrThread[] GCThreads
        {
            get
            {
                return gcThreads.Value;
            }
        }

        /// <summary>
        /// Gets the number of logical GC heaps in the process. This is always 1 for a workstation GC,
        /// and usually it's the number of logical processors in a server GC application.
        /// </summary>
        public int HeapCount
        {
            get
            {
                return ClrRuntime.HeapCount;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the process is running in server GC mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the process is running in server GC mode; otherwise, <c>false</c>.
        /// </value>
        public bool ServerGC
        {
            get
            {
                return ClrRuntime.ServerGC;
            }
        }

        /// <summary>
        /// Gets the GC heap of the process.
        /// </summary>
        public Heap Heap
        {
            get
            {
                return heap.Value;
            }
        }

        /// <summary>
        /// Gets or sets the CLR runtime.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrRuntime ClrRuntime { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Version.ToString();
        }

        /// <summary>
        /// Gets the AppDomain specified by the name. If multiple AppDomains have same name, it will throw exception.
        /// If there is no such AppDomain, it will return null.
        /// </summary>
        /// <param name="appDomainName">The application domain name.</param>
        public AppDomain GetAppDomainByName(string appDomainName)
        {
            return AppDomains.SingleOrDefault(ad => ad.Name == appDomainName);
        }

        /// <summary>
        /// Gets the module by the file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public Module GetModuleByFileName(string fileName)
        {
            return (from module in Modules
                    let file = System.IO.Path.GetFileName(module.ImageName)
                    where file.Equals(fileName, System.StringComparison.OrdinalIgnoreCase)
                    select module).SingleOrDefault();
        }
    }
}
