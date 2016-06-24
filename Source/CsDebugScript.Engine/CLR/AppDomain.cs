using CsDebugScript.Engine.Utility;
using System.Linq;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// CLR code AppDomain. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public class AppDomain
    {
        /// <summary>
        /// The cache of modules
        /// </summary>
        private SimpleCache<Module[]> modules;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomain" /> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="clrAppDomain">The CLR application domain.</param>
        internal AppDomain(Runtime runtime, Microsoft.Diagnostics.Runtime.ClrAppDomain clrAppDomain)
        {
            Runtime = runtime;
            ClrAppDomain = clrAppDomain;
            modules = SimpleCache.Create(() => Runtime.ClrRuntime.Modules.Where(m => m.AppDomains.Contains(ClrAppDomain)).Select(mm => Runtime.Process.ClrModuleCache[mm]).ToArray());
        }

        /// <summary>
        /// Gets the runtime associated with this AppDomain.
        /// </summary>
        public Runtime Runtime { get; private set; }

        /// <summary>
        /// Gets the array of modules loaded into this AppDomain.
        /// </summary>
        public Module[] Modules
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
