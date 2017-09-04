using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [ComImport, Guid("D4A5DBD1-CA02-4D90-856A-2A92BFD0F20F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugPlmClient3 : IDebugPlmClient2
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

        // ---------------------------------------------------------------------------------------------
        // IDebugPlmClient3
        // ---------------------------------------------------------------------------------------------

        void QueryPlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugOutputStream Stream);

        void QueryPlmPackageList(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugOutputStream Stream);

        void EnablePlmPackageDebugWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        void DisablePlmPackageDebugWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        void SuspendPlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        void ResumePlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        void TerminatePlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        void LaunchAndDebugPlmAppWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments);

        void ActivateAndDebugPlmBgTaskWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string BackgroundTaskId);
    }
}
