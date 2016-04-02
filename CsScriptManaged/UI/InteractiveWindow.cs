using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CsScriptManaged.UI
{
    internal class InteractiveWindow : Window
    {
        private const string DefaultStatusText = "Type 'help' to get started :)";
        private const string ExecutingStatusText = "Executing...";
        private const string InitializingStatusText = "Initializing...";
        private InteractiveCodeEditor textEditor;
        private StatusBarItem statusBarStatusText;

        public InteractiveWindow()
        {
            // Set window look
            ShowInTaskbar = false;
            Title = "C# Interactive Window";

            // Add dock panel and status bar
            DockPanel dockPanel = new DockPanel();
            StatusBar statusBar = new StatusBar();
            statusBarStatusText = new StatusBarItem();
            statusBarStatusText.Content = InitializingStatusText;
            statusBar.Items.Add(statusBarStatusText);
            DockPanel.SetDock(statusBar, Dock.Bottom);
            dockPanel.Children.Add(statusBar);
            Content = dockPanel;

            // Add text editor
            textEditor = new InteractiveCodeEditor();
            textEditor.CommandExecuted += TextEditor_CommandExecuted;
            textEditor.CommandFailed += TextEditor_CommandFailed;
            textEditor.Executing += TextEditor_Executing;
            textEditor.CloseRequested += TextEditor_CloseRequested;
            textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            dockPanel.Children.Add(textEditor);
        }

        private void TextEditor_CommandExecuted(string textOutput, object objectOutput)
        {
            // TODO:
            if (!string.IsNullOrEmpty(textOutput))
                MessageBox.Show(textOutput);
        }

        private void TextEditor_CommandFailed(string textOutput, string errorOutput)
        {
            // TODO:
            MessageBox.Show(textOutput + errorOutput);
        }

        private void TextEditor_Executing(bool started)
        {
            if (!started)
            {
                textEditor.TextArea.Focus();
                statusBarStatusText.Content = DefaultStatusText;
            }
            else
                statusBarStatusText.Content = ExecutingStatusText;
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
    }
}
