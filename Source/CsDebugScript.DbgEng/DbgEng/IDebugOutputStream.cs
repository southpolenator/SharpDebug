using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [Guid("7782D8F2-2B85-4059-AB88-28CEDDCA1C80"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDebugOutputStream
    {
        void Write(
            [In, MarshalAs(UnmanagedType.LPWStr)] string psz);
    }
}
