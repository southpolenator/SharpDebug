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

        public INativeHandleContract CreateControl()
        {
            control = new VSInteractiveWindowControl();
            return FrameworkElementAdapters.ViewToContractAdapter(control);
        }

        public void ShutdownControl()
        {
            control.Dispatcher.InvokeShutdown();
            GC.Collect();
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
        private const string DomainName = "CsDebugScript";

        private AppDomain scriptDomain;
        private VSInteractiveWindowProxy proxy;
        private TextBlock unloadedDomainControl;
        private Grid grid;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSInteractiveWindow"/> class.
        /// </summary>
        public VSInteractiveWindow() : base(null)
        {
            this.Caption = "C# Debug Scripting";

            grid = new Grid();
            this.Content = grid;

            unloadedDomainControl = new TextBlock();
            unloadedDomainControl.Text = "This window is only active while debugging";
            grid.Children.Add(unloadedDomainControl);

            VSContext.DebuggerEnteredDesignMode += () => UnloadDomain();
            VSContext.DebuggerEnteredRunMode += () => LoadDomain();
            VSContext.DebuggerEnteredBreakMode += () => LoadDomain();
            if (VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgBreakMode || VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgRunMode)
            {
                LoadDomain();
            }
        }

        private void UnloadDomain()
        {
            if (scriptDomain != null)
            {
                var domain = scriptDomain;
                var proxy = this.proxy;
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
            if (scriptDomain == null)
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
                    var control = FrameworkElementAdapters.ContractToViewAdapter(proxy.CreateControl());
#else
                    var control = new VSInteractiveWindowControl();
#endif

                    grid.Children.Clear();
                    grid.Children.Add(control);
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
