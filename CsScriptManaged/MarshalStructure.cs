using System;
using System.Runtime.InteropServices;

namespace CsScriptManaged
{
    /// <summary>
    /// Marshaled structure
    /// </summary>
    /// <typeparam name="T">The structure type</typeparam>
    public class MarshalStructure<T> : IDisposable
        where T : struct
    {
        /// <summary>
        /// The base size of the structure
        /// </summary>
        public static readonly int BaseSize = Marshal.SizeOf<T>();

        /// <summary>
        /// The extended size - more bytes to be allocated after structure
        /// </summary>
        private int extendedSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarshalStructure{T}"/> class.
        /// </summary>
        /// <param name="extended">The extended size.</param>
        public MarshalStructure(int extended = 0)
        {
            ExtendedSize = extended;
            Pointer = Marshal.AllocHGlobal(Size);
        }

        /// <summary>
        /// Gets or sets the extended size.
        /// </summary>
        public int ExtendedSize
        {
            get
            {
                return extendedSize;
            }

            protected set
            {
                if (extendedSize != value)
                {
                    extendedSize = value;
                    if (Pointer != IntPtr.Zero)
                    {
                        Pointer = Marshal.ReAllocHGlobal(Pointer, new IntPtr(Size));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size
        {
            get
            {
                return BaseSize + (int)ExtendedSize;
            }
        }

        /// <summary>
        /// Gets the unsigned size.
        /// </summary>
        public uint USize
        {
            get
            {
                return (uint)Size;
            }
        }

        /// <summary>
        /// Gets or sets the structure.
        /// </summary>
        public T Structure
        {
            get
            {
                return Marshal.PtrToStructure<T>(Pointer);
            }

            set
            {
                Marshal.StructureToPtr(value, Pointer, true);
            }
        }

        /// <summary>
        /// Gets or sets the extended pointer.
        /// </summary>
        public IntPtr ExtendedPointer
        {
            get
            {
                return Pointer + BaseSize;
            }

            set
            {
                NativeMethods.CopyMemory(ExtendedPointer, value, (uint)ExtendedSize);
            }
        }

        /// <summary>
        /// Gets the pointer.
        /// </summary>
        public IntPtr Pointer { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Marshal.DestroyStructure<T>(Pointer);
        }
    }

    /// <summary>
    /// Marshaled structure followed by ANSI string
    /// </summary>
    /// <typeparam name="T">The structure type</typeparam>
    public class MarshalStructureExtendedWithAnsiString<T> : MarshalStructure<T>
        where T : struct
    {
        /// <summary>
        /// Gets or sets the extended value.
        /// </summary>
        public string Extended
        {
            get
            {
                return Marshal.PtrToStringAnsi(ExtendedPointer);
            }

            set
            {
                IntPtr str = Marshal.StringToHGlobalAnsi(value);

                try
                {
                    ExtendedSize = value.Length + 1;
                    ExtendedPointer = str;
                }
                finally
                {
                    Marshal.FreeHGlobal(str);
                }
            }
        }
    }
}
