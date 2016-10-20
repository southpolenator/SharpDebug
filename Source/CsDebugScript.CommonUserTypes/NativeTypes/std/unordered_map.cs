using CsDebugScript.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::unordered_map
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class unordered_map<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Common code for Microsoft Visual Studio implementations of std::unordered_map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        public class VisualStudio : IReadOnlyDictionary<TKey, TValue>
        {
            /// <summary>
            /// The list
            /// </summary>
            UserMember<list<pair<TKey, TValue>>> list;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public VisualStudio(Variable variable)
            {
                list = UserMember.Create(() => new list<pair<TKey, TValue>>(variable.GetField("_List")));
            }

            /// <summary>
            /// Gets the <see cref="TValue"/> with the specified key.
            /// </summary>
            /// <param name="key">The key to locate.</param>
            /// <returns>The element that has the specified key in the read-only dictionary.</returns>
            /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and key is not found.</exception>
            public TValue this[TKey key]
            {
                get
                {
                    TValue value;

                    if (!TryGetValue(key, out value))
                    {
                        throw new KeyNotFoundException();
                    }

                    return value;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    return List.Count;
                }
            }

            /// <summary>
            /// Gets an enumerable collection that contains the keys in the read-only dictionary.
            /// </summary>
            public IEnumerable<TKey> Keys
            {
                get
                {
                    return Enumerate().Select(kvp => kvp.Key);
                }
            }

            /// <summary>
            /// Gets an enumerable collection that contains the values in the read-only dictionary.
            /// </summary>
            public IEnumerable<TValue> Values
            {
                get
                {
                    return Enumerate().Select(kvp => kvp.Value);
                }
            }

            /// <summary>
            /// Gets the list.
            /// </summary>
            private list<pair<TKey, TValue>> List
            {
                get
                {
                    return list.Value;
                }
            }

            /// <summary>
            /// Determines whether the read-only dictionary contains an element that has the specified key.
            /// </summary>
            /// <param name="key">The key to locate.</param>
            /// <returns>
            /// true if the read-only dictionary contains an element that has the specified key; otherwise, false.
            /// </returns>
            public bool ContainsKey(TKey key)
            {
                TValue value;

                return TryGetValue(key, out value);
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Gets the value that is associated with the specified key.
            /// </summary>
            /// <param name="key">The key to locate.</param>
            /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
            /// <returns>
            /// true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" /> interface contains an element that has the specified key; otherwise, false.
            /// </returns>
            public bool TryGetValue(TKey key, out TValue value)
            {
                // As we don't have comparer (for sorted tree), we will need to scan all items
                foreach (KeyValuePair<TKey, TValue> kvp in Enumerate())
                {
                    if (kvp.Key.Equals(key))
                    {
                        value = kvp.Value;
                        return true;
                    }
                }

                value = default(TValue);
                return false;
            }

            /// <summary>
            /// Enumerates this unordered_map.
            /// </summary>
            private IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
            {
                foreach (pair<TKey, TValue> item in List)
                {
                    yield return new KeyValuePair<TKey, TValue>(item.First, item.Second);
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _List
                CodeType _List;

                if (!codeType.GetFieldTypes().TryGetValue("_List", out _List))
                    return false;

                if (!list<pair<TKey, TValue>>.VerifyCodeType(_List))
                    return false;

                // TODO: We should also verify list item type

                return true;
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IReadOnlyDictionary<TKey, TValue>> typeSelector = new TypeSelector<IReadOnlyDictionary<TKey, TValue>>(new[]
        {
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio), VisualStudio.VerifyCodeType),
        });

        /// <summary>
        /// The instance used to read variable data
        /// </summary>
        private IReadOnlyDictionary<TKey, TValue> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="unordered_map{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public unordered_map(Variable variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::unordered_map");
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                return instance.Keys;
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                return instance.Values;
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return instance.Count;
            }
        }

        /// <summary>
        /// Gets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and key is not found.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return instance[key];
            }
        }

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        /// true if the read-only dictionary contains an element that has the specified key; otherwise, false.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            return instance.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2" /> interface contains an element that has the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return instance.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return instance.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return instance.GetEnumerator();
        }
    }
}
