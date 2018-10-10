using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [Guid("4BF58045-D654-4C40-B0AF-683090F356DC"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDebugOutputCallbacks
    {
        [PreserveSig]
        int Output(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Text);
    }
}
