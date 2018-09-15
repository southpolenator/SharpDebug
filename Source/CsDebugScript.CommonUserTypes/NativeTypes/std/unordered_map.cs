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
    public class unordered_map<TKey, TValue> : UserType, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Microsoft Visual Studio implementation of std::unordered_map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class VisualStudio : IReadOnlyDictionary<TKey, TValue>
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Offset of list field.
                /// </summary>
                public int ListOffset;

                /// <summary>
                /// Code type of list field.
                /// </summary>
                public CodeType ListCodeType;

                /// <summary>
                /// Code type of std::unordered_map.
                /// </summary>
                public CodeType CodeType;
            }

            /// <summary>
            /// The list
            /// </summary>
            UserMember<list<pair<TKey, TValue>>> list;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio(Variable variable, object savedData)
            {
                ExtractedData data = (ExtractedData)savedData;
                ulong address = variable.GetPointerAddress();

                list = UserMember.Create(() => new list<pair<TKey, TValue>>(Variable.Create(data.ListCodeType, address + (uint)data.ListOffset)));
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
                        throw new KeyNotFoundException();
                    return value;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count => List.Count;

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
            private list<pair<TKey, TValue>> List => list.Value;

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
                    if (kvp.Key.Equals(key))
                    {
                        value = kvp.Value;
                        return true;
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
                    yield return new KeyValuePair<TKey, TValue>(item.First, item.Second);
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _List
                CodeType _List;

                if (!codeType.GetFieldTypes().TryGetValue("_List", out _List))
                    return null;
                if (!list<pair<TKey, TValue>>.VerifyCodeType(_List))
                    return null;
                return new ExtractedData
                {
                    ListOffset = codeType.GetFieldOffset("_List"),
                    ListCodeType = _List,
                    CodeType = codeType,
                };
            }
        }

        /// <summary>
        /// Common code for all implementations of std::unordered_map
        /// </summary>
        internal class UnorderedMapBase : IReadOnlyDictionary<TKey, TValue>
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Function that reads element count field.
                /// </summary>
                public Func<ulong, int> ReadElementCount;

                /// <summary>
                /// Offset of before first element field.
                /// </summary>
                public int BeforeFirstElementOffset;

                /// <summary>
                /// Offset of item's next field.
                /// </summary>
                public int ItemNextOffset;

                /// <summary>
                /// Offset of item's value field.
                /// </summary>
                public int ItemValueOffset;

                /// <summary>
                /// Code type of item's value field.
                /// </summary>
                public CodeType ItemValueCodeType;

                /// <summary>
                /// Code type of std::unordered_map.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
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
            /// The number of elements in the map.
            /// </summary>
            private int elementCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="UnorderedMapBase"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public UnorderedMapBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                elementCount = data.ReadElementCount(address);
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
                        throw new KeyNotFoundException();
                    return value;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count => elementCount;

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
                // As we don't have hash function, we will need to scan all items
                foreach (KeyValuePair<TKey, TValue> kvp in Enumerate())
                    if (kvp.Key.Equals(key))
                    {
                        value = kvp.Value;
                        return true;
                    }

                value = default(TValue);
                return false;
            }

            /// <summary>
            /// Enumerates this unordered_map.
            /// </summary>
            private IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
            {
                ulong beforeFirstElementAddress = address + (uint)data.BeforeFirstElementOffset;
                ulong elementAddress = data.Process.ReadPointer(beforeFirstElementAddress + (uint)data.ItemNextOffset);

                while (elementAddress != 0)
                {
                    ulong valueAddress = elementAddress + (uint)data.ItemValueOffset;
                    pair<TKey, TValue> value = new pair<TKey, TValue>(Variable.Create(data.ItemValueCodeType, valueAddress));

                    yield return new KeyValuePair<TKey, TValue>(value.First, value.Second);
                    elementAddress = data.Process.ReadPointer(elementAddress + (uint)data.ItemNextOffset);
                }
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::unordered_map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class LibStdCpp6 : UnorderedMapBase
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
                // _M_h
                // | _M_before_begin
                //   | _M_nxt
                // | _M_bucket_count
                // | _M_buckets
                // | _M_element_count
                // | _M_rehash_policy
                //   | _M_max_load_factor
                //   | _M_next_resize
                // | _M_single_bucket
                CodeType _M_h, _M_before_begin, _M_nxt, _M_bucket_count, _M_buckets, _M_element_count, _M_rehash_policy, _M_max_load_factor, _M_next_resize, _M_single_bucket;

                if (!codeType.GetFieldTypes().TryGetValue("_M_h", out _M_h))
                    return null;

                var _M_hFields = _M_h.GetFieldTypes();

                if (!_M_hFields.TryGetValue("_M_before_begin", out _M_before_begin) || !_M_hFields.TryGetValue("_M_bucket_count", out _M_bucket_count) || !_M_hFields.TryGetValue("_M_buckets", out _M_buckets) || !_M_hFields.TryGetValue("_M_element_count", out _M_element_count) || !_M_hFields.TryGetValue("_M_rehash_policy", out _M_rehash_policy) || !_M_hFields.TryGetValue("_M_single_bucket", out _M_single_bucket))
                    return null;
                if (!_M_before_begin.GetFieldTypes().TryGetValue("_M_nxt", out _M_nxt))
                    return null;

                var _M_rehash_policyFields = _M_rehash_policy.GetFieldTypes();

                if (!_M_rehash_policyFields.TryGetValue("_M_max_load_factor", out _M_max_load_factor) || !_M_rehash_policyFields.TryGetValue("_M_next_resize", out _M_next_resize))
                    return null;

                CodeType valueCodeType;

                try
                {
                    valueCodeType = (CodeType)_M_h.TemplateArguments[1];
                }
                catch
                {
                    return null;
                }
                if (!pair<TKey, TValue>.VerifyCodeType(valueCodeType))
                    return null;

                return new ExtractedData
                {
                    ReadElementCount = codeType.Module.Process.GetReadInt(codeType, "_M_h._M_element_count"),
                    BeforeFirstElementOffset = codeType.GetFieldOffset("_M_h") + _M_h.GetFieldOffset("_M_before_begin"),
                    ItemNextOffset = _M_nxt.ElementType.GetFieldOffset("_M_nxt"),
                    ItemValueOffset = (int)_M_nxt.ElementType.Size,
                    ItemValueCodeType = valueCodeType,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::unordered_map
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
        internal class ClangLibCpp : UnorderedMapBase
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
                // __table_
                // | __bucket_list_
                //   | __ptr_
                // | __p1_
                //   | __value_
                //     | __next_
                // | __p2_
                //   | __value_
                // | __p3_
                //   | __value_
                CodeType __table_, __bucket_list_, __ptr_, __p1_, __value_, __next_, __p2_, __value_2, __p3_, __value_3;

                if (!codeType.GetFieldTypes().TryGetValue("__table_", out __table_))
                    return null;
                if (!__table_.GetFieldTypes().TryGetValue("__bucket_list_", out __bucket_list_) || !__table_.GetFieldTypes().TryGetValue("__p1_", out __p1_) || !__table_.GetFieldTypes().TryGetValue("__p2_", out __p2_) || !__table_.GetFieldTypes().TryGetValue("__p3_", out __p3_))
                    return null;
                if (!__bucket_list_.GetFieldTypes().TryGetValue("__ptr_", out __ptr_))
                    return null;
                if (!__p1_.GetFieldTypes().TryGetValue("__value_", out __value_))
                    return null;
                if (!__value_.GetFieldTypes().TryGetValue("__next_", out __next_))
                    return null;
                if (!__p2_.GetFieldTypes().TryGetValue("__value_", out __value_2))
                    return null;
                if (!__p3_.GetFieldTypes().TryGetValue("__value_", out __value_3))
                    return null;

                CodeType hashNodeCodeType, valueCodeType;

                try
                {
                    hashNodeCodeType = __next_.ElementType.TemplateArguments[0] as CodeType;
                    valueCodeType = hashNodeCodeType.GetFieldType("__value_").GetFieldType("__nc");
                }
                catch
                {
                    return null;
                }
                if (!pair<TKey, TValue>.VerifyCodeType(valueCodeType))
                    return null;

                return new ExtractedData
                {
                    ReadElementCount = codeType.Module.Process.GetReadInt(codeType, "__table_.__p2_.__value_"),
                    BeforeFirstElementOffset = codeType.GetFieldOffset("__table_") + __table_.GetFieldOffset("__p1_") + __p1_.GetFieldOffset("__value_"),
                    ItemNextOffset = hashNodeCodeType.GetFieldOffset("__next_"),
                    ItemValueCodeType = valueCodeType,
                    ItemValueOffset = hashNodeCodeType.GetFieldOffset("__value_") + hashNodeCodeType.GetFieldType("__value_").GetFieldOffset("__nc"),
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
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio), VisualStudio.VerifyCodeType),
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
        /// Initializes a new instance of the <see cref="unordered_map{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public unordered_map(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::unordered_map");
            }

            var a = System.Linq.Enumerable.ToArray(this);
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
    /// Simplification class for creating <see cref="unordered_map{TKey, TValue}"/> with TKey and TValue being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::unordered_map<>", CodeTypeVerification = nameof(unordered_map.VerifyCodeType))]
    [UserType(TypeName = "std::__1::unordered_map<>", CodeTypeVerification = nameof(unordered_map.VerifyCodeType))]
    public class unordered_map : unordered_map<Variable, Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="unordered_map"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public unordered_map(Variable variable)
            : base(variable)
        {
        }
    }
}
