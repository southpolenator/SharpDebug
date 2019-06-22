using System;

namespace DbgEng
{
    /// <summary>
    /// Create process flags.
    /// </summary>
    [Flags]
    public enum DebugCreateProcess : uint
    {
        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_CREATE_PROCESS_DEFAULT.</note>
        /// </summary>
        Default = 0,

        /// <summary>
        /// The child processes of a process associated with a job are not associated with the job.
        /// If the calling process is not associated with a job, this constant has no effect. If the calling process is associated with a job, the job must set the JOB_OBJECT_LIMIT_BREAKAWAY_OK limit.
        /// <note>Maps to CREATE_BREAKAWAY_FROM_JOB.</note>
        /// </summary>
        BreakawayFromJob = 0x01000000,

        /// <summary>
        /// The new process does not inherit the error mode of the calling process. Instead, the new process gets the default error mode.
        /// This feature is particularly useful for multithreaded shell applications that run with hard errors disabled.
        /// The default behavior is for the new process to inherit the error mode of the caller.Setting this flag changes that default behavior.
        /// <note>Maps to CREATE_DEFAULT_ERROR_MODE.</note>
        /// </summary>
        DefaultErrorMode = 0x04000000,

        /// <summary>
        /// The new process has a new console, instead of inheriting its parent's console (the default). For more information, see Creation of a Console.
        /// This flag cannot be used with DETACHED_PROCESS.
        /// <note>Maps to CREATE_NEW_CONSOLE.</note>
        /// </summary>
        NewConsole = 0x00000010,

        /// <summary>
        /// The new process is the root process of a new process group. The process group includes all processes that are descendants of this root process. The process identifier of the new process group is the same as the process identifier, which is returned in the lpProcessInformation parameter. Process groups are used by the GenerateConsoleCtrlEvent function to enable sending a CTRL+BREAK signal to a group of console processes.
        /// If this flag is specified, CTRL+C signals will be disabled for all processes within the new process group.
        /// This flag is ignored if specified with <see cref="NewConsole"/>.
        /// <note>Maps to CREATE_NEW_PROCESS_GROUP.</note>
        /// </summary>
        NewProcessGroup = 0x00000200,

        /// <summary>
        /// The process is a console application that is being run without a console window. Therefore, the console handle for the application is not set.
        /// This flag is ignored if the application is not a console application, or if it is used with either CREATE_NEW_CONSOLE or DETACHED_PROCESS.
        /// <note>Maps to CREATE_NO_WINDOW.</note>
        /// </summary>
        NoWindow = 0x08000000,

        /// <summary>
        /// The process is to be run as a protected process. The system restricts access to protected processes and the threads of protected processes. For more information on how processes can interact with protected processes, see Process Security and Access Rights.
        /// To activate a protected process, the binary must have a special signature.This signature is provided by Microsoft but not currently available for non-Microsoft binaries.There are currently four protected processes: media foundation, audio engine, Windows error reporting, and system.Components that load into these binaries must also be signed.Multimedia companies can leverage the first two protected processes.For more information, see Overview of the Protected Media Path.
        /// <note>Maps to CREATE_PROTECTED_PROCESS.</note>
        /// </summary>
        ProtectedProcess = 0x00040000,

        /// <summary>
        /// Allows the caller to execute a child process that bypasses the process restrictions that would normally be applied automatically to the process.
        /// <note>Maps to CREATE_PRESERVE_CODE_AUTHZ_LEVEL.</note>
        /// </summary>
        PreserveCodeAuthzLevel = 0x02000000,

        /// <summary>
        /// This flag is valid only when starting a 16-bit Windows-based application. If set, the new process runs in a private Virtual DOS Machine (VDM). By default, all 16-bit Windows-based applications run as threads in a single, shared VDM. The advantage of running separately is that a crash only terminates the single VDM; any other programs running in distinct VDMs continue to function normally. Also, 16-bit Windows-based applications that are run in separate VDMs have separate input queues. That means that if one application stops responding momentarily, applications in separate VDMs continue to receive input. The disadvantage of running separately is that it takes significantly more memory to do so. You should use this flag only if the user requests that 16-bit applications should run in their own VDM.
        /// <note>Maps to CREATE_SEPARATE_WOW_VDM.</note>
        /// </summary>
        SeparateWowVdm = 0x00000800,

        /// <summary>
        /// The flag is valid only when starting a 16-bit Windows-based application. If the DefaultSeparateVDM switch in the Windows section of WIN.INI is TRUE, this flag overrides the switch. The new process is run in the shared Virtual DOS Machine.
        /// <note>Maps to CREATE_SHARED_WOW_VDM.</note>
        /// </summary>
        SharedWowVdm = 0x00001000,

        /// <summary>
        /// The primary thread of the new process is created in a suspended state, and does not run until the ResumeThread function is called.
        /// <note>Maps to CREATE_SUSPENDED.</note>
        /// </summary>
        Suspended = 0x00000004,

        /// <summary>
        /// If this flag is set, the environment block pointed to by lpEnvironment uses Unicode characters. Otherwise, the environment block uses ANSI characters.
        /// <note>Maps to CREATE_UNICODE_ENVIRONMENT.</note>
        /// </summary>
        UnicodeEnvironment = 0x00000400,

        /// <summary>
        /// The calling thread starts and debugs the new process. It can receive all related debug events using the WaitForDebugEvent function.
        /// <note>Maps to DEBUG_ONLY_THIS_PROCESS.</note>
        /// </summary>
        DebugOnlyThisProcess = 0x00000002,

        /// <summary>
        /// The calling thread starts and debugs the new process and all child processes created by the new process. It can receive all related debug events using the WaitForDebugEvent function.
        /// A process that uses DEBUG_PROCESS becomes the root of a debugging chain. This continues until another process in the chain is created with DEBUG_PROCESS.
        /// If this flag is combined with <see cref="DebugOnlyThisProcess"/>, the caller debugs only the new process, not any child processes.
        /// <note>Maps to DEBUG_PROCESS.</note>
        /// </summary>
        DebugProcess = 0x00000001,

        /// <summary>
        /// For console processes, the new process does not inherit its parent's console (the default). The new process can call the AllocConsole function at a later time to create a console. For more information, see Creation of a Console.
        /// This value cannot be used with <see cref="NewConsole"/>.
        /// <note>Maps to DETACHED_PROCESS.</note>
        /// </summary>
        DetachedProcess = 0x00000008,

        /// <summary>
        /// The process is created with extended startup information; the lpStartupInfo parameter specifies a STARTUPINFOEX structure.
        /// <note>Maps to EXTENDED_STARTUPINFO_PRESENT.</note>
        /// </summary>
        ExtendedStartupInfoPresent = 0x00080000,

        /// <summary>
        /// The process inherits its parent's affinity. If the parent process has threads in more than one processor group, the new process inherits the group-relative affinity of an arbitrary group in use by the parent.
        /// <note>Maps to INHERIT_PARENT_AFFINITY.</note>
        /// </summary>
        InheritParentAffinity = 0x00010000,

        /// <summary>
        /// (Microsoft Windows Server 2003 and later) Prevents the debug heap from being used in the new process.
        /// <note>Maps to DEBUG_CREATE_PROCESS_NO_DEBUG_HEAP.</note>
        /// </summary>
        NoDebugHeap = 0x00000400,

        /// <summary>
        /// The native NT RTL process creation routines should be used instead of Win32. This is only meaningful for special processes that run as NT native processes. No Win32 process can be created with this flag.
        /// <note>Maps to DEBUG_CREATE_PROCESS_THROUGH_RTL.</note>
        /// </summary>
        ThroughRtl = 0x00010000,
    }
}
