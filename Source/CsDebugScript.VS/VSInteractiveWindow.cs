using Microsoft.VisualStudio.Shell;
using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;

namespace CsDebugScript.VS
{
    class VSInteractiveWindowProxy : MarshalByRefObject
    {
        private VSInteractiveWindowControl control;

        public INativeHandleContract CreateControl(InteractiveExecutionInitialization interactiveExecutionInitialization)
        {
            control = new VSInteractiveWindowControl(interactiveExecutionInitialization);
            return FrameworkElementAdapters.ViewToContractAdapter(control);
        }

        public void ShutdownControl()
        {
            control.Dispatcher.InvokeShutdown();
            GC.Collect();
        }

        public void DebuggerEnteredDesignMode()
        {
            control.DebuggerEnteredDesignMode();
        }

        public void DebuggerEnteredRunMode()
        {
            control.DebuggerEnteredRunMode();
        }

        public void DebuggerEnteredBreakMode()
        {
            control.DebuggerEnteredBreakMode();
        }
    }

    class VSInteractiveExecutionBehavior : InteractiveExecutionBehavior
    {
        public const string InitializationScriptRelativePath = "CsDebugScript/init.csx";

        public override string GetResetScriptPath()
        {
            string initializationScriptRelativePath = InitializationScriptRelativePath.Replace('/', Path.DirectorySeparatorChar);
            string initializationScript;

            // Check folder with startup project path
            EnvDTE.SolutionBuild solutionBuild = VSContext.DTE.Solution.SolutionBuild;

            if (solutionBuild != null && solutionBuild.StartupProjects != null)
                foreach (string item in (Array)solutionBuild.StartupProjects)
                {
                    EnvDTE.Project project = VSContext.DTE.Solution.Projects.Item(item);
                    string projectPath = project.FullName;
                    string projectDirectory = projectPath;

                    if (!Directory.Exists(projectPath))
                        projectDirectory = Path.GetDirectoryName(projectPath);
                    initializationScript = Path.Combine(projectDirectory, initializationScriptRelativePath);
                    if (File.Exists(initializationScript))
                        return initializationScript;
                }

            // Check folder with solution path
            string solutionPath = VSContext.DTE.Solution.FullName;
            string solutionDirectory = !Directory.Exists(solutionPath) ? Path.GetDirectoryName(solutionPath) : solutionPath;

            initializationScript = Path.Combine(solutionDirectory, initializationScriptRelativePath);
            if (File.Exists(initializationScript))
                return initializationScript;

            // Check current working directory
            initializationScript = Path.Combine(Directory.GetCurrentDirectory(), initializationScriptRelativePath);
            if (File.Exists(initializationScript))
                return initializationScript;

            // Base class anwser
            return base.GetResetScriptPath();
        }
    }

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("774827b1-2776-4746-bacd-2cd95407f32a")]
    public class VSInteractiveWindow : ToolWindowPane
    {
        internal const string CaptionText = "C# Debug Scripting";
        private const string DomainName = "CsDebugScript";

        private AppDomain scriptDomain;
#if USE_APP_DOMAIN
        private VSInteractiveWindowProxy proxy = null;
#else
        private VSInteractiveWindowControl interactiveControl;
#endif
        private TextBlock unloadedDomainControl;
        private Grid grid;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSInteractiveWindow"/> class.
        /// </summary>
        public VSInteractiveWindow()
            : base(null)
        {
            this.Caption = CaptionText;

            grid = new Grid();
            this.Content = grid;

            unloadedDomainControl = new TextBlock();
            unloadedDomainControl.Text = "This window is only active while debugging";
            grid.Children.Add(unloadedDomainControl);

            VSContext.DebuggerEnteredDesignMode += () =>
            {
#if USE_APP_DOMAIN
                proxy?.DebuggerEnteredDesignMode();
#else
                interactiveControl?.DebuggerEnteredDesignMode();
#endif
                UnloadDomain();
            };
            VSContext.DebuggerEnteredRunMode += () =>
            {
                LoadDomain();
#if USE_APP_DOMAIN
                proxy?.DebuggerEnteredRunMode();
#else
                interactiveControl?.DebuggerEnteredRunMode();
#endif
            };
            VSContext.DebuggerEnteredBreakMode += () =>
            {
                LoadDomain();
#if USE_APP_DOMAIN
                proxy?.DebuggerEnteredBreakMode();
#else
                interactiveControl?.DebuggerEnteredBreakMode();
#endif
            };
            if (VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgBreakMode || VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgRunMode)
            {
                LoadDomain();
            }
        }

        internal VSInteractiveWindowControl InteractiveControl
        {
            get
            {
#if USE_APP_DOMAIN
                throw new NotImplementedException();
#else
                return interactiveControl;
#endif
            }
        }

        private void UnloadDomain()
        {
            if (scriptDomain != null)
            {
                var domain = scriptDomain;
#if USE_APP_DOMAIN
                var proxy = this.proxy;
#endif
                scriptDomain = null;
                grid.Children.Clear();
                grid.Children.Add(unloadedDomainControl);
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
#if USE_APP_DOMAIN
                        proxy?.ShutdownControl();
#endif
                        AppDomain.Unload(domain);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Windows.MessageBox.Show("Domain failed to unload:\n" + ex.ToString());
#endif
                    }
                });
            }
        }

        private void LoadDomain()
        {
            if (scriptDomain == null && VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgBreakMode)
            {
                try
                {
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    AppDomainSetup setup = new AppDomainSetup()
                    {
                        ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        PrivateBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    };
                    scriptDomain = AppDomain.CreateDomain(DomainName, AppDomain.CurrentDomain.Evidence, setup);
                    VSContext.InitializeAppDomain(scriptDomain);
#if USE_APP_DOMAIN
                    proxy = (VSInteractiveWindowProxy)scriptDomain.CreateInstanceAndUnwrap(typeof(VSInteractiveWindowProxy).Assembly.FullName, typeof(VSInteractiveWindowProxy).FullName);
                    var interactiveControl = FrameworkElementAdapters.ContractToViewAdapter(proxy.CreateControl(VSContext.InteractiveExecutionInitialization));
#else
                    interactiveControl = new VSInteractiveWindowControl(VSContext.InteractiveExecutionInitialization);
#endif

                    grid.Children.Clear();
                    grid.Children.Add(interactiveControl);
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Windows.MessageBox.Show("Loading domain failed:\n" + ex.ToString());
#endif
                }
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
        private static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        private static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        public static string StartupPath
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(260);
                GetModuleFileName(NullHandleRef, stringBuilder, stringBuilder.Capacity);
                return Path.GetDirectoryName(stringBuilder.ToString());
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == typeof(VSInteractiveWindowProxy).Assembly.FullName)
            {
                return typeof(VSInteractiveWindowProxy).Assembly;
            }

            return null;
        }
    }
}
