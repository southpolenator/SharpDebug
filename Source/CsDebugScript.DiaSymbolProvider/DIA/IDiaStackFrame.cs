using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Exposes the properties of a stack frame.
    /// </summary>
    [ComImport, Guid("5EDBC96D-CDD6-4792-AFBE-CC89007D9610"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaStackFrame
    {
        /// <summary>
        /// Retrieves the frame type.
        /// </summary>
        [DispId(1)]
        StackFrameTypeEnum type { get; }

        /// <summary>
        /// Retrieves the address base of the frame.
        /// </summary>
        [DispId(2)]
        ulong @base { get; }

        /// <summary>
        /// Retrieves the size of the frame in bytes.
        /// </summary>
        [DispId(3)]
        uint size { get; }

        /// <summary>
        /// Retrieves the return address of the frame.
        /// </summary>
        [DispId(4)]
        ulong returnAddress { get; }

        /// <summary>
        /// Retrieves the address base of the locals.
        /// </summary>
        [DispId(5)]
        ulong localsBase { get; }

        /// <summary>
        /// Retrieves the number of bytes of local variables pushed on the stack.
        /// </summary>
        [DispId(6)]
        uint lengthLocals { get; }

        /// <summary>
        /// Retrieves the number of bytes of parameters pushed on the stack.
        /// </summary>
        [DispId(7)]
        uint lengthParams { get; }

        /// <summary>
        /// Retrieves the number of bytes of prologue code in the block.
        /// </summary>
        [DispId(8)]
        uint lengthProlog { get; }

        /// <summary>
        /// Retrieves the number of bytes of saved registers pushed on the stack.
        /// </summary>
        [DispId(9)]
        uint lengthSavedRegisters { get; }

        /// <summary>
        /// Retrieves a flag indicating that system exception handling is in effect.
        /// </summary>
        [DispId(10)]
        bool systemExceptionHandling
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating that C++ exception handling is in effect.
        /// </summary>
        [DispId(11)]
        bool cplusplusExceptionHandling
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating that the block contains the entry point of a function.
        /// </summary>
        [DispId(12)]
        bool functionStart
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves a flag indicating that the base pointer is allocated for code in this address range. This method is deprecated.
        /// </summary>
        [DispId(13)]
        bool allocatesBasePointer
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Retrieves the maximum number of bytes pushed on the stack in the frame.
        /// </summary>
        [DispId(14)]
        uint maxStack { get; }

        /// <summary>
        /// Retrieves the value of a specified register as stored in the stack frame.
        /// </summary>
        /// <param name="registerIndex">One of the <see cref="CV_HREG_e"/> enumeration values.</param>
        /// <returns>Value stored in the register.</returns>
        [DispId(15)]
        ulong get_registerValue(CV_HREG_e registerIndex);
    }
}
