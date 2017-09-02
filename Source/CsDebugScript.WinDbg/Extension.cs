using CsDebugScript.Engine;
using DbgEngManaged;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CsDebugScript.WinDbg
{
    /// <summary>
    /// Wraps extension functionality for WinDbg.
    /// </summary>
    public static class Extension
    {
        private static Executor executor = new Executor();

        /// <summary>
        /// Initializes WinDbg extension.
        /// </summary>
        /// <param name="version">The extension version.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>HRESULT</returns>
        [DllExport]
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
        [DllExport]
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
        [DllExport("execute")]
        public static int Execute(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            return ExecuteAction(client, () =>
            {
                executor.ExecuteScript(args);
            });
        }

        /// <summary>
        /// Starts C# interactive mode in WinDbg.
        /// </summary>
        /// <param name="client">The debug client native pointer.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>HRESULT</returns>
        [DllExport("interactive")]
        public static int Interactive(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            return ExecuteAction(client, () =>
            {
                executor.EnterInteractiveMode(args);
            });
        }

        /// <summary>
        /// Interprets the specified C# code.
        /// </summary>
        /// <param name="client">The debug client native pointer.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>HRESULT</returns>
        [DllExport("interpret")]
        public static int Interpret(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            return ExecuteAction(client, () =>
            {
                executor.Interpret(args);
            });
        }

        /// <summary>
        /// Opens the interactive C# UI.
        /// </summary>
        /// <param name="client">The debug client native pointer.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>HRESULT</returns>
        [DllExport("openui")]
        public static int OpenUI(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
        {
            try
            {
                IDebugClient debugClient = (IDebugClient)Marshal.GetUniqueObjectForIUnknown(client);

                executor.InitializeContext(debugClient);
                new Task(() => executor.OpenUI(args)).Start();
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

                executor.InitializeContext(debugClient);
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ex.HResult;
            }
            finally
            {
                executor.InitializeContext(null);
            }

            return 0;
        }

        private static uint CreateDebugExtensionVersion(uint Major, uint Minor)
        {
            return ((((Major) & 0xffff) << 16) | ((Minor) & 0xffff));
        }
    }
}
