using CsDebugScript.Exceptions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::map
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class map<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// std::map item
        /// </summary>
        private class item
        {
            /// <summary>
            /// Flag that indicates that we have reached end of the tree
            /// </summary>
            private UserMember<bool> isnil;

            /// <summary>
            /// The left item
            /// </summary>
            private UserMember<item> left;

            /// <summary>
            /// The right item
            /// </summary>
            private UserMember<item> right;

            /// <summary>
            /// The parent item
            /// </summary>
            private UserMember<item> parent;

            /// <summary>
            /// The key
            /// </summary>
            private UserMember<TKey> key;

            /// <summary>
            /// The value
            /// </summary>
            private UserMember<TValue> value;

            /// <summary>
            /// Initializes a new instance of the <see cref="item"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public item(Variable variable)
            {
                isnil = UserMember.Create(() => (bool)variable.GetField("_Isnil"));
                left = UserMember.Create(() => new item(variable.GetField("_Left")));
                right = UserMember.Create(() => new item(variable.GetField("_Right")));
                parent = UserMember.Create(() => new item(variable.GetField("_Parent")));
                key = UserMember.Create(() => variable.GetField("_Myval").GetField("first").CastAs<TKey>());
                value = UserMember.Create(() => variable.GetField("_Myval").GetField("second").CastAs<TValue>());
            }

            /// <summary>
            /// Gets a value indicating whether we have reached end of the tree.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has reached end of the tree; otherwise, <c>false</c>.
            /// </value>
            public bool IsNil
            {
                get
                {
                    return isnil.Value;
                }
            }

            /// <summary>
            /// Gets the left child item in the tree.
            /// </summary>
            public item Left
            {
                get
                {
                    return left.Value;
                }
            }

            /// <summary>
            /// Gets the right child item in the tree.
            /// </summary>
            public item Right
            {
                get
                {
                    return right.Value;
                }
            }

            /// <summary>
            /// Gets the parent item in the tree.
            /// </summary>
            public item Parent
            {
                get
                {
                    return parent.Value;
                }
            }

            /// <summary>
            /// Gets the key stored in the item.
            /// </summary>
            public TKey Key
            {
                get
                {
                    return key.Value;
                }
            }

            /// <summary>
            /// Gets the value stored in the item.
            /// </summary>
            public TValue Value
            {
                get
                {
                    return value.Value;
                }
            }
        }

        /// <summary>
        /// The internal value field inside the std::map
        /// </summary>
        private UserMember<Variable> value;

        /// <summary>
        /// The size of the map
        /// </summary>
        private UserMember<int> size;

        /// <summary>
        /// The root of the tree
        /// </summary>
        private UserMember<item> root;

        /// <summary>
        /// Initializes a new instance of the <see cref="map{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public map(Variable variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::map");
            }

            // Initialize members
            value = UserMember.Create(() => variable.GetField("_Mypair").GetField("_Myval2").GetField("_Myval2"));
            size = UserMember.Create(() => (int)Value.GetField("_Mysize"));
            root = UserMember.Create(() => new item(Value.GetField("_Myhead")).Parent);
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return size.Value;
            }
        }

        /// <summary>
        /// Gets the internal value field inside the std::map
        /// </summary>
        private item Root
        {
            get
            {
                return root.Value;
            }
        }

        /// <summary>
        /// Gets the internal value field inside the std::map
        /// </summary>
        private Variable Value
        {
            get
            {
                return value.Value;
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
                TValue value;

                if (!TryGetValue(key, out value))
                {
                    throw new KeyNotFoundException();
                }

                return value;
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
        /// Enumerates this map.
        /// </summary>
        private IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
        {
            if (Count > 0 && Root != null)
            {
                Stack<item> items = new Stack<item>();

                items.Push(Root);
                while (items.Count > 0)
                {
                    item item = items.Pop();

                    if (!item.IsNil)
                    {
                        yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
                        items.Push(item.Right);
                        items.Push(item.Left);
                    }
                }
            }
        }

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private static bool VerifyCodeType(CodeType codeType)
        {
            // We want to have this kind of hierarchy
            // _Mypair
            // | _Myval2
            //   | _Myval2
            //     | _Myhead
            //       | _Parent
            //       | _Left
            //       | _Right
            //       | _Isnil
            //       | _Myval
            //         | first
            //         | second
            //     | _Mysize
            CodeType _Mypair, _Myval2, _Myval22, _Myhead, _Parent, _Left, _Right, _Myval, _Mysize, _Isnil, first, second;

            if (!codeType.GetFieldTypes().TryGetValue("_Mypair", out _Mypair))
                return false;
            if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                return false;
            if (!_Myval2.GetFieldTypes().TryGetValue("_Myval2", out _Myval22))
                return false;

            var _Myval2Fields = _Myval22.GetFieldTypes();

            if (!_Myval2Fields.TryGetValue("_Myhead", out _Myhead) || !_Myval2Fields.TryGetValue("_Mysize", out _Mysize))
                return false;

            var _MyheadFields = _Myhead.GetFieldTypes();

            if (!_MyheadFields.TryGetValue("_Parent", out _Parent) || !_MyheadFields.TryGetValue("_Left", out _Left) || !_MyheadFields.TryGetValue("_Right", out _Right) || !_MyheadFields.TryGetValue("_Myval", out _Myval) || !_MyheadFields.TryGetValue("_Isnil", out _Isnil))
                return false;

            // TODO: When we add std::pair support, use it...
            if (!_Myval.GetFieldTypes().TryGetValue("first", out first) || !_Myval.GetFieldTypes().TryGetValue("second", out second))
                return false;

            return true;
        }
    }
}

namespace System.Linq
{
    /// <summary>
    /// Dictionary extensions for map
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Converts dictionary to dictionary that has string as key type.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>New dictionary that has string as key type</returns>
        public static IReadOnlyDictionary<string, TValue> ToStringDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            return dictionary.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
        }

        /// <summary>
        /// Converts dictionary to dictionary that has string as both key and value type.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>Dictionary that has string as both key and value type</returns>
        public static IReadOnlyDictionary<string, string> ToStringStringDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            return dictionary.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString());
        }
    }
}
