using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("716D14C9-119B-4BA5-AF1F-0890E672416A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugAdvanced2 : IDebugAdvanced
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugAdvanced
        /// <summary>
        /// The <see cref="GetThreadContext"/> method returns the current thread context.
        /// </summary>
        /// <param name="Context">Receives the current thread context. The type of the thread context is the CONTEXT structure for the target's effective processor. The buffer Context must be large enough to hold this structure.</param>
        /// <param name="ContextSize">Specifies the size of the buffer <paramref name="Context"/>.</param>
        [PreserveSig]
        int GetThreadContext(
            [Out] IntPtr Context,
            [In] uint ContextSize);

        /// <summary>
        /// The <see cref="SetThreadContext"/> method sets the current thread context.
        /// </summary>
        /// <param name="Context">Specifies the thread context. The type of the thread context is the CONTEXT structure for the target's effective processor. The buffer Context must be large enough to hold this structure.</param>
        /// <param name="ContextSize">Specifies the size of the buffer <paramref name="Context"/>.</param>
        [PreserveSig]
        int SetThreadContext(
            [In] IntPtr Context,
            [In] uint ContextSize);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int Request(
            [In] uint Request,
            [In] IntPtr InBuffer,
            [In] uint InBufferSize,
            [Out] IntPtr OutBuffer,
            [In] uint OutBufferSize,
            [Out] out uint OutSize);

        [PreserveSig]
        int GetSourceFileInformation(
            [In] uint Which,
            [In, MarshalAs(UnmanagedType.LPStr)] string SourceFile,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize);

        [PreserveSig]
        int FindSourceFileAndToken(
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

        [PreserveSig]
        int GetSymbolInformation(
            [In] uint Which,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder StringBuffer,
            [In] uint StringBufferSize,
            [Out] out uint StringSize);

        [PreserveSig]
        int GetSystemObjectInformation(
            [In] uint Which,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize);
    }
}
