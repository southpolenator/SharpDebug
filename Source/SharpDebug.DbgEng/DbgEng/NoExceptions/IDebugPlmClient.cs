using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("A02B66C4-AEA3-4234-A9F7-FE4C383D4E29"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugPlmClient
    {
        [PreserveSig]
        int LaunchPlmPackageForDebugWide(
            [In] ulong Server,
            [In] uint Timeout,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId);
    }
}
