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
        private ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors;
        private string fontFamily;
        private double fontSize;
        private int indentationSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindowContent" /> class.
        /// </summary>
        /// <param name="highlightingColors">The highlighting colors.</param>
        public InteractiveWindowContent(params ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors)
            : this("Consolas", 14, 4, highlightingColors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindowContent"/> class.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="indentationSize">Size of the indentation.</param>
        /// <param name="highlightingColors">The highlighting colors.</param>
        public InteractiveWindowContent(string fontFamily, double fontSize, int indentationSize, params ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors)
        {
            this.fontFamily = fontFamily;
            this.fontSize = fontSize;
            this.indentationSize = indentationSize;
            this.highlightingColors = highlightingColors;

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
            textEditor = new InteractiveCodeEditor(new InteractiveResultVisualizer(this), fontFamily, fontSize, indentationSize, highlightingColors);
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

        public FocusNavigationDirection? TraverseDirection { get; private set; }

        public FrameworkElement GetFocusedResultItem()
        {
            FrameworkElement elementWithFocus = Keyboard.FocusedElement as FrameworkElement;

            while (elementWithFocus != null && elementWithFocus != textEditor.TextArea && elementWithFocus.Parent != resultsPanel)
            {
                elementWithFocus = elementWithFocus.Parent as FrameworkElement;
            }

            if (elementWithFocus == textEditor.TextArea)
            {
                return textEditor;
            }

            return elementWithFocus;
        }

        public void TraverseNext(FrameworkElement focus)
        {
            FrameworkElement elementWithFocus = Keyboard.FocusedElement as FrameworkElement;

            focus = focus ?? GetFocusedResultItem();
            if (focus == null)
            {
                textEditor.Focus();
                return;
            }

            if (focus != textEditor)
            {
                TraverseDirection = FocusNavigationDirection.Next;
                focus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                if (elementWithFocus == Keyboard.FocusedElement as FrameworkElement)
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                }
                TraverseDirection = null;
            }
        }

        public void TraversePrevious(FrameworkElement focus)
        {
            FrameworkElement elementWithFocus = Keyboard.FocusedElement as FrameworkElement;

            focus = focus ?? GetFocusedResultItem();
            if (focus == null)
            {
                textEditor.Focus();
                return;
            }

            TraverseDirection = FocusNavigationDirection.Previous;
            focus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            if (elementWithFocus == Keyboard.FocusedElement as FrameworkElement)
            {
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            }
            if (Keyboard.FocusedElement == textEditor.TextArea)
            {
                focus.Focus();
            }
            TraverseDirection = null;
        }

        private void PromptBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            textEditor.Padding = new Thickness(promptBlock.ActualWidth, 0, 0, 0);
        }

        private void TextArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (textEditor.IsFocused)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                if (textEditor.Document.GetLocation(textEditor.CaretOffset).Line == 1)
                {
                    e.Handled = true;
                    TraversePrevious(textEditor);
                }
            }
            else if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                if (textEditor.Document.GetLocation(textEditor.CaretOffset).Line == textEditor.LineCount)
                {
                    e.Handled = true;
                    TraverseNext(textEditor);
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
            textBox.BorderThickness = new Thickness(0);
            textBox.BorderBrush = Brushes.Transparent;
            textBox.IsInactiveSelectionHighlightEnabled = false;
            if (error)
            {
                textBox.Foreground = Brushes.Red;
            }
            else
            {
                textBox.Foreground = textEditor.Foreground;
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

            var codeControl = new CsTextEditor(fontFamily, fontSize, indentationSize, highlightingColors);
            codeControl.IsReadOnly = true;
            codeControl.Text = code;
            codeControl.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            codeControl.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            codeControl.Background = Brushes.Transparent;
            panel.Children.Add(codeControl);
            codeControl.PreviewKeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    if (codeControl.Document.GetLocation(codeControl.CaretOffset).Line == 1)
                    {
                        e.Handled = true;
                        TraversePrevious(codeControl);
                    }
                }
                else if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    if (codeControl.Document.GetLocation(codeControl.CaretOffset).Line == codeControl.LineCount)
                    {
                        e.Handled = true;
                        TraverseNext(codeControl);
                    }
                }
            };

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
                    LazyUIResult lazyUI = objectOutput as LazyUIResult;
                    UIElement elementOutput = objectOutput as UIElement ?? lazyUI?.UIElement;

                    if (elementOutput != null)
                    {
                        resultsPanel.Children.Insert(textEditorIndex, elementOutput);
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
