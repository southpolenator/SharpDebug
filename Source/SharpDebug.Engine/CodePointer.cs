﻿using SharpDebug.Exceptions;
using System;
using System.Linq;

namespace SharpDebug
{
    /// <summary>
    /// Extension specialization functions for CodePointer.
    /// </summary>
    public static class CodePointerExtensions
    {
        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters. If length is -1, string is null terminated</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadString(this CodePointer<char> codePointer, int length = -1)
        {
            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetPointerAddress(), (int)codePointer.GetCodeType().ElementType.Size, length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters.</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadString(this CodePointer<char> codePointer, uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadString(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters.</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadString(this CodePointer<char> codePointer, long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadString(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in characters.</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadString(this CodePointer<char> codePointer, ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadString(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer with length specified as number of bytes.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes. If length is -1, string is null terminated</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, int length = -1)
        {
            int charSize = (int)codePointer.GetCodeType().ElementType.Size;

            if (length % charSize != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return UserType.ReadString(codePointer.GetCodeType().Module.Process, codePointer.GetPointerAddress(), charSize, length / charSize);
        }

        /// <summary>
        /// Reads the string from CodePointer with length specified as number of bytes.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes.</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadStringByteLength(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer with length specified as number of bytes.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes.</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadStringByteLength(codePointer, (int)length);
        }

        /// <summary>
        /// Reads the string from CodePointer with length specified as number of bytes.
        /// </summary>
        /// <param name="codePointer">The code pointer.</param>
        /// <param name="length">The length in bytes.</param>
        /// <returns>Read string from CodePointer.</returns>
        public static string ReadStringByteLength(this CodePointer<char> codePointer, ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadStringByteLength(codePointer, (int)length);
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
            : base(CastIfNecessary(variable))
        {
            if (!GetCodeType().IsPointer)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "pointer");
            }

            element = UserMember.Create(() => DereferencePointer().CastAs<T>());
        }

        /// <summary>
        /// Casts the specified variable to type that is usable by this class if it was naked pointer.
        /// </summary>
        /// <param name="variable">The variable.</param>
        internal static Variable CastIfNecessary(Variable variable)
        {
            if (variable.GetCodeType() is NakedPointerCodeType && !variable.IsNull())
            {
                // TODO: CodeType newCodeType = CodeType.Create<T>();
                CodeType newCodeType = BuiltinCodeTypes.GetCodeType<T>(variable.GetCodeType().Module);

                newCodeType = newCodeType.PointerToType;
                return variable.CastAs(newCodeType);
            }

            return variable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="pointerType">Type of the pointer.</param>
        /// <param name="address">The address.</param>
        public CodePointer(CodeType pointerType, ulong address)
            : this(Variable.CreatePointerNoCast(pointerType, address))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        public CodePointer(Process process, ulong address)
            : this(new NakedPointer(process, address))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer{T}"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        public CodePointer(ulong address)
            : this(new NakedPointer(address))
        {
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
        /// <param name="length">The number of elements.</param>
        /// <returns>CodeArray with specified number of elements</returns>
        public CodeArray<T> ToCodeArray(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ToCodeArray((int)length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The number of elements.</param>
        /// <returns>CodeArray with specified number of elements</returns>
        public CodeArray<T> ToCodeArray(int length)
        {
            return new CodeArray<T>(this, length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The number of elements.</param>
        /// <returns>Array with specified number of elements</returns>
        public T[] ToArray(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ToArray((int)length);
        }

        /// <summary>
        /// Converts pointer to array of specified length.
        /// </summary>
        /// <param name="length">The number of elements.</param>
        /// <returns>Array with specified number of elements</returns>
        public T[] ToArray(int length)
        {
            return ToCodeArray(length).ToArray();
        }

        /// <summary>
        /// Gets the &lt;T&gt; at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public new T this[int index]
        {
            get
            {
                return GetArrayElement(index).CastAs<T>();
            }
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters. If length is -1, string is null terminated.</param>
        /// <returns>Read ANSI string from CodePointer.</returns>
        public string ReadAnsiString(int length = -1)
        {
            return UserType.ReadString(GetCodeType().Module.Process, GetPointerAddress(), 1, length);
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters.</param>
        /// <returns>Read ANSI string from CodePointer.</returns>
        public string ReadAnsiString(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadAnsiString((int)length);
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters. If length is -1, string is null terminated.</param>
        /// <returns>Read ANSI string from CodePointer.</returns>
        public string ReadAnsiString(long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadAnsiString((int)length);
        }

        /// <summary>
        /// Reads the ANSI string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters.</param>
        /// <returns>Read ANSI string from CodePointer.</returns>
        public string ReadAnsiString(ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadAnsiString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters. If length is -1, string is null terminated.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeString(int length = -1)
        {
            return UserType.ReadString(GetCodeType().Module.Process, GetPointerAddress(), 2, length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeString(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters. If length is -1, string is null terminated.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeString(long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of characters.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeString(ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of bytes. If length is -1, string is null terminated.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeStringByteLength(int length)
        {
            if ((length & 1) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeString(length / 2);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of bytes.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeStringByteLength(uint length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeStringByteLength((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of bytes. If length is -1, string is null terminated.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeStringByteLength(long length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeStringByteLength((int)length);
        }

        /// <summary>
        /// Reads the Unicode string from CodePointer.
        /// </summary>
        /// <param name="length">The number of bytes.</param>
        /// <returns>Read Unicode string from CodePointer.</returns>
        public string ReadUnicodeStringByteLength(ulong length)
        {
            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return ReadUnicodeStringByteLength((int)length);
        }
    }

    /// <summary>
    /// Simple wrapper class for handling unknown element type of pointer as <see cref="CodeArray{Variable}"/>.
    /// </summary>
    public class CodePointer : CodePointer<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodePointer(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        public CodePointer(ulong address)
            : base(address)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer"/> class.
        /// </summary>
        /// <param name="pointerType">Type of the pointer.</param>
        /// <param name="address">The address.</param>
        public CodePointer(CodeType pointerType, ulong address)
            : base(pointerType, address)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodePointer"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The address.</param>
        public CodePointer(Process process, ulong address)
            : base(process, address)
        {
        }
    }
}
