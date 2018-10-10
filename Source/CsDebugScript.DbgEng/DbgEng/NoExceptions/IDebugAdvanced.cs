using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("F2DF5F53-071F-47BD-9DE6-5734C3FED689"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugAdvanced
    {
        [PreserveSig]
        int GetThreadContext(
            [Out] IntPtr Context,
            [In] uint ContextSize);

        [PreserveSig]
        int SetThreadContext(
            [In] IntPtr Context,
            [In] uint ContextSize);
    }
}
