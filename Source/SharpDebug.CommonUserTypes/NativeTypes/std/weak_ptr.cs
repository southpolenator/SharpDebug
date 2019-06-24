using SharpDebug.Exceptions;
using System;
using System.Text;

namespace SharpDebug.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::weak_ptr
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class weak_ptr<T> : UserType
    {
        /// <summary>
        /// Verifies that type user type can work with the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if user type can work with the specified code type; <c>false</c> otherwise</returns>
        public static bool VerifyCodeType(CodeType codeType)
        {
            return shared_ptr<T>.typeSelector.VerifyCodeType(codeType);
        }

        /// <summary>
        /// The instance used to read variable data
        /// </summary>
        private shared_ptr<T>.Ishared_ptr instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="weak_ptr{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public weak_ptr(Variable variable)
            : base(variable)
        {
            // Verify code type
            instance = shared_ptr<T>.typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::weak_ptr");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is created with make shared.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is created with make shared; otherwise, <c>false</c>.
        /// </value>
        public bool IsCreatedWithMakeShared => instance.IsCreatedWithMakeShared;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => instance.IsEmpty || instance.SharedCount <= 0;

        /// <summary>
        /// Gets the dereferenced pointer.
        /// </summary>
        [ForceDefaultVisualizerAtttribute]
        public T Element
        {
            get
            {
                if (SharedCount > 0)
                {
                    return UnsafeElement;
                }

                throw new AccessViolationException("Dereferencing weak pointer with shared count == 0");
            }
        }

        /// <summary>
        /// Gets the unsafe dereferenced pointer (not checking if shared count > 0).
        /// </summary>
        public T UnsafeElement => instance.Element;

        /// <summary>
        /// Gets the shared count.
        /// </summary>
        public int SharedCount => instance.SharedCount;

        /// <summary>
        /// Gets the weak count.
        /// </summary>
        public int WeakCount => instance.WeakCount;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (instance.IsEmpty)
                return "empty";

            StringBuilder sb = new StringBuilder();

            if (SharedCount == 0)
                sb.Append("expired ");
            else
            {
                sb.Append("weak_ptr ");
                sb.Append(Element?.ToString());
            }
            sb.Append(" [");
            if (SharedCount > 0)
            {
                sb.Append(SharedCount);
                if (SharedCount == 1)
                    sb.Append(" strong ref, ");
                else
                    sb.Append(" strong refs, ");
            }
            int weakCount = WeakCount - (SharedCount > 0 ? 1 : 0);
            sb.Append(weakCount);
            if (weakCount == 1)
                sb.Append(" weak ref");
            else
                sb.Append(" weak refs");
            sb.Append("]");
            if (IsCreatedWithMakeShared)
                sb.Append(" [make_shared]");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Simplification class for creating <see cref="weak_ptr{T}"/> with T being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::weak_ptr<>", CodeTypeVerification = nameof(weak_ptr.VerifyCodeType))]
    [UserType(TypeName = "std::__1::weak_ptr<>", CodeTypeVerification = nameof(weak_ptr.VerifyCodeType))]
    public class weak_ptr : weak_ptr<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="weak_ptr"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public weak_ptr(Variable variable)
            : base(variable)
        {
        }
    }
}
