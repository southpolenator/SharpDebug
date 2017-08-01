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
    public class list<T> : IReadOnlyCollection<T>
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
                CodeType intCodeType = (CodeType)codeType.GetFieldType("_M_impl").GetFieldType("_M_node").GetFieldType("_M_storage").TemplateArguments[0];

                count = UserMember.Create(() => new list<int>.LibStdCpp6.item(variable.GetField("_M_impl").GetField("_M_node"), intCodeType).Value);
                head = UserMember.Create(() => new item(variable.GetField("_M_impl").GetField("_M_node"), templateCodeType).Next);
            }

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
                    item = item.Next;
                    yield return item.Value;
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
        /// The type selector
        /// </summary>
        private static TypeSelector<IReadOnlyCollection<T>> typeSelector = new TypeSelector<IReadOnlyCollection<T>>(new[]
        {
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(LibStdCpp6), LibStdCpp6.VerifyCodeType),
        });

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
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        internal static bool VerifyCodeType(CodeType codeType)
        {
            return typeSelector.VerifyCodeType(codeType);
        }
    }
}
