using System;
using System.Runtime.InteropServices;
using CsScriptManaged.Native;

namespace CsScriptManaged.Marshaling
{
    /// <summary>
    /// Marshaled structure that can have extended part of the structure.
    /// </summary>
    /// <typeparam name="T">The structure type</typeparam>
    public class MarshalStructure<T> : IDisposable
        where T : struct
    {
        /// <summary>
        /// The base size of the structure
        /// </summary>
        public static readonly int BaseSize = Marshal.SizeOf(typeof(T));

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
        /// Gets or sets the size of extended part of the structure.
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
        /// Gets the total size of the structure with extended part.
        /// </summary>
        public int Size
        {
            get
            {
                return BaseSize + ExtendedSize;
            }
        }

        /// <summary>
        /// Gets the unsigned integer of the total size of the structure with extended part.
        /// </summary>
        public uint USize
        {
            get
            {
                return (uint)Size;
            }
        }

        /// <summary>
        /// Gets or sets the structure by marshaling.
        /// </summary>
        public T Structure
        {
            get
            {
                return (T)Marshal.PtrToStructure(Pointer, typeof(T));
            }

            set
            {
                Marshal.StructureToPtr(value, Pointer, true);
            }
        }

        /// <summary>
        /// Gets or sets the native pointer to the extended part of the structure.
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
        /// Gets the native pointer to the structure.
        /// </summary>
        public IntPtr Pointer { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Marshal.DestroyStructure(Pointer, typeof(T));
        }
    }

    /// <summary>
    /// Marshaled structure extended by ANSI string.
    /// </summary>
    /// <typeparam name="T">The structure type</typeparam>
    public class MarshalStructureExtendedWithAnsiString<T> : MarshalStructure<T>
        where T : struct
    {
        /// <summary>
        /// Gets or sets the extended part string value.
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
