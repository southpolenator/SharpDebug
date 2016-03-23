using System;
using System.Collections.Generic;
using System.Collections;
using CsScriptManaged.Utility;
using CsScriptManaged;

namespace CsScripts
{
    /// <summary>
    /// Represents read-only list that can reuse element when getting new one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReuseUserTypeReadOnlyList<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// Reuses the specified element for getting the specified index.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">The index.</param>
        T Reuse(T element, int index);
    }

    /// <summary>
    /// Wrapper class that represents a "static" array. For example "int a[4]";
    /// </summary>
    /// <typeparam name="T">The type of elements in the array</typeparam>
    public class CodeArray<T> : IReuseUserTypeReadOnlyList<T>
    {
        /// <summary>
        /// The actual variable where we get all the values.
        /// </summary>
        private Variable variable;

        /// <summary>
        /// The pre-calculated array (if we were initialized with it, or we know how to read whole array)
        /// </summary>
        private IReadOnlyList<T> preCalculatedArray;

        /// <summary>
        /// The pre-calculated array that supports reuse
        /// </summary>
        private IReuseUserTypeReadOnlyList<T> preCalculatedReuseArray;

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
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
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
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            Initialize(variable, length);
        }

        /// <summary>
        /// Initializes this instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        private void Initialize(Variable variable, int length)
        {
            this.variable = variable;
            Length = length;
            InitPreCalculatedArray();
            if (preCalculatedArray == null && variable.GetCodeType().ElementType.IsPointer)
            {
                var process = variable.GetCodeType().Module.Process;
                var pointerSize = process.GetPointerSize();
                var buffer = Debugger.ReadMemory(process, variable.GetPointerAddress(), (uint)Length * pointerSize);

                addressesArray = UserType.ReadPointerArray(buffer, 0, Length, pointerSize);
            }
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
                    item = addressesArray[index] == 0 ? null : Variable.CreatePointerNoCast(variable.GetCodeType().ElementType, addressesArray[index]);
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
        /// Reuses the specified element for getting the specified index.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">The index.</param>
        public T Reuse(T element, int index)
        {
            if (element == null || preCalculatedReuseArray == null)
            {
                return this[index];
            }

            return preCalculatedReuseArray.Reuse(element, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
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

        private UserTypeDelegates<T> GetDelegates()
        {
            var elementType = variable.GetCodeType().ElementType;
            var type = typeof(T);

            if (!elementType.IsPointer)
            {
                if (type.IsSubclassOf(typeof(UserType)))
                {
                    var process = variable.GetCodeType().Module.Process;

                    // Verify that CodeType for this user type is exactly elementType
                    var descriptions = process.TypeToUserTypeDescription[type];

                    foreach (var description in descriptions)
                    {
                        CodeType newType = description.UserType;

                        if (newType == elementType)
                        {
                            return UserTypeDelegates<T>.Instance;
                        }
                    }
                }
            }

            return null;
        }

        private void InitPreCalculatedArray()
        {
            var delegates = GetDelegates();

            if (delegates != null && delegates.PhysicalConstructor != null)
            {
                var address = variable.GetPointerAddress();
                var elementType = variable.GetCodeType().ElementType;

                if (elementType.Module.Process.DumpFileMemoryReader != null)
                {
                    var buffer = Debugger.ReadMemory(elementType.Module.Process, address, (uint)(Length * elementType.Size));

                    preCalculatedArray = new BufferedElementCreatorReadOnlyList(delegates, elementType, buffer, address, address);
                }
                else
                {
                    preCalculatedArray = new ElementCreatorReadOnlyList(delegates, elementType, address);
                }

                if (delegates.SupportsUserTypeReuse)
                {
                    preCalculatedReuseArray = (IReuseUserTypeReadOnlyList<T>)preCalculatedArray;
                }
            }
        }

        private class ElementCreatorReadOnlyList : IReuseUserTypeReadOnlyList<T>
        {
            private UserTypeDelegates<T> delegates;
            private CodeType elementType;
            private ulong arrayStartAddress;
            private uint elementTypeSize;

            public ElementCreatorReadOnlyList(UserTypeDelegates<T> delegates, CodeType elementType, ulong arrayStartAddress)
            {
                this.delegates = delegates;
                this.elementType = elementType;
                this.arrayStartAddress = arrayStartAddress;
                elementTypeSize = elementType.Size;
            }

            public T this[int index]
            {
                get
                {
                    ulong address = arrayStartAddress + (ulong)index * elementTypeSize;
                    var buffer = Debugger.ReadMemory(elementType.Module.Process, address, elementTypeSize);

                    return delegates.PhysicalConstructor(buffer, 0, address, elementType, address, Variable.ComputedName, Variable.UntrackedPath);
                }
            }

            public T Reuse(T element, int index)
            {
                ulong address = arrayStartAddress + (ulong)index * elementTypeSize;
                var buffer = Debugger.ReadMemory(elementType.Module.Process, address, elementTypeSize);

                return delegates.Reuse(element, buffer, 0, address, elementType, address);
            }

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
        }

        private class BufferedElementCreatorReadOnlyList : IReuseUserTypeReadOnlyList<T>
        {
            private UserTypeDelegates<T> delegates;
            private CodeType elementType;
            private ulong arrayStartAddress;
            private uint elementTypeSize;
            private MemoryBuffer buffer;
            private ulong bufferAddress;

            public BufferedElementCreatorReadOnlyList(UserTypeDelegates<T> delegates, CodeType elementType, MemoryBuffer buffer, ulong bufferAddress, ulong arrayStartAddress)
            {
                this.delegates = delegates;
                this.elementType = elementType;
                this.arrayStartAddress = arrayStartAddress;
                this.buffer = buffer;
                this.bufferAddress = bufferAddress;
                elementTypeSize = elementType.Size;
            }

            public T this[int index]
            {
                get
                {
                    int offset = (int)(index * elementTypeSize);
                    ulong address = arrayStartAddress + (ulong)offset;

                    return delegates.PhysicalConstructor(buffer, offset, bufferAddress, elementType, address, Variable.ComputedName, Variable.UntrackedPath);
                }
            }

            public T Reuse(T element, int index)
            {
                int offset = (int)(index * elementTypeSize);
                ulong address = arrayStartAddress + (ulong)offset;

                return delegates.Reuse(element, buffer, offset, bufferAddress, elementType, address);
            }

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
        }
    }
}
