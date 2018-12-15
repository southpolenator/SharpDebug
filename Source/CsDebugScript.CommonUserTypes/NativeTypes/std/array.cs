using CsDebugScript.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class array<T> : UserType, IReadOnlyCollection<T>
    {
        /// <summary>
        /// Common code for all implementations of std::array
        /// </summary>
        internal class ArrayBase : IReadOnlyCollection<T>
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Size of the array.
                /// </summary>
                public int Length;

                /// <summary>
                /// Offset of array's first element.
                /// </summary>
                public int FirstElementOffset;

                /// <summary>
                /// Code type of element.
                /// </summary>
                public CodeType ElementCodeType;

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
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// The first element address.
            /// </summary>
            private ulong firstElementAddress;

            /// <summary>
            /// Initializes a new instance of the <see cref="ArrayBase"/> class.
            /// </summary>
            /// <param name="variable">The value.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public ArrayBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                ulong address = variable.GetPointerAddress();
                firstElementAddress = address + (uint)data.FirstElementOffset;
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count => data.Length;

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
                ulong address = firstElementAddress;
                uint elementSize = data.ElementCodeType.Size;

                for (int i = 0, n = Count; i < n; i++, address += elementSize)
                    yield return Variable.Create(data.ElementCodeType, address).CastAs<T>();
            }
        }

        /// <summary>
        /// Microsoft Visual Studio implementations of std::array
        /// </summary>
        internal class VisualStudio : ArrayBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio(Variable variable, object savedData)
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
                // _Elems
                CodeType _Elems;

                if (!codeType.GetFieldTypes().TryGetValue("_Elems", out _Elems))
                    return null;

                return new ExtractedData
                {
                    Length = (int)Convert.ChangeType(codeType.TemplateArguments[1], typeof(int)),
                    FirstElementOffset = codeType.GetFieldOffset("_Elems"),
                    ElementCodeType = (CodeType)codeType.TemplateArguments[0],
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::array
        /// </summary>
        internal class LibStdCpp6 : ArrayBase
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
                // _M_elems
                CodeType _M_elems;

                if (!codeType.GetFieldTypes().TryGetValue("_M_elems", out _M_elems))
                    return null;

                return new ExtractedData
                {
                    Length = (int)Convert.ChangeType(codeType.TemplateArguments[1], typeof(int)),
                    FirstElementOffset = codeType.GetFieldOffset("_M_elems"),
                    ElementCodeType = (CodeType)codeType.TemplateArguments[0],
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::array
        /// </summary>
        internal class ClangLibCpp : ArrayBase
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
                // __elems_
                CodeType __elems_;

                if (!codeType.GetFieldTypes().TryGetValue("__elems_", out __elems_))
                    return null;

                return new ExtractedData
                {
                    Length = (int)Convert.ChangeType(codeType.TemplateArguments[1], typeof(int)),
                    FirstElementOffset = codeType.GetFieldOffset("__elems_"),
                    ElementCodeType = (CodeType)codeType.TemplateArguments[0],
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
        private IReadOnlyCollection<T> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="array{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::array</exception>
        public array(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
                throw new WrongCodeTypeException(variable, nameof(variable), "std::array");
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => instance.Count;

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
    [UserType(TypeName = "std::array<>", CodeTypeVerification = nameof(array.VerifyCodeType))]
    [UserType(TypeName = "std::__1::array<>", CodeTypeVerification = nameof(array.VerifyCodeType))]
    public class array : array<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="array"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public array(Variable variable)
            : base(variable)
        {
        }
    }
}
