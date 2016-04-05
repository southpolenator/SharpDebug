using CsScriptManaged.UI.CodeWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CsScriptManaged.UI
{
    internal class InteractiveWindow : Window
    {
        private const string DefaultStatusText = "Type 'help' to get started :)";
        private const string ExecutingStatusText = "Executing...";
        private const string InitializingStatusText = "Initializing...";
        private const string ExecutingPrompt = "...> ";
        private static readonly Brush ExecutingBackground = Brushes.LightGray;
        private static readonly Brush NormalBackground = Brushes.White;
        private InteractiveCodeEditor textEditor;
        private StackPanel resultsPanel;
        private TextBlock promptBlock;
        private StatusBarItem statusBarStatusText;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindow"/> class.
        /// </summary>
        public InteractiveWindow()
        {
            // Set window look
            Background = ExecutingBackground;
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

            // Add results panel
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Margin = new Thickness(5);
            dockPanel.Children.Add(scrollViewer);
            resultsPanel = new StackPanel();
            resultsPanel.Orientation = Orientation.Vertical;
            resultsPanel.CanVerticallyScroll = true;
            resultsPanel.CanHorizontallyScroll = true;
            scrollViewer.Content = resultsPanel;

            // Add prompt for text editor
            var panel = new DockPanel();
            resultsPanel.Children.Add(panel);

            promptBlock = new TextBlock();
            promptBlock.FontFamily = new FontFamily("Consolas");
            promptBlock.FontSize = 14;
            promptBlock.Text = ExecutingPrompt;
            DockPanel.SetDock(promptBlock, Dock.Left);
            panel.Children.Add(promptBlock);

            // Add text editor
            textEditor = new InteractiveCodeEditor();
            textEditor.Background = Brushes.Transparent;
            textEditor.CommandExecuted += TextEditor_CommandExecuted;
            textEditor.CommandFailed += TextEditor_CommandFailed;
            textEditor.Executing += TextEditor_Executing;
            textEditor.CloseRequested += TextEditor_CloseRequested;
            textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textEditor.TextArea.PreviewKeyDown += TextEditor_PreviewKeyDown;
            panel.Children.Add(textEditor);
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                if (textEditor.Document.GetLocation(textEditor.CaretOffset).Line == 1)
                {
                    e.Handled = true;
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    textEditor.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                }
            }
            else if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                if (textEditor.Document.GetLocation(textEditor.CaretOffset).Line == textEditor.LineCount)
                {
                    e.Handled = true;
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        private UIElement CreateTextOutput(string textOutput, bool error = false)
        {
            textOutput = textOutput.Replace("\r\n", "\n");
            if (textOutput.EndsWith("\n"))
                textOutput = textOutput.Substring(0, textOutput.Length - 1);

            var textBox = new TextBox();
            textBox.FontFamily = new FontFamily("Consolas");
            textBox.FontSize = 14;
            textBox.Text = textOutput;
            textBox.IsReadOnly = true;
            textBox.Background = Brushes.Transparent;
            textBox.BorderBrush = Brushes.Transparent;
            if (error)
                textBox.Foreground = Brushes.Red;
            return textBox;
        }

        private UIElement CreateDbgCode(string text)
        {
            var textBlock = new TextBlock();
            textBlock.FontFamily = new FontFamily("Consolas");
            textBlock.FontSize = 14;
            textBlock.Text = "#dbg> " + text;
            textBlock.Background = Brushes.Transparent;
            return textBlock;
        }

        private UIElement CreateCSharpCode(string code)
        {
            var panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;

            var textBlock = new TextBlock();
            textBlock.FontFamily = new FontFamily("Consolas");
            textBlock.FontSize = 14;
            textBlock.Text = InteractiveExecution.DefaultPrompt;
            panel.Children.Add(textBlock);

            var codeControl = new CsTextEditor();
            codeControl.IsEnabled = false;
            codeControl.Text = code;
            codeControl.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            codeControl.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            codeControl.Background = Brushes.Transparent;
            panel.Children.Add(codeControl);

            return panel;
        }

        private void AddSpacing(UIElement uiElement)
        {
            AddSpacing((FrameworkElement)uiElement);
        }

        private void AddSpacing(FrameworkElement element)
        {
            element.Margin = new Thickness(0, 0, 0, 10);
        }

        private void TextEditor_CommandExecuted(bool csharpCode, string textOutput, IEnumerable<object> objectsOutput)
        {
            int initialLength = resultsPanel.Children.Count;
            int textEditorIndex = initialLength - 1;

            foreach (var objectOutput in objectsOutput.Reverse())
                if (objectOutput != null)
                {
                    UIElement elementOutput = objectOutput as UIElement;
                    LazyUIResult lazyUI = objectOutput as LazyUIResult;

                    if (elementOutput != null)
                        resultsPanel.Children.Insert(textEditorIndex, elementOutput);
                    else if (lazyUI != null)
                        resultsPanel.Children.Insert(textEditorIndex, lazyUI.UIElement);
                    else
                        resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(objectOutput.ToString()));
                }

            if (!string.IsNullOrEmpty(textOutput))
                resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(textOutput));
            resultsPanel.Children.Insert(textEditorIndex, csharpCode ? CreateCSharpCode(textEditor.Text) : CreateDbgCode(textEditor.Text));
            AddSpacing(resultsPanel.Children[textEditorIndex]);
            if (resultsPanel.Children.Count - initialLength > 1)
                AddSpacing(resultsPanel.Children[textEditorIndex + resultsPanel.Children.Count - initialLength - 1]);
        }

        private void TextEditor_CommandFailed(bool csharpCode, string textOutput, string errorOutput)
        {
            int initialLength = resultsPanel.Children.Count;
            int textEditorIndex = initialLength - 1;

            resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(errorOutput, error: true));
            if (!string.IsNullOrEmpty(textOutput))
                resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(textOutput));
            resultsPanel.Children.Insert(textEditorIndex, csharpCode ? CreateCSharpCode(textEditor.Text) : CreateDbgCode(textEditor.Text));
            AddSpacing(resultsPanel.Children[textEditorIndex]);
            AddSpacing(resultsPanel.Children[textEditorIndex + resultsPanel.Children.Count - initialLength - 1]);
        }

        private void TextEditor_Executing(bool started)
        {
            if (!started)
            {
                textEditor.TextArea.Focus();
                statusBarStatusText.Content = DefaultStatusText;
                Background = NormalBackground;
                promptBlock.Text = InteractiveExecution.DefaultPrompt;
            }
            else
            {
                statusBarStatusText.Content = ExecutingStatusText;
                Background = ExecutingBackground;
                promptBlock.Text = ExecutingPrompt;
            }
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
