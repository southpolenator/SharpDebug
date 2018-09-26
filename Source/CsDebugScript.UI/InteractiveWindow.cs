using Avalonia;
using Avalonia.Controls;
using System;

namespace CsDebugScript.UI
{
    /// <summary>
    /// C# REPL interactive window
    /// </summary>
    /// <seealso cref="Window" />
    public class InteractiveWindow : Window
    {
        internal const string WindowTitle = "C# Interactive Window";

        static InteractiveWindow()
        {
            // Initialize application and all platform specifics
            if (Application.Current == null)
                AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindow"/> class.
        /// </summary>
        public InteractiveWindow()
        {
            // Set window look
            ShowInTaskbar = false;
            Title = WindowTitle;

            // Add content
            Grid grid = new Grid();
            Content = grid;
            //ContentControl = new InteractiveWindowContent();
            //ContentControl.TextEditor.CloseRequested += TextEditor_CloseRequested;
            //grid.Children.Add(ContentControl);
        }

        internal InteractiveWindowContent ContentControl { get; private set; }

        private void TextEditor_CloseRequested()
        {
            Close();
        }

        /// <summary>
        /// Shows the window as modal dialog.
        /// </summary>
        /// <param name="initializer">Action that will initialize <see cref="InteractiveWindow"/> before showing it.</param>
        public static void ShowModalWindow(Action<InteractiveWindow> initializer = null)
        {
            try
            {
                InteractiveWindow window = new InteractiveWindow();

                initializer?.Invoke(window);
                window.Show();
                Application.Current.Run(window);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        /// <param name="initializer">Action that will initialize <see cref="InteractiveWindow"/> before showing it.</param>
        public static void ShowWindow(Action<InteractiveWindow> initializer = null)
        {
            System.Threading.AutoResetEvent windowShown = new System.Threading.AutoResetEvent(false);

            ExecuteInSTA(() =>
            {
                InteractiveWindow window = null;

                try
                {
                    window = new InteractiveWindow();
                    initializer?.Invoke(window);
                    window.Show();
                    windowShown.Set();

                    var _dispatcherFrame = new System.Windows.Threading.DispatcherFrame();
                    window.Closed += (obj, e) => { _dispatcherFrame.Continue = false; };
                    System.Windows.Threading.Dispatcher.PushFrame(_dispatcherFrame);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
                finally
                {
                    windowShown.Set();
                }

                window?.Close();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }, waitForExecution: false);
            windowShown.WaitOne();
        }

        internal static void ExecuteInSTA(Action action, bool waitForExecution = true)
        {
            System.Threading.Thread thread = new System.Threading.Thread(() => { action(); });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            if (waitForExecution)
            {
                thread.Join();
            }
        }
    }
}
