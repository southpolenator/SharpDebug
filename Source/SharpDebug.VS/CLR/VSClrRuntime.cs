using SharpDebug.CLR;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpDebug.VS.CLR
{
    /// <summary>
    /// Visual Studio implemenatation of <see cref="IClrRuntime"/>.
    /// </summary>
    internal class VSClrRuntime : IClrRuntime
    {
        /// <summary>
        /// The threads and GC threads cache.
        /// </summary>
        private SimpleCache<Tuple<VSClrThread[], VSClrThread[]>> threadsAndGcThreads;

        /// <summary>
        /// The modules cache.
        /// </summary>
        private SimpleCache<VSClrModule[]> modulesCache;

        /// <summary>
        /// The CLR types cache.
        /// </summary>
        private DictionaryCache<int, VSClrType> clrTypesCache;

        /// <summary>
        /// The application domains cache.
        /// </summary>
        private SimpleCache<VSClrAppDomain[]> appDomainsCache;

        /// <summary>
        /// The shared application domain cache.
        /// </summary>
        private SimpleCache<VSClrAppDomain> sharedAppDomainCache;

        /// <summary>
        /// The system application domain cache.
        /// </summary>
        private SimpleCache<VSClrAppDomain> systemAppDomainCache;

        /// <summary>
        /// The heap count cache.
        /// </summary>
        private SimpleCache<int> heapCountCache;

        /// <summary>
        /// Is server GC cache.
        /// </summary>
        private SimpleCache<bool> serverGCCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSClrRuntime" /> class.
        /// </summary>
        /// <param name="debugger">The Visual Studio debugger.</param>
        /// <param name="process">The process.</param>
        /// <param name="id">The runtime identifier.</param>
        /// <param name="version">The runtime version.</param>
        public VSClrRuntime(VSDebugger debugger, Process process, int id, ModuleVersion version)
        {
            Debugger = debugger;
            Id = id;
            Process = process;
            Version = version;
            VSHeap = new VSClrHeap(this);
            clrTypesCache = new DictionaryCache<int, VSClrType>((clrTypeId) => new VSClrType(this, clrTypeId));
            threadsAndGcThreads = SimpleCache.Create(() =>
            {
                Tuple<bool, uint, bool, ulong>[] threadTuples = Proxy.GetClrRuntimeThreads(Process.Id, Id);
                List<VSClrThread> threads = new List<VSClrThread>();
                List<VSClrThread> gcThreads = new List<VSClrThread>();

                for (int i = 0; i < threadTuples.Length; i++)
                {
                    VSClrThread thread = new VSClrThread(this, threadTuples[i].Item2, threadTuples[i].Item3, threadTuples[i].Item4);

                    if (!threadTuples[i].Item1)
                        gcThreads.Add(thread);
                    threads.Add(thread);
                }
                return Tuple.Create(threads.ToArray(), gcThreads.ToArray());
            });
            modulesCache = SimpleCache.Create(() =>
            {
                ulong[] moduleAddresses = Proxy.GetClrRuntimeModules(Process.Id, Id);
                VSClrModule[] modules = new VSClrModule[moduleAddresses.Length];

                for (int i = 0; i < modules.Length; i++)
                    modules[i] = new VSClrModule(this, moduleAddresses[i]);
                return modules;
            });
            appDomainsCache = SimpleCache.Create(() =>
            {
                Tuple<int, string, ulong, string, string>[] tuples = Proxy.GetClrRuntimeAppDomains(Process.Id, Id);
                VSClrAppDomain[] appDomains = new VSClrAppDomain[tuples.Length];

                for (int i = 0; i < tuples.Length; i++)
                    appDomains[i] = new VSClrAppDomain(this, tuples[i].Item1, tuples[i].Item2, tuples[i].Item3, tuples[i].Item4, tuples[i].Item5);
                return appDomains;
            });
            sharedAppDomainCache = SimpleCache.Create(() =>
            {
                Tuple<int, string, ulong, string, string> tuple = Proxy.GetClrRuntimeSharedAppDomain(Process.Id, Id);

                if (tuple.Item1 == int.MinValue)
                    return null;
                return new VSClrAppDomain(this, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
            });
            systemAppDomainCache = SimpleCache.Create(() =>
            {
                Tuple<int, string, ulong, string, string> tuple = Proxy.GetClrRuntimeSystemAppDomain(Process.Id, Id);

                if (tuple.Item1 == int.MinValue)
                    return null;
                return new VSClrAppDomain(this, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
            });
            heapCountCache = SimpleCache.Create(() => Proxy.GetClrRuntimeHeapCount(Process.Id, Id));
            serverGCCache = SimpleCache.Create(() => Proxy.GetClrRuntimeServerGC(Process.Id, Id));
        }

        /// <summary>
        /// Gets the Visual Studio debugger.
        /// </summary>
        public VSDebugger Debugger { get; private set; }

        /// <summary>
        /// Gets the Visual Studio debugger proxy.
        /// </summary>
        public VSDebuggerProxy Proxy => Debugger.Proxy;

        /// <summary>
        /// Gets the runtime identifier.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the CLR version.
        /// </summary>
        public ModuleVersion Version { get; private set; }

        /// <summary>
        /// Gets the runtime heap.
        /// </summary>
        public VSClrHeap VSHeap { get; private set; }

        /// <summary>
        /// Gets the array of application domains in the process. Note that System AppDomain and Shared AppDomain are omitted.
        /// </summary>
        public IClrAppDomain[] AppDomains => appDomainsCache.Value;

        /// <summary>
        /// Gets the enumeration of all application domains in the process including System and Shared AppDomain.
        /// </summary>
        public IEnumerable<IClrAppDomain> AllAppDomains
        {
            get
            {
                foreach (IClrAppDomain appDomain in AppDomains)
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
        public IClrAppDomain SharedDomain => sharedAppDomainCache.Value;

        /// <summary>
        /// Gets the System AppDomain.
        /// </summary>
        public IClrAppDomain SystemDomain => systemAppDomainCache.Value;

        /// <summary>
        /// Gets the array of all modules loaded in the runtime.
        /// </summary>
        public IClrModule[] Modules => modulesCache.Value;

        /// <summary>
        /// Gets the array of all managed threads in the process. Only threads which have previously run managed code will be enumerated.
        /// </summary>
        public IClrThread[] Threads => threadsAndGcThreads.Value.Item1;

        /// <summary>
        /// Gets the array of GC threads in the runtime.
        /// </summary>
        public IClrThread[] GCThreads => threadsAndGcThreads.Value.Item2;

        /// <summary>
        /// Gets the number of logical GC heaps in the process. This is always 1 for a workstation GC,
        /// and usually it's the number of logical processors in a server GC application.
        /// </summary>
        public int HeapCount => heapCountCache.Value;

        /// <summary>
        /// Gets a value indicating whether the process is running in server GC mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the process is running in server GC mode; otherwise, <c>false</c>.
        /// </value>
        public bool ServerGC => serverGCCache.Value;

        /// <summary>
        /// Gets the GC heap of the process.
        /// </summary>
        public IClrHeap Heap => VSHeap;

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
                    let file = Path.GetFileName(module.Name) // TODO: Check if this is correct (it was ImageName)
                    where file.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                    select module).SingleOrDefault();
        }

        /// <summary>
        /// Reads the function name and displacement of the specified code address.
        /// </summary>
        /// <param name="address">The code address</param>
        public Tuple<string, ulong> ReadFunctionNameAndDisplacement(ulong address)
        {
            return Proxy.ReadClrRuntimeFunctionNameAndDisplacement(Process.Id, Id, address);
        }

        /// <summary>
        /// Reads the name of the source file, line and displacement of the specified code address.
        /// </summary>
        /// <param name="address">The code address</param>
        public Tuple<string, uint, ulong> ReadSourceFileNameAndLine(ulong address)
        {
            return Proxy.ReadClrRuntimeSourceFileNameAndLine(Process.Id, Id, address);
        }

        /// <summary>
        /// Gets the module by the specified module address.
        /// </summary>
        /// <param name="moduleAddress">The module base address.</param>
        internal VSClrModule GetModule(ulong moduleAddress)
        {
            return modulesCache.Value.FirstOrDefault(m => m.ImageBase == moduleAddress);
        }

        /// <summary>
        /// Gets the CLR type by the specified identifier.
        /// </summary>
        /// <param name="clrTypeId">The CLR type identifier.</param>
        internal VSClrType GetClrType(int clrTypeId)
        {
            if (clrTypeId < 0)
                return null;
            return clrTypesCache[clrTypeId];
        }
    }
}
