using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Exposes the details of a stack frame.
    /// </summary>
    [ComImport, Guid("A39184B7-6A36-42DE-8EEC-7DF9F3F59F33"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaFrameData
    {
        /// <summary>
        /// Retrieves the section part of the code address for the frame.
        /// </summary>
        [DispId(2)]
        uint addressSection { get; }

        /// <summary>
        /// Retrieves the offset part of the code address for the frame.
        /// </summary>
        [DispId(3)]
        uint addressOffset { get; }

        /// <summary>
        /// Retrieves the image relative virtual address (RVA) of the code for the frame.
        /// </summary>
        [DispId(4)]
        uint relativeVirtualAddress { get; }

        /// <summary>
        /// Retrieves the virtual address (VA) of the code for the frame.
        /// </summary>
        [DispId(5)]
        ulong virtualAddress { get; }

        /// <summary>
        /// Retrieves the length, in bytes, of the block of code described by the frame.
        /// </summary>
        [DispId(6)]
        uint lengthBlock { get; }

        /// <summary>
        /// Retrieves the number of bytes of local variables pushed on the stack.
        /// </summary>
        [DispId(7)]
        uint lengthLocals { get; }

        /// <summary>
        /// Retrieves the number of bytes of parameters pushed on the stack.
        /// </summary>
        [DispId(8)]
        uint lengthParams { get; }

        /// <summary>
        /// Retrieves the maximum number of bytes pushed on the stack in the frame.
        /// </summary>
        [DispId(9)]
        uint maxStack { get; }

        /// <summary>
        /// Retrieves the number of bytes of prologue code in the block.
        /// </summary>
        [DispId(10)]
        uint lengthProlog { get; }

        /// <summary>
        /// Retrieves the number of bytes of saved registers pushed on the stack.
        /// </summary>
        [DispId(11)]
        uint lengthSavedRegisters { get; }

        /// <summary>
        /// Retrieves the program string that is used to compute the register set before the call to the current function.
        /// </summary>
        [DispId(12)]
        string program
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that indicates that system exception handling is in effect.
        /// </summary>
        [DispId(13)]
        bool systemExceptionHandling
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that indicates that C++ exception handling is in effect.
        /// </summary>
        [DispId(14)]
        bool cplusplusExceptionHandling
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that indicates that the block contains the entry point of a function.
        /// </summary>
        [DispId(15)]
        bool functionStart
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag that indicates that the base pointer is allocated for code in this address range. This method is deprecated.
        /// </summary>
        [DispId(16)]
        bool allocatesBasePointer
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the compiler-specific frame type.
        /// </summary>
        [DispId(17)]
        StackFrameTypeEnum type
        {
            get;
        }

        /// <summary>
        /// Retrieves frame data interface for enclosing function.
        /// </summary>
        [DispId(18)]
        IDiaFrameData functionParent
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Performs stack unwinding and returns results in a stack walk frame interface.
        /// </summary>
        /// <param name="frame">An <see cref="IDiaStackWalkFrame"/> object that holds the state of frame registers.</param>
        /// <remarks>
        /// This method is called during debugging to unwind the stack.
        /// The <see cref="IDiaStackWalkFrame"/> object is implemented by the client application to receive updates to the registers and to provide methods used by the execute method.
        /// </remarks>
        void execute(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaStackWalkFrame frame);
    }
}
