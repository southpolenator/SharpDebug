using CsDebugScript.Engine.Utility;
using System;
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
        /// Creates <see cref="DictionaryCache{TKey, TValue}"/> that is added to <see cref="Caches"/> when it is populated.
        /// </summary>
        /// <typeparam name="TKey">Dictionary cache key type.</typeparam>
        /// <typeparam name="TValue">Dictionary cache value type.</typeparam>
        /// <param name="populateAction">Function that will populate dictionary cache entries.</param>
        internal static DictionaryCache<TKey, TValue> CreateDictionaryCache<TKey, TValue>(Func<TKey, TValue> populateAction)
        {
            DictionaryCache<TKey, TValue> dictionaryCache = null;
            Func<TKey, TValue> cachedPopulateAction = (key) =>
            {
                if (dictionaryCache.Count == 0)
                    Caches.Add(dictionaryCache);
                return populateAction(key);
            };
            dictionaryCache = new DictionaryCache<TKey, TValue>(cachedPopulateAction);
            return dictionaryCache;
        }

        /// <summary>
        /// Creates <see cref="SimpleCache{T}"/> that is added to <see cref="Caches"/> when it is populated.
        /// </summary>
        /// <typeparam name="T">Simple cache value type.</typeparam>
        /// <param name="populateAction">Function that will populate simple cache.</param>
        internal static SimpleCache<T> CreateSimpleCache<T>(Func<T> populateAction)
        {
            SimpleCache<T> simpleCache = null;
            Func<T> cachedPopulateAction = () =>
            {
                Caches.Add(simpleCache);
                return populateAction();
            };
            simpleCache = SimpleCache.Create(cachedPopulateAction);
            return simpleCache;
        }

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
