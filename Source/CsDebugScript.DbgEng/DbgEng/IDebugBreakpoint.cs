using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("5BD9D474-5975-423A-B88B-65A8E7110E65"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugBreakpoint
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugBreakpoint
        // ---------------------------------------------------------------------------------------------

        uint GetId();

        void GetType(
            [Out] out uint BreakType,
            [Out] out uint ProcType);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugClient GetAdder();

        uint GetFlags();

        void AddFlags(
            [In] uint Flags);

        void RemoveFlags(
            [In] uint Flags);

        void SetFlags(
            [In] uint Flags);

        ulong GetOffset();

        void SetOffset(
            [In] ulong Offset);

        void GetDataParameters(
            [Out] out uint Size,
            [Out] out uint AccessType);

        void SetDataParameters(
            [In] uint Size,
            [In] uint AccessType);

        uint GetPassCount();

        void SetPassCount(
            [In] uint Count);

        uint GetCurrentPassCount();

        uint GetMatchThreadId();

        void SetMatchThreadId(
            [In] uint Thread);

        void GetCommand(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        void SetCommand(
            [In, MarshalAs(UnmanagedType.LPStr)] string Command);

        void GetOffsetExpression(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ExpressionSize);

        void SetOffsetExpression(
            [In, MarshalAs(UnmanagedType.LPStr)] string Expression);

        _DEBUG_BREAKPOINT_PARAMETERS GetParameters();
    }
}
