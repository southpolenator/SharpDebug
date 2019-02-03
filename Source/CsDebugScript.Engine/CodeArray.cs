using CsDebugScript.Engine;
using CsDebugScript.Exceptions;
using SharpUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CsDebugScript
{
    /// <summary>
    /// Extension specialization functions for CodeArray.
    /// </summary>
    public static class CodeArrayExtensions
    {
        /// <summary>
        /// Reads the string from CodeArray.
        /// </summary>
        /// <param name="codeArray">The code array.</param>
        /// <returns>Read string from CodeArray.</returns>
        public static string ReadString(this CodeArray<char> codeArray)
        {
            if (codeArray.variable != null)
            {
                return UserType.ReadString(codeArray.variable.GetCodeType().Module.Process, codeArray.variable.GetPointerAddress(), (int)codeArray.variable.GetCodeType().ElementType.Size, codeArray.Length);
            }
            else
            {
                return new string(codeArray.ToArray());
            }
        }

        /// <summary>
        /// Reads the memory buffer from CodeArray.
        /// </summary>
        /// <param name="codeArray">The code array.</param>
        /// <returns>Read memory buffer from CodeArray.</returns>
        public static MemoryBuffer ReadMemory(this CodeArray<byte> codeArray)
        {
            return Debugger.ReadMemory(codeArray.variable, (uint)codeArray.Length);
        }
    }

    /// <summary>
    /// Wrapper class that represents a "static" array. For example "int a[4]";
    /// </summary>
    /// <typeparam name="T">The type of elements in the array</typeparam>
    public class CodeArray<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// The actual variable where we get all the values.
        /// </summary>
        internal Variable variable;

        /// <summary>
        /// The pre-calculated array (if we were initialized with it, or we know how to read whole array)
        /// </summary>
        private IReadOnlyList<T> preCalculatedArray;

        /// <summary>
        /// The addresses array (if we don't know how to read the array, but we know that we have array of pointer and we could optimize a bit)
        /// </summary>
        private ulong[] addressesArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodeArray(Variable variable)
        {
            if (!variable.GetCodeType().IsArray)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "array");
            }

            Initialize(variable, variable.GetArrayLength());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, int length)
        {
            if (!variable.GetCodeType().IsArray && !variable.GetCodeType().IsPointer)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "array or pointer");
            }

            Initialize(variable, length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, uint length)
        {
            if (!variable.GetCodeType().IsArray && !variable.GetCodeType().IsPointer)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "array or pointer");
            }

            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Initialize(variable, (int)length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, ulong length)
        {
            if (!variable.GetCodeType().IsArray && !variable.GetCodeType().IsPointer)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "array or pointer");
            }

            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Initialize(variable, (int)length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, long length)
        {
            if (!variable.GetCodeType().IsArray && !variable.GetCodeType().IsPointer)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "array or pointer");
            }

            if (length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Initialize(variable, (int)length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="preCalculatedArray">The pre-calculated array.</param>
        public CodeArray(T[] preCalculatedArray)
        {
            this.preCalculatedArray = preCalculatedArray;
            Length = preCalculatedArray.Length;
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return Length;
            }
        }

        /// <summary>
        /// Gets the &lt;T&gt; at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public T this[int index]
        {
            get
            {
                if (preCalculatedArray != null)
                {
                    return preCalculatedArray[index];
                }

                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Index out of array length");
                }

                Variable item;

                if (addressesArray != null)
                {
                    if (addressesArray[index] == 0)
                        item = null;
                    else if (typeof(T) == typeof(Variable))
                        item = Variable.CreatePointer(variable.GetCodeType().ElementType, addressesArray[index]);
                    else
                        item = Variable.CreatePointerNoCast(variable.GetCodeType().ElementType, addressesArray[index]);
                }
                else
                {
                    item = variable.GetArrayElement(index);
                }

                if (item == null)
                {
                    return default(T);
                }

                return item.CastAs<T>();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Enumerates this array.
        /// </summary>
        private IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
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
            return $"Length = {Length}, Type = {variable.GetCodeType().ElementType}";
        }

        /// <summary>
        /// Initializes this instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        private void Initialize(Variable variable, int length)
        {
            variable = CodePointer<T>.CastIfNecessary(variable);
            this.variable = variable;
            Length = length;
            preCalculatedArray = ReadArray();
            if (preCalculatedArray == null && variable.GetCodeType().ElementType.IsPointer && length > 0)
            {
                var process = variable.GetCodeType().Module.Process;
                var pointerSize = process.GetPointerSize();
                var buffer = Debugger.ReadMemory(process, variable.GetPointerAddress(), (uint)Length * pointerSize);

                addressesArray = UserType.ReadPointerArray(buffer, 0, Length, pointerSize);
            }
        }

        /// <summary>
        /// Gets the user type delegates for the type T.
        /// </summary>
        private IUserTypeDelegates<T> GetDelegates()
        {
            var elementType = variable.GetCodeType().ElementType;
            var type = typeof(T);

            if (!elementType.IsPointer) // TODO: Will this work for single pointers?
            {
                if (type.GetTypeInfo().IsSubclassOf(typeof(UserType)))
                {
                    foreach (Type ut in elementType.UserTypes)
                    {
                        if (ut == type)
                        {
                            return UserTypeDelegates<T>.Instance;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Reads the precalculated array.
        /// </summary>
        private unsafe IReadOnlyList<T> ReadArray()
        {
            // Check if we have phisical constructor in delegates
            var delegates = GetDelegates();

            if (delegates != null && delegates.PhysicalConstructor != null)
            {
                var address = variable.GetPointerAddress();
                var elementType = variable.GetCodeType().ElementType;

                if (elementType.Module.Process.DumpFileMemoryReader != null)
                {
                    var buffer = Debugger.ReadMemory(elementType.Module.Process, address, (uint)(Length * elementType.Size));

                    return new BufferedElementCreatorReadOnlyList(delegates, elementType, buffer, address, address);
                }

                return new ElementCreatorReadOnlyList(delegates, elementType, address);
            }

            // Check if type is basic type and we can read it directly with marshaler.
            Type type = typeof(T);

            if (type.IsPrimitive)
            {
                // Try to use binary conversion of memory blocks
                try
                {
                    CodeType builtinCodeType = BuiltinCodeTypes.GetCodeType<T>(variable.GetCodeType().Module);
                    CodeType elementType = variable.GetCodeType().ElementType;

                    if ((builtinCodeType == elementType)
                       || (builtinCodeType.Size == elementType.Size && elementType.IsSimple && builtinCodeType.IsDouble == elementType.IsDouble && builtinCodeType.IsFloat == elementType.IsFloat))
                    {
                        var address = variable.GetPointerAddress();
                        uint bytesCount = (uint)(Length * elementType.Size);
                        var buffer = Debugger.ReadMemory(elementType.Module.Process, address, bytesCount);

                        if (elementType.Module.Process.DumpFileMemoryReader == null)
                        {
                            if (type != typeof(byte))
                            {
                                T[] result = new T[Length];

                                Buffer.BlockCopy(buffer.Bytes, 0, result, 0, buffer.Bytes.Length);
                                return result;
                            }

                            return (T[])(object)buffer.Bytes;
                        }
                        else
                        {
                            T[] result = new T[Length];

                            var handle = System.Runtime.InteropServices.GCHandle.Alloc(result, System.Runtime.InteropServices.GCHandleType.Pinned);
                            try
                            {
                                void* pointer = handle.AddrOfPinnedObject().ToPointer();

                                MemoryBuffer.MemCpy(pointer, buffer.BytePointer, bytesCount);
                            }
                            finally
                            {
                                handle.Free();
                            }
                            return result;
                        }
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Reads all data to managed array.
        /// </summary>
        /// <returns>Managed array.</returns>
        public T[] ToArray()
        {
            if (preCalculatedArray != null && preCalculatedArray is T[])
            {
                return (T[])preCalculatedArray;
            }

            T[] result = new T[Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = this[i];
            }

            return result;
        }

        /// <summary>
        /// Partially buffered element creator helper class. It reads one element at the time.
        /// </summary>
        private class ElementCreatorReadOnlyList : IReadOnlyList<T>
        {
            /// <summary>
            /// The user type delegates
            /// </summary>
            private IUserTypeDelegates<T> delegates;

            /// <summary>
            /// The element type
            /// </summary>
            private CodeType elementType;

            /// <summary>
            /// The array start address
            /// </summary>
            private ulong arrayStartAddress;

            /// <summary>
            /// The element type size
            /// </summary>
            private uint elementTypeSize;

            /// <summary>
            /// Initializes a new instance of the <see cref="ElementCreatorReadOnlyList"/> class.
            /// </summary>
            /// <param name="delegates">The user type delegates.</param>
            /// <param name="elementType">The element type.</param>
            /// <param name="arrayStartAddress">The array start address.</param>
            public ElementCreatorReadOnlyList(IUserTypeDelegates<T> delegates, CodeType elementType, ulong arrayStartAddress)
            {
                this.delegates = delegates;
                this.elementType = elementType;
                this.arrayStartAddress = arrayStartAddress;
                elementTypeSize = elementType.Size;
            }

            /// <summary>
            /// Gets the &lt;T&gt; at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            public T this[int index]
            {
                get
                {
                    ulong address = arrayStartAddress + (ulong)index * elementTypeSize;
                    var buffer = Debugger.ReadMemory(elementType.Module.Process, address, elementTypeSize);

                    return delegates.PhysicalConstructor(buffer, 0, address, elementType, address, Variable.ComputedName, Variable.UntrackedPath);
                }
            }

            #region Intentionally not implementing
            public int Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
            #endregion
        }

        /// <summary>
        /// Fully buffered element creator helper class. It reads the whole array into memory buffer and returns elements from within that buffer.
        /// </summary>
        private class BufferedElementCreatorReadOnlyList : IReadOnlyList<T>
        {
            /// <summary>
            /// The user type delegates
            /// </summary>
            private IUserTypeDelegates<T> delegates;

            /// <summary>
            /// The element type
            /// </summary>
            private CodeType elementType;

            /// <summary>
            /// The array start address
            /// </summary>
            private ulong arrayStartAddress;

            /// <summary>
            /// The element type size
            /// </summary>
            private uint elementTypeSize;

            /// <summary>
            /// The memory buffer
            /// </summary>
            private MemoryBuffer buffer;

            /// <summary>
            /// The memory buffer address
            /// </summary>
            private ulong bufferAddress;

            /// <summary>
            /// Initializes a new instance of the <see cref="BufferedElementCreatorReadOnlyList"/> class.
            /// </summary>
            /// <param name="delegates">The user type delegates.</param>
            /// <param name="elementType">The element type.</param>
            /// <param name="buffer">The memory buffer.</param>
            /// <param name="bufferAddress">The memory buffer address.</param>
            /// <param name="arrayStartAddress">The array start address.</param>
            public BufferedElementCreatorReadOnlyList(IUserTypeDelegates<T> delegates, CodeType elementType, MemoryBuffer buffer, ulong bufferAddress, ulong arrayStartAddress)
            {
                this.delegates = delegates;
                this.elementType = elementType;
                this.arrayStartAddress = arrayStartAddress;
                this.buffer = buffer;
                this.bufferAddress = bufferAddress;
                elementTypeSize = elementType.Size;
            }

            /// <summary>
            /// Gets the &lt;T&gt; at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            public T this[int index]
            {
                get
                {
                    int offset = (int)(index * elementTypeSize);
                    ulong address = arrayStartAddress + (ulong)offset;

                    return delegates.PhysicalConstructor(buffer, offset, bufferAddress, elementType, address, Variable.ComputedName, Variable.UntrackedPath);
                }
            }

            #region Intentionally not implementing
            public int Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
            #endregion
        }
    }

    /// <summary>
    /// Simple wrapper class for handling unknown element type of array as <see cref="CodeArray{Variable}"/>.
    /// </summary>
    public class CodeArray : CodeArray<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodeArray(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray"/> class.
        /// </summary>
        /// <param name="preCalculatedArray">The pre-calculated array.</param>
        public CodeArray(Variable[] preCalculatedArray)
            : base(preCalculatedArray)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, int length)
            : base(variable, length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, uint length)
            : base(variable, length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, ulong length)
            : base(variable, length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, long length)
            : base(variable, length)
        {
        }
    }
}
