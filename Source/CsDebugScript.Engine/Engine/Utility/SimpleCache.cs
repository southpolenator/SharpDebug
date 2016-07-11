using System;
using System.Collections;

namespace CsDebugScript.Engine.Utility
{
    /// <summary>
    /// Interface for all caching structures.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets a value indicating whether value is cached.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cached; otherwise, <c>false</c>.
        /// </value>
        bool Cached { get; }

        /// <summary>
        /// Invalidates this cache.
        /// </summary>
        void InvalidateCache();

        /// <summary>
        /// Gets or sets the value. The value will be populated if it wasn't cached.
        /// </summary>
        object ValueRaw { get; }
    }

    /// <summary>
    /// Interface for all caching collections.
    /// </summary>
    public interface ICacheCollection : ICache
    {
        /// <summary>
        /// Returns IEnumerable for all cached values.
        /// </summary>
        IEnumerable ValuesRaw { get; }
    }

    /// <summary>
    /// Helper class for caching results - it is being used as lazy evaluation
    /// </summary>
    public static class SimpleCache
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SimpleCache{T}" /> class.
        /// </summary>
        /// <typeparam name="T">Type to be cached</typeparam>
        /// <param name="populateAction">The function that populates the cache on demand.</param>
        /// <returns>Simple cache of &lt;T&gt;</returns>
        public static SimpleCache<T> Create<T>(Func<T> populateAction)
        {
            return new SimpleCache<T>(populateAction);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SimpleCacheStruct{T}" /> class.
        /// </summary>
        /// <typeparam name="T">Type to be cached</typeparam>
        /// <param name="populateAction">The function that populates the cache on demand.</param>
        /// <returns>Simple cache of &lt;T&gt;</returns>
        public static SimpleCacheStruct<T> CreateStruct<T>(Func<T> populateAction)
        {
            return new SimpleCacheStruct<T>(populateAction);
        }

        /// <summary>
        /// Invalidates all the instances of type <see cref="SimpleCacheStruct{T}" /> and
        /// <see cref="SimpleCache{T}" /> which are fields of class given as root object and any
        /// fields of the same type in child fields recursively.
        /// Use it when there are massive changes and all the caches need to be invalidated.
        /// </summary>
        /// <typeparam name="T">Type of the root object.</typeparam>
        /// <param name="rootObject">Root object from which we recursively drop the caches.</param>
        public static void InvalidateCacheRecursively<T>(T rootObject)
        {
            // TODO: Only drop cache on objects which are not cached in
            // order to avoid infinite recursion.

        }
    }

    /// <summary>
    /// Helper class for caching results - it is being used as lazy evaluation
    /// </summary>
    /// <typeparam name="T">Type to be cached</typeparam>
    public class SimpleCache<T> : ICache
    {
        /// <summary>
        /// The populate action
        /// </summary>
        private Func<T> populateAction;

        /// <summary>
        /// The value that is cached
        /// </summary>
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCache{T}"/> class.
        /// </summary>
        /// <param name="populateAction">The function that populates the cache on demand.</param>
        public SimpleCache(Func<T> populateAction)
        {
            this.populateAction = populateAction;
        }

        /// <summary>
        /// Gets a value indicating whether value is cached.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cached; otherwise, <c>false</c>.
        /// </value>
        public bool Cached { get; internal set; }

        /// <summary>
        /// Gets or sets the value. The value will be populated if it wasn't cached.
        /// </summary>
        public T Value
        {
            get
            {
                if (!Cached)
                {
                    lock(this)
                    {
                        if (!Cached)
                        {
                            value = populateAction();
                            Cached = true;
                        }
                    }
                }

                return value;
            }

            set
            {
                this.value = value;
                Cached = true;
            }
        }

        /// <summary>
        /// Explicit implementation of ICache interface in order not to bloat up the namespace.
        /// Prefered way is to use generic Value.
        /// </summary>
        object ICache.ValueRaw { get { return value; } }

        /// <summary>
        /// Invalidate cache entry.
        /// </summary>
        public void InvalidateCache()
        {
            Cached = false;
        }
    }

    /// <summary>
    /// Helper class for caching results - it is being used as lazy evaluation
    /// </summary>
    /// <typeparam name="T">Type to be cached</typeparam>
    public struct SimpleCacheStruct<T> : ICache
    {
        /// <summary>
        /// The populate action
        /// </summary>
        private Func<T> populateAction;

        /// <summary>
        /// The value that is cached
        /// </summary>
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCacheStruct{T}"/> class.
        /// </summary>
        /// <param name="populateAction">The function that populates the cache on demand.</param>
        public SimpleCacheStruct(Func<T> populateAction)
        {
            this.populateAction = populateAction;
            value = default(T);
            Cached = false;
        }

        /// <summary>
        /// Gets a value indicating whether value is cached.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cached; otherwise, <c>false</c>.
        /// </value>
        public bool Cached { get; internal set; }

        /// <summary>
        /// Gets or sets the value. The value will be populated if it wasn't cached.
        /// </summary>
        public T Value
        {
            get
            {
                if (!Cached)
                {
                    value = populateAction();
                    Cached = true;
                }

                return value;
            }

            set
            {
                this.value = value;
                Cached = true;
            }
        }

        /// <summary>
        /// Explicit implementation of ICache interface in order not to bloat up the namespace.
        /// Prefered way is to use generic Value.
        /// </summary>
        object ICache.ValueRaw { get { return value; } }

        /// <summary>
        /// Invalidate cache entry.
        /// </summary>
        public void InvalidateCache()
        {
            Cached = false;
        }
    }
}
