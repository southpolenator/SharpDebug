using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    /// <summary>
    /// Static class with functions that provide creation of interfaces pointers to debug client objects.
    /// </summary>
    public static class DebugClient
    {
        /// <summary>
        /// Opens the specified dump file.
        /// </summary>
        /// <param name="dumpFile">The dump file.</param>
        /// <param name="symbolPath">The symbol path.</param>
        public static IDebugClient OpenDumpFile(string dumpFile, string symbolPath)
        {
            IDebugClient client = DebugCreate();
            IDebugSymbols5 symbols = (IDebugSymbols5)client;
            IDebugControl7 control = (IDebugControl7)client;

            symbols.SetSymbolPathWide(symbolPath);
            client.OpenDumpFile(dumpFile);
            control.WaitForEvent(0, uint.MaxValue);
            symbols.SetSymbolPathWide(symbolPath);
            control.Execute(0, ".reload -f", 0);
            return client;
        }

        /// <summary>
        /// Starts a new process.
        /// </summary>
        /// <param name="processPath">Process path.</param>
        /// <param name="processArguments">Process arguments.</param>
        /// <param name="symbolPath">Symbol path.</param>
        /// <param name="debugEngineOptions">Debug engine options.</param>
        /// <returns></returns>
        public static IDebugClient OpenProcess(string processPath, string processArguments, string symbolPath, uint debugEngineOptions)
        {
            string processCommandLine = processPath + " " + processArguments;

            IDebugClient client = DebugCreate();
            IDebugSymbols5 symbols = (IDebugSymbols5)client;
            IDebugControl7 control = (IDebugControl7)client;

            symbols.SetSymbolPathWide(symbolPath);
            control.SetEngineOptions(debugEngineOptions);
            client.CreateProcessAndAttach(0, processCommandLine, (uint)Defines.DebugProcessOnlyThisProcess, 0, 0);
            control.WaitForEvent(0, uint.MaxValue);
            return client;
        }

        /// <summary>
        /// The DebugCreate function creates a new client object and returns an interface pointer to it.
        /// </summary>
        public static IDebugClient DebugCreate()
        {
            IDebugClient client;

            DebugCreate(new Guid("27FE5639-8407-4F47-8364-EE118FB08AC8"), out client);
            return client;
        }

        /// <summary>
        /// The DebugCreateEx function creates a new client object and returns an interface pointer to it.
        /// </summary>
        /// <param name="dbgEngOptions">Supplies debugger option flags.</param>
        public static IDebugClient DebugCreateEx(uint dbgEngOptions)
        {
            IDebugClient client;

            DebugCreateEx(new Guid("27FE5639-8407-4F47-8364-EE118FB08AC8"), dbgEngOptions, out client);
            return client;
        }

        #region Native functions
        /// <summary>
        /// The DebugCreate function creates a new client object and returns an interface pointer to it.
        /// </summary>
        /// <param name="InterfaceId">Specifies the interface identifier (IID) of the desired debugger engine client interface. This is the type of the interface that will be returned in Interface.</param>
        /// <param name="client">Receives an interface pointer for the new client. The type of this interface is specified by InterfaceId.</param>
        [DllImport("dbgeng.dll", EntryPoint = "DebugCreate", SetLastError = false, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
        private static extern void DebugCreate([In][MarshalAs(UnmanagedType.LPStruct)]Guid InterfaceId, out IDebugClient client);

        /// <summary>
        /// The DebugCreateEx function creates a new client object and returns an interface pointer to it.
        /// </summary>
        /// <param name="InterfaceId">Specifies the interface identifier (IID) of the desired debugger engine client interface. This is the type of the interface that will be returned in Interface.</param>
        /// <param name="DbgEngOptions">Supplies debugger option flags.</param>
        /// <param name="client">Receives an interface pointer for the new client. The type of this interface is specified by InterfaceId.</param>
        [DllImport("dbgeng.dll", EntryPoint = "DebugCreateEx", SetLastError = false, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
        private static extern void DebugCreateEx([In][MarshalAs(UnmanagedType.LPStruct)]Guid InterfaceId, uint DbgEngOptions, out IDebugClient client);
        #endregion
    }
}
