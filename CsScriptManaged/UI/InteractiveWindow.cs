using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CsScriptManaged.UI
{
    internal class InteractiveWindow
    {
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
                    window = CreateWindow();
                    window.ShowDialog();
                }
                catch (ExitRequestedException)
                {
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                window.Close();
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
                    window = CreateWindow();
                    window.Show();

                    var _dispatcherFrame = new System.Windows.Threading.DispatcherFrame();
                    window.Closed += (obj, e) => { _dispatcherFrame.Continue = false; };
                    System.Windows.Threading.Dispatcher.PushFrame(_dispatcherFrame);
                }
                catch (ExitRequestedException)
                {
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                window.Close();
            }, waitForExecution: false);
        }

        private static void ExecuteInSTA(Action action, bool waitForExecution = true)
        {
            Thread thread = new Thread(() => { action(); });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            if (waitForExecution)
            {
                thread.Join();
            }
        }

        private static Window CreateWindow()
        {
            Window window = new Window();
            InteractiveCodeEditor textEditor = new InteractiveCodeEditor();

            textEditor.CommandExecuted += (text, result) =>
            {
                // TODO:
                if (!string.IsNullOrEmpty(text))
                    MessageBox.Show(text);
            };
            textEditor.CommandFailed += (text, error) =>
            {
                // TODO:
                MessageBox.Show(text + error);
            };
            window.Content = textEditor;
            return window;
        }
    }
}
