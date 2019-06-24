using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpDebug.Engine.Marshaling
{
    /// <summary>
    /// Marshaled array of elements
    /// </summary>
    /// <typeparam name="T">Array structure type</typeparam>
    internal abstract class MarshalArrayReader<T> : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarshalArrayReader{T}" /> class.
        /// </summary>
        /// <param name="count">The number of elements in buffer.</param>
        /// <param name="size">The size of single element.</param>
        protected MarshalArrayReader(int count, int size)
        {
            Size = size;
            Count = count;
            Pointer = Marshal.AllocHGlobal(Count * Size);
        }

        /// <summary>
        /// The size of single element
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the pointer.
        /// </summary>
        public IntPtr Pointer { get; private set; }

        /// <summary>
        /// Gets the elements.
        /// </summary>
        public IEnumerable<T> Elements
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return PtrToStructure(Pointer + i * Size);
                }
            }
        }

        /// <summary>
        /// Gets or sets the &lt;T&gt; at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[int index]
        {
            get
            {
                return PtrToStructure(Pointer + index * Size);
            }

            set
            {
                StructureToPtr(value, Pointer + index * Size);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
                Pointer = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Unmarshals pointer to structure.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        protected abstract T PtrToStructure(IntPtr pointer);

        /// <summary>
        /// Marshals structures to pointer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="pointer">The pointer.</param>
        protected abstract void StructureToPtr(T value, IntPtr pointer);
    }

    /// <summary>
    /// Marshaled array of elements using regular marshaler
    /// </summary>
    /// <typeparam name="T">Array structure type</typeparam>
    internal class RegularMarshalArrayReader<T> : MarshalArrayReader<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegularMarshalArrayReader{T}"/> class.
        /// </summary>
        /// <param name="count">The count.</param>
        public RegularMarshalArrayReader(int count)
            : base(count, Marshal.SizeOf(typeof(T)))
        {
        }

        /// <summary>
        /// Unmarshals pointer to structure.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns></returns>
        protected override T PtrToStructure(IntPtr pointer)
        {
            return (T)Marshal.PtrToStructure(pointer, typeof(T));
        }

        /// <summary>
        /// Marshals structures to pointer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="pointer">The pointer.</param>
        protected override void StructureToPtr(T value, IntPtr pointer)
        {
            Marshal.StructureToPtr(value, pointer, true);
        }
    }

    /// <summary>
    /// Marshaled array of elements using custom marshaler
    /// </summary>
    /// <typeparam name="T">Array structure type</typeparam>
    internal class CustomMarshalArrayReader<T> : MarshalArrayReader<T>
    {
        /// <summary>
        /// Unmarshals pointer to structure.
        /// </summary>
        private Func<IntPtr, T> ptrToStructure;

        /// <summary>
        /// Marshals structures to pointer.
        /// </summary>
        private Action<T, IntPtr> structureToPtr;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMarshalArrayReader{T}"/> class.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="size">The size.</param>
        /// <param name="ptrToStructure">Unmarshals pointer to structure.</param>
        /// <param name="structureToPtr">Marshals structures to pointer.</param>
        public CustomMarshalArrayReader(int count, int size, Func<IntPtr, T> ptrToStructure, Action<T, IntPtr> structureToPtr)
            : base(count, size)
        {
            this.ptrToStructure = ptrToStructure;
            this.structureToPtr = structureToPtr;
        }

        /// <summary>
        /// Unmarshals pointer to structure.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns></returns>
        protected override T PtrToStructure(IntPtr pointer)
        {
            return ptrToStructure(pointer);
        }

        /// <summary>
        /// Marshals structures to pointer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="pointer">The pointer.</param>
        protected override void StructureToPtr(T value, IntPtr pointer)
        {
            structureToPtr(value, pointer);
        }
    }
}
