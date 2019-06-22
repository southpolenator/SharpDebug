using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [ComImport, Guid("0690E046-9C23-45AC-A04F-987AC29AD0D3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugEventCallbacksWide
    {
        uint GetInterestMask();

        void Breakpoint(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 Bp);

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
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName = null,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName = null,
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
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName = null,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName = null,
            [In] uint CheckSum = default(uint),
            [In] uint TimeDateStamp = default(uint));

        void UnloadModule(
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImageBaseName = null,
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
