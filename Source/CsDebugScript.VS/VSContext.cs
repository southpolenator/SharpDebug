using CsDebugScript.Engine.SymbolProviders;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Runtime.InteropServices;

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
        /// The Visual Studio debugger proxy running in Default AppDomain.
        /// </summary>
        private static VSDebuggerProxy debuggerProxy;

        /// <summary>
        /// Gets the current debugging mode.
        /// </summary>
        public static dbgDebugMode CurrentDebugMode
        {
            get
            {
                if (DTE != null)
                {
                    return DTE.Debugger.CurrentMode;
                }
                else
                {
                    return dbgDebugMode.dbgDesignMode;
                }
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
        /// Gets the DTE.
        /// </summary>
        internal static DTE DTE { get; private set; }

        /// <summary>
        /// The VS debugger
        /// </summary>
        internal static VSDebugger VSDebugger { get; private set; }

        /// <summary>
        /// Initializes the <see cref="VSContext"/> class.
        /// </summary>
        static VSContext()
        {
            InitializeDTE();
        }

        /// <summary>
        /// Initializes the DTE.
        /// </summary>
        private static void InitializeDTE()
        {
            SetDTE(GetDTE());
        }

        /// <summary>
        /// Sets the instance of currently running Visual Studio IDE.
        /// </summary>
        /// <param name="dte">The DTE.</param>
        internal static void SetDTE(DTE dte)
        {
            DTE = dte;
            if (dte != null)
            {
                debuggerEvents = DTE.Events.DebuggerEvents;
                debuggerEvents.OnEnterBreakMode += DebuggerEvents_OnEnterBreakMode;
                debuggerEvents.OnEnterDesignMode += DebuggerEvents_OnEnterDesignMode;
                debuggerEvents.OnEnterRunMode += DebuggerEvents_OnEnterRunMode;
                debuggerProxy = (VSDebuggerProxy)AppDomain.CurrentDomain.GetData(VSDebuggerProxy.AppDomainDataName) ?? new VSDebuggerProxy();
                VSDebugger = new VSDebugger(debuggerProxy);
                Engine.Context.InitializeDebugger(VSDebugger, new DiaSymbolProvider());
            }
            else
            {
                // 90s all over again :)
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += (a, b) =>
                {
                    timer.Stop();
                    if (DTE == null)
                    {
                        InitializeDTE();
                    }
                };
                timer.Interval = TimeSpan.FromSeconds(0.1);
                timer.Start();
            }
        }

        /// <summary>
        /// Initializes the application domain with the debugger proxy.
        /// </summary>
        /// <param name="scriptDomain">The script domain.</param>
        internal static void InitializeAppDomain(AppDomain scriptDomain)
        {
            scriptDomain.SetData(VSDebuggerProxy.AppDomainDataName, debuggerProxy);
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

        /// <summary>
        /// Gets the instance of currently running Visual Studio IDE.
        /// </summary>
        private static DTE GetDTE()
        {
            //rot entry for visual studio running under current process.
            IRunningObjectTable rot;
            IEnumMoniker enumMoniker;
            uint fetched;
            IMoniker[] moniker = new IMoniker[1];
            string processId = String.Format(":{0}", System.Diagnostics.Process.GetCurrentProcess().Id);

            GetRunningObjectTable(0, out rot);
            rot.EnumRunning(out enumMoniker);
            enumMoniker.Reset();
            while (enumMoniker.Next(1, moniker, out fetched) == 0)
            {
                IBindCtx bindCtx;
                string displayName;

                CreateBindCtx(0, out bindCtx);
                moniker[0].GetDisplayName(bindCtx, null, out displayName);
                if (displayName.StartsWith("!VisualStudio.DTE.") && displayName.EndsWith(processId))
                {
                    object comObject;

                    rot.GetObject(moniker[0], out comObject);
                    return (DTE)comObject;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the instance of currently running Visual Studio IDE.
        /// </summary>
        /// <param name="programmaticId">The programmatic identifier.</param>
        private static DTE GetDTE(string programmaticId)
        {
            try
            {
                return (DTE)Marshal.GetActiveObject(programmaticId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region Native methods
        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);
        [DllImport("ole32.dll")]
        private static extern void GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
        #endregion
    }
}
