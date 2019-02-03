using CsDebugScript.CLR;
using SharpUtilities;

namespace CsDebugScript.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of <see cref="IClrAppDomain"/>.
    /// </summary>
    internal class VSClrAppDomain : IClrAppDomain
    {
        /// <summary>
        /// The cache of modules
        /// </summary>
        private SimpleCache<VSClrModule[]> modulesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSClrAppDomain" /> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="id">The application domain identifier.</param>
        /// <param name="address">The application domain address.</param>
        /// <param name="applicationBase">The application domain base directory.</param>
        /// <param name="configurationFile">The configuration file used for application domain.</param>
        public VSClrAppDomain(VSClrRuntime runtime, int id, string name, ulong address, string applicationBase, string configurationFile)
        {
            VSRuntime = runtime;
            Id = id;
            Name = name;
            Address = address;
            ApplicationBase = applicationBase;
            ConfigurationFile = configurationFile;
            modulesCache = SimpleCache.Create(() =>
            {
                ulong[] moduleAddresses = VSRuntime.Proxy.GetClrAppDomainModules(VSRuntime.Process.Id, VSRuntime.Id, Id);
                VSClrModule[] modules = new VSClrModule[moduleAddresses.Length];

                for (int i = 0; i < modules.Length; i++)
                    modules[i] = VSRuntime.GetModule(moduleAddresses[i]);
                return modules;
            });
        }

        /// <summary>
        /// Gets the Visual Studio implementation of the runtime.
        /// </summary>
        public VSClrRuntime VSRuntime { get; private set; }

        /// <summary>
        /// Gets the runtime associated with this AppDomain.
        /// </summary>
        public IClrRuntime Runtime => VSRuntime;

        /// <summary>
        /// Gets the array of modules loaded into this AppDomain.
        /// </summary>
        public IClrModule[] Modules => modulesCache.Value;

        /// <summary>
        /// Gets the base directory for this AppDomain. This may return null if the targeted
        /// runtime does not support enumerating this information.
        /// </summary>
        public string ApplicationBase { get; private set; }

        /// <summary>
        /// Gets the configuration file used for the AppDomain. This may be null if there was
        /// no configuration file loaded, or if the targeted runtime does not support enumerating that data.
        /// </summary>
        public string ConfigurationFile { get; private set; }

        /// <summary>
        /// Gets the AppDomain's ID.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the name of the AppDomain, as specified when the domain was created.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the CLR application domain.
        /// </summary>
        public ulong Address { get; private set; }
    }
}
