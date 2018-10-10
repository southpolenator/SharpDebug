using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [Guid("7782D8F2-2B85-4059-AB88-28CEDDCA1C80"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDebugOutputStream
    {
        [PreserveSig]
        int Write(
            [In, MarshalAs(UnmanagedType.LPWStr)] string psz);
    }
}
