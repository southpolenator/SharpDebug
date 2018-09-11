using CsDebugScript.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::vector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class vector<T> : UserType, IReadOnlyList<T>
    {
        /// <summary>
        /// Interface that describes vector instance abilities.
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IReadOnlyList{T}" />
        private interface IVector : IReadOnlyList<T>
        {
            /// <summary>
            /// Gets the reserved space in buffer (number of elements).
            /// </summary>
            int Reserved { get; }

            /// <summary>
            /// Creates <see cref="CodeArray{T}"/> for better performance access to buffer data (for example byte arrays when reading images).
            /// </summary>
            /// <returns>An <see cref="CodeArray{T}"/> instance.</returns>
            CodeArray<T> ToCodeArray();
        }

        /// <summary>
        /// Common code for Microsoft Visual Studio implementations of std::vector
        /// </summary>
        public class VisualStudio : IVector
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
                    if (First == null || Last == null)
                        return 0;

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
                    if (First == null || End == null)
                        return 0;

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
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                {
                    return specializedEnumerable.GetEnumerator();
                }

                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                {
                    return specializedEnumerable.GetEnumerator();
                }

                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Creates <see cref="CodeArray{T}"/> for better performance access to buffer data (for example byte arrays when reading images).
            /// </summary>
            /// <returns>An <see cref="CodeArray{T}"/> instance.</returns>
            public CodeArray<T> ToCodeArray()
            {
                return new CodeArray<T>(First, Length);
            }

            /// <summary>
            /// Gets enumerable of specialized types (like byte[]).
            /// </summary>
            private IEnumerable<T> GetSpecializedEnumerable()
            {
                if (typeof(T) == typeof(byte))
                {
                    return Debugger.ReadMemory(First, (uint)Length).Bytes.Cast<T>();
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Enumerates this vector.
            /// </summary>
            private IEnumerable<T> Enumerate()
            {
                for (int i = 0, len = Length; i < len; i++)
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
        /// libstdc++ 6 implementations of std::vector
        /// </summary>
        public class LibStdCpp6 : IVector
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
            /// Initializes a new instance of the <see cref="LibStdCpp6" /> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public LibStdCpp6(Variable variable)
            {
                value = UserMember.Create(() => variable.GetField("_M_impl"));
                first = UserMember.Create(() => Value.GetField("_M_start"));
                last = UserMember.Create(() => Value.GetField("_M_finish"));
                end = UserMember.Create(() => Value.GetField("_M_end_of_storage"));
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
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                {
                    return specializedEnumerable.GetEnumerator();
                }

                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                {
                    return specializedEnumerable.GetEnumerator();
                }

                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Creates <see cref="CodeArray{T}"/> for better performance access to buffer data (for example byte arrays when reading images).
            /// </summary>
            /// <returns>An <see cref="CodeArray{T}"/> instance.</returns>
            public CodeArray<T> ToCodeArray()
            {
                return new CodeArray<T>(First, Length);
            }

            /// <summary>
            /// Gets enumerable of specialized types (like byte[]).
            /// </summary>
            private IEnumerable<T> GetSpecializedEnumerable()
            {
                if (typeof(T) == typeof(byte))
                {
                    return Debugger.ReadMemory(First, (uint)Length).Bytes.Cast<T>();
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Enumerates this vector.
            /// </summary>
            private IEnumerable<T> Enumerate()
            {
                for (int i = 0, len = Length; i < len; i++)
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

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _M_impl
                // | _M_start
                // | _M_finish
                // | _M_end_of_storage
                CodeType _M_impl, _M_start, _M_finish, _M_end_of_storage;

                if (!codeType.GetFieldTypes().TryGetValue("_M_impl", out _M_impl))
                    return false;

                var _M_implFields = _M_impl.GetFieldTypes();

                if (!_M_implFields.TryGetValue("_M_start", out _M_start) || !_M_implFields.TryGetValue("_M_finish", out _M_finish) || !_M_implFields.TryGetValue("_M_end_of_storage", out _M_end_of_storage))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::vector
        /// </summary>
        public class ClangLibCpp : IVector
        {
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
            /// Initializes a new instance of the <see cref="ClangLibCpp" /> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public ClangLibCpp(Variable variable)
            {
                first = UserMember.Create(() => variable.GetField("__begin_"));
                last = UserMember.Create(() => variable.GetField("__end_"));
                end = UserMember.Create(() => variable.GetField("__end_cap_").GetField("__value_"));
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
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                {
                    return specializedEnumerable.GetEnumerator();
                }

                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                {
                    return specializedEnumerable.GetEnumerator();
                }

                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Creates <see cref="CodeArray{T}"/> for better performance access to buffer data (for example byte arrays when reading images).
            /// </summary>
            /// <returns>An <see cref="CodeArray{T}"/> instance.</returns>
            public CodeArray<T> ToCodeArray()
            {
                return new CodeArray<T>(First, Length);
            }

            /// <summary>
            /// Gets enumerable of specialized types (like byte[]).
            /// </summary>
            private IEnumerable<T> GetSpecializedEnumerable()
            {
                if (typeof(T) == typeof(byte))
                {
                    return Debugger.ReadMemory(First, (uint)Length).Bytes.Cast<T>();
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Enumerates this vector.
            /// </summary>
            private IEnumerable<T> Enumerate()
            {
                for (int i = 0, len = Length; i < len; i++)
                {
                    yield return this[i];
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

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // __begin_
                // __end_
                // __end_cap_
                // | __value_
                CodeType __begin_, __end_, __end_cap_, __value_;

                if (!codeType.GetFieldTypes().TryGetValue("__begin_", out __begin_))
                    return false;
                if (!codeType.GetFieldTypes().TryGetValue("__end_", out __end_))
                    return false;
                if (!codeType.GetFieldTypes().TryGetValue("__end_cap_", out __end_cap_))
                    return false;
                if (!__end_cap_.GetFieldTypes().TryGetValue("__value_", out __value_))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IVector> typeSelector = new TypeSelector<IVector>(new[]
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
        private IVector instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="vector"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::vector</exception>
        public vector(Variable variable)
            : base(variable)
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
        /// Gets the reserved space in buffer (number of elements).
        /// </summary>
        public int Reserved
        {
            get
            {
                return instance.Reserved;
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
        /// Creates <see cref="CodeArray{T}"/> for better performance access to buffer data (for example byte arrays when reading images).
        /// </summary>
        /// <returns>An <see cref="CodeArray{T}"/> instance.</returns>
        public CodeArray<T> ToCodeArray()
        {
            return instance.ToCodeArray();
        }

        /// <summary>
        /// Reads all data to managed array.
        /// </summary>
        /// <returns>Managed array.</returns>
        public T[] ToArray()
        {
            return ToCodeArray().ToArray();
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
    /// Simplification class for creating <see cref="vector{T}"/> with T being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::vector<>", CodeTypeVerification = nameof(vector.VerifyCodeType))]
    public class vector : vector<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="vector"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public vector(Variable variable)
            : base(variable)
        {
        }
    }
}
