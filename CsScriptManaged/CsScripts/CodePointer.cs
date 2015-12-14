using CsScripts;
using System;

namespace CsScriptManaged.CsScripts
{
    /// <summary>
    /// Helper class that represents pointer
    /// </summary>
    /// <typeparam name="T">The type of element this pointer points to.</typeparam>
    public class CodePointer<T>
    {
        /// <summary>
        /// The actual variable where we get all the values.
        /// </summary>
        private Variable variable;

        /// <summary>
        /// The element
        /// </summary>
        private UserMember<T> element;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodePointer(Variable variable)
        {
            if (!variable.GetCodeType().IsPointer)
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            this.variable = variable;
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
                return variable.IsNullPointer();
            }
        }
    }
}
