using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("0690E046-9C23-45AC-A04F-987AC29AD0D3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugEventCallbacksWide
    {
        [PreserveSig]
        int GetInterestMask(
            [Out] out uint Mask);

        [PreserveSig]
        int Breakpoint(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 Bp);

        [PreserveSig]
        int Exception(
            [In] ref _EXCEPTION_RECORD64 Exception,
            [In] uint FirstChance);

        [PreserveSig]
        int CreateThread(
            [In] ulong Handle,
            [In] ulong DataOffset,
            [In] ulong StartOffset);

        [PreserveSig]
        int ExitThread(
            [In] uint ExitCode);

        [PreserveSig]
        int CreateProcess(
            [In] ulong ImageFileHandle,
            [In] ulong Handle,
            [In] ulong BaseOffset,
            [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName = null,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName = null,
            [In] uint CheckSum = default(uint),
            [In] uint TimeDateStamp = default(uint),
            [In] ulong InitialThreadHandle = default(ulong),
            [In] ulong ThreadDataOffset = default(ulong),
            [In] ulong StartOffset = default(ulong));

        [PreserveSig]
        int ExitProcess(
            [In] uint ExitCode);

        [PreserveSig]
        int LoadModule(
            [In] ulong ImageFileHandle,
            [In] ulong BaseOffset,
            [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName = null,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName = null,
            [In] uint CheckSum = default(uint),
            [In] uint TimeDateStamp = default(uint));

        [PreserveSig]
        int UnloadModule(
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageBaseName = null,
            [In] ulong BaseOffset = default(ulong));

        [PreserveSig]
        int SystemError(
            [In] uint Error,
            [In] uint Level);

        [PreserveSig]
        int SessionStatus(
            [In] uint Status);

        [PreserveSig]
        int ChangeDebuggeeState(
            [In] uint Flags,
            [In] ulong Argument);

        [PreserveSig]
        int ChangeEngineState(
            [In] uint Flags,
            [In] ulong Argument);

        [PreserveSig]
        int ChangeSymbolState(
            [In] uint Flags,
            [In] ulong Argument);
    }
}
