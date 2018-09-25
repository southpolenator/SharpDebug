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
        /// Common code for all implementations of std::vector
        /// </summary>
        internal class VectorBase : IVector
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Offset of first field.
                /// </summary>
                public int FirstOffset;

                /// <summary>
                /// Offset of last field.
                /// </summary>
                public int LastOffset;

                /// <summary>
                /// Offset of end field.
                /// </summary>
                public int EndOffset;

                /// <summary>
                /// Size of the element.
                /// </summary>
                public uint ElementSize;

                /// <summary>
                /// Element code type.
                /// </summary>
                public CodeType ElementCodeType;

                /// <summary>
                /// Code type of std::vector.
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
            /// Address of first element.
            /// </summary>
            private ulong firstAddress;

            /// <summary>
            /// Address of last element.
            /// </summary>
            private ulong lastAddress;

            /// <summary>
            /// Address of end element.
            /// </summary>
            private ulong endAddress;

            /// <summary>
            /// Initializes a new instance of the <see cref="VectorBase"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VectorBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                ulong address = variable.GetPointerAddress();
                firstAddress = data.Process.ReadPointer(address + (uint)data.FirstOffset);
                lastAddress = data.Process.ReadPointer(address + (uint)data.LastOffset);
                endAddress = data.Process.ReadPointer(address + (uint)data.EndOffset);
            }

            /// <summary>
            /// Gets the reserved space in buffer (number of elements).
            /// </summary>
            public int Reserved
            {
                get
                {
                    if (firstAddress == 0 || endAddress == 0)
                        return 0;

                    if (endAddress <= firstAddress)
                        return 0;

                    ulong diff = endAddress - firstAddress;

                    diff /= data.ElementSize;
                    if (diff > int.MaxValue)
                        return 0;

                    return (int)diff;
                }
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count
            {
                get
                {
                    if (firstAddress == 0 || lastAddress == 0)
                        return 0;

                    if (lastAddress <= firstAddress)
                        return 0;

                    ulong diff = lastAddress - firstAddress;

                    diff /= data.ElementSize;
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
                        throw new ArgumentOutOfRangeException("index", index, "Querying index outside of the vector buffer");

                    ulong address = firstAddress + data.ElementSize * (uint)index;

                    return Variable.Create(data.ElementCodeType, address).CastAs<T>();
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            public IEnumerator<T> GetEnumerator()
            {
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                    return specializedEnumerable.GetEnumerator();
                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable<T> specializedEnumerable = GetSpecializedEnumerable();

                if (specializedEnumerable != null)
                    return specializedEnumerable.GetEnumerator();
                return Enumerate().GetEnumerator();
            }

            /// <summary>
            /// Creates <see cref="CodeArray{T}"/> for better performance access to buffer data (for example byte arrays when reading images).
            /// </summary>
            /// <returns>An <see cref="CodeArray{T}"/> instance.</returns>
            public CodeArray<T> ToCodeArray()
            {
                Variable first = Variable.CreatePointer(data.ElementCodeType.PointerToType, firstAddress);

                return new CodeArray<T>(first, Count);
            }

            /// <summary>
            /// Gets enumerable of specialized types (like byte[]).
            /// </summary>
            private IEnumerable<T> GetSpecializedEnumerable()
            {
                if (typeof(T) == typeof(byte))
                    return Debugger.ReadMemory(data.Process, firstAddress, (uint)Count).Bytes.Cast<T>();
                else
                    return null;
            }

            /// <summary>
            /// Enumerates this vector.
            /// </summary>
            private IEnumerable<T> Enumerate()
            {
                for (int i = 0, len = Count; i < len; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2013 implementation of std::vector
        /// </summary>
        internal class VisualStudio2013 : VectorBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2013" /> class.
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
                // _Myfirst
                // _Mylast
                // _Myend
                CodeType _Myfirst, _Mylast, _Myend;

                var fields = codeType.GetFieldTypes();

                if (!fields.TryGetValue("_Myfirst", out _Myfirst) || !fields.TryGetValue("_Mylast", out _Mylast) || !fields.TryGetValue("_Myend", out _Myend))
                    return null;

                return new ExtractedData
                {
                    FirstOffset = codeType.GetFieldOffset("_Myfirst"),
                    LastOffset = codeType.GetFieldOffset("_Mylast"),
                    EndOffset = codeType.GetFieldOffset("_Myend"),
                    ElementSize = _Myfirst.ElementType.Size,
                    ElementCodeType = _Myfirst.ElementType,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Microsoft Visual Studio 2015 implementation of std::vector
        /// </summary>
        internal class VisualStudio2015 : VectorBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio2015" /> class.
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
                //   | _Myfirst
                //   | _Mylast
                //   | _Myend
                CodeType _Mypair, _Myval2, _Myfirst, _Mylast, _Myend;

                if (!codeType.GetFieldTypes().TryGetValue("_Mypair", out _Mypair))
                    return null;
                if (!_Mypair.GetFieldTypes().TryGetValue("_Myval2", out _Myval2))
                    return null;

                var _Myval2Fields = _Myval2.GetFieldTypes();

                if (!_Myval2Fields.TryGetValue("_Myfirst", out _Myfirst) || !_Myval2Fields.TryGetValue("_Mylast", out _Mylast) || !_Myval2Fields.TryGetValue("_Myend", out _Myend))
                    return null;

                return new ExtractedData
                {
                    FirstOffset = codeType.GetFieldOffset("_Mypair") + _Mypair.GetFieldOffset("_Myval2") + _Myval2.GetFieldOffset("_Myfirst"),
                    LastOffset = codeType.GetFieldOffset("_Mypair") + _Mypair.GetFieldOffset("_Myval2") + _Myval2.GetFieldOffset("_Mylast"),
                    EndOffset = codeType.GetFieldOffset("_Mypair") + _Mypair.GetFieldOffset("_Myval2") + _Myval2.GetFieldOffset("_Myend"),
                    ElementSize = _Myfirst.ElementType.Size,
                    ElementCodeType = _Myfirst.ElementType,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::vector
        /// </summary>
        internal class LibStdCpp6 : VectorBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp6" /> class.
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
                // | _M_start
                // | _M_finish
                // | _M_end_of_storage
                CodeType _M_impl, _M_start, _M_finish, _M_end_of_storage;

                if (!codeType.GetFieldTypes().TryGetValue("_M_impl", out _M_impl))
                    return null;

                var _M_implFields = _M_impl.GetFieldTypes();

                if (!_M_implFields.TryGetValue("_M_start", out _M_start) || !_M_implFields.TryGetValue("_M_finish", out _M_finish) || !_M_implFields.TryGetValue("_M_end_of_storage", out _M_end_of_storage))
                    return null;

                return new ExtractedData
                {
                    FirstOffset = codeType.GetFieldOffset("_M_impl") + _M_impl.GetFieldOffset("_M_start"),
                    LastOffset = codeType.GetFieldOffset("_M_impl") + _M_impl.GetFieldOffset("_M_finish"),
                    EndOffset = codeType.GetFieldOffset("_M_impl") + _M_impl.GetFieldOffset("_M_end_of_storage"),
                    ElementSize = _M_start.ElementType.Size,
                    ElementCodeType = _M_start.ElementType,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::vector
        /// </summary>
        internal class ClangLibCpp : VectorBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClangLibCpp" /> class.
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
                // __begin_
                // __end_
                // __end_cap_
                // | __value_
                CodeType __begin_, __end_, __end_cap_, __value_;

                if (!codeType.GetFieldTypes().TryGetValue("__begin_", out __begin_))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("__end_", out __end_))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("__end_cap_", out __end_cap_))
                    return null;
                if (!__end_cap_.GetFieldTypes().TryGetValue("__value_", out __value_))
                    return null;
                return new ExtractedData
                {
                    FirstOffset = codeType.GetFieldOffset("__begin_"),
                    LastOffset = codeType.GetFieldOffset("__end_"),
                    EndOffset = codeType.GetFieldOffset("__end_cap_") + __end_cap_.GetFieldOffset("__value_"),
                    ElementSize = __begin_.ElementType.Size,
                    ElementCodeType = __begin_.ElementType,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IVector> typeSelector = new TypeSelector<IVector>(new[]
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
    [UserType(TypeName = "std::__1::vector<>", CodeTypeVerification = nameof(vector.VerifyCodeType))]
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
