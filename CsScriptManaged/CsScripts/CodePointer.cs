using System;
using System.Linq;

namespace CsScripts
{
    /// <summary>
    /// Extension specialization functions for CodePointer.
    /// </summary>
    public static class CodePointerExtensions
    {
        /// <summary>
        /// Reads the string from CodeArray.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public static string ReadString(this CodePointer<char> codePointer, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetAddress(), (int)codePointer.GetCodeType().ElementType.Size, length);
        }

        /// <summary>
        /// Reads the string from CodeArray.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public static string ReadString(this CodePointer<short> codePointer, int charSize, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetAddress(), charSize, length);
        }

        /// <summary>
        /// Reads the string from CodeArray.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public static string ReadString(this CodePointer<ushort> codePointer, int charSize, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetAddress(), charSize, length);
        }

        /// <summary>
        /// Reads the string from CodeArray.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public static string ReadString(this CodePointer<byte> codePointer, int charSize, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetAddress(), charSize, length);
        }

        /// <summary>
        /// Reads the string from CodeArray.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public static string ReadString(this CodePointer<sbyte> codePointer, int charSize, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetAddress(), charSize, length);
        }

        /// <summary>
        /// Reads the string from CodeArray.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated</param>
        public static string ReadString(this CodePointer<VoidType> codePointer, int charSize, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetAddress(), charSize, length);
        }
    }

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
        public CodeArray<T> ToCodeArray(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            return ToCodeArray((int)length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public CodeArray<T> ToCodeArray(int length)
        {
            return new CodeArray<T>(this, length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public T[] ToArray(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            return ToArray((int)length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        public T[] ToArray(int length)
        {
            return ToCodeArray(length).ToArray();
        }

        /// <summary>
        /// Gets the &lt;T&gt; at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public T this[int index]
        {
            get
            {
                return GetArrayElement(index).CastAs<T>();
            }
        }
    }
}
