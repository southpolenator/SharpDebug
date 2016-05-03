using CsDebugScript.Engine.Debuggers;
using CsDebugScript.Engine.SymbolProviders;
using DbgEngManaged;
using System;
using System.IO;
using System.Reflection;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// Static class that has the whole debugging engine context
    /// </summary>
    public static class Context
    {
        /// <summary>
        /// The debugger engine interface
        /// </summary>
        public static IDebuggerEngine Debugger;

        /// <summary>
        /// The symbol provider interface
        /// </summary>
        public static ISymbolProvider SymbolProvider;

        /// <summary>
        /// The DIA symbol provider
        /// </summary>
        private static DiaSymbolProvider DiaSymbolProvider = new DiaSymbolProvider();

        /// <summary>
        /// The user type metadata (used for casting to user types)
        /// </summary>
        internal static UserTypeMetadata[] UserTypeMetadata;

        /// <summary>
        /// The settings for script execution
        /// </summary>
        internal static Settings Settings = new Settings();

        /// <summary>
        /// Gets or sets a value indicating whether variable caching is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if variable caching is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableVariableCaching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user casted variable caching is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if user casted variable caching is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableUserCastedVariableCaching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether variable path tracking is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if variable path tracking is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableVariablePathTracking { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether debugger is currently in live debugging.
        /// </summary>
        /// <value>
        /// <c>true</c> if debugger is currently in live debugging; otherwise, <c>false</c>.
        /// </value>
        public static bool IsLiveDebugging
        {
            get
            {
                return Debugger.IsLiveDebugging;
            }
        }

        /// <summary>
        /// Initializes the Context with the specified DbgEng.dll Client interface.
        /// </summary>
        /// <param name="client">The DbgEng.dll Client interface.</param>
        public static void Initalize(IDebugClient client)
        {
            Debugger = new DbgEngDll(client);
            SymbolProvider = Debugger.CreateDefaultSymbolProvider();
            SymbolProvider = DiaSymbolProvider;
        }

        /// <summary>
        /// Initializes the Context with the specified debugger engine interface.
        /// </summary>
        /// <param name="debuggerEngine">The debugger engine interface.</param>
        public static void InitializeDebugger(IDebuggerEngine debuggerEngine)
        {
            Debugger = debuggerEngine;
            //SymbolProvider = Debugger.CreateDefaultSymbolProvider();
            SymbolProvider = DiaSymbolProvider;
        }

        /// <summary>
        /// Clears the metadata cache.
        /// </summary>
        internal static void ClearMetadataCache()
        {
            // Clear metadata from processes
            foreach (var process in GlobalCache.Processes.Values)
            {
                process.ClearMetadataCache();
            }

            // Clear user types metadata
            UserTypeMetadata = new UserTypeMetadata[0];
            foreach (var cacheEntry in GlobalCache.VariablesUserTypeCastedFields)
            {
                cacheEntry.Cached = false;
            }

            foreach (var cacheEntry in GlobalCache.VariablesUserTypeCastedFieldsByName)
            {
                cacheEntry.Clear();
            }

            foreach (var cacheEntry in GlobalCache.UserTypeCastedVariableCollections)
            {
                cacheEntry.Cached = false;
            }

            foreach (var cacheEntry in GlobalCache.UserTypeCastedVariables)
            {
                cacheEntry.Clear();
            }

            GlobalCache.VariablesUserTypeCastedFields.Clear();
            GlobalCache.VariablesUserTypeCastedFieldsByName.Clear();
            GlobalCache.UserTypeCastedVariableCollections.Clear();
        }

        /// <summary>
        /// Gets the assembly directory.
        /// </summary>
        internal static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            path = Path.GetDirectoryName(path);
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            return path;
        }
    }
}
