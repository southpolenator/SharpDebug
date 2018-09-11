using CsDebugScript.Engine.Utility;
using System.Collections.Concurrent;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// Helper class for caching global objects.
    /// </summary>
    internal class GlobalCache
    {
        /// <summary>
        /// The processes
        /// </summary>
        internal static DictionaryCache<uint, Process> Processes = new DictionaryCache<uint, Process>(CreateProcess);

        /// <summary>
        /// Collection of caches that should be cleared when requested.
        /// </summary>
        internal static ConcurrentBag<ICache> Caches = new ConcurrentBag<ICache>();

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
