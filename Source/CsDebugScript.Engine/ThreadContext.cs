using CsDebugScript.Engine;
using CsDebugScript.Engine.Marshaling;
using System;
using System.Runtime.InteropServices;

namespace CsDebugScript
{
    /// <summary>
    /// Thread context of the process being debugged.
    /// </summary>
    public class ThreadContext
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="ThreadContext"/> class from being created.
        /// </summary>
        private ThreadContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadContext" /> class.
        /// </summary>
        /// <param name="instructionPointer">The instruction pointer.</param>
        /// <param name="stackPointer">The stack pointer.</param>
        /// <param name="framePointer">The frame pointer.</param>
        /// <param name="bytes">The native structure bytes.</param>
        public ThreadContext(ulong instructionPointer, ulong stackPointer, ulong framePointer, byte[] bytes)
        {
            InstructionPointer = instructionPointer;
            StackPointer = stackPointer;
            FramePointer = framePointer;
            Bytes = bytes;
        }

        /// <summary>
        /// Gets the instruction pointer.
        /// </summary>
        public ulong InstructionPointer { get; private set; }

        /// <summary>
        /// Gets the stack pointer.
        /// </summary>
        public ulong StackPointer { get; private set; }

        /// <summary>
        /// Gets the frame pointer.
        /// </summary>
        public ulong FramePointer { get; private set; }

        /// <summary>
        /// Gets the structure bytes.
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Interface that allows access to register values. It has different implementation for different symbol provider.
        /// </summary>
        internal object Registers { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"IP = 0x{InstructionPointer:X}, SP = 0x{StackPointer:X}, FP = 0x{FramePointer:X}";
        }
    }

    internal class WindowsThreadContext : ThreadContext
    {
        #region Native structures
#pragma warning disable 0649
#pragma warning disable 0169
        private const int SIZE_OF_80387_REGISTERS = 80;
        private const int MAXIMUM_SUPPORTED_EXTENSION = 512;
        private const int WOW64_SIZE_OF_80387_REGISTERS = 80;
        private const int WOW64_MAXIMUM_SUPPORTED_EXTENSION = 512;

        private struct FLOATING_SAVE_AREA
        {
            public uint ControlWord;
            public uint StatusWord;
            public uint TagWord;
            public uint ErrorOffset;
            public uint ErrorSelector;
            public uint DataOffset;
            public uint DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIZE_OF_80387_REGISTERS)]
            public byte[] RegisterArea;
            public uint Spare0;
        }

        private struct CONTEXT_X86
        {
            public uint ContextFlags;

            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;

            public FLOATING_SAVE_AREA FloatSave;

            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;

            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;

            public uint Ebp;
            public uint Eip;
            public uint SegCs;              // MUST BE SANITIZED
            public uint EFlags;             // MUST BE SANITIZED
            public uint Esp;
            public uint SegSs;


            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXIMUM_SUPPORTED_EXTENSION)]
            public byte[] ExtendedRegisters;
        }

        private struct XMM_SAVE_AREA32
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2 * 2)]
            public ulong[] Header;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 * 2)]
            public ulong[] Legacy;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16 * 2)]
            public ulong[] Xmm;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public ulong[] Alignment;
        }

        //[StructLayout(LayoutKind.Explicit)]
        private struct CONTEXT_X64
        {

            // Register parameter home addresses.
            public ulong P1Home;
            public ulong P2Home;
            public ulong P3Home;
            public ulong P4Home;
            public ulong P5Home;
            public ulong P6Home;

            // Control flags.
            public uint ContextFlags;
            public uint MxCsr;

            // Segment Registers and processor flags.
            public ushort SegCs;
            public ushort SegDs;
            public ushort SegEs;
            public ushort SegFs;
            public ushort SegGs;
            public ushort SegSs;
            public uint EFlags;

            // Debug registers
            public ulong Dr0;
            public ulong Dr1;
            public ulong Dr2;
            public ulong Dr3;
            public ulong Dr6;
            public ulong Dr7;

            // Integer registers.
            public ulong Rax;
            public ulong Rcx;
            public ulong Rdx;
            public ulong Rbx;
            public ulong Rsp;
            public ulong Rbp;
            public ulong Rsi;
            public ulong Rdi;
            public ulong R8;
            public ulong R9;
            public ulong R10;
            public ulong R11;
            public ulong R12;
            public ulong R13;
            public ulong R14;
            public ulong R15;

            // Program counter.
            public ulong Rip;

            // Floating point state.
            public XMM_SAVE_AREA32 FltSave;

            // Vector registers.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26 * 2)]
            public ulong[] VectorRegister;
            public ulong VectorControl;

            // Special debug control registers.
            public ulong DebugControl;
            public ulong LastBranchToRip;
            public ulong LastBranchFromRip;
            public ulong LastExceptionToRip;
            public ulong LastExceptionFromRip;
        }

        private struct WOW64_FLOATING_SAVE_AREA
        {
            public uint ControlWord;
            public uint StatusWord;
            public uint TagWord;
            public uint ErrorOffset;
            public uint ErrorSelector;
            public uint DataOffset;
            public uint DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WOW64_SIZE_OF_80387_REGISTERS)]
            public byte[] RegisterArea;
            public uint Cr0NpxState;
        }

        private struct WOW64_CONTEXT
        {
            // The flags values within this flag control the contents of
            // a CONTEXT record.
            //
            // If the context record is used as an input parameter, then
            // for each portion of the context record controlled by a flag
            // whose value is set, it is assumed that that portion of the
            // context record contains valid context. If the context record
            // is being used to modify a threads context, then only that
            // portion of the threads context will be modified.
            //
            // If the context record is used as an IN OUT parameter to capture
            // the context of a thread, then only those portions of the thread's
            // context corresponding to set flags will be returned.
            //
            // The context record is never used as an OUT only parameter.
            public uint ContextFlags;

            // This section is specified/returned if CONTEXT_DEBUG_REGISTERS is
            // set in ContextFlags.  Note that CONTEXT_DEBUG_REGISTERS is NOT
            // included in CONTEXT_FULL.
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;

            // This section is specified/returned if the
            // ContextFlags word contians the flag CONTEXT_FLOATING_POINT.
            WOW64_FLOATING_SAVE_AREA FloatSave;

            // This section is specified/returned if the
            // ContextFlags word contians the flag CONTEXT_SEGMENTS.
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;

            // This section is specified/returned if the
            // ContextFlags word contians the flag CONTEXT_INTEGER.
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;

            // This section is specified/returned if the
            // ContextFlags word contians the flag CONTEXT_CONTROL.
            public uint Ebp;
            public uint Eip;
            public uint SegCs;              // MUST BE SANITIZED
            public uint EFlags;             // MUST BE SANITIZED
            public uint Esp;
            public uint SegSs;

            // This section is specified/returned if the ContextFlags word
            // contains the flag CONTEXT_EXTENDED_REGISTERS.
            // The format and contexts are processor specific
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = WOW64_MAXIMUM_SUPPORTED_EXTENSION)]
            public byte[] ExtendedRegisters;
        }
#pragma warning restore 0169
#pragma warning restore 0649
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsThreadContext" /> class.
        /// </summary>
        /// <param name="instructionPointer">The instruction pointer.</param>
        /// <param name="stackPointer">The stack pointer.</param>
        /// <param name="framePointer">The frame pointer.</param>
        /// <param name="bytes">The native structure bytes.</param>
        private WindowsThreadContext(ulong instructionPointer, ulong stackPointer, ulong framePointer, byte[] bytes)
            : base(instructionPointer, stackPointer, framePointer, bytes)
        {
        }

        /// <summary>
        /// Gets the size of the native structure.
        /// </summary>
        internal static uint NativeStructureSize
        {
            get
            {
                return GetNativeStructureSize(Process.Current);
            }
        }

        /// <summary>
        /// Gets the size of the native structure.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <returns></returns>
        private static uint GetNativeStructureSize(Process process)
        {
            switch (process.ArchitectureType)
            {
                case ArchitectureType.X86:
                    return (uint)Marshal.SizeOf(typeof(CONTEXT_X86));
                case ArchitectureType.Amd64:
                    return (uint)Marshal.SizeOf(typeof(CONTEXT_X64));
                case ArchitectureType.X86OverAmd64:
                    return (uint)Marshal.SizeOf(typeof(WOW64_CONTEXT));
                default:
                    throw new Exception($"Unknown architecture type: {process.ArchitectureType}");
            }
        }

        /// <summary>
        /// Creates the array marshaler.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="elementsCount">The number of elements.</param>
        internal static MarshalArrayReader<ThreadContext> CreateArrayMarshaler(Process process, int elementsCount)
        {
            return new CustomMarshalArrayReader<ThreadContext>(elementsCount, (int)GetNativeStructureSize(process), (p) => PtrToStructure(process, p), StructureToPtr);
        }

        /// <summary>
        /// Converts from native structure to managed object.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="pointer">The pointer.</param>
        internal static ThreadContext PtrToStructure(Process process, IntPtr pointer)
        {
            switch (process.ArchitectureType)
            {
                case ArchitectureType.X86:
                    return ReadX86Structure(pointer);
                case ArchitectureType.Amd64:
                    return ReadX64Structure(pointer);
                case ArchitectureType.X86OverAmd64:
                    return ReadWowX64Structure(pointer);
                default:
                    throw new Exception($"Unknown architecture type: {process.ArchitectureType}");
            }
        }

        /// <summary>
        /// Converts from managed object to native structure.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="pointer">The pointer.</param>
        private static void StructureToPtr(ThreadContext value, IntPtr pointer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the X86 structure.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        private static ThreadContext ReadX86Structure(IntPtr pointer)
        {
            CONTEXT_X86 structure = (CONTEXT_X86)Marshal.PtrToStructure(pointer, typeof(CONTEXT_X86));

            return new WindowsThreadContext(structure.Eip, structure.Esp, structure.Ebp, ReadBytes(pointer, typeof(CONTEXT_X86)));
        }

        /// <summary>
        /// Reads the WoW X64 structure.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        private static ThreadContext ReadWowX64Structure(IntPtr pointer)
        {
            WOW64_CONTEXT structure = (WOW64_CONTEXT)Marshal.PtrToStructure(pointer, typeof(WOW64_CONTEXT));

            return new WindowsThreadContext(structure.Eip, structure.Esp, structure.Ebp, ReadBytes(pointer, typeof(WOW64_CONTEXT)));
        }

        /// <summary>
        /// Reads the X64 structure.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        private static ThreadContext ReadX64Structure(IntPtr pointer)
        {
            CONTEXT_X64 structure = (CONTEXT_X64)Marshal.PtrToStructure(pointer, typeof(CONTEXT_X64));

            return new WindowsThreadContext(structure.Rip, structure.Rsp, structure.Rbp, ReadBytes(pointer, typeof(CONTEXT_X64)));
        }

        /// <summary>
        /// Reads the structure bytes.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="type">The structure type.</param>
        private static byte[] ReadBytes(IntPtr pointer, Type type)
        {
            byte[] buffer = new byte[Marshal.SizeOf(type)];

            Marshal.Copy(pointer, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// Gets the size of the context.
        /// </summary>
        /// <param name="process">The process.</param>
        internal static int GetContextSize(Process process)
        {
            switch (process.ArchitectureType)
            {
                case ArchitectureType.X86:
                    return Marshal.SizeOf(typeof(CONTEXT_X86));
                case ArchitectureType.Amd64:
                    return Marshal.SizeOf(typeof(CONTEXT_X64));
                case ArchitectureType.X86OverAmd64:
                    return Marshal.SizeOf(typeof(WOW64_CONTEXT));
                default:
                    throw new Exception($"Unknown architecture type: {process.ArchitectureType}");
            }
        }
    }
}
