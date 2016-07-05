using System;

namespace CsDebugScript.Engine.Utility
{
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
    }

    /// <summary>
    /// Helper class for caching results - it is being used as lazy evaluation
    /// </summary>
    /// <typeparam name="T">Type to be cached</typeparam>
    public class SimpleCache<T>
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
                // value = populateAction();
                // return value;

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
    public struct SimpleCacheStruct<T>
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
        /// Invalidate cache entry.
        /// </summary>
        public void InvalidateCache()
        {
            Cached = false;
        }
    }
}
