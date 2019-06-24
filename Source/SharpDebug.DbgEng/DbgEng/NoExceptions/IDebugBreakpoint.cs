using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("5BD9D474-5975-423A-B88B-65A8E7110E65"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugBreakpoint
    {
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
    }
}
