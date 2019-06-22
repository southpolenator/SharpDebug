using CsDebugScript.CLR;
using SharpUtilities;
using System.Linq;

namespace CsDebugScript.ClrMdProvider
{
    /// <summary>
    /// ClrMD implementation of <see cref="IClrAppDomain"/>.
    /// </summary>
    internal class ClrMdAppDomain : IClrAppDomain
    {
        /// <summary>
        /// The cache of modules
        /// </summary>
        private SimpleCache<ClrMdModule[]> modules;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdAppDomain" /> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="clrAppDomain">The CLR application domain.</param>
        internal ClrMdAppDomain(ClrMdRuntime runtime, Microsoft.Diagnostics.Runtime.ClrAppDomain clrAppDomain)
        {
            Runtime = runtime;
            ClrAppDomain = clrAppDomain;
            modules = SimpleCache.Create(() => runtime.ClrRuntime.Modules.Where(m => m.AppDomains.Contains(ClrAppDomain)).Select(mm => runtime.Provider.FromClrModule(mm)).ToArray());
        }

        /// <summary>
        /// Gets the runtime associated with this AppDomain.
        /// </summary>
        public IClrRuntime Runtime { get; private set; }

        /// <summary>
        /// Gets the array of modules loaded into this AppDomain.
        /// </summary>
        public IClrModule[] Modules
        {
            get
            {
                return modules.Value;
            }
        }

        /// <summary>
        /// Gets the base directory for this AppDomain. This may return null if the targeted
        /// runtime does not support enumerating this information.
        /// </summary>
        public string ApplicationBase
        {
            get
            {
                return ClrAppDomain.ApplicationBase;
            }
        }

        /// <summary>
        /// Gets the configuration file used for the AppDomain. This may be null if there was
        /// no configuration file loaded, or if the targeted runtime does not support enumerating that data.
        /// </summary>
        public string ConfigurationFile
        {
            get
            {
                return ClrAppDomain.ConfigurationFile;
            }
        }

        /// <summary>
        /// Gets the AppDomain's ID.
        /// </summary>
        public int Id
        {
            get
            {
                return ClrAppDomain.Id;
            }
        }

        /// <summary>
        /// Gets the name of the AppDomain, as specified when the domain was created.
        /// </summary>
        public string Name
        {
            get
            {
                return ClrAppDomain.Name;
            }
        }

        /// <summary>
        /// Gets the CLR application domain.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrAppDomain ClrAppDomain { get; private set; }

        /// <summary>
        /// Gets the address of the AppDomain
        /// </summary>
        public ulong Address
        {
            get
            {
                return ClrAppDomain.Address;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }
}
