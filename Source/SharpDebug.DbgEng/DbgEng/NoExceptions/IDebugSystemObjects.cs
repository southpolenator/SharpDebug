using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("6B86FE2C-2C4F-4F0C-9DA2-174311ACC327"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSystemObjects
    {
        [PreserveSig]
        int GetEventThread(
            [Out] out uint Thread);

        [PreserveSig]
        int GetEventProcess(
            [Out] out uint Process);

        [PreserveSig]
        int GetCurrentThreadId(
            [Out] out uint Id);

        [PreserveSig]
        int SetCurrentThreadId(
            [In] uint Id);

        [PreserveSig]
        int GetCurrentProcessId(
            [Out] out uint Id);

        [PreserveSig]
        int SetCurrentProcessId(
            [In] uint Id);

        [PreserveSig]
        int GetNumberThreads(
            [Out] out uint Number);

        [PreserveSig]
        int GetTotalNumberThreads(
            [Out] out uint Total,
            [Out] out uint LargestProcess);

        [PreserveSig]
        int GetThreadIdsByIndex(
            [In] uint Start,
            [In] uint Count,
            [Out] out uint Ids,
            [Out] out uint SysIds);

        [PreserveSig]
        int GetThreadIdByProcessor(
            [In] uint Processor,
            [Out] out uint Id);

        [PreserveSig]
        int GetCurrentThreadDataOffset(
            [Out] out ulong Offset);

        [PreserveSig]
        int GetThreadIdByDataOffset(
            [In] ulong Offset,
            [Out] out uint Id);

        [PreserveSig]
        int GetCurrentThreadTeb(
            [Out] out ulong Teb);

        [PreserveSig]
        int GetThreadIdByTeb(
            [In] ulong Offset,
            [Out] out uint Id);

        [PreserveSig]
        int GetCurrentThreadSystemId(
            [Out] out uint Id);

        [PreserveSig]
        int GetThreadIdBySystemId(
            [In] uint SysId,
            [Out] out uint Id);

        [PreserveSig]
        int GetCurrentThreadHandle(
            [Out] out ulong Handle);

        [PreserveSig]
        int GetThreadIdByHandle(
            [In] ulong Handle,
            [Out] out uint Id);

        [PreserveSig]
        int GetNumberProcesses(
            [Out] out uint Number);

        [PreserveSig]
        int GetProcessIdsByIndex(
            [In] uint Start,
            [In] uint Count,
            [Out] out uint Ids,
            [Out] out uint SysIds);

        [PreserveSig]
        int GetCurrentProcessDataOffset(
            [Out] out ulong Offset);

        [PreserveSig]
        int GetProcessIdByDataOffset(
            [In] ulong Offset,
            [Out] uint Id);

        [PreserveSig]
        int GetCurrentProcessPeb(
            [Out] out ulong Peb);

        [PreserveSig]
        int GetProcessIdByPeb(
            [In] ulong Offset,
            [Out] out uint Id);

        [PreserveSig]
        int GetCurrentProcessSystemId(
            [Out] out uint Id);

        [PreserveSig]
        int GetProcessIdBySystemId(
            [In] uint SysId,
            [Out] out uint Id);

        [PreserveSig]
        int GetCurrentProcessHandle(
            [Out] out ulong Handle);

        [PreserveSig]
        int GetProcessIdByHandle(
            [In] ulong Handle,
            [Out] out uint Id);

        [PreserveSig]
        int GetCurrentProcessExecutableName(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ExeSize);
    }
}
