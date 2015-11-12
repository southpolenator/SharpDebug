using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CsScriptManaged
{
    public class MarshalStructure<T> : IDisposable
        where T : struct
    {
        public static readonly int BaseSize = Marshal.SizeOf<T>();
        private int extendedSize;

        public MarshalStructure(int extended = 0)
        {
            ExtendedSize = extended;
            Pointer = Marshal.AllocHGlobal(Size);
        }

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

        public int Size
        {
            get
            {
                return BaseSize + (int)ExtendedSize;
            }
        }

        public uint USize
        {
            get
            {
                return (uint)Size;
            }
        }

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

        public IntPtr Pointer { get; private set; }

        public void Dispose()
        {
            Marshal.DestroyStructure<T>(Pointer);
        }
    }

    public class MarshalStructureExtendedWithAnsiString<T> : MarshalStructure<T>
        where T : struct
    {
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
