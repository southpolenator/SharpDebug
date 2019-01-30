using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies which operation to perform during <see cref="IDebugAdvanced2.Request(DebugRequest, IntPtr, uint, IntPtr, uint, out uint)"/> call.
    /// </summary>
    public enum DebugRequest
    {
        /// <summary>
        /// Check the source path for a source server.
        /// </summary>
        SourcePathHasSourceServer,

        /// <summary>
        /// Return the thread context for the stored event in a user-mode minidump file.
        /// </summary>
        TargetExceptionContext,

        /// <summary>
        /// Return the operating system thread ID for the stored event in a user-mode minidump file.
        /// </summary>
        TargetExceptionThread,

        /// <summary>
        /// Return the exception record for the stored event in a user-mode minidump file.
        /// </summary>
        TargetExceptionRecord,

        /// <summary>
        /// Return the default process creation options.
        /// </summary>
        GetAdditionalCreateOptions,

        /// <summary>
        /// Set the default process creation options.
        /// </summary>
        SetAdditionalCreateOptions,

        /// <summary>
        /// Return the version of Windows that is currently running on the target.
        /// </summary>
        GetWin32MajorMinorVersions,

        /// <summary>
        /// Read a stream from a user-mode minidump target.
        /// </summary>
        ReadUserMinidumpStream,

        /// <summary>
        /// Check to see if it is possible for the debugger engine to detach from the current process (leaving the process running but no longer being debugged).
        /// </summary>
        TargetCanDetach,

        /// <summary>
        /// Set the debugger engine's implicit command line.
        /// </summary>
        SetLocalImplicitCommandLine,

        /// <summary>
        /// Return the current event's instruction pointer.
        /// </summary>
        GetCapturedEventCodeOffset,

        /// <summary>
        /// Return up to 64 bytes of memory at the current event's instruction pointer.
        /// </summary>
        ReadCapturedEventCodeStream,

        /// <summary>
        /// Perform a variety of different operations that aid in the interpretation of typed data.
        /// </summary>
        ExtTypedDataAnsi,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        GetExtensionSearchPathWide,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        GetTextCompletionsWide,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        GetCachedSymbolInfo,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        AddCachedSymbolInfo,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        RemoveCachedSymbolInfo,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        GetTextCompletionsAnsi,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        CurrentOutputCallbacksAreDmlAware,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        GetOffsetUnwindInformation,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        GetDumpHeader,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        SetDumpHeader,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        Midori,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        ProcessDescriptors,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        MiscInformation,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        OpenProcessToken,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        OpenThreadToken,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        DuplicateToken,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        QueryInfoToken,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        CloseToken,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        WowProcess,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        WowModule,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        DebugLiveUserNonInvasive,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        ResumeThread,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        InlineQuery,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        TlInstrumentationAware,

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        GetInstrumentationVersion
    }
}
