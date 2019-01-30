using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [Guid("61A4905B-23F9-4247-B3C5-53D087529AB7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public unsafe interface IDebugEventContextCallbacks
    {
        [PreserveSig]
        int GetInterestMask(
            [Out] out uint Mask);

        [PreserveSig]
        int Breakpoint(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 Bp,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int Exception(
            [In] ref _EXCEPTION_RECORD64 Exception,
            [In] uint FirstChance,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int CreateThread(
            [In] ulong Handle,
            [In] ulong DataOffset,
            [In] ulong StartOffset,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int ExitThread(
            [In] uint ExitCode,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int CreateProcess(
            [In] ulong ImageFileHandle,
            [In] ulong Handle,
            [In] ulong BaseOffset,
            [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName,
            [In] uint CheckSum,
            [In] uint TimeDateStamp,
            [In] ulong InitialThreadHandle,
            [In] ulong ThreadDataOffset,
            [In] ulong StartOffset,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int ExitProcess(
            [In] uint ExitCode,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int LoadModule(
            [In] ulong ImageFileHandle,
            [In] ulong BaseOffset,
            [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName,
            [In] uint CheckSum,
            [In] uint TimeDateStamp,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int UnloadModule(
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageBaseName,
            [In] ulong BaseOffset,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int SystemError(
            [In] uint Error,
            [In] uint Level,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int SessionStatus(
            [In] uint Status);

        [PreserveSig]
        int ChangeDebuggeeState(
            [In] uint Flags,
            [In] ulong Argument,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int ChangeEngineState(
            [In] uint Flags,
            [In] ulong Argument,
            [In] _DEBUG_EVENT_CONTEXT* Context,
            [In] uint ContextSize);

        [PreserveSig]
        int ChangeSymbolState(
            [In] uint Flags,
            [In] ulong Argument);
    }
}
