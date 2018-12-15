using CsDebugScript.Exceptions;
using System;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std.filesystem
{
    /// <summary>
    /// Implementation of std::filesystem::path
    /// </summary>
    [UserType(TypeName = "std::filesystem::path", CodeTypeVerification = nameof(any.VerifyCodeType))]
    [UserType(TypeName = "std::__1::__fs::filesystem::path", CodeTypeVerification = nameof(any.VerifyCodeType))]
    public class path : UserType
    {
        /// <summary>
        /// Common code for all implementations of std::filesystem::path
        /// </summary>
        internal class PathBase
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Function that reads path.
                /// </summary>
                public Func<ulong, string> ReadPath;

                /// <summary>
                /// Code type of std::filesystem::path.
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
            /// The address of this instance.
            /// </summary>
            private ulong address;

            /// <summary>
            /// Initializes a new instance of the <see cref="PathBase"/> class.
            /// </summary>
            /// <param name="variable">The value.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public PathBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
            }

            /// <summary>
            /// Gets the path as string.
            /// </summary>
            public string Path => data.ReadPath(address);
        }

        /// <summary>
        /// Microsoft Visual Studio implementations of std::filesystem::path
        /// </summary>
        internal class VisualStudio : PathBase
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
                // _Text
                CodeType _Text;

                if (!codeType.GetFieldTypes().TryGetValue("_Text", out _Text))
                    return null;
                if (!basic_string.VerifyCodeType(_Text))
                    return null;

                int offset = codeType.GetFieldOffset("_Text");

                return new ExtractedData
                {
                    ReadPath = (address) => new basic_string(Variable.Create(_Text, address + (uint)offset)).Text,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 8 implementations of std::filesystem::path
        /// </summary>
        internal class LibStdCpp8 : PathBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp8"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public LibStdCpp8(Variable variable, object savedData)
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
                // _M_pathname
                CodeType _M_pathname;

                if (!codeType.GetFieldTypes().TryGetValue("_M_pathname", out _M_pathname))
                    return null;
                if (!basic_string.VerifyCodeType(_M_pathname))
                    return null;

                int offset = codeType.GetFieldOffset("_M_pathname");

                return new ExtractedData
                {
                    ReadPath = (address) => new basic_string(Variable.Create(_M_pathname, address + (uint)offset)).Text,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::filesystem::path
        /// </summary>
        internal class ClangLibCpp : PathBase
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
                // __pn_
                CodeType __pn_;

                if (!codeType.GetFieldTypes().TryGetValue("__pn_", out __pn_))
                    return null;
                if (!basic_string.VerifyCodeType(__pn_))
                    return null;

                int offset = codeType.GetFieldOffset("__pn_");

                return new ExtractedData
                {
                    ReadPath = (address) => new basic_string(Variable.Create(__pn_, address + (uint)offset)).Text,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<PathBase> typeSelector = new TypeSelector<PathBase>(new[]
        {
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio), VisualStudio.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp8), LibStdCpp8.VerifyCodeType),
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
        private PathBase instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="path"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::path</exception>
        public path(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
                throw new WrongCodeTypeException(variable, nameof(variable), "std::filesystem::path");
        }

        /// <summary>
        /// Gets the path as string.
        /// </summary>
        public string Path => instance.Path;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Path;
        }
    }
}
