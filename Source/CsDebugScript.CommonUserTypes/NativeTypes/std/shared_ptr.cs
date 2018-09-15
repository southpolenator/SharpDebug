using CsDebugScript.Exceptions;
using System;
using System.Text;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::shared_ptr
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class shared_ptr<T> : UserType
    {
        /// <summary>
        /// Interface that describes shared_ptr instance abilities.
        /// </summary>
        internal interface Ishared_ptr
        {
            /// <summary>
            /// Gets the shared count.
            /// </summary>
            int SharedCount { get; }

            /// <summary>
            /// Gets the weak count.
            /// </summary>
            int WeakCount { get; }

            /// <summary>
            /// Gets the dereferenced pointer.
            /// </summary>
            T Element { get; }

            /// <summary>
            /// Gets a value indicating whether this instance is empty.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
            /// </value>
            bool IsEmpty { get; }

            /// <summary>
            /// Gets a value indicating whether this instance is created with make shared.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is created with make shared; otherwise, <c>false</c>.
            /// </value>
            bool IsCreatedWithMakeShared { get; }
        }

        /// <summary>
        /// Common code for all implementations of std::shared_ptr
        /// </summary>
        internal class SharedPtrBase : Ishared_ptr
        {
            /// <summary>
            /// Code type extracted data
            /// </summary>
            protected class ExtractedData
            {
                /// <summary>
                /// Offset of pointer field.
                /// </summary>
                public int PointerOffset;

                /// <summary>
                /// Code type of pointer field element.
                /// </summary>
                public CodeType PointerCodeType;

                /// <summary>
                /// Offset of reference counting structure pointer.
                /// </summary>
                public int ReferenceCountPointerOffset;

                /// <summary>
                /// Code type of reference counting structure.
                /// </summary>
                public CodeType ReferenceCountCodeType;

                /// <summary>
                /// Function that reads shared count from reference counting structure.
                /// </summary>
                public Func<ulong, int> ReadSharedCount;

                /// <summary>
                /// Function that reads weak count from reference counting structure.
                /// </summary>
                public Func<ulong, int> ReadWeakCount;

                /// <summary>
                /// Function that tests specified code type if it is reference counting structure created with std::make_shared.
                /// </summary>
                public Func<CodeType, bool> TestCreatedWithMakeShared;

                /// <summary>
                /// Code type of std::shared_ptr.
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
            /// Address stored in pointer field.
            /// </summary>
            private ulong pointerAddress;

            /// <summary>
            /// Address of reference counting structure.
            /// </summary>
            private ulong referenceCountAddress;

            /// <summary>
            /// Flag that indicated whether this instance was created with make shared
            /// </summary>
            private UserMember<bool> isCreatedWithMakeShared;

            /// <summary>
            /// Initializes a new instance of the <see cref="SharedPtrBase"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public SharedPtrBase(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                ulong address = variable.GetPointerAddress();
                pointerAddress = data.Process.ReadPointer(address + (uint)data.PointerOffset);
                referenceCountAddress = data.Process.ReadPointer(address + (uint)data.ReferenceCountPointerOffset);
                isCreatedWithMakeShared = UserMember.Create(() => data.TestCreatedWithMakeShared(Variable.Create(data.ReferenceCountCodeType, referenceCountAddress).DowncastInterface().GetCodeType()));
            }

            /// <summary>
            /// Gets a value indicating whether this instance is created with make shared.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is created with make shared; otherwise, <c>false</c>.
            /// </value>
            public bool IsCreatedWithMakeShared => isCreatedWithMakeShared.Value;

            /// <summary>
            /// Gets a value indicating whether this instance is empty.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
            /// </value>
            public bool IsEmpty => pointerAddress == 0;

            /// <summary>
            /// Gets the dereferenced pointer.
            /// </summary>
            public T Element => Variable.Create(data.PointerCodeType, pointerAddress).CastAs<T>();

            /// <summary>
            /// Gets the shared count.
            /// </summary>
            public int SharedCount => data.ReadSharedCount(referenceCountAddress);

            /// <summary>
            /// Gets the weak count.
            /// </summary>
            public int WeakCount => data.ReadWeakCount(referenceCountAddress);
        }

        /// <summary>
        /// Microsoft Visual Studio implementations of std::shared_ptr
        /// </summary>
        internal class VisualStudio : SharedPtrBase
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
                // _Ptr
                // _Rep
                // | _Uses
                // | _Weaks
                CodeType _Rep, _Ptr, _Uses, _Weaks;

                if (!codeType.GetFieldTypes().TryGetValue("_Ptr", out _Ptr))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("_Rep", out _Rep))
                    return null;
                if (!_Rep.GetFieldTypes().TryGetValue("_Uses", out _Uses))
                    return null;
                if (!_Rep.GetFieldTypes().TryGetValue("_Weaks", out _Weaks))
                    return null;

                return new ExtractedData
                {
                    PointerOffset = codeType.GetFieldOffset("_Ptr"),
                    PointerCodeType = _Ptr.ElementType,
                    ReferenceCountPointerOffset = codeType.GetFieldOffset("_Rep"),
                    ReferenceCountCodeType = _Rep.ElementType,
                    TestCreatedWithMakeShared = (testCodeType) => testCodeType.Name.StartsWith("std::_Ref_count_obj<"),
                    ReadSharedCount = codeType.Module.Process.GetReadInt(_Rep.ElementType, "_Uses"),
                    ReadWeakCount = codeType.Module.Process.GetReadInt(_Rep.ElementType, "_Weaks"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// libstdc++ 6 implementations of std::shared_ptr
        /// </summary>
        internal class LibStdCpp6 : SharedPtrBase
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
                // _M_ptr
                // _M_refcount
                // | _M_pi
                //   | _M_use_count
                //   | _M_weak_count
                CodeType _M_refcount, _M_pi, _M_ptr, _M_use_count, _M_weak_count;

                if (!codeType.GetFieldTypes().TryGetValue("_M_ptr", out _M_ptr))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("_M_refcount", out _M_refcount))
                    return null;
                if (!_M_refcount.GetFieldTypes().TryGetValue("_M_pi", out _M_pi))
                    return null;
                if (!_M_pi.GetFieldTypes().TryGetValue("_M_use_count", out _M_use_count))
                    return null;
                if (!_M_pi.GetFieldTypes().TryGetValue("_M_weak_count", out _M_weak_count))
                    return null;

                return new ExtractedData
                {
                    PointerOffset = codeType.GetFieldOffset("_M_ptr"),
                    PointerCodeType = _M_ptr.ElementType,
                    ReferenceCountPointerOffset = codeType.GetFieldOffset("_M_refcount") + _M_refcount.GetFieldOffset("_M_pi"),
                    ReferenceCountCodeType = _M_pi.ElementType,
                    TestCreatedWithMakeShared = (testCodeType) =>
                    {
                        if (testCodeType.Name.StartsWith("std::_Sp_counted_ptr_inplace<"))
                        {
                            return true;
                        }

                        if (!testCodeType.Name.StartsWith("std::_Sp_counted_deleter<"))
                        {
                            return false;
                        }

                        try
                        {
                            testCodeType = (CodeType)testCodeType.TemplateArguments[1];
                            return testCodeType.Name.StartsWith("std::__shared_ptr<") && testCodeType.Name.Contains("::_Deleter<");
                        }
                        catch
                        {
                            return false;
                        }
                    },
                    ReadSharedCount = codeType.Module.Process.GetReadInt(_M_pi.ElementType, "_M_use_count"),
                    ReadWeakCount = codeType.Module.Process.GetReadInt(_M_pi.ElementType, "_M_weak_count"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementation of std::shared_ptr
        /// </summary>
        internal class ClangLibCpp : SharedPtrBase
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
                // __ptr_
                // __cntrl_
                // | __shared_owners_
                // | __shared_weak_owners_
                CodeType __ptr_, __cntrl_, __shared_owners_, __shared_weak_owners_;

                if (!codeType.GetFieldTypes().TryGetValue("__ptr_", out __ptr_))
                    return null;
                if (!codeType.GetFieldTypes().TryGetValue("__cntrl_", out __cntrl_))
                    return null;
                if (!__cntrl_.GetFieldTypes().TryGetValue("__shared_owners_", out __shared_owners_))
                    return null;
                if (!__cntrl_.GetFieldTypes().TryGetValue("__shared_weak_owners_", out __shared_weak_owners_))
                    return null;

                Func<ulong, int> readSharedCount = codeType.Module.Process.GetReadInt(__cntrl_.ElementType, "__shared_owners_");
                Func<ulong, int> readWeakCount = codeType.Module.Process.GetReadInt(__cntrl_.ElementType, "__shared_weak_owners_");

                return new ExtractedData
                {
                    PointerOffset = codeType.GetFieldOffset("__ptr_"),
                    PointerCodeType = __ptr_.ElementType,
                    ReferenceCountPointerOffset = codeType.GetFieldOffset("__cntrl_"),
                    ReferenceCountCodeType = __cntrl_.ElementType,
                    TestCreatedWithMakeShared = (testCodeType) => testCodeType.Name.StartsWith("std::__1::__shared_ptr_emplace<"),
                    ReadSharedCount = (address) => readSharedCount(address) + 1,
                    ReadWeakCount = (address) => readWeakCount(address) + 1,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        internal static TypeSelector<Ishared_ptr> typeSelector = new TypeSelector<Ishared_ptr>(new[]
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
        private Ishared_ptr instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="shared_ptr{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public shared_ptr(Variable variable)
            : base(variable)
        {
            // Verify code type
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::shared_ptr");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is created with make shared.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is created with make shared; otherwise, <c>false</c>.
        /// </value>
        public bool IsCreatedWithMakeShared
        {
            get
            {
                return instance.IsCreatedWithMakeShared;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                return instance.IsEmpty;
            }
        }

        /// <summary>
        /// Gets the dereferenced pointer.
        /// </summary>
        [ForceDefaultVisualizerAtttribute]
        public T Element
        {
            get
            {
                return instance.Element;
            }
        }

        /// <summary>
        /// Gets the shared count.
        /// </summary>
        public int SharedCount
        {
            get
            {
                return instance.SharedCount;
            }
        }

        /// <summary>
        /// Gets the weak count.
        /// </summary>
        public int WeakCount
        {
            get
            {
                return instance.WeakCount;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsEmpty)
                return "empty";

            StringBuilder sb = new StringBuilder();

            sb.Append("shared_ptr ");
            sb.Append(Element?.ToString());
            sb.Append(" [");
            sb.Append(SharedCount);
            if (SharedCount == 1)
                sb.Append(" strong ref");
            else
                sb.Append(" strong refs");
            if (WeakCount > 1)
            {
                sb.Append(", ");
                sb.Append(WeakCount - 1);
                if (WeakCount == 2)
                    sb.Append(" weak ref");
                else
                    sb.Append(" weak refs");
            }
            sb.Append("]");
            if (IsCreatedWithMakeShared)
                sb.Append(" [make_shared]");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Simplification class for creating <see cref="shared_ptr{T}"/> with T being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::shared_ptr<>", CodeTypeVerification = nameof(shared_ptr.VerifyCodeType))]
    [UserType(TypeName = "std::__1::shared_ptr<>", CodeTypeVerification = nameof(shared_ptr.VerifyCodeType))]
    public class shared_ptr : shared_ptr<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="shared_ptr"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public shared_ptr(Variable variable)
            : base(variable)
        {
        }
    }
}
