using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [ComImport, Guid("597C980D-E7BD-4309-962C-9D9B69A7372C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugPlmClient2 : IDebugPlmClient
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugPlmClient
        void LaunchPlmPackageForDebugWide(
            [In] ulong Server,
            [In] uint Timeout,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        void LaunchPlmBgTaskForDebugWide(
            [In] ulong Server,
            [In] uint Timeout,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string BackgroundTaskId,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId);
    }
}
