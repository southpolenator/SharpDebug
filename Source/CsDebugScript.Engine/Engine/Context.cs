using CsDebugScript.CLR;
using CsDebugScript.Engine.Utility;
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
        /// The CLR provider interface
        /// </summary>
        public static IClrProvider ClrProvider;

        /// <summary>
        /// The user type metadata (used for casting to user types)
        /// </summary>
        internal static UserTypeMetadata[] UserTypeMetadata;

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
        /// Initializes the Context with the specified debugger engine interface.
        /// </summary>
        /// <param name="debuggerEngine">The debugger engine interface.</param>
        /// <param name="symbolProvider">The symbol provider interface.</param>
        public static void InitializeDebugger(IDebuggerEngine debuggerEngine, ISymbolProvider symbolProvider)
        {
            ClearCache();
            Debugger = debuggerEngine;
            SymbolProvider = symbolProvider;
        }

        /// <summary>
        /// Initializes the Context with the specified debugger engine interface.
        /// </summary>
        /// <param name="debuggerEngine">The debugger engine interface.</param>
        public static void InitializeDebugger(IDebuggerEngine debuggerEngine)
        {
            InitializeDebugger(debuggerEngine, debuggerEngine.GetDefaultSymbolProvider());
        }

        /// <summary>
        /// Clears the internal Engine caches.
        /// </summary>
        public static void ClearCache()
        {
            CacheInvalidator.InvalidateCaches(ClrProvider);
            GlobalCache.Processes.Clear();
            GlobalCache.UserTypeCastedVariableCollections.Clear();
            GlobalCache.UserTypeCastedVariables.Clear();
            GlobalCache.VariablesUserTypeCastedFields.Clear();
            GlobalCache.VariablesUserTypeCastedFieldsByName.Clear();
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
