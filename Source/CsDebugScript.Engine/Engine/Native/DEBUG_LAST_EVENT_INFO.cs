using System;
using System.Runtime.InteropServices;

namespace CsDebugScript.Engine.Native
{
    public enum DEBUG_EVENT
    {
        /// <summary>
        /// A breakpoint exception occurred in the target.
        /// </summary>
        DEBUG_EVENT_BREAKPOINT,

        /// <summary>
        /// An exception debugging event occurred in the target.
        /// </summary>
        DEBUG_EVENT_EXCEPTION,

        /// <summary>
        /// A create-thread debugging event occurred in the target.
        /// </summary>
        DEBUG_EVENT_CREATE_THREAD,

        /// <summary>
        /// An exit-thread debugging event occurred in the target.
        /// </summary>
        DEBUG_EVENT_EXIT_THREAD,

        /// <summary>
        /// A create-process debugging event occurred in the target.
        /// </summary>
        DEBUG_EVENT_CREATE_PROCESS,

        /// <summary>
        /// An exit-process debugging event occurred in the target.
        /// </summary>
        DEBUG_EVENT_EXIT_PROCESS,

        /// <summary>
        /// A module-load debugging event occurred in the target.
        /// </summary>
        DEBUG_EVENT_LOAD_MODULE,

        /// <summary>
        /// A module-unload debugging event occurred in the target.
        /// </summary>
        DEBUG_EVENT_UNLOAD_MODULE,

        /// <summary>
        /// A system error occurred in the target.
        /// </summary>
        DEBUG_EVENT_SYSTEM_ERROR,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DEBUG_LAST_EVENT_INFO
    {
        [FieldOffset(0)]
        public DEBUG_LAST_EVENT_INFO_BREAKPOINT Breakpoint;
        [FieldOffset(0)]
        public DEBUG_LAST_EVENT_INFO_EXCEPTION Exception;
        [FieldOffset(0)]
        public DEBUG_LAST_EVENT_INFO_EXIT_THREAD ExitThread;
        [FieldOffset(0)]
        public DEBUG_LAST_EVENT_INFO_EXIT_PROCESS ExitProcess;
        [FieldOffset(0)]
        public DEBUG_LAST_EVENT_INFO_LOAD_MODULE LoadModule;
        [FieldOffset(0)]
        public DEBUG_LAST_EVENT_INFO_UNLOAD_MODULE UnloadModule;
        [FieldOffset(0)]
        public DEBUG_LAST_EVENT_INFO_SYSTEM_ERROR SystemError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EXCEPTION_RECORD64
    {
        public UInt32 ExceptionCode;
        public UInt32 ExceptionFlags;
        public UInt64 ExceptionRecord;
        public UInt64 ExceptionAddress;
        public UInt32 NumberParameters;
        public UInt32 __unusedAlignment;
        public fixed UInt64 ExceptionInformation[15]; //EXCEPTION_MAXIMUM_PARAMETERS
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_LAST_EVENT_INFO_BREAKPOINT
    {
        public uint Id;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_LAST_EVENT_INFO_EXCEPTION
    {
        public EXCEPTION_RECORD64 ExceptionRecord;
        public uint FirstChance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_LAST_EVENT_INFO_EXIT_THREAD
    {
        public uint ExitCode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_LAST_EVENT_INFO_EXIT_PROCESS
    {
        public uint ExitCode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_LAST_EVENT_INFO_LOAD_MODULE
    {
        public ulong Base;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_LAST_EVENT_INFO_UNLOAD_MODULE
    {
        public ulong Base;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_LAST_EVENT_INFO_SYSTEM_ERROR
    {
        public uint Error;
        public uint Level;
    }
}
