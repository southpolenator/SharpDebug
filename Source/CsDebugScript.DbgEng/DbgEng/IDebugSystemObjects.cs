using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("6B86FE2C-2C4F-4F0C-9DA2-174311ACC327"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSystemObjects
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugSystemObjects
        // ---------------------------------------------------------------------------------------------

        uint GetEventThread();

        uint GetEventProcess();

        uint GetCurrentThreadId();

        void SetCurrentThreadId(
            [In] uint Id);

        uint GetCurrentProcessId();

        void SetCurrentProcessId(
            [In] uint Id);

        uint GetNumberThreads();

        void GetTotalNumberThreads(
            [Out] out uint Total,
            [Out] out uint LargestProcess);

        void GetThreadIdsByIndex(
            [In] uint Start,
            [In] uint Count,
            [Out] out uint Ids,
            [Out] out uint SysIds);

        uint GetThreadIdByProcessor(
            [In] uint Processor);

        ulong GetCurrentThreadDataOffset();

        uint GetThreadIdByDataOffset(
            [In] ulong Offset);

        ulong GetCurrentThreadTeb();

        uint GetThreadIdByTeb(
            [In] ulong Offset);

        uint GetCurrentThreadSystemId();

        uint GetThreadIdBySystemId(
            [In] uint SysId);

        ulong GetCurrentThreadHandle();

        uint GetThreadIdByHandle(
            [In] ulong Handle);

        uint GetNumberProcesses();

        void GetProcessIdsByIndex(
            [In] uint Start,
            [In] uint Count,
            [Out] out uint Ids,
            [Out] out uint SysIds);

        ulong GetCurrentProcessDataOffset();

        uint GetProcessIdByDataOffset(
            [In] ulong Offset);

        ulong GetCurrentProcessPeb();

        uint GetProcessIdByPeb(
            [In] ulong Offset);

        uint GetCurrentProcessSystemId();

        uint GetProcessIdBySystemId(
            [In] uint SysId);

        ulong GetCurrentProcessHandle();

        uint GetProcessIdByHandle(
            [In] ulong Handle);

        void GetCurrentProcessExecutableName(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ExeSize);
    }
}
