using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("FD28B4C5-C498-4686-A28E-62CAD2154EB3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugClient6 : IDebugClient5
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

        // ---------------------------------------------------------------------------------------------
        // IDebugClient2
        // ---------------------------------------------------------------------------------------------

        void WriteDumpFile2(
            [In, MarshalAs(UnmanagedType.LPStr)] string DumpFile,
            [In] uint Qualifier,
            [In] uint FormatFlags,
            [In, MarshalAs(UnmanagedType.LPStr)] string Comment = null);

        void AddDumpInformationFile(
            [In, MarshalAs(UnmanagedType.LPStr)] string InfoFile, [In] uint Type);

        void EndProcessServer(
            [In] ulong Server);

        void WaitForProcessServerEnd(
            [In] uint Timeout);

        void IsKernelDebuggerEnabled();

        void TerminateCurrentProcess();

        void DetachCurrentProcess();

        void AbandonCurrentProcess();

        // ---------------------------------------------------------------------------------------------
        // IDebugClient3
        // ---------------------------------------------------------------------------------------------

        uint GetRunningProcessSystemIdByExecutableNameWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ExeName,
            [In] uint Flags);

        void GetRunningProcessDescriptionWide(
            [In] ulong Server,
            [In] uint SystemId,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder ExeName,
            [In] uint ExeNameSize,
            [Out] out uint ActualExeNameSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Description,
            [In] uint DescriptionSize,
            [Out] out uint ActualDescriptionSize);

        void CreateProcessWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string CommandLine,
            [In] uint CreateFlags);

        void CreateProcessAndAttachWide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string CommandLine = null,
            [In] uint CreateFlags = default(uint),
            [In] uint ProcessId = default(uint),
            [In] uint AttachFlags = default(uint));

        // ---------------------------------------------------------------------------------------------
        // IDebugClient4
        // ---------------------------------------------------------------------------------------------

        void OpenDumpFileWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string FileName = null,
            [In] ulong FileHandle = default(ulong));

        void WriteDumpFileWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string FileName = null,
            [In] ulong FileHandle = default(ulong),
            [In] uint Qualifier = default(uint),
            [In] uint FormatFlags = default(uint),
            [In, MarshalAs(UnmanagedType.LPWStr)] string Comment = null);

        void AddDumpInformationFileWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string FileName = null,
            [In] ulong FileHandle = default(ulong),
            [In] uint Type = default(uint));

        uint GetNumberDumpFiles();

        void GetDumpFile(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Handle,
            [Out] out uint Type);

        void GetDumpFileWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Handle,
            [Out] out uint Type);

        // ---------------------------------------------------------------------------------------------
        // IDebugClient5
        // ---------------------------------------------------------------------------------------------

        void AttachKernelWide(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ConnectOptions = null);

        void GetKernelConnectionOptionsWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint OptionsSize);

        void SetKernelConnectionOptionsWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Options);

        void StartProcessServerWide(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Options,
            [In] IntPtr Reserved = default(IntPtr));

        ulong ConnectProcessServerWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string RemoteOptions);

        void StartServerWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Options);

        void OutputServersWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Machine,
            [In] uint Flags);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugOutputCallbacksWide GetOutputCallbacksWide();

        void SetOutputCallbacksWide(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugOutputCallbacksWide Callbacks = null);

        void GetOutputLinePrefixWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PrefixSize);

        void SetOutputLinePrefixWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Prefix);

        void GetIdentityWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint IdentitySize);

        void OutputIdentityWide(
            [In] uint OutputControl,
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugEventCallbacksWide GetEventCallbacksWide();

        void SetEventCallbacksWide(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugEventCallbacksWide Callbacks);

        void CreateProcess2(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPStr)] string CommandLine,
            [In] IntPtr OptionsBuffer,
            [In] uint OptionsBufferSize,
            [In, MarshalAs(UnmanagedType.LPStr)] string InitialDirectory = null,
            [In, MarshalAs(UnmanagedType.LPStr)] string Environment = null);

        void CreateProcess2Wide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string CommandLine,
            [In] IntPtr OptionsBuffer,
            [In] uint OptionsBufferSize,
            [In, MarshalAs(UnmanagedType.LPWStr)] string InitialDirectory = null,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Environment = null);

        void CreateProcessAndAttach2(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPStr)] string CommandLine = null,
            [In] IntPtr OptionsBuffer = default(IntPtr),
            [In] uint OptionsBufferSize = default(uint),
            [In, MarshalAs(UnmanagedType.LPStr)] string InitialDirectory = null,
            [In, MarshalAs(UnmanagedType.LPStr)] string Environment = null,
            [In] uint ProcessId = default(uint),
            [In] uint AttachFlags = default(uint));

        void CreateProcessAndAttach2Wide(
            [In] ulong Server,
            [In, MarshalAs(UnmanagedType.LPWStr)] string CommandLine = null,
            [In] IntPtr OptionsBuffer = default(IntPtr),
            [In] uint OptionsBufferSize = default(uint),
            [In, MarshalAs(UnmanagedType.LPWStr)] string InitialDirectory = null,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Environment = null,
            [In] uint ProcessId = default(uint),
            [In] uint AttachFlags = default(uint));

        void PushOutputLinePrefix(
            [In, MarshalAs(UnmanagedType.LPStr)] string NewPrefix,
            [Out] out ulong Handle);

        void PushOutputLinePrefixWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string NewPrefix,
            [Out] out ulong Handle);

        void PopOutputLinePrefix(
            [In] ulong Handle);

        uint GetNumberInputCallbacks();

        uint GetNumberOutputCallbacks();

        uint GetNumberEventCallbacks(
            [In] uint EventFlags);

        void GetQuitLockString(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void SetQuitLockString(
            [In, MarshalAs(UnmanagedType.LPStr)] string String);

        void GetQuitLockStringWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void SetQuitLockStringWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string String);

        // ---------------------------------------------------------------------------------------------
        // IDebugClient6
        // ---------------------------------------------------------------------------------------------

        void SetEventContextCallbacks(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugEventContextCallbacks Callbacks = null);
    }
}
