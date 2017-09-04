using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [ComImport, Guid("597C980D-E7BD-4309-962C-9D9B69A7372C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugPlmClient2 : IDebugPlmClient
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugPlmClient
        // ---------------------------------------------------------------------------------------------

        void LaunchPlmPackageForDebugWide(
            [In] ulong Server,
            [In] uint Timeout,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId);

        // ---------------------------------------------------------------------------------------------
        // IDebugPlmClient2
        // ---------------------------------------------------------------------------------------------

        void LaunchPlmBgTaskForDebugWide(
            [In] ulong Server,
            [In] uint Timeout,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string BackgroundTaskId,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId);
    }
}
