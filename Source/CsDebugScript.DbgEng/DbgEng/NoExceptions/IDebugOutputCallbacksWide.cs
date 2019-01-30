using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [Guid("4C7FD663-C394-4E26-8EF1-34AD5ED3764C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDebugOutputCallbacksWide
    {
        [PreserveSig]
        int Output(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Text);
    }
}
