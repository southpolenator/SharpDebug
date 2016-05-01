using EnvDTE;
using System;

namespace CsDebugScript.VS
{
    internal static class VSContext
    {
        private static DebuggerEvents debuggerEvents;

        public static dbgDebugMode CurrentDebugMode
        {
            get
            {
                return DTE.Debugger.CurrentMode;
            }
        }

        public static event Action DebuggerEnteredBreakMode;

        public static event Action DebuggerEnteredDesignMode;

        public static event Action DebuggerEnteredRunMode;

        internal static IServiceProvider ServiceProvider { get; private set; }

        internal static DTE DTE { get; private set; }

        internal static VSDebugger VSDebugger;

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

        private static void DebuggerEvents_OnEnterRunMode(dbgEventReason Reason)
        {
            DebuggerEnteredRunMode?.Invoke();
        }

        private static void DebuggerEvents_OnEnterDesignMode(dbgEventReason Reason)
        {
            DebuggerEnteredDesignMode?.Invoke();
        }

        private static void DebuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            DebuggerEnteredBreakMode?.Invoke();
            // TODO: VSDebugger?.UpdateCache();
        }
    }
}
