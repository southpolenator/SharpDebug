using System;
using System.Collections;

namespace CsDebugScript.Engine.Utility
{
    /// <summary>
    /// Helper class for caching objects inside the array. New object will be cached on the request.
    /// </summary>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public class ArrayCache<TValue> : ICache
        where TValue : class
    {
        /// <summary>
        /// The populate action
        /// </summary>
        private Func<int, TValue> populateAction;

        /// <summary>
        /// The cached values
        /// </summary>
        private TValue[] values;

        /// <summary>
        /// Disables locking during value population.
        /// </summary>
        private bool disableLocking;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayCache{TValue}"/> class.
        /// </summary>
        /// <param name="length">Length of the array.</param>
        /// <param name="populateAction">The populate action.</param>
        public ArrayCache(int length, Func<int, TValue> populateAction)
            : this(length, false, populateAction)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayCache{TValue}"/> class.
        /// </summary>
        /// <param name="length">Length of the array.</param>
        /// <param name="disableLocking">Disables locking during value population.</param>
        /// <param name="populateAction">The populate action.</param>
        public ArrayCache(int length, bool disableLocking, Func<int, TValue> populateAction)
        {
            this.populateAction = populateAction;
            this.disableLocking = disableLocking;
            values = new TValue[length];
        }

        /// <summary>
        /// Clears this cache.
        /// </summary>
        public void Clear()
        {
            values = new TValue[values.Length];
        }

        /// <summary>
        /// Gets the number of entries inside the cache.
        /// </summary>
        public int Count
        {
            get
            {
                return values.Length;
            }
        }

        /// <summary>
        /// Gets or sets the &lt;TValue&gt; with the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public TValue this[int index]
        {
            get
            {
                TValue value = values[index];

                if (value == null)
                    if (disableLocking)
                        value = values[index] = populateAction(index);
                    else
                        lock (values)
                        {
                            value = values[index];
                            if (value == null)
                                values[index] = value = populateAction(index);
                        }

                return value;
            }

            set
            {
                values[index] = value;
            }
        }

        /// <summary>
        /// Returns all cached values in this cache.
        /// </summary>
        /// <returns>IEnumerator of all the cache values.</returns>
        public IEnumerator GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Invalidates this cache.
        /// </summary>
        public void InvalidateCache()
        {
            Clear();
        }
    }
}
