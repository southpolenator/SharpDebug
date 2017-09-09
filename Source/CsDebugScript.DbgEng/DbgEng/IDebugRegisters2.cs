using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("1656AFA9-19C6-4E3A-97E7-5DC9160CF9C4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugRegisters2 : IDebugRegisters
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugRegisters
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
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        void GetDescriptionWide(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out _DEBUG_REGISTER_DESCRIPTION Desc);

        uint GetIndexByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name);

        uint GetNumberPseudoRegisters();

        void GetPseudoDescription(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong TypeModule,
            [Out] out uint TypeId);

        void GetPseudoDescriptionWide(
            [In] uint Register,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong TypeModule,
            [Out] out uint TypeId);

        uint GetPseudoIndexByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name);

        uint GetPseudoIndexByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name);

        void GetPseudoValues(
            [In] uint Source,
            [In] uint Count,
            [In] ref uint Indices,
            [In] uint Start,
            [Out] IntPtr Values);

        void SetPseudoValues(
            [In] uint Source,
            [In] uint Count,
            [In] ref uint Indices,
            [In] uint Start,
            [In] IntPtr Values);

        void GetValues2(
            [In] uint Source,
            [In] uint Count,
            [In] ref uint Indices,
            [In] uint Start,
            [Out] IntPtr Values);

        void SetValues2(
            [In] uint Source,
            [In] uint Count,
            [In] ref uint Indices,
            [In] uint Start = default(uint),
            [In] IntPtr Values = default(IntPtr));

        void OutputRegisters2(
            [In] uint OutputControl,
            [In] uint Source,
            [In] uint Flags);

        ulong GetInstructionOffset2(
            [In] uint Source);

        ulong GetStackOffset2(
            [In] uint Source);

        ulong GetFrameOffset2(
            [In] uint Source);
    }
}
