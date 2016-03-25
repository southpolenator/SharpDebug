using System;

namespace CsScripts
{
    /// <summary>
    /// Wrapper class that represents a pointer to void (i.e. void*)
    /// </summary>
    public class NakedPointer : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public NakedPointer(Variable variable)
            : base(variable)
        {
            if (!GetCodeType().IsPointer)
            {
                throw new Exception("Wrong code type of passed variable " + GetCodeType().Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is null.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is null; otherwise, <c>false</c>.
        /// </value>
        public bool IsNull
        {
            get
            {
                return IsNullPointer();
            }
        }

        /// <summary>
        /// Casts void pointer to a specified type.
        /// </summary>
        /// <typeparam name="T">The type of element new pointer should point to.</typeparam>
        public new CodePointer<T> CastAs<T>()
        {
            return new CodePointer<T>(this);
        }

        /// <summary>
        /// Reads the string.
        /// </summary>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public string ReadString(int charSize, int length = -1)
        {
            return UserType.ReadString(GetCodeType().Module.Process, GetPointerAddress(), charSize, length);
        }
    }
}
