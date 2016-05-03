using EnvDTE;
using System;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Context for interacting with Visual Studio
    /// </summary>
    internal static class VSContext
    {
        /// <summary>
        /// The debugger events (we need to store this in order to get events)
        /// </summary>
        private static DebuggerEvents debuggerEvents;

        /// <summary>
        /// Gets the current debugging mode.
        /// </summary>
        public static dbgDebugMode CurrentDebugMode
        {
            get
            {
                return DTE.Debugger.CurrentMode;
            }
        }

        /// <summary>
        /// Occurs when debugger enters break mode.
        /// </summary>
        public static event Action DebuggerEnteredBreakMode;

        /// <summary>
        /// Occurs when debugger enters design mode.
        /// </summary>
        public static event Action DebuggerEnteredDesignMode;

        /// <summary>
        /// Occurs when debugger enters run mode.
        /// </summary>
        public static event Action DebuggerEnteredRunMode;

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        internal static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the DTE.
        /// </summary>
        internal static DTE DTE { get; private set; }

        /// <summary>
        /// The VS debugger
        /// </summary>
        internal static VSDebugger VSDebugger { get; private set; }

        /// <summary>
        /// Sets the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            if (ServiceProvider != serviceProvider)
            {
                ServiceProvider = serviceProvider;
                DTE = (DTE)ServiceProvider.GetService(typeof(DTE));
                debuggerEvents = DTE.Events.DebuggerEvents;
                debuggerEvents.OnEnterBreakMode += DebuggerEvents_OnEnterBreakMode;
                debuggerEvents.OnEnterDesignMode += DebuggerEvents_OnEnterDesignMode;
                debuggerEvents.OnEnterRunMode += DebuggerEvents_OnEnterRunMode;
                VSDebugger = new VSDebugger();
            }
        }

        /// <summary>
        /// Occurs when debugger enters run mode.
        /// </summary>
        /// <param name="reason">The reason.</param>
        private static void DebuggerEvents_OnEnterRunMode(dbgEventReason reason)
        {
            DebuggerEnteredRunMode?.Invoke();
        }

        /// <summary>
        /// Occurs when debugger enters design mode.
        /// </summary>
        /// <param name="reason">The reason.</param>
        private static void DebuggerEvents_OnEnterDesignMode(dbgEventReason reason)
        {
            DebuggerEnteredDesignMode?.Invoke();
        }

        /// <summary>
        /// Occurs when debugger enters break mode.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <param name="executionAction">The execution action.</param>
        private static void DebuggerEvents_OnEnterBreakMode(dbgEventReason reason, ref dbgExecutionAction executionAction)
        {
            DebuggerEnteredBreakMode?.Invoke();
            VSDebugger?.UpdateCache();
        }
    }
}
