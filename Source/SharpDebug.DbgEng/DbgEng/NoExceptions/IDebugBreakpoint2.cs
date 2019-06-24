using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("1B278D20-79F2-426E-A3F9-C1DDF375D48E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugBreakpoint2 : IDebugBreakpoint
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugBreakpoint
        [PreserveSig]
        int GetId(
            [Out] out uint Id);

        [PreserveSig]
        int GetType(
            [Out] out uint BreakType,
            [Out] out uint ProcType);

        [PreserveSig]
        int GetAdder(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugClient Adder);

        [PreserveSig]
        int GetFlags(
            [Out] out uint Flags);

        [PreserveSig]
        int AddFlags(
            [In] uint Flags);

        [PreserveSig]
        int RemoveFlags(
            [In] uint Flags);

        [PreserveSig]
        int SetFlags(
            [In] uint Flags);

        [PreserveSig]
        int GetOffset(
            [Out] out ulong Offset);

        [PreserveSig]
        int SetOffset(
            [In] ulong Offset);

        [PreserveSig]
        int GetDataParameters(
            [Out] out uint Size,
            [Out] out uint AccessType);

        [PreserveSig]
        int SetDataParameters(
            [In] uint Size,
            [In] uint AccessType);

        [PreserveSig]
        int GetPassCount(
            [Out] out uint Count);

        [PreserveSig]
        int SetPassCount(
            [In] uint Count);

        [PreserveSig]
        int GetCurrentPassCount(
            [Out] out uint Count);

        [PreserveSig]
        int GetMatchThreadId(
            [Out] out uint Id);

        [PreserveSig]
        int SetMatchThreadId(
            [In] uint Thread);

        [PreserveSig]
        int GetCommand(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        [PreserveSig]
        int SetCommand(
            [In, MarshalAs(UnmanagedType.LPStr)] string Command);

        [PreserveSig]
        int GetOffsetExpression(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ExpressionSize);

        [PreserveSig]
        int SetOffsetExpression(
            [In, MarshalAs(UnmanagedType.LPStr)] string Expression);

        [PreserveSig]
        int GetParameters(
            [Out] out _DEBUG_BREAKPOINT_PARAMETERS Parameters);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int GetCommandWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        [PreserveSig]
        int SetCommandWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command);

        [PreserveSig]
        int GetOffsetExpressionWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ExpressionSize);

        [PreserveSig]
        int SetOffsetExpressionWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Expression);
    }
}
