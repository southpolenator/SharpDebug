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
        /// Common code for all implementations of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class MapBase : IReadOnlyDictionary<TKey, TValue>
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Function that reads size field.
                /// </summary>
                public Func<ulong, int> ReadSize;

                /// <summary>
                /// Offset of head field.
                /// </summary>
                public int HeadOffset;

                /// <summary>
                /// Flag indicating if head field is pointer.
                /// </summary>
                public bool HeadIsPointer;

                /// <summary>
                /// Flag indicating if head is at parent pointer.
                /// </summary>
                public bool HeadIsAtParent;

                /// <summary>
                /// Function that reads item's IsNil field.
                /// </summary>
                public Func<ulong, int> ReadItemIsNil;

                /// <summary>
                /// Offset of item's left field.
                /// </summary>
                public int ItemLeftOffset;

                /// <summary>
                /// Offset of item's right field.
                /// </summary>
                public int ItemRightOffset;

                /// <summary>
                /// Offset of item's parent field.
                /// </summary>
                public int ItemParentOffset;

                /// <summary>
                /// Offset of item's value field.
                /// </summary>
                public int ItemValueOffset;

                /// <summary>
                /// Code type of item's value field.
                /// </summary>
                public CodeType ItemValueCodeType;

                /// <summary>
                /// Code type of std::list.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

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
                /// <param name="address">The address of item.</param>
                /// <param name="data">Data returned from VerifyCodeType function.</param>
                public item(ulong address, ExtractedData data)
                {
                    isnil = UserMember.Create(() => data.ReadItemIsNil != null && data.ReadItemIsNil(address) != 0);
                    left = UserMember.Create(() =>
                    {
                        ulong leftAddress = data.Process.ReadPointer(address + (uint)data.ItemLeftOffset);

                        if (leftAddress == 0)
                            return null;
                        return new item(leftAddress, data);
                    });
                    right = UserMember.Create(() =>
                    {
                        ulong rightAddress = data.Process.ReadPointer(address + (uint)data.ItemRightOffset);

                        if (rightAddress == 0)
                            return null;
                        return new item(rightAddress, data);
                    });
                    parent = UserMember.Create(() =>
                    {
                        ulong parentAddress = data.Process.ReadPointer(address + (uint)data.ItemParentOffset);

                        if (parentAddress == 0)
                            return null;
                        return new item(parentAddress, data);
                    });
                    pair = UserMember.Create(() =>
                    {
                        ulong pairAddress = address + (uint)data.ItemValueOffset;
                        Variable pairVariable = Variable.Create(data.ItemValueCodeType, pairAddress);

                        return new pair<TKey, TValue>(pairVariable);
                    });
                }

                /// <summary>
                /// Gets a value indicating whether we have reached end of the tree.
                /// </summary>
                /// <value>
                ///   <c>true</c> if this instance has reached end of the tree; otherwise, <c>false</c>.
                /// </value>
                public bool IsNil => isnil.Value;

                /// <summary>
                /// Gets the left child item in the tree.
                /// </summary>
                public item Left => left.Value;

                /// <summary>
                /// Gets the right child item in the tree.
                /// </summary>
                public item Right => right.Value;

                /// <summary>
                /// Gets the parent item in the tree.
                /// </summary>
                public item Parent => parent.Value;

                /// <summary>
                /// Gets the key stored in the item.
                /// </summary>
                public TKey Key => pair.Value.First;

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public TValue Value => pair.Value.Second;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// The size of the map
            /// </summary>
            private int size;

            /// <summary>
            /// Initializes a new instance of the <see cref="MapBase"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public MapBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                size = data.ReadSize(address);
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count => size;

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
                if (Count > 0)
                {
                    ulong headAddress = address + (uint)data.HeadOffset;

                    if (data.HeadIsPointer)
                        headAddress = data.Process.ReadPointer(headAddress);
                    if (headAddress != 0)
                    {
                        item root = new item(headAddress, data);

                        if (data.HeadIsAtParent)
                            root = root.Parent;

                        if (root != null)
                        {
                            Stack<item> items = new Stack<item>();

                            items.Push(root);
                            while (items.Count > 0)
                            {
                                item item = items.Pop();

                                if (item != null && !item.IsNil)
                                {
                                    yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
                                    items.Push(item.Right);
                                    items.Push(item.Left);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class VisualStudio2013 : MapBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2013"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio2013(Variable variable, object savedData)
                : base(variable, savedData)
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
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
                    return null;

                var _MyheadFields = _Myhead.GetFieldTypes();

                if (!_MyheadFields.TryGetValue("_Parent", out _Parent) || !_MyheadFields.TryGetValue("_Left", out _Left) || !_MyheadFields.TryGetValue("_Right", out _Right) || !_MyheadFields.TryGetValue("_Myval", out _Myval) || !_MyheadFields.TryGetValue("_Isnil", out _Isnil))
                    return null;

                if (!pair<TKey, TValue>.VerifyCodeType(_Myval))
                    return null;

                return new ExtractedData
                {
                    ReadSize = codeType.Module.Process.GetReadInt(codeType, "_Mysize"),
                    HeadIsPointer = true,
                    HeadIsAtParent = true,
                    HeadOffset = codeType.GetFieldOffset("_Myhead"),
                    ItemLeftOffset = _Myhead.GetFieldOffset("_Left"),
                    ItemParentOffset = _Myhead.GetFieldOffset("_Parent"),
                    ItemRightOffset = _Myhead.GetFieldOffset("_Right"),
                    ItemValueOffset = _Myhead.GetFieldOffset("_Myval"),
                    ItemValueCodeType = _Myval,
                    ReadItemIsNil = codeType.Module.Process.GetReadInt(_Myhead, "_Isnil"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class VisualStudio2015 : MapBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2015"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio2015(Variable variable, object savedData)
                : base(variable, savedData)
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
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
                    return null;
                if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                    return null;
                if (!_Myval2.GetFieldTypes().TryGetValue("_Myval2", out _Myval22))
                    return null;

                var _Myval2Fields = _Myval22.GetFieldTypes();

                if (!_Myval2Fields.TryGetValue("_Myhead", out _Myhead) || !_Myval2Fields.TryGetValue("_Mysize", out _Mysize))
                    return null;

                var _MyheadFields = _Myhead.GetFieldTypes();

                if (!_MyheadFields.TryGetValue("_Parent", out _Parent) || !_MyheadFields.TryGetValue("_Left", out _Left) || !_MyheadFields.TryGetValue("_Right", out _Right) || !_MyheadFields.TryGetValue("_Myval", out _Myval) || !_MyheadFields.TryGetValue("_Isnil", out _Isnil))
                    return null;

                if (!pair<TKey, TValue>.VerifyCodeType(_Myval))
                    return null;

                return new ExtractedData
                {
                    ReadSize = codeType.Module.Process.GetReadInt(codeType, "_Mypair._Myval2._Myval2._Mysize"),
                    HeadIsPointer = true,
                    HeadIsAtParent = true,
                    HeadOffset = codeType.GetFieldOffset("_Mypair") + _Mypair.GetFieldOffset("_Myval2") + _Myval2.GetFieldOffset("_Myval2") + _Myval22.GetFieldOffset("_Myhead"),
                    ItemLeftOffset = _Myhead.GetFieldOffset("_Left"),
                    ItemParentOffset = _Myhead.GetFieldOffset("_Parent"),
                    ItemRightOffset = _Myhead.GetFieldOffset("_Right"),
                    ItemValueOffset = _Myhead.GetFieldOffset("_Myval"),
                    ItemValueCodeType = _Myval,
                    ReadItemIsNil = codeType.Module.Process.GetReadInt(_Myhead, "_Isnil"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class LibStdCpp6 : MapBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public LibStdCpp6(Variable variable, object savedData)
                : base(variable, savedData)
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
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
                    return null;
                if (!_M_t.GetFieldTypes().TryGetValue("_M_impl", out _M_impl))
                    return null;

                var _M_implFields = _M_impl.GetFieldTypes();

                if (!_M_implFields.TryGetValue("_M_header", out _M_header) || !_M_implFields.TryGetValue("_M_node_count", out _M_node_count) || !_M_implFields.TryGetValue("_M_key_compare", out _M_key_compare))
                    return null;

                var _M_headerFields = _M_header.GetFieldTypes();

                if (!_M_headerFields.TryGetValue("_M_color", out _M_color) || !_M_headerFields.TryGetValue("_M_left", out _M_left) || !_M_headerFields.TryGetValue("_M_parent", out _M_parent) || !_M_headerFields.TryGetValue("_M_right", out _M_right))
                    return null;

                CodeType pairCodeType;
                try
                {
                    pairCodeType = (CodeType)_M_t.TemplateArguments[1];
                }
                catch
                {
                    return null;
                }

                if (!pair<TKey, TValue>.VerifyCodeType(pairCodeType))
                    return null;

                return new ExtractedData
                {
                    ReadSize = codeType.Module.Process.GetReadInt(codeType, "_M_t._M_impl._M_node_count"),
                    HeadIsPointer = false,
                    HeadIsAtParent = true,
                    HeadOffset = codeType.GetFieldOffset("_M_t") + _M_t.GetFieldOffset("_M_impl") + _M_impl.GetFieldOffset("_M_header"),
                    ItemLeftOffset = _M_header.GetFieldOffset("_M_left"),
                    ItemParentOffset = _M_header.GetFieldOffset("_M_parent"),
                    ItemRightOffset = _M_header.GetFieldOffset("_M_right"),
                    ItemValueOffset = (int)_M_header.ElementType.Size,
                    ItemValueCodeType = pairCodeType,
                    ReadItemIsNil = null,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class ClangLibCpp : MapBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClangLibCpp"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public ClangLibCpp(Variable variable, object savedData)
                : base(variable, savedData)
            {
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
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
                    return null;
                if (!__tree_.GetFieldTypes().TryGetValue("__begin_node_", out __begin_node_) || !__tree_.GetFieldTypes().TryGetValue("__pair1_", out __pair1_) || !__tree_.GetFieldTypes().TryGetValue("__pair3_", out __pair3_))
                    return null;
                if (!__begin_node_.GetFieldTypes().TryGetValue("__left_", out __left_))
                    return null;
                if (!__left_.GetFieldTypes().TryGetValue("__is_black_", out __is_black_) || !__left_.GetFieldTypes().TryGetValue("__left_", out __left_2) || !__left_.GetFieldTypes().TryGetValue("__parent_", out __parent_) || !__left_.GetFieldTypes().TryGetValue("__right_", out __right_))
                    return null;
                if (!__pair1_.GetFieldTypes().TryGetValue("__value_", out __value_) || __value_ != __begin_node_.ElementType)
                    return null;
                if (!__pair3_.GetFieldTypes().TryGetValue("__value_", out __value_2))
                    return null;

                CodeType pairCodeType, treeNodeCodeType;

                try
                {
                    CodeType valueCodeType = (CodeType)__tree_.TemplateArguments[0];
                    pairCodeType = valueCodeType.GetFieldType("__nc");
                    CodeType treeNodeBaseCodeType = __left_.ElementType;
                    const string treeNodeBaseNameStart = "std::__1::__tree_node_base<";
                    const string treeNodeNameStart = "std::__1::__tree_node<";
                    string treeNodeName = treeNodeNameStart + valueCodeType.Name + ", " + treeNodeBaseCodeType.Name.Substring(treeNodeBaseNameStart.Length);
                    treeNodeCodeType = CodeType.Create(treeNodeName, valueCodeType.Module);
                }
                catch
                {
                    return null;
                }

                if (!pair<TKey, TValue>.VerifyCodeType(pairCodeType))
                    return null;

                return new ExtractedData
                {
                    ReadSize = codeType.Module.Process.GetReadInt(codeType, "__tree_.__pair3_.__value_"),
                    HeadIsPointer = true,
                    HeadIsAtParent = false,
                    HeadOffset = codeType.GetFieldOffset("__tree_") + __tree_.GetFieldOffset("__pair1_") + __pair1_.GetFieldOffset("__value_") + __value_.GetFieldOffset("__left_"),
                    ItemLeftOffset = treeNodeCodeType.GetFieldOffset("__left_"),
                    ItemParentOffset = treeNodeCodeType.GetFieldOffset("__parent_"),
                    ItemRightOffset = treeNodeCodeType.GetFieldOffset("__right_"),
                    ItemValueOffset = treeNodeCodeType.GetFieldOffset("__value_") + treeNodeCodeType.GetFieldType("__value_").GetFieldOffset("__nc"),
                    ItemValueCodeType = pairCodeType,
                    ReadItemIsNil = null,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IReadOnlyDictionary<TKey, TValue>> typeSelector = new TypeSelector<IReadOnlyDictionary<TKey, TValue>>(new[]
        {
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp6), LibStdCpp6.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(ClangLibCpp), ClangLibCpp.VerifyCodeType),
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
    [UserType(TypeName = "std::__1::map<>", CodeTypeVerification = nameof(map.VerifyCodeType))]
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
