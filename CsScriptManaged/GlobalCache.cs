using CsScripts;
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
        /// The processes
        /// </summary>
        public static GlobalCache<Tuple<uint, uint>, Process> Processes = new GlobalCache<Tuple<uint, uint>, Process>(CreateProcess);

        /// <summary>
        /// Creates the process.
        /// </summary>
        /// <param name="processKey">The process key.</param>
        private static Process CreateProcess(Tuple<uint, uint> processKey)
        {
            return new Process(processKey.Item1, processKey.Item2);
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
