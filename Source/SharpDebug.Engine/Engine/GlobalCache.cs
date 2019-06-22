using SharpUtilities;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// Helper class for caching global objects that will be cleared when <see cref="Context.ClearCache"/> is called.
    /// </summary>
    public static class GlobalCache
    {
        /// <summary>
        /// Collection of caches that should be cleared when requested.
        /// </summary>
        public static CacheInvalidator Caches { get; private set; } = new CacheInvalidator();

        /// <summary>
        /// The processes
        /// </summary>
        internal static DictionaryCache<uint, Process> Processes = new DictionaryCache<uint, Process>(CreateProcess);

        /// <summary>
        /// Creates the process.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        private static Process CreateProcess(uint processId)
        {
            return new Process(processId);
        }
    }
}
