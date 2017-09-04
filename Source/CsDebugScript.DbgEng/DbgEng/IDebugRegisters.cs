using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("CE289126-9E84-45A7-937E-67BB18691493"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugRegisters
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugRegisters
        // ---------------------------------------------------------------------------------------------

        uint GetNumberRegisters();

        void GetDescription(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out _DEBUG_REGISTER_DESCRIPTION Desc);

        uint GetIndexByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name);

        _DEBUG_VALUE GetValue(
            [In] uint Register);

        void SetValue(
            [In] uint Register,
            [In] ref _DEBUG_VALUE Value);

        void GetValues(
            [In] uint Count,
            [In] ref uint Indices,
            [In] uint Start,
            [Out] IntPtr Values);

        void SetValues(
            [In] uint Count,
            [In] ref uint Indices,
            [In] uint Start,
            [In] IntPtr Values);

        void OutputRegisters(
            [In] uint OutputControl,
            [In] uint Flags);

        ulong GetInstructionOffset();

        ulong GetStackOffset();

        ulong GetFrameOffset();
    }
}
