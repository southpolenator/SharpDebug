namespace CsScriptManaged
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Marshaled array of elements
    /// </summary>
    /// <typeparam name="T">Array structure type</typeparam>
    internal class MarshalArrayReader<T> : IDisposable
        where T : struct
    {
        /// <summary>
        /// The size of single element
        /// </summary>
        static readonly int Size = Marshal.SizeOf<T>();

        /// <summary>
        /// The unsigned size of single element
        /// </summary>
        static readonly uint USize = (uint)Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarshalArrayReader{T}"/> class.
        /// </summary>
        /// <param name="count">The number of elements in buffer.</param>
        public MarshalArrayReader(int count)
        {
            Count = count;
            Pointer = Marshal.AllocHGlobal(Count * Size);
        }

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
                    yield return Marshal.PtrToStructure<T>(Pointer + i * Size);
                }
            }
        }

        public T this[int index]
        {
            get
            {
                return Marshal.PtrToStructure<T>(Pointer + index * Size);
            }

            set
            {
                Marshal.StructureToPtr<T>(value, Pointer + index * Size, true);
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
    }
}
