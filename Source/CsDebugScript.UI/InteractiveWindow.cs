using System;
using System.Windows;
using System.Windows.Controls;

namespace CsDebugScript.UI
{
    /// <summary>
    /// C# REPL interactive window
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public class InteractiveWindow : Window
    {
        private InteractiveWindowContent contentControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindow"/> class.
        /// </summary>
        public InteractiveWindow()
        {
            // Set window look
            ShowInTaskbar = false;
            Title = "C# Interactive Window";

            // Add content
            Grid grid = new Grid();
            Content = grid;
            contentControl = new InteractiveWindowContent();
            contentControl.TextEditor.CloseRequested += TextEditor_CloseRequested;
            grid.Children.Add(contentControl);
        }

        private void TextEditor_CloseRequested()
        {
            Close();
        }

        /// <summary>
        /// Shows the window as modal dialog.
        /// </summary>
        public static void ShowModalWindow()
        {
            ExecuteInSTA(() =>
            {
                Window window = null;

                try
                {
                    window = new InteractiveWindow();
                    window.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                window.Close();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            });
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public static void ShowWindow()
        {
            ExecuteInSTA(() =>
            {
                Window window = null;

                try
                {
                    window = new InteractiveWindow();
                    window.Show();

                    var _dispatcherFrame = new System.Windows.Threading.DispatcherFrame();
                    window.Closed += (obj, e) => { _dispatcherFrame.Continue = false; };
                    System.Windows.Threading.Dispatcher.PushFrame(_dispatcherFrame);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                window.Close();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }, waitForExecution: false);
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
