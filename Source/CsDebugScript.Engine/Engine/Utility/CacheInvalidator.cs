using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CsDebugScript.Engine.Utility
{
    /// <summary>
    /// Helper class that is used to collect caches that needs to be invalidated on particular event.
    /// </summary>
    public class CacheInvalidator : ICache
    {
        /// <summary>
        /// Collected caches that should be invalidated.
        /// </summary>
        public ConcurrentBag<ICache> Caches { get; private set; } = new ConcurrentBag<ICache>();

        /// <summary>
        /// Creates <see cref="DictionaryCache{TKey, TValue}"/> that is added to <see cref="Caches"/> when it is populated.
        /// </summary>
        /// <typeparam name="TKey">Dictionary cache key type.</typeparam>
        /// <typeparam name="TValue">Dictionary cache value type.</typeparam>
        /// <param name="populateAction">Function that will populate dictionary cache entries.</param>
        public DictionaryCache<TKey, TValue> CreateDictionaryCache<TKey, TValue>(Func<TKey, TValue> populateAction)
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
        public SimpleCache<T> CreateSimpleCache<T>(Func<T> populateAction)
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
        /// Invalidate cache entry.
        /// </summary>
        public void InvalidateCache()
        {
            ConcurrentBag<ICache> oldCaches = Caches;
            Caches = new ConcurrentBag<ICache>();
            foreach (ICache cache in oldCaches)
                cache.InvalidateCache();
        }

        /// <summary>
        /// Gets enumerator for all the cached objects in this cache.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return Caches.GetEnumerator();
        }

        /// <summary>
        /// Invalidates all the instances of type <see cref="ICache" /> and
        /// <see cref="DictionaryCache{TKey, TValue}" /> which are fields of class given as root object and any
        /// fields of the same type in child fields recursively.
        /// Use it when there are massive changes and all the caches need to be invalidated.
        /// </summary>
        /// <param name="rootObject">Root object from which we recursively drop the caches.</param>
        public static void InvalidateCaches(object rootObject)
        {
            if (rootObject == null)
            {
                return;
            }

            // Gets all the cache fields of given type.
            IEnumerable<FieldInfo> cacheFields =
                rootObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fieldInfo => fieldInfo.FieldType.GetInterfaces().Contains(typeof(ICache)));

            foreach (FieldInfo field in cacheFields)
            {
                ICache cache = field.GetValue(rootObject) as ICache;

                // Clear only fields which are cached.
                if (cache != null)
                {
                    object[] cacheEntries = cache.OfType<object>().ToArray();
                    cache.InvalidateCache();

                    // Invalidate all the cached object if any.
                    foreach (object cachedCollectionEntry in cacheEntries)
                    {
                        InvalidateCaches(cachedCollectionEntry);
                    }
                }
            }
        }
    }
}
