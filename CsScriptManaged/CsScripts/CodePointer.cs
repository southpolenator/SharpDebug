using System;
using System.Linq;

namespace CsScripts
{
    /// <summary>
    /// Wrapper class that represents a pointer.
    /// </summary>
    /// <typeparam name="T">The type of element this pointer points to.</typeparam>
    public class CodePointer<T> : Variable
    {
        /// <summary>
        /// The element
        /// </summary>
        private UserMember<T> element;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodePointer(Variable variable)
            : base(variable)
        {
            if (!GetCodeType().IsPointer)
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            element = UserMember.Create(() => variable.DereferencePointer().CastAs<T>());
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        public T Element
        {
            get
            {
                return element.Value;
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
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public CodeArray<T> ConvertToArray(int length)
        {
            return new CodeArray<T>(this, length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public T[] ToArray(int length)
        {
            return ConvertToArray(length).ToArray();
        }
    }
}
