using CsDebugScript.Exceptions;
using System;
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
    public class map<TKey, TValue> : UserType, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Common code for Microsoft Visual Studio implementations of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        public class VisualStudio : IReadOnlyDictionary<TKey, TValue>
        {
            /// <summary>
            /// std::map item
            /// </summary>
            protected class item
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
                /// The key/value pair
                /// </summary>
                private UserMember<pair<TKey, TValue>> pair;

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
                    pair = UserMember.Create(() => new pair<TKey, TValue>(variable.GetField("_Myval")));
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
                        return pair.Value.First;
                    }
                }

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public TValue Value
                {
                    get
                    {
                        return pair.Value.Second;
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
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="value">The value variable.</param>
            public VisualStudio(UserMember<Variable> value)
            {
                this.value = value;
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
            /// Gets the <c>TValue</c> with the specified key.
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
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        public class VisualStudio2013 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2013"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public VisualStudio2013(Variable variable)
                : base(UserMember.Create(() => variable))
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _Myhead
                // | _Parent
                // | _Left
                // | _Right
                // | _Isnil
                // | _Myval
                //   | first
                //   | second
                // _Mysize
                CodeType _Myhead, _Parent, _Left, _Right, _Myval, _Mysize, _Isnil;
                var fields = codeType.GetFieldTypes();

                if (!fields.TryGetValue("_Myhead", out _Myhead) || !fields.TryGetValue("_Mysize", out _Mysize))
                    return false;

                var _MyheadFields = _Myhead.GetFieldTypes();

                if (!_MyheadFields.TryGetValue("_Parent", out _Parent) || !_MyheadFields.TryGetValue("_Left", out _Left) || !_MyheadFields.TryGetValue("_Right", out _Right) || !_MyheadFields.TryGetValue("_Myval", out _Myval) || !_MyheadFields.TryGetValue("_Isnil", out _Isnil))
                    return false;

                if (!pair<TKey, TValue>.VerifyCodeType(_Myval))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        public class VisualStudio2015 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2015"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public VisualStudio2015(Variable variable)
                : base(UserMember.Create(() => variable.GetField("_Mypair").GetField("_Myval2").GetField("_Myval2")))
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
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
                CodeType _Mypair, _Myval2, _Myval22, _Myhead, _Parent, _Left, _Right, _Myval, _Mysize, _Isnil;

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

                if (!pair<TKey, TValue>.VerifyCodeType(_Myval))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        public class LibStdCpp6 : IReadOnlyDictionary<TKey, TValue>
        {
            /// <summary>
            /// std::map item
            /// </summary>
            protected class item
            {
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
                /// The key/value pair
                /// </summary>
                private UserMember<pair<TKey, TValue>> pair;

                /// <summary>
                /// Initializes a new instance of the <see cref="item" /> class.
                /// </summary>
                /// <param name="variable">The variable.</param>
                /// <param name="pairCodeType">The key value pair code type.</param>
                public item(Variable variable, CodeType pairCodeType)
                {
                    left = UserMember.Create(() =>
                    {
                        Variable field = variable.GetField("_M_left");

                        return field.IsNull() ? null : new item(field, pairCodeType);
                    });
                    right = UserMember.Create(() =>
                    {
                        Variable field = variable.GetField("_M_right");

                        return field.IsNull() ? null : new item(field, pairCodeType);
                    });
                    parent = UserMember.Create(() =>
                    {
                        Variable field = variable.GetField("_M_parent");

                        return field.IsNull() ? null : new item(field, pairCodeType);
                    });
                    pair = UserMember.Create(() =>
                    {
                        // Since we cannot get field with address to key value pair, do pointer arithmetic to find address
                        ulong pairAddress = variable.GetPointerAddress() + variable.GetCodeType().ElementType.Size;

                        return new pair<TKey, TValue>(Variable.Create(pairCodeType, pairAddress));
                    });
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
                        return pair.Value.First;
                    }
                }

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public TValue Value
                {
                    get
                    {
                        return pair.Value.Second;
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

            private CodeType pairCodeType;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public LibStdCpp6(Variable variable)
            {
                value = UserMember.Create(() =>
                {
                    Variable t = variable.GetField("_M_t");

                    pairCodeType = (CodeType)t.GetCodeType().TemplateArguments[1];
                    return t.GetField("_M_impl");
                });
                size = UserMember.Create(() => (int)Value.GetField("_M_node_count"));
                root = UserMember.Create(() => new item(Value.GetField("_M_header"), pairCodeType).Parent);
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
            /// Gets the <c>TValue</c> with the specified key.
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

                        yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
                        if (item.Right != null)
                        {
                            items.Push(item.Right);
                        }
                        if (item.Left != null)
                        {
                            items.Push(item.Left);
                        }
                    }
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _M_t
                // | _M_impl
                //   | _M_header
                //     | _M_color
                //     | _M_left
                //     | _M_parent
                //     | _M_right
                //     | ?_M_storage
                //       | first
                //       | second
                //   | _M_node_count
                //   | _M_key_compare
                CodeType _M_t, _M_impl, _M_header, _M_color, _M_left, _M_parent, _M_right, _M_node_count, _M_key_compare;

                if (!codeType.GetFieldTypes().TryGetValue("_M_t", out _M_t))
                    return false;
                if (!_M_t.GetFieldTypes().TryGetValue("_M_impl", out _M_impl))
                    return false;

                var _M_implFields = _M_impl.GetFieldTypes();

                if (!_M_implFields.TryGetValue("_M_header", out _M_header) || !_M_implFields.TryGetValue("_M_node_count", out _M_node_count) || !_M_implFields.TryGetValue("_M_key_compare", out _M_key_compare))
                    return false;

                var _M_headerFields = _M_header.GetFieldTypes();

                if (!_M_headerFields.TryGetValue("_M_color", out _M_color) || !_M_headerFields.TryGetValue("_M_left", out _M_left) || !_M_headerFields.TryGetValue("_M_parent", out _M_parent) || !_M_headerFields.TryGetValue("_M_right", out _M_right))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        public class ClangLibCpp : IReadOnlyDictionary<TKey, TValue>
        {
            /// <summary>
            /// std::map item
            /// </summary>
            protected class item
            {
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
                /// The key/value pair
                /// </summary>
                private UserMember<pair<TKey, TValue>> pair;

                /// <summary>
                /// Initializes a new instance of the <see cref="item" /> class.
                /// </summary>
                /// <param name="variable">The variable.</param>
                /// <param name="treeNodeCodeType">The tree node code type (it includes key value pair field).</param>
                public item(Variable variable, CodeType treeNodeCodeType)
                {
                    left = UserMember.Create(() =>
                    {
                        Variable field = variable.GetField("__left_");

                        return field.IsNull() ? null : new item(field, treeNodeCodeType);
                    });
                    right = UserMember.Create(() =>
                    {
                        Variable field = variable.GetField("__right_");

                        return field.IsNull() ? null : new item(field, treeNodeCodeType);
                    });
                    parent = UserMember.Create(() =>
                    {
                        Variable parent = variable.GetField("__parent_");

                        if (parent.IsNull())
                            return null;

                        Variable field = parent.GetField("__left_");

                        return field.IsNull() ? null : new item(field, treeNodeCodeType);
                    });
                    pair = UserMember.Create(() =>
                    {
                        Variable value = variable.CastAs(treeNodeCodeType).GetField("__value_").GetField("__nc");

                        return new pair<TKey, TValue>(value);
                    });
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
                        return pair.Value.First;
                    }
                }

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public TValue Value
                {
                    get
                    {
                        return pair.Value.Second;
                    }
                }
            }

            /// <summary>
            /// The internal tree field inside the std::map
            /// </summary>
            private UserMember<Variable> tree;

            /// <summary>
            /// The size of the map
            /// </summary>
            private UserMember<int> size;

            /// <summary>
            /// The root of the tree
            /// </summary>
            private UserMember<item> root;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClangLibCpp"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public ClangLibCpp(Variable variable)
            {
                tree = UserMember.Create(() => variable.GetField("__tree_"));
                size = UserMember.Create(() => (int)tree.Value.GetField("__pair3_").GetField("__value_"));
                root = UserMember.Create(() =>
                {
                    Variable root = tree.Value.GetField("__pair1_").GetField("__value_").GetField("__left_");
                    CodeType valueType = tree.Value.GetCodeType().TemplateArguments[0] as CodeType;
                    CodeType treeNodeBaseCodeType = root.GetCodeType().ElementType;
                    const string treeNodeBaseNameStart = "std::__1::__tree_node_base<";
                    const string treeNodeNameStart = "std::__1::__tree_node<";
                    string treeNodeName = treeNodeNameStart + valueType.Name + ", " + treeNodeBaseCodeType.Name.Substring(treeNodeBaseNameStart.Length);
                    CodeType treeNodeCodeType = CodeType.Create(treeNodeName, valueType.Module);
                    return new item(root, treeNodeCodeType).Parent;
                });
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
            /// Gets the <c>TValue</c> with the specified key.
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

                        yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
                        if (item.Right != null)
                        {
                            items.Push(item.Right);
                        }
                        if (item.Left != null)
                        {
                            items.Push(item.Left);
                        }
                    }
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // __tree_
                // | __begin_node_
                //   | __left_
                //     | __is_black_
                //     | __left_
                //     | __parent_
                //     | __right_
                // | __pair1_
                //   | __value_ (same type as __begin_node_)
                //     | __left_
                //       | __is_black_
                //       | __left_
                //       | __parent_
                //       | __right_
                // | __pair3_
                //   | __value_
                CodeType __tree_, __begin_node_, __left_, __is_black_, __left_2, __parent_, __right_, __pair1_, __value_, __pair3_, __value_2;

                if (!codeType.GetFieldTypes().TryGetValue("__tree_", out __tree_))
                    return false;
                if (!__tree_.GetFieldTypes().TryGetValue("__begin_node_", out __begin_node_) || !__tree_.GetFieldTypes().TryGetValue("__pair1_", out __pair1_) || !__tree_.GetFieldTypes().TryGetValue("__pair3_", out __pair3_))
                    return false;
                if (!__begin_node_.GetFieldTypes().TryGetValue("__left_", out __left_))
                    return false;
                if (!__left_.GetFieldTypes().TryGetValue("__is_black_", out __is_black_) || !__left_.GetFieldTypes().TryGetValue("__left_", out __left_2) || !__left_.GetFieldTypes().TryGetValue("__parent_", out __parent_) || !__left_.GetFieldTypes().TryGetValue("__right_", out __right_))
                    return false;
                if (!__pair1_.GetFieldTypes().TryGetValue("__value_", out __value_) || __value_ != __begin_node_.ElementType)
                    return false;
                if (!__pair3_.GetFieldTypes().TryGetValue("__value_", out __value_2))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IReadOnlyDictionary<TKey, TValue>> typeSelector = new TypeSelector<IReadOnlyDictionary<TKey, TValue>>(new[]
        {
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(LibStdCpp6), LibStdCpp6.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(ClangLibCpp), ClangLibCpp.VerifyCodeType),
        });

        /// <summary>
        /// Verifies that type user type can work with the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if user type can work with the specified code type; <c>false</c> otherwise</returns>
        public static bool VerifyCodeType(CodeType codeType)
        {
            return typeSelector.VerifyCodeType(codeType);
        }

        /// <summary>
        /// The instance used to read variable data
        /// </summary>
        private IReadOnlyDictionary<TKey, TValue> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="map{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public map(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::map");
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
        /// Gets the <c>TValue</c> with the specified key.
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{{ size={Count} }}";
        }
    }

    /// <summary>
    /// Simplification class for creating <see cref="map{TKey, TValue}"/> with TKey and TValue being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::map<>", CodeTypeVerification = nameof(map.VerifyCodeType))]
    public class map : map<Variable, Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="map"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public map(Variable variable)
            : base(variable)
        {
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
