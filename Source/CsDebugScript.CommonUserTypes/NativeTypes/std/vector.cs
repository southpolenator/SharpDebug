using CsDebugScript.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::vector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class vector<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Common code for Microsoft Visual Studio implementations of std::vector
        /// </summary>
        public class VisualStudio : IReadOnlyList<T>
        {
            /// <summary>
            /// The internal value field inside the std::vector
            /// </summary>
            private UserMember<Variable> value;

            /// <summary>
            /// The first element in the vector
            /// </summary>
            private UserMember<Variable> first;

            /// <summary>
            /// The last element in the vector
            /// </summary>
            private UserMember<Variable> last;

            /// <summary>
            /// The end of the vector buffer
            /// </summary>
            private UserMember<Variable> end;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public VisualStudio(UserMember<Variable> value)
            {
                this.value = value;
                first = UserMember.Create(() => Value.GetField("_Myfirst"));
                last = UserMember.Create(() => Value.GetField("_Mylast"));
                end = UserMember.Create(() => Value.GetField("_Myend"));
            }

            /// <summary>
            /// Gets the vector length.
            /// </summary>
            public int Length
            {
                get
                {
                    ulong firstAddress = First.GetPointerAddress();
                    ulong lastAddress = Last.GetPointerAddress();

                    if (lastAddress <= firstAddress)
                        return 0;

                    ulong diff = lastAddress - firstAddress;

                    diff /= First.GetCodeType().ElementType.Size;
                    if (diff > int.MaxValue)
                        return 0;

                    return (int)diff;
                }
            }

            /// <summary>
            /// Gets the reserved space in buffer (number of elements).
            /// </summary>
            public int Reserved
            {
                get
                {
                    ulong firstAddress = First.GetPointerAddress();
                    ulong endAddress = End.GetPointerAddress();

                    if (endAddress <= firstAddress)
                        return 0;

                    ulong diff = endAddress - firstAddress;

                    diff /= First.GetCodeType().ElementType.Size;
                    if (diff > int.MaxValue)
                        return 0;

                    return (int)diff;
                }
            }

            /// <summary>
            /// Gets the &lt;T&gt; at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            public T this[int index]
            {
                get
                {
                    if (index < 0 || index >= Reserved)
                    {
                        throw new ArgumentOutOfRangeException("index", index, "Querying index outside of the vector buffer");
                    }

                    return First.GetArrayElement(index).CastAs<T>();
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    return Length;
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
            /// Enumerates this vector.
            /// </summary>
            private IEnumerable<T> Enumerate()
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return this[i];
                }
            }

            /// <summary>
            /// Gets the internal value field inside the std::vector
            /// </summary>
            private Variable Value
            {
                get
                {
                    return value.Value;
                }
            }

            /// <summary>
            /// Gets the first element in the vector
            /// </summary>
            private Variable First
            {
                get
                {
                    return first.Value;
                }
            }

            /// <summary>
            /// Gets the last element in the vector
            /// </summary>
            private Variable Last
            {
                get
                {
                    return last.Value;
                }
            }

            /// <summary>
            /// Gets end of the vector buffer
            /// </summary>
            private Variable End
            {
                get
                {
                    return end.Value;
                }
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::vector
        /// </summary>
        public class VisualStudio2013 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2013" /> class.
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
                // _Myfirst
                // _Mylast
                // _Myend
                CodeType _Myfirst, _Mylast, _Myend;

                var fields = codeType.GetFieldTypes();

                if (!fields.TryGetValue("_Myfirst", out _Myfirst) || !fields.TryGetValue("_Mylast", out _Mylast) || !fields.TryGetValue("_Myend", out _Myend))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::vector
        /// </summary>
        public class VisualStudio2015 : VisualStudio
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2015" /> class.
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
                //   | _Myfirst
                //   | _Mylast
                //   | _Myend
                CodeType _Mypair, _Myval2, _Myfirst, _Mylast, _Myend;

                if (!codeType.GetFieldTypes().TryGetValue("_Mypair", out _Mypair))
                    return false;
                if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                    return false;

                var _Myval2Fields = _Myval2.GetFieldTypes();

                if (!_Myval2Fields.TryGetValue("_Myfirst", out _Myfirst) || !_Myval2Fields.TryGetValue("_Mylast", out _Mylast) || !_Myval2Fields.TryGetValue("_Myend", out _Myend))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IReadOnlyList<T>> typeSelector = new TypeSelector<IReadOnlyList<T>>(new[]
        {
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2013), VisualStudio2013.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio2015), VisualStudio2015.VerifyCodeType),
        });

        /// <summary>
        /// The instance used to read variable data
        /// </summary>
        private IReadOnlyList<T> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="vector"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::vector</exception>
        public vector(Variable variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::vector");
            }
        }

        /// <summary>
        /// Gets the &lt;T&gt; at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[int index]
        {
            get
            {
                return instance[index];
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
    }
}
