using CsDebugScript.Exceptions;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::shared_ptr
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class shared_ptr<T>
    {
        /// <summary>
        /// The pointer field
        /// </summary>
        private UserMember<Variable> pointer;

        /// <summary>
        /// The dereferenced pointer
        /// </summary>
        private UserMember<T> element;

        /// <summary>
        /// The shared count
        /// </summary>
        private UserMember<int> sharedCount;

        /// <summary>
        /// The weak count
        /// </summary>
        private UserMember<int> weakCount;

        /// <summary>
        /// Flag that indicated whether this instance was created with make shared
        /// </summary>
        private UserMember<bool> isCreatedWithMakeShared;

        /// <summary>
        /// Initializes a new instance of the <see cref="shared_ptr{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public shared_ptr(Variable variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::shared_ptr");
            }

            // Initialize members
            pointer = UserMember.Create(() => variable.GetField("_Ptr"));
            element = UserMember.Create(() => pointer.Value.DereferencePointer().CastAs<T>());
            sharedCount = UserMember.Create(() => (int)variable.GetField("_Rep").GetField("_Uses"));
            weakCount = UserMember.Create(() => (int)variable.GetField("_Rep").GetField("_Weaks"));
            isCreatedWithMakeShared = UserMember.Create(() => variable.GetField("_Rep").DowncastInterface().GetCodeType().Name.StartsWith("std::_Ref_count_obj<"));
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
                return isCreatedWithMakeShared.Value;
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
                return pointer.Value.IsNullPointer();
            }
        }

        /// <summary>
        /// Gets the dereferenced pointer.
        /// </summary>
        public T Element
        {
            get
            {
                return element.Value;
            }
        }

        /// <summary>
        /// Gets the shared count.
        /// </summary>
        public int SharedCount
        {
            get
            {
                return sharedCount.Value;
            }
        }

        /// <summary>
        /// Gets the weak count.
        /// </summary>
        public int WeakCount
        {
            get
            {
                return weakCount.Value;
            }
        }

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private static bool VerifyCodeType(CodeType codeType)
        {
            // We want to have this kind of hierarchy
            // _Ptr
            // _Rep
            // | _Uses
            // | _Weaks
            CodeType _Rep, _Ptr, _Uses, _Weaks;

            if (!codeType.GetFieldTypes().TryGetValue("_Ptr", out _Ptr))
                return false;
            if (!codeType.GetFieldTypes().TryGetValue("_Rep", out _Rep))
                return false;
            if (!_Rep.GetFieldTypes().TryGetValue("_Uses", out _Uses))
                return false;
            if (!_Rep.GetFieldTypes().TryGetValue("_Weaks", out _Weaks))
                return false;
            return true;
        }
    }
}
