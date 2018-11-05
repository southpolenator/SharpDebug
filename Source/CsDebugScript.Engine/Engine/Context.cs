using CsDebugScript.CLR;
using CsDebugScript.Drawing.Interfaces;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Concurrent;
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
        /// Gets graphics object used for creating drawing objects.
        /// </summary>
        public static IGraphics Graphics { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether debugger is currently in live debugging.
        /// </summary>
        /// <value>
        /// <c>true</c> if debugger is currently in live debugging; otherwise, <c>false</c>.
        /// </value>
        public static bool IsLiveDebugging => Debugger.IsLiveDebugging;

        /// <summary>
        /// The user type metadata (used for casting to user types)
        /// </summary>
        internal static UserTypeMetadata[] UserTypeMetadata { get; private set; }

        /// <summary>
        /// The user type metadata caches (references of caches that can be cleared when <see cref="UserTypeMetadata"/> can be changed).
        /// </summary>
        internal static ConcurrentBag<ICache> UserTypeMetadataCaches { get; private set; } = new ConcurrentBag<ICache>();

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
            var caches = GlobalCache.Caches;
            GlobalCache.Caches = new ConcurrentBag<ICache>();
            foreach (ICache cache in caches)
                cache.InvalidateCache();
        }

        /// <summary>
        /// Updates <see cref="UserTypeMetadata"/> with the new user type metadata collection.
        /// </summary>
        /// <param name="userTypeMetadata">New user type metadata collection.</param>
        internal static void SetUserTypeMetadata(UserTypeMetadata[] userTypeMetadata)
        {
            foreach (ICache cache in UserTypeMetadataCaches)
                cache.InvalidateCache();
            UserTypeMetadataCaches = new ConcurrentBag<ICache>();
            UserTypeMetadata = userTypeMetadata;
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
