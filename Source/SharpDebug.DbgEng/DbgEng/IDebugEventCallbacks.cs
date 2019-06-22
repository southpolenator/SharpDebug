using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [ComImport, Guid("337BE28B-5036-4D72-B6BF-C45FBB9F2EAA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugEventCallbacks
    {
        uint GetInterestMask();

        int Breakpoint(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp);

        void Exception(
            [In] ref _EXCEPTION_RECORD64 Exception,
            [In] uint FirstChance);

        void CreateThread(
            [In] ulong Handle,
            [In] ulong DataOffset,
            [In] ulong StartOffset);

        void ExitThread(
            [In] uint ExitCode);

        void CreateProcess(
            [In] ulong ImageFileHandle,
            [In] ulong Handle,
            [In] ulong BaseOffset,
            [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName = null,
            [In, MarshalAs(UnmanagedType.LPStr)] string ImageName = null,
            [In] uint CheckSum = default(uint),
            [In] uint TimeDateStamp = default(uint),
            [In] ulong InitialThreadHandle = default(ulong),
            [In] ulong ThreadDataOffset = default(ulong),
            [In] ulong StartOffset = default(ulong));

        void ExitProcess(
            [In] uint ExitCode);

        void LoadModule(
            [In] ulong ImageFileHandle,
            [In] ulong BaseOffset,
            [In] uint ModuleSize,
            [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName = null,
            [In, MarshalAs(UnmanagedType.LPStr)] string ImageName = null,
            [In] uint CheckSum = default(uint),
            [In] uint TimeDateStamp = default(uint));

        void UnloadModule(
            [In, MarshalAs(UnmanagedType.LPStr)] string ImageBaseName = null,
            [In] ulong BaseOffset = default(ulong));

        void SystemError(
            [In] uint Error,
            [In] uint Level);

        void SessionStatus(
            [In] uint Status);

        void ChangeDebuggeeState(
            [In] uint Flags,
            [In] ulong Argument);

        void ChangeEngineState(
            [In] uint Flags,
            [In] ulong Argument);

        void ChangeSymbolState(
            [In] uint Flags,
            [In] ulong Argument);
    }
}
