using CsDebugScript.UI.CodeWindow;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CsDebugScript.UI
{
    internal class InteractiveWindowContent : UserControl
    {
        private const string ExecutingPrompt = "...> ";
        private InteractiveCodeEditor textEditor;
        private StackPanel resultsPanel;
        private TextBlock promptBlock;
        private ScrollViewer scrollViewer;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindowContent"/> class.
        /// </summary>
        public InteractiveWindowContent()
        {
            // Add results panel
            scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Margin = new Thickness(0);
            scrollViewer.Padding = new Thickness(5);
            resultsPanel = new StackPanel();
            resultsPanel.Orientation = Orientation.Vertical;
            resultsPanel.CanVerticallyScroll = true;
            resultsPanel.CanHorizontallyScroll = true;
            scrollViewer.Content = resultsPanel;

            // Add prompt for text editor
            var panel = new Grid();
            resultsPanel.Children.Add(panel);

            promptBlock = new TextBlock();
            promptBlock.HorizontalAlignment = HorizontalAlignment.Left;
            promptBlock.VerticalAlignment = VerticalAlignment.Top;
            promptBlock.FontFamily = new FontFamily("Consolas");
            promptBlock.FontSize = 14;
            promptBlock.Text = ExecutingPrompt;
            promptBlock.SizeChanged += PromptBlock_SizeChanged;
            panel.Children.Add(promptBlock);

            // Add text editor
            textEditor = new InteractiveCodeEditor();
            textEditor.HorizontalAlignment = HorizontalAlignment.Stretch;
            textEditor.Background = Brushes.Transparent;
            textEditor.CommandExecuted += TextEditor_CommandExecuted;
            textEditor.CommandFailed += TextEditor_CommandFailed;
            textEditor.Executing += TextEditor_Executing;
            textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            textEditor.TextArea.PreviewKeyDown += TextEditor_PreviewKeyDown;
            textEditor.TextArea.SizeChanged += TextArea_SizeChanged;
            panel.Children.Add(textEditor);

            Content = scrollViewer;
        }

        internal InteractiveCodeEditor TextEditor
        {
            get
            {
                return textEditor;
            }
        }

        private void PromptBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            textEditor.Padding = new Thickness(promptBlock.ActualWidth, 0, 0, 0);
        }

        private void TextArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.ScrollToEnd();
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
            {
                textOutput = textOutput.Substring(0, textOutput.Length - 1);
            }

            var textBox = new TextBox();
            textBox.FontFamily = new FontFamily("Consolas");
            textBox.FontSize = 14;
            textBox.Text = textOutput;
            textBox.IsReadOnly = true;
            textBox.Background = Brushes.Transparent;
            textBox.BorderBrush = Brushes.Transparent;
            if (error)
            {
                textBox.Foreground = Brushes.Red;
            }

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
            codeControl.IsReadOnly = true;
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
            {
                if (objectOutput != null)
                {
                    UIElement elementOutput = objectOutput as UIElement;
                    LazyUIResult lazyUI = objectOutput as LazyUIResult;

                    if (elementOutput != null)
                    {
                        resultsPanel.Children.Insert(textEditorIndex, elementOutput);
                    }
                    else if (lazyUI != null)
                    {
                        resultsPanel.Children.Insert(textEditorIndex, lazyUI.UIElement);
                    }
                    else
                    {
                        resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(objectOutput.ToString()));
                    }
                }
            }

            if (!string.IsNullOrEmpty(textOutput))
            {
                resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(textOutput));
            }

            resultsPanel.Children.Insert(textEditorIndex, csharpCode ? CreateCSharpCode(textEditor.Text) : CreateDbgCode(textEditor.Text));
            AddSpacing(resultsPanel.Children[textEditorIndex]);
            if (resultsPanel.Children.Count - initialLength > 1)
            {
                AddSpacing(resultsPanel.Children[textEditorIndex + resultsPanel.Children.Count - initialLength - 1]);
            }
        }

        private void TextEditor_CommandFailed(bool csharpCode, string textOutput, string errorOutput)
        {
            int initialLength = resultsPanel.Children.Count;
            int textEditorIndex = initialLength - 1;

            resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(errorOutput, error: true));
            if (!string.IsNullOrEmpty(textOutput))
            {
                resultsPanel.Children.Insert(textEditorIndex, CreateTextOutput(textOutput));
            }

            resultsPanel.Children.Insert(textEditorIndex, csharpCode ? CreateCSharpCode(textEditor.Text) : CreateDbgCode(textEditor.Text));
            AddSpacing(resultsPanel.Children[textEditorIndex]);
            AddSpacing(resultsPanel.Children[textEditorIndex + resultsPanel.Children.Count - initialLength - 1]);
        }

        private void TextEditor_Executing(bool started)
        {
            if (!started)
            {
                textEditor.TextArea.Focus();
                promptBlock.Text = InteractiveExecution.DefaultPrompt;
            }
            else
            {
                promptBlock.Text = ExecutingPrompt;
            }
        }
    }
}
