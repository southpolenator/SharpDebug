using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("D1069067-2A65-4BF0-AE97-76184B67856B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugAdvanced4 : IDebugAdvanced3
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

        #region IDebugAdvanced2
        [PreserveSig]
        /// <summary>
        /// The Request method performs a variety of different operations.
        /// </summary>
        /// <param name="Request">Specifies which operation to perform.</param>
        /// <param name="InBuffer">Specifies the input to this method. The type and interpretation of the input depends on the <paramref name="Request"/> parameter.</param>
        /// <param name="InBufferSize">Specifies the size of the input buffer <paramref name="InBuffer"/>. If the request requires no input, <paramref name="InBufferSize"/> should be set to zero.</param>
        /// <param name="OutBuffer">Receives the output from this method. The type and interpretation of the output depends on the <paramref name="Request"/> parameter. If <paramref name="OutBuffer"/> is <c>null</c>, the output is not returned.</param>
        /// <param name="OutBufferSize">Specifies the size of the output buffer <paramref name="OutBufferSize"/>. If the type of the output returned to <paramref name="OutBuffer"/> has a known size, <paramref name="OutBufferSize"/> is usually expected to be exactly that size, even if <paramref name="OutBuffer"/> is set to <c>null</c>.</param>
        /// <param name="OutSize">Receives the size of the output returned in the output buffer <paramref name="OutBuffer"/>.</param>
        int Request(
            [In] DebugRequest Request,
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
        #endregion

        #region IDebugAdvanced3
        [PreserveSig]
        int GetSourceFileInformationWide(
            [In] uint Which,
            [In, MarshalAs(UnmanagedType.LPWStr)] string SourceFile,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize);

        [PreserveSig]
        int FindSourceFileAndTokenWide(
            [In] uint StartElement,
            [In] ulong ModAddr,
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] uint Flags,
            [In] IntPtr FileToken,
            [In] uint FileTokenSize,
            [Out] out uint FoundElement,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FoundSize);

        [PreserveSig]
        int GetSymbolInformationWide(
            [In] uint Which,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder StringBuffer,
            [In] uint StringBufferSize,
            [Out] out uint StringSize);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int GetSymbolInformationWideEx(
            [In] uint Which,
            [In] ulong Arg64,
            [In] uint Arg32,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder StringBuffer,
            [In] uint StringBufferSize,
            [Out] out uint StringSize,
            [Out] out _SYMBOL_INFO_EX pInfoEx);
    }
}
