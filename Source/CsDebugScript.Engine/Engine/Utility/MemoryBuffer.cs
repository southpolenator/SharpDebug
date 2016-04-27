using System;
using System.Reflection.Emit;

namespace CsDebugScript.Engine.Utility
{
    /// <summary>
    /// Helper structure that holds pointer to memory buffer or byte array
    /// </summary>
    public unsafe struct MemoryBuffer
    {
        /// <summary>
        /// The memory buffer bytes
        /// </summary>
        private byte[] bytes;

        /// <summary>
        /// The byte pointer
        /// </summary>
        private byte* bytePointer;

        /// <summary>
        /// The byte pointer length
        /// </summary>
        private int bytePointerLength;

        internal delegate void MemCpyFunction(void* des, void* src, uint bytes);

        internal static readonly MemCpyFunction MemCpy;

        static MemoryBuffer()
        {
            var dynamicMethod = new DynamicMethod
            (
                "MemCpy",
                typeof(void),
                new[] { typeof(void*), typeof(void*), typeof(uint) },
                typeof(DumpFileMemoryReader)
            );

            var ilGenerator = dynamicMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);

            ilGenerator.Emit(OpCodes.Cpblk);
            ilGenerator.Emit(OpCodes.Ret);

            MemCpy = (MemCpyFunction)dynamicMethod.CreateDelegate(typeof(MemCpyFunction));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryBuffer"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public MemoryBuffer(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            this.bytes = bytes;
            bytePointer = null;
            bytePointerLength = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryBuffer" /> class.
        /// </summary>
        /// <param name="bytePointer">The byte pointer.</param>
        /// <param name="length">The length of the buffer.</param>
        public MemoryBuffer(byte* bytePointer, int length)
        {
            if (bytePointer == null)
                throw new ArgumentNullException("bytePointer");
            this.bytePointer = bytePointer;
            bytePointerLength = length;
            this.bytes = null;
        }

        /// <summary>
        /// Gets the bytes from the memory buffer.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                if (bytes == null)
                {
                    bytes = new byte[bytePointerLength];
                    fixed (byte* destination = bytes)
                    {
                        byte* source = bytePointer;

                        MemoryBuffer.MemCpy(destination, source, (uint)bytes.Length);
                    }
                }

                return bytes;
            }
        }

        /// <summary>
        /// Gets the byte pointer.
        /// </summary>
        public byte* BytePointer
        {
            get
            {
                return bytePointer;
            }
        }

        /// <summary>
        /// Gets the length of the memory buffer where byte pointer points.
        /// </summary>
        public int BytePointerLength
        {
            get
            {
                return bytePointerLength;
            }
        }
    }
}
