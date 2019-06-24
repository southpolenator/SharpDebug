using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("1656AFA9-19C6-4E3A-97E7-5DC9160CF9C4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugRegisters2 : IDebugRegisters
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugRegisters
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
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int GetDescriptionWide(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out _DEBUG_REGISTER_DESCRIPTION Desc);

        [PreserveSig]
        int GetIndexByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [Out] out uint Index);

        [PreserveSig]
        int GetNumberPseudoRegisters(
            [Out] out uint Number);

        [PreserveSig]
        int GetPseudoDescription(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong TypeModule,
            [Out] out uint TypeId);

        [PreserveSig]
        int GetPseudoDescriptionWide(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong TypeModule,
            [Out] out uint TypeId);

        [PreserveSig]
        int GetPseudoIndexByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [Out] out uint Index);

        [PreserveSig]
        int GetPseudoIndexByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [Out] out uint Index);

        [PreserveSig]
        int GetPseudoValues(
            [In] uint Source,
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] Indices,
            [In] uint Start,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] Values);

        [PreserveSig]
        int SetPseudoValues(
            [In] uint Source,
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] Indices,
            [In] uint Start,
            [In, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] Values);

        [PreserveSig]
        int GetValues2(
            [In] uint Source,
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] Indices,
            [In] uint Start,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] Values);

        [PreserveSig]
        int SetValues2(
            [In] uint Source,
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] Indices,
            [In] uint Start,
            [In, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] Values);

        [PreserveSig]
        int OutputRegisters2(
            [In] uint OutputControl,
            [In] uint Source,
            [In] uint Flags);

        [PreserveSig]
        int GetInstructionOffset2(
            [In] uint Source,
            [Out] out ulong Offset);

        [PreserveSig]
        int GetStackOffset2(
            [In] uint Source,
            [Out] out ulong Offset);

        [PreserveSig]
        int GetFrameOffset2(
            [In] uint Source,
            [Out] out ulong Offset);
    }
}
