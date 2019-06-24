using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("E9676E2F-E286-4EA3-B0F9-DFE5D9FC330E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSystemObjects3 : IDebugSystemObjects2
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugSystemObjects
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
        #endregion

        #region IDebugSystemObjects2
        uint GetCurrentProcessUpTime();

        ulong GetImplicitThreadDataOffset();

        void SetImplicitThreadDataOffset(
            [In] ulong Offset);

        ulong GetImplicitProcessDataOffset();

        void SetImplicitProcessDataOffset(
            [In] ulong Offset);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        uint GetEventSystem();

        uint GetCurrentSystemId();

        void SetCurrentSystemId(
            [In] uint Id);

        uint GetNumberSystems();

        uint GetSystemIdsByIndex(
            [In] uint Start,
            [In] uint Count);

        void GetTotalNumberThreadsAndProcesses(
            [Out] out uint TotalThreads,
            [Out] out uint TotalProcesses,
            [Out] out uint LargestProcessThreads,
            [Out] out uint LargestSystemThreads,
            [Out] out uint LargestSystemProcesses);

        ulong GetCurrentSystemServer();

        uint GetSystemByServer(
            [In] ulong Server);

        void GetCurrentSystemServerName(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);
    }
}
