using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("27FE5639-8407-4F47-8364-EE118FB08AC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugClient
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugClient
        // ---------------------------------------------------------------------------------------------

        void AttachKernel(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.LPStr)] string ConnectOptions = null);

        void GetKernelConnectionOptions(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint OptionsSize);

        void SetKernelConnectionOptions(
            [In, MarshalAs(UnmanagedType.LPStr)] string Options);

        void StartProcessServer(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.LPStr)] string Options,
            [In] IntPtr Reserved = default(IntPtr));

        ulong ConnectProcessServer(
            [In, MarshalAs(UnmanagedType.LPStr)] string RemoteOptions);

        void DisconnectProcessServer(
            [In] ulong Server);

        void GetRunningProcessSystemIds(
            [In] ulong Server,
            [Out] out uint Ids,
            [In] uint Count,
            [Out] out uint ActualCount);

        uint GetRunningProcessSystemIdByExecutableName(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPStr)] string ExeName,
            [In] uint Flags);

        void GetRunningProcessDescription(
            [In] ulong Server,
            [In] uint SystemId,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder ExeName,
            [In] uint ExeNameSize,
            [Out] out uint ActualExeNameSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Description,
            [In] uint DescriptionSize,
            [Out] out uint ActualDescriptionSize);

        void AttachProcess(
            [In] ulong Server,
            [In] uint ProcessId,
            [In] uint AttachFlags);

        void CreateProcess(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPStr)] string CommandLine,
            [In] uint CreateFlags);

        void CreateProcessAndAttach(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPStr)] string CommandLine = null,
            [In] uint CreateFlags = default(uint),
            [In] uint ProcessId = default(uint),
            [In] uint AttachFlags = default(uint));

        uint GetProcessOptions();

        void AddProcessOptions(
            [In] uint Options);

        void RemoveProcessOptions(
            [In] uint Options);

        void SetProcessOptions(
            [In] uint Options);

        void OpenDumpFile(
            [In, MarshalAs(UnmanagedType.LPStr)] string DumpFile);

        void WriteDumpFile(
            [In, MarshalAs(UnmanagedType.LPStr)] string DumpFile,
            [In] uint Qualifier);

        void ConnectSession(
            [In] uint Flags,
            [In] uint HistoryLimit);

        void StartServer(
            [In, MarshalAs(UnmanagedType.LPStr)] string Options);

        void OutputServers(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string Machine,
            [In] uint Flags);

        void TerminateProcesses();

        void DetachProcesses();

        void EndSession(
            [In] uint Flags);

        uint GetExitCode();

        void DispatchCallbacks(
            [In] uint Timeout);

        void ExitDispatch(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugClient Client);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugClient CreateClient();

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugInputCallbacks GetInputCallbacks();

        void SetInputCallbacks(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugInputCallbacks Callbacks = null);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugOutputCallbacks GetOutputCallbacks();

        void SetOutputCallbacks(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugOutputCallbacks Callbacks = null);

        uint GetOutputMask();

        void SetOutputMask(
            [In] uint Mask);

        uint GetOtherOutputMask(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugClient Client);

        void SetOtherOutputMask(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugClient Client,
            [In] uint Mask);

        uint GetOutputWidth();

        void SetOutputWidth(
            [In] uint Columns);

        void GetOutputLinePrefix(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PrefixSize);

        void SetOutputLinePrefix(
            [In, MarshalAs(UnmanagedType.LPStr)] string Prefix = null);

        void GetIdentity(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint IdentitySize);

        void OutputIdentity(
            [In] uint OutputControl,
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugEventCallbacks GetEventCallbacks();

        void SetEventCallbacks(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugEventCallbacks Callbacks = null);

        void FlushCallbacks();
    }
}
