using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("1B278D20-79F2-426E-A3F9-C1DDF375D48E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugBreakpoint2 : IDebugBreakpoint
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

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

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
    }
}
