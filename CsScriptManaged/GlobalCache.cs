using System;
using System.Collections.Generic;

namespace CsScriptManaged
{
    /// <summary>
    /// Helper class for caching global objects.
    /// </summary>
    public class GlobalCache
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SimpleCache{TKey, TValue}"/> class.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="populateAction">The populate action.</param>
        public static GlobalCache<TKey, TValue> Create<TKey, TValue>(Func<TKey, TValue> populateAction)
        {
            return new GlobalCache<TKey, TValue>(populateAction);
        }
    }

    /// <summary>
    /// Helper class for caching global objects.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class GlobalCache<TKey, TValue>
    {
        /// <summary>
        /// The populate action
        /// </summary>
        private Func<TKey, TValue> populateAction;

        /// <summary>
        /// The cached values
        /// </summary>
        private Dictionary<TKey, TValue> values = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="populateAction">The populate action.</param>
        public GlobalCache(Func<TKey, TValue> populateAction)
        {
            this.populateAction = populateAction;
        }

        /// <summary>
        /// Clears this cache.
        /// </summary>
        public void Clear()
        {
            values.Clear();
        }

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;

                if (!values.TryGetValue(key, out value))
                {
                    value = populateAction(key);
                    values.Add(key, value);
                }

                return value;
            }

            set
            {
                values[key] = value;
            }
        }
    }
}
