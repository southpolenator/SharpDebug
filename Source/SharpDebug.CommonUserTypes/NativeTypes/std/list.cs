using SharpDebug.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpDebug.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class list<T> : UserType, IReadOnlyCollection<T>
    {
        /// <summary>
        /// Common code for all implementations of std::list
        /// </summary>
        internal class ListBase : IReadOnlyCollection<T>
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Function that reads length field.
                /// </summary>
                public Func<ulong, int> ReadLength;

                /// <summary>
                /// Offset of head field.
                /// </summary>
                public int HeadOffset;

                /// <summary>
                /// Flag indicating whether head item has meaningful value.
                /// </summary>
                public bool HeadHasValue;

                /// <summary>
                /// Offset of item's next field.
                /// </summary>
                public int ItemNextOffset;

                /// <summary>
                /// Offset of item's previous field.
                /// </summary>
                public int ItemPreviousOffset;

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
            /// std::list item
            /// </summary>
            private class item
            {
                /// <summary>
                /// The next item
                /// </summary>
                private UserMember<item> next;

                /// <summary>
                /// The previous item
                /// </summary>
                private UserMember<item> previous;

                /// <summary>
                /// The value
                /// </summary>
                private UserMember<T> value;

                /// <summary>
                /// Initializes a new instance of the <see cref="item"/> class.
                /// </summary>
                /// <param name="address">The address of item.</param>
                /// <param name="data">Data returned from VerifyCodeType function.</param>
                public item(ulong address, ExtractedData data)
                {
                    next = UserMember.Create(() =>
                    {
                        ulong nextAddress = data.Process.ReadPointer(address + (uint)data.ItemNextOffset);

                        return new item(nextAddress, data);
                    });
                    previous = UserMember.Create(() =>
                    {
                        ulong previousAddress = data.Process.ReadPointer(address + (uint)data.ItemPreviousOffset);

                        return new item(previousAddress, data);
                    });
                    value = UserMember.Create(() =>
                    {
                        ulong valueAddress = address + (uint)data.ItemValueOffset;

                        return Variable.Create(data.ItemValueCodeType, valueAddress).CastAs<T>();
                    });
                }

                /// <summary>
                /// Gets the next item in the list.
                /// </summary>
                public item Next => next.Value;

                /// <summary>
                /// Gets the previous item in the list.
                /// </summary>
                public item Previous => previous.Value;

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public T Value => value.Value;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// The head of the list address.
            /// </summary>
            private ulong headAddress;

            /// <summary>
            /// The list length.
            /// </summary>
            private int length;

            /// <summary>
            /// Initializes a new instance of the <see cref="ListBase"/> class.
            /// </summary>
            /// <param name="variable">The value.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public ListBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                ulong address = variable.GetPointerAddress();
                length = data.ReadLength(address);
                headAddress = data.Process.ReadPointer(address + (uint)data.HeadOffset);
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count => length;

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            public IEnumerator<T> GetEnumerator()
            {
                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Enumerates this list.
            /// </summary>
            private IEnumerable<T> Enumerate()
            {
                item item = new item(headAddress, data);

                if (!data.HeadHasValue)
                    item = item.Next;
                for (int i = 0; i < Count; i++)
                {
                    yield return item.Value;
                    item = item.Next;
                }
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::list
        /// </summary>
        internal class VisualStudio2013 : ListBase
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
                // | _Next
                // | _Prev
                // | _Myval
                // _Mysize
                CodeType _Myhead, _Next, Prev, _Myval, _Mysize;

                var fields = codeType.GetFieldTypes();

                if (!fields.TryGetValue("_Myhead", out _Myhead) || !fields.TryGetValue("_Mysize", out _Mysize))
                    return null;

                var _MyheadFields = _Myhead.GetFieldTypes();

                if (!_MyheadFields.TryGetValue("_Next", out _Next) || !_MyheadFields.TryGetValue("_Prev", out Prev) || !_MyheadFields.TryGetValue("_Myval", out _Myval))
                    return null;

                return new ExtractedData
                {
                    ReadLength = codeType.Module.Process.GetReadInt(codeType, "_Mysize"),
                    HeadOffset = codeType.GetFieldOffset("_Myhead"),
                    HeadHasValue = false,
                    ItemNextOffset = _Myhead.GetFieldOffset("_Next"),
                    ItemPreviousOffset = _Myhead.GetFieldOffset("_Prev"),
                    ItemValueOffset = _Myhead.GetFieldOffset("_Myval"),
                    ItemValueCodeType = _Myval,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::list
        /// </summary>
        internal class VisualStudio2015 : ListBase
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
                //   | _Myhead
                //     | _Next
                //     | _Prev
                //     | _Myval
                //   | _Mysize
                CodeType _Mypair, _Myval2, _Myhead, _Next, Prev, _Myval, _Mysize;

                if (!codeType.GetFieldTypes().TryGetValue("_Mypair", out _Mypair))
                    return null;
                if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                    return null;

                var _Myval2Fields = _Myval2.GetFieldTypes();

                if (!_Myval2Fields.TryGetValue("_Myhead", out _Myhead) || !_Myval2Fields.TryGetValue("_Mysize", out _Mysize))
                    return null;

                var _MyheadFields = _Myhead.GetFieldTypes();

                if (!_MyheadFields.TryGetValue("_Next", out _Next) || !_MyheadFields.TryGetValue("_Prev", out Prev) || !_MyheadFields.TryGetValue("_Myval", out _Myval))
                    return null;

                return new ExtractedData
                {
                    ReadLength = codeType.Module.Process.GetReadInt(codeType, "_Mypair._Myval2._Mysize"),
                    HeadOffset = codeType.GetFieldOffset("_Mypair") + _Mypair.GetFieldOffset("_Myval2") + _Myval2.GetFieldOffset("_Myhead"),
                    HeadHasValue = false,
                    ItemNextOffset = _Myhead.GetFieldOffset("_Next"),
                    ItemPreviousOffset = _Myhead.GetFieldOffset("_Prev"),
                    ItemValueOffset = _Myhead.GetFieldOffset("_Myval"),
                    ItemValueCodeType = _Myval,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string
        /// </summary>
        internal class LibStdCpp6 : ListBase
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
                // _M_impl
                // | _M_node
                //   | _M_next
                //   | _M_prev
                //   | _M_storage
                //     | _M_storage
                CodeType _M_impl, _M_node, _M_next, _M_prev, _M_storage, _M_storage2;

                if (!codeType.GetFieldTypes().TryGetValue("_M_impl", out _M_impl))
                    return null;
                if (!_M_impl.GetFieldTypes().TryGetValue("_M_node", out _M_node))
                    return null;

                var _M_node_Fields = _M_node.GetFieldTypes();

                if (!_M_node_Fields.TryGetValue("_M_next", out _M_next) || !_M_node_Fields.TryGetValue("_M_prev", out _M_prev) || !_M_node_Fields.TryGetValue("_M_storage", out _M_storage))
                    return null;
                if (!_M_storage.GetFieldTypes().TryGetValue("_M_storage", out _M_storage2))
                    return null;

                CodeType valueCodeType;
                CodeType lengthCodeType;

                try
                {
                    valueCodeType = (CodeType)codeType.TemplateArguments[0];
                    lengthCodeType = (CodeType)_M_storage.TemplateArguments[0];
                }
                catch
                {
                    return null;
                }

                return new ExtractedData
                {
                    ReadLength = codeType.Module.Process.GetReadInt(codeType, "_M_impl._M_node._M_storage._M_storage", readingCodeType: lengthCodeType),
                    HeadOffset = codeType.GetFieldOffset("_M_impl") + _M_impl.GetFieldOffset("_M_node") + _M_node.GetFieldOffset("_M_next"),
                    HeadHasValue = true,
                    ItemNextOffset = _M_node.GetFieldOffset("_M_next"),
                    ItemPreviousOffset = _M_node.GetFieldOffset("_M_prev"),
                    ItemValueOffset = _M_node.GetFieldOffset("_M_storage"),
                    ItemValueCodeType = valueCodeType,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string with _GLIBCXX_USE_CXX11_ABI not defined.
        /// </summary>
        internal class LibStdCpp6_NoAbi : ListBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6_NoAbi"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public LibStdCpp6_NoAbi(Variable variable, object savedData)
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
                // _M_impl
                // | _M_node
                //   | _M_next
                //   | _M_prev
                // _List_node code type needs to exist in the same domain where code type for _M_node exists, just class name should be changed to _List_node
                // | _M_storage
                CodeType _M_impl, _M_node, _M_next, _M_prev, _List_node, _M_storage;

                if (!codeType.GetFieldTypes().TryGetValue("_M_impl", out _M_impl))
                    return null;
                if (!_M_impl.GetFieldTypes().TryGetValue("_M_node", out _M_node))
                    return null;

                var _M_node_Fields = _M_node.GetFieldTypes();

                if (!_M_node_Fields.TryGetValue("_M_next", out _M_next) || !_M_node_Fields.TryGetValue("_M_prev", out _M_prev))
                    return null;

                CodeType valueCodeType;

                try
                {
                    _List_node = GetListNodeCodeType(codeType);
                    valueCodeType = (CodeType)_List_node.TemplateArguments[0];
                }
                catch
                {
                    return null;
                }

                if (!_List_node.GetFieldTypes().TryGetValue("_M_storage", out _M_storage))
                    return null;

                int anchorOffset = codeType.GetFieldOffset("_M_impl") + _M_impl.GetFieldOffset("_M_node");
                int nextOffset = _M_node.GetFieldOffset("_M_next");
                Process process = codeType.Module.Process;

                return new ExtractedData
                {
                    ReadLength = (address) =>
                    {
                        int count = 0;
                        ulong start = address + (uint)anchorOffset;
                        ulong next = process.ReadPointer(start + (uint)nextOffset);

                        while (next != start)
                        {
                            next = process.ReadPointer(next + (uint)nextOffset);
                            count++;
                        }
                        return count;
                    },
                    HeadOffset = codeType.GetFieldOffset("_M_impl") + _M_impl.GetFieldOffset("_M_node") + _M_node.GetFieldOffset("_M_next"),
                    HeadHasValue = true,
                    ItemNextOffset = _List_node.GetFieldOffset("_M_next"),
                    ItemPreviousOffset = _List_node.GetFieldOffset("_M_prev"),
                    ItemValueOffset = _List_node.GetFieldOffset("_M_storage"),
                    ItemValueCodeType = valueCodeType,
                    CodeType = codeType,
                    Process = process,
                };
            }

            /// <summary>
            /// Finds _List_node code type for the specified list code type.
            /// </summary>
            /// <param name="codeType">std::list code type</param>
            private static CodeType GetListNodeCodeType(CodeType codeType)
            {
                string elementTypeName = codeType.TemplateArgumentsStrings[0];
                string listNodeCodeTypeName;
                if (elementTypeName.EndsWith(">"))
                    listNodeCodeTypeName = $"std::_List_node<{elementTypeName} >";
                else
                    listNodeCodeTypeName = $"std::_List_node<{elementTypeName}>";
                return CodeType.Create(listNodeCodeTypeName, codeType.Module);
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::basic_string
        /// </summary>
        internal class ClangLibCpp : ListBase
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
                // __end_
                // | __next_
                // | __prev_
                // | __value_ (exists in list node type)
                // __size_alloc_
                // | __value_
                CodeType __end_, __next_, __prev_, __size_alloc_, __value_;

                if (!codeType.GetFieldTypes().TryGetValue("__end_", out __end_))
                    return null;
                if (!__end_.GetFieldTypes().TryGetValue("__next_", out __next_) || !__end_.GetFieldTypes().TryGetValue("__prev_", out __prev_))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("__size_alloc_", out __size_alloc_))
                    return null;
                if (!__size_alloc_.GetFieldTypes().TryGetValue("__value_", out __value_))
                    return null;

                CodeType listNodeType, __value_2;

                try
                {
                    CodeType listNodeBaseType = codeType.GetFieldType("__end_");
                    const string listNodeBaseNameStart = "std::__1::__list_node_base<";
                    const string listNodeNameStart = "std::__1::__list_node<";
                    listNodeType = CodeType.Create(listNodeNameStart + listNodeBaseType.Name.Substring(listNodeBaseNameStart.Length), listNodeBaseType.Module);
                }
                catch
                {
                    return null;
                }

                if (!listNodeType.GetFieldTypes().TryGetValue("__value_", out __value_2))
                    return null;

                return new ExtractedData
                {
                    ReadLength = codeType.Module.Process.GetReadInt(codeType, "__size_alloc_.__value_"),
                    HeadOffset = codeType.GetFieldOffset("__end_") + __end_.GetFieldOffset("__next_"),
                    HeadHasValue = true,
                    ItemNextOffset = listNodeType.GetFieldOffset("__next_"),
                    ItemPreviousOffset = listNodeType.GetFieldOffset("__prev_"),
                    ItemValueOffset = listNodeType.GetFieldOffset("__value_"),
                    ItemValueCodeType = __value_2,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IReadOnlyCollection<T>> typeSelector = new TypeSelector<IReadOnlyCollection<T>>(new[]
        {
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp6), LibStdCpp6.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp6_NoAbi), LibStdCpp6_NoAbi.VerifyCodeType),
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
        private IReadOnlyCollection<T> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="list{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::list</exception>
        public list(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::list");
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
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
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
    /// Simplification class for creating <see cref="list{T}"/> with T being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::list<>", CodeTypeVerification = nameof(list.VerifyCodeType))]
    [UserType(TypeName = "std::__1::list<>", CodeTypeVerification = nameof(list.VerifyCodeType))]
    public class list : list<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="list"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public list(Variable variable)
            : base(variable)
        {
        }
    }
}
