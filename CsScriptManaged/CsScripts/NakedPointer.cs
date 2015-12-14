using CsScripts;
using System;

namespace CsScriptManaged.CsScripts
{
    /// <summary>
    /// Helper class that represents pointer to void (i.e. void*)
    /// </summary>
    public class NakedPointer
    {
        /// <summary>
        /// The actual variable where we get all the values.
        /// </summary>
        private Variable variable;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public NakedPointer(Variable variable)
        {
            if (!variable.GetCodeType().IsPointer)
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            this.variable = variable;
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

        /// <summary>
        /// Casts void pointer to a specified type.
        /// </summary>
        /// <typeparam name="T">The type of element new pointer should point to.</typeparam>
        public CodePointer<T> CastAs<T>()
        {
            return new CodePointer<T>(variable);
        }
    }
}
