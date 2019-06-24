using SharpDebug.CLR;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpDebug.ClrMdProvider
{
    /// <summary>
    /// ClrMD implemenatation of <see cref="IClrRuntime"/>.
    /// </summary>
    internal class ClrMdRuntime : IClrRuntime
    {
        /// <summary>
        /// The application domains
        /// </summary>
        private SimpleCache<ClrMdAppDomain[]> appDomains;

        /// <summary>
        /// The modules
        /// </summary>
        private SimpleCache<ClrMdModule[]> modules;

        /// <summary>
        /// The shared domain
        /// </summary>
        private SimpleCache<ClrMdAppDomain> sharedDomain;

        /// <summary>
        /// The system domain
        /// </summary>
        private SimpleCache<ClrMdAppDomain> systemDomain;

        /// <summary>
        /// The threads
        /// </summary>
        private SimpleCache<ClrMdThread[]> threads;

        /// <summary>
        /// The GC threads
        /// </summary>
        private SimpleCache<ClrMdThread[]> gcThreads;

        /// <summary>
        /// The cache of CLR heap
        /// </summary>
        private SimpleCache<ClrMdHeap> heap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdRuntime" /> class.
        /// </summary>
        /// <param name="provider">The ClrMD provider.</param>
        /// <param name="process">The process.</param>
        /// <param name="clrRuntime">The CLR runtime.</param>
        internal ClrMdRuntime(CLR.ClrMdProvider provider, Process process, Microsoft.Diagnostics.Runtime.ClrRuntime clrRuntime)
        {
            Provider = provider;
            Process = process;
            ClrRuntime = clrRuntime;
            appDomains = SimpleCache.Create(() => ClrRuntime.AppDomains.Select(ad => new ClrMdAppDomain(this, ad)).ToArray());
            modules = SimpleCache.Create(() => ClrRuntime.Modules.Select(mm => Provider.FromClrModule(mm)).ToArray());
            sharedDomain = SimpleCache.Create(() => ClrRuntime.SharedDomain != null ? new ClrMdAppDomain(this, ClrRuntime.SharedDomain) : null);
            systemDomain = SimpleCache.Create(() => ClrRuntime.SystemDomain != null ? new ClrMdAppDomain(this, ClrRuntime.SystemDomain) : null);
            threads = SimpleCache.Create(() => ClrRuntime.Threads.Select(tt => new ClrMdThread(Process.Threads.Where(t => t.SystemId == tt.OSThreadId).FirstOrDefault(), tt, Process)).ToArray());
            gcThreads = SimpleCache.Create(() => ClrRuntime.EnumerateGCThreads().Select(tt => new ClrMdThread(Process.Threads.Where(t => t.SystemId == tt).FirstOrDefault(), ClrRuntime.Threads.First(ct => ct.OSThreadId == tt), Process)).ToArray());
            heap = SimpleCache.Create(() => new ClrMdHeap(this, ClrRuntime.GetHeap()));

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
        /// Gets the ClrMD provider.
        /// </summary>
        public CLR.ClrMdProvider Provider { get; private set; }

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
        public IClrAppDomain[] AppDomains
        {
            get
            {
                return appDomains.Value;
            }
        }

        /// <summary>
        /// Gets the enumeration of all application domains in the process including System and Shared AppDomain.
        /// </summary>
        public IEnumerable<IClrAppDomain> AllAppDomains
        {
            get
            {
                foreach (ClrMdAppDomain appDomain in AppDomains)
                    yield return appDomain;
                if (SharedDomain != null)
                    yield return SharedDomain;
                if (SystemDomain != null)
                    yield return SystemDomain;
            }
        }

        /// <summary>
        /// Gets the Shared AppDomain.
        /// </summary>
        public IClrAppDomain SharedDomain
        {
            get
            {
                return sharedDomain.Value;
            }
        }

        /// <summary>
        /// Gets the System AppDomain.
        /// </summary>
        public IClrAppDomain SystemDomain
        {
            get
            {
                return systemDomain.Value;
            }
        }

        /// <summary>
        /// Gets the array of all modules loaded in the runtime.
        /// </summary>
        public IClrModule[] Modules
        {
            get
            {
                return modules.Value;
            }
        }

        /// <summary>
        /// Gets the array of all managed threads in the process. Only threads which have previously run managed code will be enumerated.
        /// </summary>
        public IClrThread[] Threads
        {
            get
            {
                return threads.Value;
            }
        }

        /// <summary>
        /// Gets the array of GC threads in the runtime.
        /// </summary>
        public IClrThread[] GCThreads
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
        public IClrHeap Heap
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
        public IClrAppDomain GetAppDomainByName(string appDomainName)
        {
            return AppDomains.SingleOrDefault(ad => ad.Name == appDomainName);
        }

        /// <summary>
        /// Gets the module by the file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public IClrModule GetModuleByFileName(string fileName)
        {
            return (from module in Modules
                    let file = System.IO.Path.GetFileName(module.Name) // TODO: Check if this is correct (it was ImageName)
                    where file.Equals(fileName, System.StringComparison.OrdinalIgnoreCase)
                    select module).SingleOrDefault();
        }

        /// <summary>
        /// Reads the name of the source file, line and displacement of the specified code address.
        /// </summary>
        /// <param name="address">The code address</param>
        public Tuple<string, uint, ulong> ReadSourceFileNameAndLine(ulong address)
        {
            Microsoft.Diagnostics.Runtime.ClrMethod method = ClrRuntime.GetMethodByAddress(address);
            ClrMdModule clrModule = Provider.FromClrModule(method.Type.Module);

            return ClrMdStackFrame.ReadSourceFileNameAndLine(clrModule, method, address);
        }

        /// <summary>
        /// Reads the function name and displacement of the specified code address.
        /// </summary>
        /// <param name="address">The code address</param>
        public Tuple<string, ulong> ReadFunctionNameAndDisplacement(ulong address)
        {
            Microsoft.Diagnostics.Runtime.ClrMethod method = ClrRuntime.GetMethodByAddress(address);
            IClrModule clrModule = Provider.FromClrModule(method.Type.Module);

            return ClrMdStackFrame.ReadFunctionNameAndDisplacement(clrModule.Module, method, address);
        }
    }
}
