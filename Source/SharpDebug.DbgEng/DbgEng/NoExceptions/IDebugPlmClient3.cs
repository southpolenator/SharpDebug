using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("D4A5DBD1-CA02-4D90-856A-2A92BFD0F20F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugPlmClient3 : IDebugPlmClient2
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugPlmClient
        [PreserveSig]
        int LaunchPlmPackageForDebugWide(
            [In] ulong Server,
            [In] uint Timeout,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId);
        #endregion

        #region IDebugPlmClient2
        [PreserveSig]
        int LaunchPlmBgTaskForDebugWide(
            [In] ulong Server,
            [In] uint Timeout,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string BackgroundTaskId,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int QueryPlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugOutputStream Stream);

        [PreserveSig]
        int QueryPlmPackageList(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugOutputStream Stream);

        [PreserveSig]
        int EnablePlmPackageDebugWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        [PreserveSig]
        int DisablePlmPackageDebugWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        [PreserveSig]
        int SuspendPlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        [PreserveSig]
        int ResumePlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        [PreserveSig]
        int TerminatePlmPackageWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName);

        [PreserveSig]
        int LaunchAndDebugPlmAppWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string AppName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments);

        [PreserveSig]
        int ActivateAndDebugPlmBgTaskWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string PackageFullName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string BackgroundTaskId);
    }
}
