using CsDebugScript.Exceptions;
using System;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::variant
    /// </summary>
    [UserType(TypeName = "std::variant<>", CodeTypeVerification = nameof(variant.VerifyCodeType))]
    [UserType(TypeName = "std::__1::variant<>", CodeTypeVerification = nameof(variant.VerifyCodeType))]
    public class variant : UserType
    {
        private interface IVariant
        {
            /// <summary>
            /// Gets the zero-based index of the alternative that is currently held by the variant.
            /// </summary>
            int Index { get; }

            /// <summary>
            /// Gets the value stored in this instance. If <see cref="Index"/> is <c>-1</c> this will be <c>null</c>.
            /// </summary>
            Variable Value { get; }
        }

        /// <summary>
        /// Common code for all implementations of std::variant
        /// </summary>
        internal class VariantBase : IVariant
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Offset of value fields.
                /// </summary>
                public int[] Offsets;

                /// <summary>
                /// Code types of fields.
                /// </summary>
                public CodeType[] CodeTypes;

                /// <summary>
                /// Function that reads index.
                /// </summary>
                public Func<ulong, int> ReadIndex;

                /// <summary>
                /// Code type of std::variant.
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
            /// Initializes a new instance of the <see cref="VariantBase"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VariantBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                Index = data.ReadIndex(address);
            }

            /// <summary>
            /// Gets the zero-based index of the alternative that is currently held by the variant.
            /// </summary>
            public int Index { get; private set; }

            /// <summary>
            /// Gets the value stored in this instance. If <see cref="Index"/> is <c>-1</c> this will be <c>null</c>.
            /// </summary>
            public Variable Value
            {
                get
                {
                    if (Index < 0)
                        return null;
                    return Variable.Create(data.CodeTypes[Index], address + (uint)data.Offsets[Index]);
                }
            }
        }

        /// <summary>
        /// libstdc++ 7 implementation of std::variant
        /// </summary>
        internal class LibStdCpp7 : VariantBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp7"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public LibStdCpp7(Variable variable, object savedData)
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
                // _M_index
                // _M_u
                // | _M_first
                //   | _M_storage
                // | _M_rest
                //   | _M_first?
                //     | _M_storage?
                //   | _M_rest?
                CodeType _M_index, _M_u;

                if (!codeType.GetFieldTypes().TryGetValue("_M_index", out _M_index)
                    || !codeType.GetFieldTypes().TryGetValue("_M_u", out _M_u))
                    return null;

                int arguments = codeType.TemplateArguments.Length;

                if (arguments <= 0)
                    arguments = codeType.TemplateArgumentsStrings.Length;
                else
                    for (int i = 0; i < arguments; i++)
                        if ((codeType.TemplateArguments[i] as CodeType) == null)
                            return null;

                int[] offsets = new int[arguments];
                CodeType[] codeTypes = new CodeType[arguments];
                CodeType p = _M_u;
                int baseOffset = 0;
                for (int i = 0; i < arguments; i++)
                {
                    if (!p.GetFieldTypes().TryGetValue("_M_first", out CodeType first)
                        || !first.GetFieldTypes().TryGetValue("_M_storage", out CodeType storage)
                        || !p.GetFieldTypes().TryGetValue("_M_rest", out CodeType rest))
                        return null;
                    offsets[i] = p.GetFieldOffset("_M_first") + first.GetFieldOffset("_M_storage") + baseOffset;
                    if (storage.Name.StartsWith("__gnu_cxx::__aligned_membuf<"))
                        storage = (CodeType)storage.TemplateArguments[0];
                    codeTypes[i] = storage;
                    baseOffset += p.GetFieldOffset("_M_rest");
                    p = rest;
                }

                return new ExtractedData()
                {
                    Offsets = offsets,
                    CodeTypes = codeTypes,
                    ReadIndex = codeType.Module.Process.GetReadInt(codeType, "_M_index"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementation of std::variant
        /// </summary>
        internal class ClangLibCpp : VariantBase
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
                // __impl
                // | __data
                //   | __dummy
                //   | __head
                //     | __value
                //   | __tail
                //     | __dummy?
                //     | __head?
                //       | __value?
                //     | __tail?
                // | __index
                CodeType __impl, __data, __index;

                if (!codeType.GetFieldTypes().TryGetValue("__impl", out __impl)
                    || !__impl.GetFieldTypes().TryGetValue("__data", out __data)
                    || !__impl.GetFieldTypes().TryGetValue("__index", out __index))
                    return null;

                int arguments = codeType.TemplateArguments.Length;

                if (arguments <= 0)
                    arguments = codeType.TemplateArgumentsStrings.Length;
                else
                    for (int i = 0; i < arguments; i++)
                        if ((codeType.TemplateArguments[i] as CodeType) == null)
                            return null;

                int[] offsets = new int[arguments];
                CodeType[] codeTypes = new CodeType[arguments];
                CodeType p = __data;
                int baseOffset = 0;
                for (int i = 0; i < arguments; i++)
                {
                    if (!p.GetFieldTypes().TryGetValue("__dummy", out CodeType __dummy)
                        || !p.GetFieldTypes().TryGetValue("__head", out CodeType __head)
                        || !__head.GetFieldTypes().TryGetValue("__value", out CodeType __value)
                        || !p.GetFieldTypes().TryGetValue("__tail", out CodeType __tail))
                        return null;
                    offsets[i] = p.GetFieldOffset("__head") + __head.GetFieldOffset("__value") + baseOffset;
                    codeTypes[i] = __value;
                    baseOffset += p.GetFieldOffset("__tail");
                    p = __tail;
                }

                return new ExtractedData()
                {
                    Offsets = offsets,
                    CodeTypes = codeTypes,
                    ReadIndex = codeType.Module.Process.GetReadInt(codeType, "__impl.__index"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Microsoft Visual Studio implementation of std::variant
        /// </summary>
        internal class VisualStudio : VariantBase
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
                // _Head
                // _Tail
                // | _Head?
                // | _Tail?
                //   | _Head?
                //   | _Tail?
                // _Which
                CodeType _Head, _Tail, _Which;

                if (!codeType.GetFieldTypes().TryGetValue("_Head", out _Head)
                    || !codeType.GetFieldTypes().TryGetValue("_Tail", out _Tail)
                    || !codeType.GetFieldTypes().TryGetValue("_Which", out _Which))
                    return null;

                for (int i = 0; i < codeType.TemplateArguments.Length; i++)
                    if ((codeType.TemplateArguments[i] as CodeType) == null)
                        return null;

                int[] offsets = new int[codeType.TemplateArguments.Length];
                CodeType[] codeTypes = new CodeType[codeType.TemplateArguments.Length];
                CodeType p = codeType;
                int baseOffset = 0;
                for (int i = 0; i < codeType.TemplateArguments.Length; i++)
                {
                    if (!p.GetFieldTypes().TryGetValue("_Head", out CodeType h)
                        || !p.GetFieldTypes().TryGetValue("_Tail", out CodeType t))
                        return null;
                    offsets[i] = p.GetFieldOffset("_Head") + baseOffset;
                    codeTypes[i] = h;
                    baseOffset += p.GetFieldOffset("_Tail");
                    p = t;
                }

                return new ExtractedData()
                {
                    Offsets = offsets,
                    CodeTypes = codeTypes,
                    ReadIndex = codeType.Module.Process.GetReadInt(codeType, "_Which"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IVariant> typeSelector = new TypeSelector<IVariant>(new[]
        {
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio), VisualStudio.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp7), LibStdCpp7.VerifyCodeType),
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
        private IVariant instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::variant</exception>
        public variant(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
                throw new WrongCodeTypeException(variable, nameof(variable), "std::variant");
        }

        /// <summary>
        /// Gets the zero-based index of the alternative that is currently held by the variant.
        /// </summary>
        public int Index => instance.Index;

        /// <summary>
        /// Gets the value stored in this instance. If <see cref="Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        [ForceDefaultVisualizerAtttribute]
        public Variable Value => instance.Value;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (Index < 0)
                return "empty";
            return Value.ToString();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
            where T : T1
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            return base.Value.CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1, T2> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    case 1:
                        return base.Value.CastAs<T2>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T2))
            {
                if (Index != 1)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else
                throw new ArgumentException("Wrong type", nameof(T));
            return base.Value.CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1, T2, T3> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    case 1:
                        return base.Value.CastAs<T2>();
                    case 2:
                        return base.Value.CastAs<T3>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T2))
            {
                if (Index != 1)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T3))
            {
                if (Index != 2)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else
                throw new ArgumentException("Wrong type", nameof(T));
            return base.Value.CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1, T2, T3, T4> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    case 1:
                        return base.Value.CastAs<T2>();
                    case 2:
                        return base.Value.CastAs<T3>();
                    case 3:
                        return base.Value.CastAs<T4>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T2))
            {
                if (Index != 1)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T3))
            {
                if (Index != 2)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T4))
            {
                if (Index != 3)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else
                throw new ArgumentException("Wrong type", nameof(T));
            return base.Value.CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1, T2, T3, T4, T5> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    case 1:
                        return base.Value.CastAs<T2>();
                    case 2:
                        return base.Value.CastAs<T3>();
                    case 3:
                        return base.Value.CastAs<T4>();
                    case 4:
                        return base.Value.CastAs<T5>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T2))
            {
                if (Index != 1)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T3))
            {
                if (Index != 2)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T4))
            {
                if (Index != 3)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T5))
            {
                if (Index != 4)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else
                throw new ArgumentException("Wrong type", nameof(T));
            return base.Value.CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1, T2, T3, T4, T5, T6> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    case 1:
                        return base.Value.CastAs<T2>();
                    case 2:
                        return base.Value.CastAs<T3>();
                    case 3:
                        return base.Value.CastAs<T4>();
                    case 4:
                        return base.Value.CastAs<T5>();
                    case 5:
                        return base.Value.CastAs<T6>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T2))
            {
                if (Index != 1)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T3))
            {
                if (Index != 2)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T4))
            {
                if (Index != 3)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T5))
            {
                if (Index != 4)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T6))
            {
                if (Index != 5)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else
                throw new ArgumentException("Wrong type", nameof(T));
            return base.Value.CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1, T2, T3, T4, T5, T6, T7> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    case 1:
                        return base.Value.CastAs<T2>();
                    case 2:
                        return base.Value.CastAs<T3>();
                    case 3:
                        return base.Value.CastAs<T4>();
                    case 4:
                        return base.Value.CastAs<T5>();
                    case 5:
                        return base.Value.CastAs<T6>();
                    case 6:
                        return base.Value.CastAs<T7>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T2))
            {
                if (Index != 1)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T3))
            {
                if (Index != 2)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T4))
            {
                if (Index != 3)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T5))
            {
                if (Index != 4)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T6))
            {
                if (Index != 5)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T7))
            {
                if (Index != 6)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else
                throw new ArgumentException("Wrong type", nameof(T));
            return base.Value.CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class for C# specializations of std::variant.
    /// </summary>
    public class variant<T1, T2, T3, T4, T5, T6, T7, T8> : variant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="variant"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public variant(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the value stored in this instance converted to correct type. If <see cref="variant.Index"/> is <c>-1</c> this will be <c>null</c>.
        /// </summary>
        public new object Value
        {
            get
            {
                switch (Index)
                {
                    case 0:
                        return base.Value.CastAs<T1>();
                    case 1:
                        return base.Value.CastAs<T2>();
                    case 2:
                        return base.Value.CastAs<T3>();
                    case 3:
                        return base.Value.CastAs<T4>();
                    case 4:
                        return base.Value.CastAs<T5>();
                    case 5:
                        return base.Value.CastAs<T6>();
                    case 6:
                        return base.Value.CastAs<T7>();
                    case 7:
                        return base.Value.CastAs<T8>();
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets value stored in std::variant casted as T if it is correct by index, or throws exception if index is not correct.
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                if (Index != 0)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T2))
            {
                if (Index != 1)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T3))
            {
                if (Index != 2)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T4))
            {
                if (Index != 3)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T5))
            {
                if (Index != 4)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T6))
            {
                if (Index != 5)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T7))
            {
                if (Index != 6)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else if (typeof(T) == typeof(T8))
            {
                if (Index != 7)
                    throw new ArgumentException("Bad type stored in std::variant", nameof(T));
            }
            else
                throw new ArgumentException("Wrong type", nameof(T));
            return base.Value.CastAs<T>();
        }
    }
}
