using SharpDebug.Engine;
using SharpDebug.Engine.Debuggers;
using SharpDebug.UI;
using DbgEng;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SharpDebug.WinDbg
{
    /// <summary>
    /// Wraps extension functionality for WinDbg.
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// The interactive execution
        /// </summary>
        public static InteractiveExecution InteractiveExecution { get; private set; } = new InteractiveExecution();

        /// <summary>
        /// Initializes WinDbg extension.
        /// </summary>
        /// <param name="version">The extension version.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>HRESULT</returns>
#if HAS_DLL_EXPORT
        [DllExport]
#endif
        public static int DebugExtensionInitialize(ref uint version, ref uint flags)
        {
            // Set the extension version to 1, which expects exports with this signature:
            //      void _stdcall function(IDebugClient *client, const char *args)
            version = CreateDebugExtensionVersion(1, 0);
            flags = 0;
            return 0;
        }

        /// <summary>
        /// Uninitializes WinDbgExtension.
        /// </summary>
        /// <returns>HRESULT</returns>
#if HAS_DLL_EXPORT
        [DllExport]
#endif
        public static int DebugExtensionUninitialize()
        {
            Context.ClearCache();
            return 0;
        }

        /// <summary>
        /// Executes the specified script.
        /// </summary>
        /// <param name="client">The debug client native pointer.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>HRESULT</returns>
#if HAS_DLL_EXPORT
        [DllExport("execute")]
#endif
        public static int Execute(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            string[] arguments = args.Split(" ".ToCharArray());

            return ExecuteAction(client, () =>
            {
                ScriptExecution.Execute(arguments[0], arguments.Skip(1).ToArray());
            });
        }

        /// <summary>
        /// Starts C# interactive mode in WinDbg.
        /// </summary>
        /// <param name="client">The debug client native pointer.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>HRESULT</returns>
#if HAS_DLL_EXPORT
        [DllExport("interactive")]
#endif
        public static int Interactive(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            return ExecuteAction(client, () =>
            {
                InteractiveExecution.Run();
            });
        }

        /// <summary>
        /// Interprets the specified C# code.
        /// </summary>
        /// <param name="client">The debug client native pointer.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>HRESULT</returns>
#if HAS_DLL_EXPORT
        [DllExport("interpret")]
#endif
        public static int Interpret(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            return ExecuteAction(client, () =>
            {
                InteractiveExecution.Interpret(args);
            });
        }

        /// <summary>
        /// Opens the interactive C# UI.
        /// </summary>
        /// <param name="client">The debug client native pointer.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>HRESULT</returns>
#if HAS_DLL_EXPORT
        [DllExport("openui")]
#endif
        public static int OpenUI(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            string[] arguments = args.Split(" ".ToCharArray());
            bool showModal = false;

            if (arguments.Length > 0 && !bool.TryParse(arguments[0], out showModal))
            {
                showModal = false;
            }

            try
            {
                IDebugClient debugClient = (IDebugClient)Marshal.GetUniqueObjectForIUnknown(client);

                DbgEngDll.InitializeContext(debugClient);
                new Task(() =>
                {
                    if (showModal)
                    {
                        InteractiveWindow.ShowModalWindow();
                    }
                    else
                    {
                        InteractiveWindow.ShowWindow();
                    }
                }).Start();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ex.HResult;
            }
        }

        private static int ExecuteAction(IntPtr client, Action action)
        {
            try
            {
                IDebugClient debugClient = (IDebugClient)Marshal.GetUniqueObjectForIUnknown(client);

                DbgEngDll.InitializeContext(debugClient);
                Context.ClrProvider = new CLR.ClrMdProvider();
                DbgEngDll dbgEngDll = Context.Debugger as DbgEngDll;

                dbgEngDll.ExecuteAction(action);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ex.HResult;
            }
            finally
            {
                DbgEngDll.InitializeContext(null);
            }

            return 0;
        }

        private static uint CreateDebugExtensionVersion(uint Major, uint Minor)
        {
            return ((((Major) & 0xffff) << 16) | ((Minor) & 0xffff));
        }
    }
}
