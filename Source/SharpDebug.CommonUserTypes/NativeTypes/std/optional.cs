using SharpDebug.Engine.Utility;
using SharpDebug.Exceptions;
using System;

namespace SharpDebug.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::optional
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class optional<T> : UserType
    {
        /// <summary>
        /// Common code for all implementations of std::optional
        /// </summary>
        internal class OptionalBase
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Offset of has value field.
                /// </summary>
                public int HasValueOffset;

                /// <summary>
                /// Offset of value field.
                /// </summary>
                public int ValueOffset;

                /// <summary>
                /// Code type of value.
                /// </summary>
                public CodeType ValueCodeType;

                /// <summary>
                /// Code type of std::optional.
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
            /// The value address.
            /// </summary>
            private ulong valueAddress;

            /// <summary>
            /// Initializes a new instance of the <see cref="OptionalBase"/> class.
            /// </summary>
            /// <param name="variable">The value.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public OptionalBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                ulong address = variable.GetPointerAddress();
                valueAddress = address + (uint)data.ValueOffset;
                HasValue = data.Process.ReadByte(address + (uint)data.HasValueOffset) != 0;
            }

            /// <summary>
            /// Gets the value.
            /// </summary>
            public T Value => Variable.Create(data.ValueCodeType, valueAddress).CastAs<T>();

            /// <summary>
            /// Gets the flag representing if this instance contains a value.
            /// </summary>
            public bool HasValue { get; private set; }
        }

        /// <summary>
        /// Microsoft Visual Studio implementations of std::optional
        /// </summary>
        internal class VisualStudio : OptionalBase
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
                // _Has_value
                // _Value
                CodeType _Has_value, _Value;

                if (!codeType.GetFieldTypes().TryGetValue("_Has_value", out _Has_value) || !codeType.GetFieldTypes().TryGetValue("_Value", out _Value))
                    return null;

                return new ExtractedData
                {
                    HasValueOffset = codeType.GetFieldOffset("_Has_value"),
                    ValueCodeType = (CodeType)codeType.TemplateArguments[0],
                    ValueOffset = codeType.GetFieldOffset("_Value"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::optional
        /// </summary>
        internal class LibStdCpp6 : OptionalBase
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
                // _M_payload
                // | _M_engaged
                // | _M_payload
                CodeType _M_payload, _M_engaged, _M_payload2;

                if (!codeType.GetFieldTypes().TryGetValue("_M_payload", out _M_payload)
                    || !_M_payload.GetFieldTypes().TryGetValue("_M_engaged", out _M_engaged)
                    || !_M_payload.GetFieldTypes().TryGetValue("_M_payload", out _M_payload2))
                    return null;

                return new ExtractedData
                {
                    HasValueOffset = codeType.GetFieldOffset("_M_payload") + _M_payload.GetFieldOffset("_M_engaged"),
                    ValueCodeType = (CodeType)codeType.TemplateArguments[0],
                    ValueOffset = codeType.GetFieldOffset("_M_payload") + _M_payload.GetFieldOffset("_M_payload"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementations of std::optional
        /// </summary>
        internal class ClangLibCpp : OptionalBase
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
                // __engaged_
                // __val_
                CodeType __engaged_, __val_;

                if (!codeType.GetFieldTypes().TryGetValue("__engaged_", out __engaged_) || !codeType.GetFieldTypes().TryGetValue("__val_", out __val_))
                    return null;

                return new ExtractedData
                {
                    HasValueOffset = codeType.GetFieldOffset("__engaged_"),
                    ValueCodeType = (CodeType)codeType.TemplateArguments[0],
                    ValueOffset = codeType.GetFieldOffset("__val_"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<OptionalBase> typeSelector = new TypeSelector<OptionalBase>(new[]
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
        private OptionalBase instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="optional{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::optional</exception>
        public optional(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
                throw new WrongCodeTypeException(variable, nameof(variable), "std::optional");
        }

        /// <summary>
        /// Gets the value stored in this instance.
        /// </summary>
        [ForceDefaultVisualizerAtttribute]
        public T Value => instance.Value;

        /// <summary>
        /// Gets the flag indicating if this instance has value set.
        /// </summary>
        public bool HasValue => instance.HasValue;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!HasValue)
                return "nullopt";
            return Value.ToString();
        }
    }

    /// <summary>
    /// Simplification class for creating <see cref="optional{T}"/> with T being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::optional<>", CodeTypeVerification = nameof(optional.VerifyCodeType))]
    [UserType(TypeName = "std::__1::optional<>", CodeTypeVerification = nameof(optional.VerifyCodeType))]
    public class optional : optional<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="optional"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public optional(Variable variable)
            : base(variable)
        {
        }
    }
}
