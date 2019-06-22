using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("CE289126-9E84-45A7-937E-67BB18691493"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugRegisters
    {
        [PreserveSig]
        int GetNumberRegisters(
            [Out] out uint Number);

        [PreserveSig]
        int GetDescription(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out _DEBUG_REGISTER_DESCRIPTION Desc);

        [PreserveSig]
        int GetIndexByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [Out] out uint Index);

        [PreserveSig]
        int GetValue(
            [In] uint Register,
            [Out] out _DEBUG_VALUE Value);

        [PreserveSig]
        int SetValue(
            [In] uint Register,
            [In] _DEBUG_VALUE Value);

        [PreserveSig]
        int GetValues(
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] Indices,
            [In] uint Start,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] Values);

        [PreserveSig]
        int SetValues(
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] Indices,
            [In] uint Start,
            [In, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] Values);

        [PreserveSig]
        int OutputRegisters(
            [In] uint OutputControl,
            [In] uint Flags);

        [PreserveSig]
        int GetInstructionOffset(
            [Out] out ulong Offset);

        [PreserveSig]
        int GetStackOffset(
            [Out] out ulong Offset);

        [PreserveSig]
        int GetFrameOffset(
            [Out] out ulong Offset);
    }
}
