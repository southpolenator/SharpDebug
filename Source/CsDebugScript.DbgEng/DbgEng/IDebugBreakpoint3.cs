using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("38F5C249-B448-43BB-9835-579D4EC02249"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugBreakpoint3 : IDebugBreakpoint2
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugBreakpoint
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
        #endregion

        #region IDebugBreakpoint2
        void GetCommandWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        void SetCommandWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command);

        void GetOffsetExpressionWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ExpressionSize);

        void SetOffsetExpressionWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Expression);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        Guid GetGuid();
    }
}
