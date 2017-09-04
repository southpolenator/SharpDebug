using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("716D14C9-119B-4BA5-AF1F-0890E672416A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugAdvanced2 : IDebugAdvanced
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugAdvanced
        // ---------------------------------------------------------------------------------------------

        void GetThreadContext(
            [Out] IntPtr Context,
            [In] uint ContextSize);

        void SetThreadContext(
            [In] IntPtr Context,
            [In] uint ContextSize);

        // ---------------------------------------------------------------------------------------------
        // IDebugAdvanced2
        // ---------------------------------------------------------------------------------------------

        void Request(
            [In] uint Request,
            [In] IntPtr InBuffer,
            [In] uint InBufferSize,
            [Out] IntPtr OutBuffer,
            [In] uint OutBufferSize,
            [Out] out uint OutSize);

        void GetSourceFileInformation(
            [In] uint Which,
            [In, MarshalAs(UnmanagedType.LPStr)] string SourceFile,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize);

        void FindSourceFileAndToken(
            [In] uint StartElement,
            [In] ulong ModAddr,
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] uint Flags,
            [In] IntPtr FileToken,
            [In] uint FileTokenSize,
            [Out] out uint FoundElement,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FoundSize);

        void GetSymbolInformation(
            [In] uint Which,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder StringBuffer,
            [In] uint StringBufferSize,
            [Out] out uint StringSize);

        void GetSystemObjectInformation(
            [In] uint Which,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize);
    }
}
