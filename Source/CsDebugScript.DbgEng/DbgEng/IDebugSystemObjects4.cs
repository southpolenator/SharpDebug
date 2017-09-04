using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("489468E6-7D0F-4AF5-87AB-25207454D553"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSystemObjects4 : IDebugSystemObjects3
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

        // ---------------------------------------------------------------------------------------------
        // IDebugSystemObjects2
        // ---------------------------------------------------------------------------------------------

        uint GetCurrentProcessUpTime();

        ulong GetImplicitThreadDataOffset();

        void SetImplicitThreadDataOffset(
            [In] ulong Offset);

        ulong GetImplicitProcessDataOffset();

        void SetImplicitProcessDataOffset(
            [In] ulong Offset);

        // ---------------------------------------------------------------------------------------------
        // IDebugSystemObjects3
        // ---------------------------------------------------------------------------------------------

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

        // ---------------------------------------------------------------------------------------------
        // IDebugSystemObjects4
        // ---------------------------------------------------------------------------------------------

        void GetCurrentProcessExecutableNameWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ExeSize);

        void GetCurrentSystemServerNameWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);
    }
}
