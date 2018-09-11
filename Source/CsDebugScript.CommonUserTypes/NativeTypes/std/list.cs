using CsDebugScript.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class list<T> : UserType, IReadOnlyCollection<T>
    {
        /// <summary>
        /// Common code for Microsoft Visual Studio implementations of std::list
        /// </summary>
        public class VisualStudio : IReadOnlyCollection<T>
        {
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
                /// <param name="variable">The variable.</param>
                public item(Variable variable)
                {
                    next = UserMember.Create(() => new item(variable.GetField("_Next")));
                    previous = UserMember.Create(() => new item(variable.GetField("_Prev")));
                    value = UserMember.Create(() => variable.GetField("_Myval").CastAs<T>());
                }

                /// <summary>
                /// Gets the next item in the list.
                /// </summary>
                public item Next
                {
                    get
                    {
                        return next.Value;
                    }
                }

                /// <summary>
                /// Gets the previous item in the list.
                /// </summary>
                public item Previous
                {
                    get
                    {
                        return previous.Value;
                    }
                }

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public T Value
                {
                    get
                    {
                        return value.Value;
                    }
                }
            }

            /// <summary>
            /// The internal value field inside the std::list
            /// </summary>
            private UserMember<Variable> value;

            /// <summary>
            /// The length of the list
            /// </summary>
            private UserMember<int> length;

            /// <summary>
            /// The head of the list
            /// </summary>
            private UserMember<item> head;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public VisualStudio(UserMember<Variable> value)
            {
                this.value = value;
                length = UserMember.Create(() => (int)Value.GetField("_Mysize"));
                head = UserMember.Create(() => new item(Value.GetField("_Myhead")));
            }

            /// <summary>
            /// Gets the head of the list
            /// </summary>
            private item Head
            {
                get
                {
                    return head.Value;
                }
            }

            /// <summary>
            /// Gets the internal value field inside the std::list
            /// </summary>
            private Variable Value
            {
                get
                {
                    return value.Value;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    return length.Value;
                }
            }

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
                item item = Head;
                for (int i = 0; i < Count; i++)
                {
                    item = item.Next;
                    yield return item.Value;
                }
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::list
        /// </summary>
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
                // | _Mynext
                // | _Myprev
                // | _Myval
                // _Mysize
                CodeType _Myhead, _Next, Prev, _Myval, _Mysize;

                var fields = codeType.GetFieldTypes();

                if (!fields.TryGetValue("_Myhead", out _Myhead) || !fields.TryGetValue("_Mysize", out _Mysize))
                    return false;

                var _MyheadFields = _Myhead.GetFieldTypes();

                if (!_MyheadFields.TryGetValue("_Next", out _Next) || !_MyheadFields.TryGetValue("_Prev", out Prev) || !_MyheadFields.TryGetValue("_Myval", out _Myval))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::list
        /// </summary>
        public class VisualStudio2015 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2015"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public VisualStudio2015(Variable variable)
                : base(UserMember.Create(() => variable.GetField("_Mypair").GetField("_Myval2")))
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
                //   | _Myhead
                //     | _Mynext
                //     | _Myprev
                //     | _Myval
                //   | _Mysize
                CodeType _Mypair, _Myval2, _Myhead, _Next, Prev, _Myval, _Mysize;

                if (!codeType.GetFieldTypes().TryGetValue("_Mypair", out _Mypair))
                    return false;
                if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                    return false;

                var _Myval2Fields = _Myval2.GetFieldTypes();

                if (!_Myval2Fields.TryGetValue("_Myhead", out _Myhead) || !_Myval2Fields.TryGetValue("_Mysize", out _Mysize))
                    return false;

                var _MyheadFields = _Myhead.GetFieldTypes();

                if (!_MyheadFields.TryGetValue("_Next", out _Next) || !_MyheadFields.TryGetValue("_Prev", out Prev) || !_MyheadFields.TryGetValue("_Myval", out _Myval))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string
        /// </summary>
        public class LibStdCpp6 : IReadOnlyCollection<T>
        {
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
                /// Initializes a new instance of the <see cref="item" /> class.
                /// </summary>
                /// <param name="variable">The variable.</param>
                /// <param name="templateCodeType">Template code type.</param>
                public item(Variable variable, CodeType templateCodeType)
                {
                    next = UserMember.Create(() => new item(variable.GetField("_M_next").CastAs(variable.GetCodeType()), templateCodeType));
                    previous = UserMember.Create(() => new item(variable.GetField("_M_prev").CastAs(variable.GetCodeType()), templateCodeType));
                    value = UserMember.Create(() => variable.GetField("_M_storage").GetField("_M_storage").CastAs(templateCodeType).CastAs<T>());
                }

                /// <summary>
                /// Gets the next item in the list.
                /// </summary>
                public item Next
                {
                    get
                    {
                        return next.Value;
                    }
                }

                /// <summary>
                /// Gets the previous item in the list.
                /// </summary>
                public item Previous
                {
                    get
                    {
                        return previous.Value;
                    }
                }

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public T Value
                {
                    get
                    {
                        return value.Value;
                    }
                }
            }

            /// <summary>
            /// The number of elements
            /// </summary>
            UserMember<int> count;

            /// <summary>
            /// The head of the list
            /// </summary>
            UserMember<item> head;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public LibStdCpp6(Variable variable)
            {
                CodeType codeType = variable.GetCodeType();
                CodeType templateCodeType = (CodeType)codeType.TemplateArguments[0];
                CodeType dataCodeType = (CodeType)codeType.GetFieldType("_M_impl").GetFieldType("_M_node").GetFieldType("_M_storage").TemplateArguments[0];

                count = UserMember.Create(() => new list<int>.LibStdCpp6.item(variable.GetField("_M_impl").GetField("_M_node"), dataCodeType).Value);
                head = UserMember.Create(() => new item(variable.GetField("_M_impl").GetField("_M_node"), templateCodeType).Next);
            }

            /// <summary>
            /// Gets the head of the list
            /// </summary>
            private item Head
            {
                get
                {
                    return head.Value;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    return count.Value;
                }
            }

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
                item item = Head;
                for (int i = 0; i < Count; i++)
                {
                    yield return item.Value;
                    item = item.Next;
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _M_impl
                // | _M_node
                //   | _M_next
                //   | _M_prev
                //   | _M_storage
                CodeType _M_impl, _M_node, _M_next, _M_prev, _M_storage;

                if (!codeType.GetFieldTypes().TryGetValue("_M_impl", out _M_impl))
                    return false;
                if (!_M_impl.GetFieldTypes().TryGetValue("_M_node", out _M_node))
                    return false;

                var _M_node_Fields = _M_node.GetFieldTypes();

                if (!_M_node_Fields.TryGetValue("_M_next", out _M_next) || !_M_node_Fields.TryGetValue("_M_prev", out _M_prev) || !_M_node_Fields.TryGetValue("_M_storage", out _M_storage))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::basic_string with _GLIBCXX_USE_CXX11_ABI not defined.
        /// </summary>
        public class LibStdCpp6_NoAbi : IReadOnlyCollection<T>
        {
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
                /// Initializes a new instance of the <see cref="item" /> class.
                /// </summary>
                /// <param name="variable">The variable.</param>
                /// <param name="templateCodeType">Template code type.</param>
                public item(Variable variable, CodeType templateCodeType)
                {
                    next = UserMember.Create(() => new item(variable.GetField("_M_next").CastAs(variable.GetCodeType()), templateCodeType));
                    previous = UserMember.Create(() => new item(variable.GetField("_M_prev").CastAs(variable.GetCodeType()), templateCodeType));
                    value = UserMember.Create(() => variable.GetField("_M_storage").GetField("_M_storage").CastAs(templateCodeType).CastAs<T>());
                }

                /// <summary>
                /// Gets the next item in the list.
                /// </summary>
                public item Next
                {
                    get
                    {
                        return next.Value;
                    }
                }

                /// <summary>
                /// Gets the previous item in the list.
                /// </summary>
                public item Previous
                {
                    get
                    {
                        return previous.Value;
                    }
                }

                /// <summary>
                /// Gets the value stored in the item.
                /// </summary>
                public T Value
                {
                    get
                    {
                        return value.Value;
                    }
                }
            }

            /// <summary>
            /// The number of elements
            /// </summary>
            UserMember<int> count;

            /// <summary>
            /// The anchor of the list
            /// </summary>
            UserMember<Variable> anchor;

            /// <summary>
            /// The head of the list
            /// </summary>
            UserMember<item> head;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6_NoAbi"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public LibStdCpp6_NoAbi(Variable variable)
            {
                CodeType codeType = variable.GetCodeType();
                CodeType templateCodeType = (CodeType)codeType.TemplateArguments[0];
                CodeType listNodeCodeType = GetListNodeCodeType(codeType);

                count = UserMember.Create(() =>
                {
                    int count = 0;
                    Variable start = anchor.Value;
                    Variable next = start.GetField("_M_next");

                    while (next.GetPointerAddress() != start.GetPointerAddress())
                    {
                        next = next.GetField("_M_next");
                        count++;
                    }
                    return count;
                });
                anchor = UserMember.Create(() => variable.GetField("_M_impl").GetField("_M_node"));
                head = UserMember.Create(() => new item(anchor.Value.GetField("_M_next").CastAs(listNodeCodeType), templateCodeType));
            }

            /// <summary>
            /// Gets the head of the list
            /// </summary>
            private item Head
            {
                get
                {
                    return head.Value;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    return count.Value;
                }
            }

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
                item item = Head;
                for (int i = 0; i < Count; i++)
                {
                    yield return item.Value;
                    item = item.Next;
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
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
                    return false;
                if (!_M_impl.GetFieldTypes().TryGetValue("_M_node", out _M_node))
                    return false;

                var _M_node_Fields = _M_node.GetFieldTypes();

                if (!_M_node_Fields.TryGetValue("_M_next", out _M_next) || !_M_node_Fields.TryGetValue("_M_prev", out _M_prev))
                    return false;

                try
                {
                    _List_node = GetListNodeCodeType(codeType);
                }
                catch
                {
                    return false;
                }

                if (!_List_node.GetFieldTypes().TryGetValue("_M_storage", out _M_storage))
                    return false;
                return true;
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
        public class ClangLibCpp : IReadOnlyCollection<T>
        {
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
                /// Initializes a new instance of the <see cref="item" /> class.
                /// </summary>
                /// <param name="variable">The variable.</param>
                /// <param name="listNodeCodeType">List node code type.</param>
                public item(Variable variable, CodeType listNodeCodeType)
                {
                    next = UserMember.Create(() => new item(variable.GetField("__next_"), listNodeCodeType));
                    previous = UserMember.Create(() => new item(variable.GetField("__prev_"), listNodeCodeType));
                    value = UserMember.Create(() => variable.CastAs(listNodeCodeType).GetField("__value_").CastAs<T>());
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
            /// The number of elements
            /// </summary>
            UserMember<int> count;

            /// <summary>
            /// The head of the list
            /// </summary>
            UserMember<item> head;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClangLibCpp"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public ClangLibCpp(Variable variable)
            {
                CodeType codeType = variable.GetCodeType();
                CodeType listNodeBaseType = codeType.GetFieldType("__end_");
                const string listNodeBaseNameStart = "std::__1::__list_node_base<";
                const string listNodeNameStart = "std::__1::__list_node<";
                CodeType listNodeType = CodeType.Create(listNodeNameStart + listNodeBaseType.Name.Substring(listNodeBaseNameStart.Length), listNodeBaseType.Module);

                count = UserMember.Create(() => (int)variable.GetField("__size_alloc_").GetField("__value_"));
                head = UserMember.Create(() => new item(variable.GetField("__end_").GetField("__next_"), listNodeType));
            }

            /// <summary>
            /// Gets the head of the list
            /// </summary>
            private item Head
            {
                get
                {
                    return head.Value;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    return count.Value;
                }
            }

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
                item item = Head;
                for (int i = 0; i < Count; i++)
                {
                    yield return item.Value;
                    item = item.Next;
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // __end_
                // | __next_
                // | __prev_
                // __size_alloc_
                // | __value_
                CodeType __end_, __next_, __prev_, __size_alloc_, __value_;

                if (!codeType.GetFieldTypes().TryGetValue("__end_", out __end_))
                    return false;
                if (!__end_.GetFieldTypes().TryGetValue("__next_", out __next_) || !__end_.GetFieldTypes().TryGetValue("__prev_", out __prev_))
                    return false;
                if (!codeType.GetFieldTypes().TryGetValue("__size_alloc_", out __size_alloc_))
                    return false;
                if (!__size_alloc_.GetFieldTypes().TryGetValue("__value_", out __value_))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IReadOnlyCollection<T>> typeSelector = new TypeSelector<IReadOnlyCollection<T>>(new[]
        {
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(LibStdCpp6), LibStdCpp6.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(LibStdCpp6_NoAbi), LibStdCpp6_NoAbi.VerifyCodeType),
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
