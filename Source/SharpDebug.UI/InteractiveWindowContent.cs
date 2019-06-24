using SharpDebug.UI.CodeWindow;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpDebug.UI
{
    internal class InteractiveWindowContent : UserControl
    {
        private StackPanel resultsPanel;
        private TextBlock promptBlock;
        private ScrollViewer scrollViewer;
        private ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors;
        private string fontFamily;
        private double fontSize;
        private int indentationSize;
        private List<string> previousCommands = new List<string>();
        private int previousCommandsIndex = -1;
        private bool clearAfterExecution = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindowContent" /> class.
        /// </summary>
        /// <param name="interactiveExecutionInitialization">Interactive execution initialization.</param>
        /// <param name="highlightingColors">The highlighting colors.</param>
        public InteractiveWindowContent(InteractiveExecutionInitialization interactiveExecutionInitialization, params ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors)
            : this(interactiveExecutionInitialization, "Consolas", 14, 4, highlightingColors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveWindowContent"/> class.
        /// </summary>
        /// <param name="interactiveExecutionInitialization">Interactive execution initialization.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="indentationSize">Size of the indentation.</param>
        /// <param name="highlightingColors">The highlighting colors.</param>
        public InteractiveWindowContent(InteractiveExecutionInitialization interactiveExecutionInitialization, string fontFamily, double fontSize, int indentationSize, params ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors)
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
            interactiveExecutionInitialization.InteractiveExecutionBehavior.ClearDone += () => clearAfterExecution = true;

            // Add prompt for text editor
            var panel = new Grid();
            resultsPanel.Children.Add(panel);

            promptBlock = new TextBlock();
            promptBlock.HorizontalAlignment = HorizontalAlignment.Left;
            promptBlock.VerticalAlignment = VerticalAlignment.Top;
            promptBlock.FontFamily = new FontFamily(fontFamily);
            promptBlock.FontSize = fontSize;
            promptBlock.Text = interactiveExecutionInitialization.InteractiveExecutionBehavior.GetReplExecutingPrompt();
            promptBlock.SizeChanged += PromptBlock_SizeChanged;
            panel.Children.Add(promptBlock);

            // Add text editor
            TextEditor = new InteractiveCodeEditor(new InteractiveResultVisualizer(this), interactiveExecutionInitialization.InteractiveExecutionCache, fontFamily, fontSize, indentationSize, highlightingColors);
            TextEditor.HorizontalAlignment = HorizontalAlignment.Stretch;
            TextEditor.Background = Brushes.Transparent;
            TextEditor.CommandExecuted += TextEditor_CommandExecuted;
            TextEditor.CommandFailed += TextEditor_CommandFailed;
            TextEditor.Executing += TextEditor_Executing;
            TextEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            TextEditor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            TextEditor.TextArea.PreviewKeyDown += TextEditor_PreviewKeyDown;
            TextEditor.TextArea.SizeChanged += TextArea_SizeChanged;
            panel.Children.Add(TextEditor);

            Content = scrollViewer;

            // Enable drag and drop
            AllowDrop = true;
            DragEnter += (s, e) => e.Effects = GetFilename(e) != null ? DragDropEffects.Link : DragDropEffects.None;
            Drop += (s, e) => FileDropped(GetFilename(e));
        }

        internal InteractiveCodeEditor TextEditor { get; private set; }

        internal string PreviousCommand
        {
            get
            {
                if (previousCommandsIndex >= 0 && previousCommandsIndex < previousCommands.Count)
                {
                    return previousCommands[previousCommandsIndex];
                }

                return null;
            }
        }

        public FocusNavigationDirection? TraverseDirection { get; private set; }

        public FrameworkElement GetFocusedResultItem()
        {
            FrameworkElement elementWithFocus = Keyboard.FocusedElement as FrameworkElement;

            while (elementWithFocus != null && elementWithFocus != TextEditor.TextArea && elementWithFocus.Parent != resultsPanel)
            {
                elementWithFocus = elementWithFocus.Parent as FrameworkElement;
            }

            if (elementWithFocus == TextEditor.TextArea)
            {
                return TextEditor;
            }

            return elementWithFocus;
        }

        public void TraverseNext(FrameworkElement focus)
        {
            FrameworkElement elementWithFocus = Keyboard.FocusedElement as FrameworkElement;

            focus = focus ?? GetFocusedResultItem();
            if (focus == null)
            {
                TextEditor.Focus();
                return;
            }

            if (focus != TextEditor)
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
                TextEditor.Focus();
                return;
            }

            TraverseDirection = FocusNavigationDirection.Previous;
            focus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            if (elementWithFocus == Keyboard.FocusedElement as FrameworkElement)
            {
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            }
            if (Keyboard.FocusedElement == TextEditor.TextArea)
            {
                focus.Focus();
            }
            TraverseDirection = null;
        }

        private bool executingDroppedFile = false;
        private string savedTextBeforeExecutingDroppedFile;

        private void FileDropped(string filename)
        {
            savedTextBeforeExecutingDroppedFile = TextEditor.Text;
            executingDroppedFile = true;
            TextEditor.Text = $"#load \"{filename}\"";
            TextEditor.ExecuteCSharpScript();
        }

        private static string GetFilename(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] files = args.Data.GetData(DataFormats.FileDrop, true) as string[];

                if (files != null && files.Length == 1 && File.Exists(files[0]))
                    return files[0];
            }
            return null;
        }

        private void PromptBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextEditor.Padding = new Thickness(promptBlock.ActualWidth, 0, 0, 0);
        }

        private void TextArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TextEditor.IsFocused)
            {
                scrollViewer.ScrollToEnd();
            }
        }

        private void SetCommandFromBuffer(bool forward)
        {
            if (forward)
            {
                previousCommandsIndex++;
            }
            else
            {
                previousCommandsIndex--;
            }

            if (previousCommandsIndex >= previousCommands.Count)
            {
                previousCommandsIndex = previousCommands.Count - 1;
            }
            else if (previousCommandsIndex < 0)
            {
                previousCommandsIndex = 0;
            }

            if (TextEditor.Text != PreviousCommand)
            {
                TextEditor.Text = PreviousCommand;
                TextEditor.SelectionStart = TextEditor.Text.Length;
            }
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                if (TextEditor.Document.GetLocation(TextEditor.CaretOffset).Line == 1)
                {
                    e.Handled = true;
                    if (TextEditor.Text == PreviousCommand || string.IsNullOrEmpty(TextEditor.Text))
                    {
                        SetCommandFromBuffer(false);
                    }
                    else
                    {
                        TraversePrevious(TextEditor);
                    }
                }
            }
            else if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                if (TextEditor.Document.GetLocation(TextEditor.CaretOffset).Line == TextEditor.LineCount)
                {
                    e.Handled = true;
                    if (TextEditor.Text == PreviousCommand || string.IsNullOrEmpty(TextEditor.Text))
                    {
                        SetCommandFromBuffer(true);
                    }
                    else
                    {
                        TraverseNext(TextEditor);
                    }
                }
            }
            else if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                SetCommandFromBuffer(false);
            }
            else if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                SetCommandFromBuffer(true);
            }
            else if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                TraversePrevious(TextEditor);
            }
            else if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                TraverseNext(TextEditor);
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
            textBox.FontFamily = new FontFamily(fontFamily);
            textBox.FontSize = fontSize;
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
                textBox.Foreground = TextEditor.Foreground;
            }

            return textBox;
        }

        private UIElement CreateDbgCode(string text)
        {
            var textBlock = new TextBlock();
            textBlock.FontFamily = new FontFamily(fontFamily);
            textBlock.FontSize = fontSize;
            textBlock.Text = "#dbg> " + text;
            textBlock.Background = Brushes.Transparent;
            return textBlock;
        }

        private UIElement CreateCSharpCode(string code)
        {
            var panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;

            var textBlock = new TextBlock();
            textBlock.FontFamily = new FontFamily(fontFamily);
            textBlock.FontSize = fontSize;
            textBlock.Text = TextEditor.InteractiveExecution.Behavior.GetReplPrompt();
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

            if (previousCommands.Count == 0 || previousCommands.Last() != code)
            {
                previousCommands.Add(code);
                previousCommandsIndex = previousCommands.Count;
            }

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
            if (clearAfterExecution)
            {
                resultsPanel.Children.RemoveRange(0, resultsPanel.Children.Count - 1);
                clearAfterExecution = false;
            }
            else
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

                resultsPanel.Children.Insert(textEditorIndex, csharpCode ? CreateCSharpCode(TextEditor.Text) : CreateDbgCode(TextEditor.Text));
                AddSpacing(resultsPanel.Children[textEditorIndex]);
                if (resultsPanel.Children.Count - initialLength > 1)
                {
                    AddSpacing(resultsPanel.Children[textEditorIndex + resultsPanel.Children.Count - initialLength - 1]);
                }
            }

            if (executingDroppedFile)
            {
                executingDroppedFile = false;
                Dispatcher.InvokeAsync(() =>
                {
                    TextEditor.Text = savedTextBeforeExecutingDroppedFile;
                    TextEditor.SelectionStart = savedTextBeforeExecutingDroppedFile.Length;
                    TextEditor.SelectionLength = 0;
                });
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

            resultsPanel.Children.Insert(textEditorIndex, csharpCode ? CreateCSharpCode(TextEditor.Text) : CreateDbgCode(TextEditor.Text));
            AddSpacing(resultsPanel.Children[textEditorIndex]);
            AddSpacing(resultsPanel.Children[textEditorIndex + resultsPanel.Children.Count - initialLength - 1]);

            if (executingDroppedFile)
            {
                executingDroppedFile = false;
                TextEditor.Text = savedTextBeforeExecutingDroppedFile;
            }
        }

        private void TextEditor_Executing(bool started)
        {
            if (!started)
            {
                TextEditor.TextArea.Focus();
                promptBlock.Text = TextEditor.InteractiveExecution.Behavior.GetReplPrompt();
            }
            else
            {
                promptBlock.Text = TextEditor.InteractiveExecution.Behavior.GetReplExecutingPrompt();
            }
        }
    }
}
