using CsDebugScript.Exceptions;
using System;

namespace CsDebugScript
{
    /// <summary>
    /// Wrapper class that represents a pointer to void (i.e. void*)
    /// </summary>
    public class NakedPointer : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NakedPointer"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public NakedPointer(Variable variable)
            : base(variable)
        {
            if (!GetCodeType().IsPointer)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "pointer");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NakedPointer"/> class.
        /// </summary>
        /// <param name="pointerType">Type of the pointer.</param>
        /// <param name="address">The address.</param>
        public NakedPointer(CodeType pointerType, ulong address)
            : this(Variable.CreatePointerNoCast(pointerType, address))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NakedPointer"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        public NakedPointer(Process process, ulong address)
            : this(GetPointerCodeType(process), address)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NakedPointer"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        public NakedPointer(ulong address)
            : this(Process.Current, address)
        {
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
        /// Gets the pointer code type for the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        private static CodeType GetPointerCodeType(Process process)
        {
            return CodeType.Create(CodeType.NakedPointerCodeTypeName, process.Modules[0]);
        }
    }
}
