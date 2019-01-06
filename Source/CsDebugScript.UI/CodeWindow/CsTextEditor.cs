using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace CsDebugScript.UI.CodeWindow
{
    internal class CsTextEditor : TextEditor
    {
        protected IProjectContent projectContent;
        private CompletionWindow autoCompletePopup;
        private CompletionWindow fixedToolTipPopup = null;
        private OverloadInsightWindow functionCallPopup;
        private Brush tooltipTextColor;
        private Brush tooltipTextBackground;
        private Brush completionTextColor;
        private Brush completionTextBackground;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsTextEditor"/> class.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="indentationSize">Size of the indentation.</param>
        /// <param name="highlightingColors">The highlighting colors.</param>
        public CsTextEditor(string fontFamily, double fontSize, int indentationSize, params ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors)
        {
            // Adjust appearance
            FontFamily = new FontFamily(fontFamily);
            FontSize = fontSize;
            Options.IndentationSize = indentationSize;
            Options.HighlightCurrentLine = true;
            SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            foreach (var color in highlightingColors)
            {
                foreach (var color2 in SyntaxHighlighting.NamedHighlightingColors)
                {
                    if (color.Name == color2.Name)
                    {
                        color2.Background = color.Background;
                        color2.FontStyle = color.FontStyle;
                        color2.FontWeight = color.FontWeight;
                        color2.Foreground = color.Foreground;
                        color2.Underline = color.Underline;
                    }
                }
            }

            var regularText = highlightingColors.FirstOrDefault(c => c.Name == "#RegularText#");
            if (regularText != null)
            {
                Background = regularText.Background.GetBrush(null);
                Foreground = regularText.Foreground.GetBrush(null);
                FontStyle = regularText.FontStyle.Value;
                FontWeight = regularText.FontWeight.Value;
            }

            var currentLine = highlightingColors.FirstOrDefault(c => c.Name == "#CurrentLine#");
            if (currentLine != null)
            {
                TextArea.TextView.SetValue(
                    ICSharpCode.AvalonEdit.Rendering.TextView.CurrentLineBackgroundProperty,
                    currentLine.Background.GetBrush(null));
                TextArea.TextView.SetValue(
                    ICSharpCode.AvalonEdit.Rendering.TextView.CurrentLineBorderProperty,
                    new Pen(currentLine.Foreground.GetBrush(null), 1));
            }

            var tooltipText = highlightingColors.FirstOrDefault(c => c.Name == "#TooltipText#");
            if (tooltipText != null)
            {
                tooltipTextColor = tooltipText.Foreground.GetBrush(null);
                tooltipTextBackground = tooltipText.Background.GetBrush(null);
            }
            else
            {
                tooltipTextColor = Foreground;
                tooltipTextBackground = Background;
            }

            var completionText = highlightingColors.FirstOrDefault(c => c.Name == "#CompletionText#");
            if (completionText != null)
            {
                completionTextColor = completionText.Foreground.GetBrush(null);
                completionTextBackground = completionText.Background.GetBrush(null);
            }
            else
            {
                completionTextColor = Foreground;
                completionTextBackground = Background;
            }

            var indentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(Options);
            indentationStrategy.IndentationString = new string(' ', indentationSize);
            TextArea.IndentationStrategy = indentationStrategy;

            // Add event handlers
            TextArea.TextEntering += TextArea_TextEntering;
            TextArea.TextEntered += TextArea_TextEntered;

            // Remove binding for Enter (we want to do something else :))
            InputBinding newLineBinding = null;

            foreach (var binding in TextArea.InputBindings)
            {
                var keyBinding = binding as KeyBinding;

                if (keyBinding != null)
                {
                    if (keyBinding.Key == Key.Enter)
                    {
                        newLineBinding = keyBinding;
                        break;
                    }
                }
            }

            TextArea.InputBindings.Remove(newLineBinding);

            // Add special commands
            AddKeyGesture(ModifierKeys.Control, Key.Space, TextArea_ControlSpace);
            AddKeyGesture(ModifierKeys.None, Key.Enter, TextArea_ExecuteCSharpScript);
            AddKeyGesture(ModifierKeys.Control, Key.Enter, TextArea_ExecuteWinDbgCommand);
            AddKeyGesture(ModifierKeys.Shift, Key.Enter, TextArea_RegularNewLine);

            // Initialize project content
            projectContent = new ICSharpCode.NRefactory.CSharp.CSharpProjectContent();

            // Initialize images
            CompletionData testData = new CompletionData(CompletionDataType.Unknown, "");

            // Start timer to find tooltips
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += (a, b) =>
            {
                if (autoCompletePopup != null && autoCompletePopup != fixedToolTipPopup)
                {
                    var tooltip = System.Windows.PresentationSource.CurrentSources.OfType<System.Windows.Interop.HwndSource>()
                        .Select(h => h.RootVisual)
                        .Select(v => System.Windows.LogicalTreeHelper.GetParent(v))
                        .OfType<System.Windows.Controls.Primitives.Popup>()
                        .Select(p => p.Child)
                        .OfType<System.Windows.Controls.ToolTip>()
                        .Where(t => t.PlacementTarget == autoCompletePopup)
                        .FirstOrDefault();

                    if (tooltip != null)
                    {
                        fixedToolTipPopup = autoCompletePopup;
                        if (tooltipTextBackground != null)
                        {
                            tooltip.Background = tooltipTextBackground;
                            tooltip.BorderThickness = new System.Windows.Thickness(1);
                        }
                    }
                }
            };
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Start();
        }

        protected void Initialize()
        {
            // TODO: Unify getting auto complete data
            IEnumerable<CompletionData> completionData = null;
            int startPosition, wordLength = 0;
            string word = string.Empty;
            string filename = "filename.csx";
            int offset = ScriptStart.Length;
            string newSourceCode = CorrectSource("");
            var document = new ICSharpCode.NRefactory.Editor.ReadOnlyDocument(new ICSharpCode.NRefactory.Editor.StringTextSource(newSourceCode), filename);
            var syntaxTree = new ICSharpCode.NRefactory.CSharp.CSharpParser().Parse(document, document.FileName);
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();
            var projectContent = this.projectContent.AddOrUpdateFiles(unresolvedFile);
            var compilation = projectContent.CreateCompilation();
            var location = document.GetLocation(offset);
            var resolver = unresolvedFile.GetResolver(compilation, location);
            var typeResolveContextAtCaret = unresolvedFile.GetTypeResolveContext(compilation, location);
            var completionContextProvider = new ICSharpCode.NRefactory.CSharp.Completion.DefaultCompletionContextProvider(document, unresolvedFile);
            var completionDataFactory = new CompletionDataFactory(tooltipTextColor);
            var cce = new ICSharpCode.NRefactory.CSharp.Completion.CSharpCompletionEngine(document, completionContextProvider, completionDataFactory, projectContent, typeResolveContextAtCaret);
            cce.EolMarker = Environment.NewLine;
            cce.FormattingPolicy = ICSharpCode.NRefactory.CSharp.FormattingOptionsFactory.CreateSharpDevelop();
            var completionChar = document.GetCharAt(offset - 1);

            if (!cce.TryGetCompletionWord(offset, out startPosition, out wordLength))
            {
                startPosition = offset;
                wordLength = 0;
            }

            completionData = cce.GetCompletionData(startPosition, true).Cast<CompletionData>();
            if (wordLength > 0)
            {
                word = document.GetText(offset - wordLength, wordLength);
            }

            var parameterCompletionDataFactory = new ParameterCompletionDataFactory(tooltipTextColor);
            var pce = new ICSharpCode.NRefactory.CSharp.Completion.CSharpParameterCompletionEngine(
                document,
                completionContextProvider,
                parameterCompletionDataFactory,
                projectContent,
                typeResolveContextAtCaret
            );
            var parameterDataProvider = pce.GetParameterDataProvider(offset, completionChar);
        }

        protected virtual void OnExecuteCSharpScript()
        {
        }

        protected virtual void OnExecuteWinDbgCommand()
        {
        }

        /// <summary>
        /// Adds the key gesture to the command bindings list.
        /// </summary>
        /// <param name="modifierKeys">The modifier keys.</param>
        /// <param name="key">The key.</param>
        /// <param name="handler">The handler.</param>
        private void AddKeyGesture(ModifierKeys modifierKeys, Key key, ExecutedRoutedEventHandler handler)
        {
            var routedCommand = new RoutedCommand();
            routedCommand.InputGestures.Add(new KeyGesture(key, modifierKeys));
            CommandBindings.Add(new CommandBinding(routedCommand, handler));
        }

        internal void TextArea_ExecuteCSharpScript(object sender, ExecutedRoutedEventArgs e)
        {
            if (InteractiveExecution.IsCompleteSubmission(Document.Text))
            {
                OnExecuteCSharpScript();
            }
            else
            {
                TextArea.PerformTextInput(Environment.NewLine);
            }
        }

        private void TextArea_ExecuteWinDbgCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OnExecuteWinDbgCommand();
        }

        private void TextArea_RegularNewLine(object sender, ExecutedRoutedEventArgs e)
        {
            TextArea.PerformTextInput(Environment.NewLine);
        }

        /// <summary>
        /// Handles the ControlSpace event of the TextArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void TextArea_ControlSpace(object sender, ExecutedRoutedEventArgs e)
        {
            ShowAutoComplete(autoOpening: false, text: string.Empty);
        }

        /// <summary>
        /// Handles the TextEntered event of the TextArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs"/> instance containing the event data.</param>
        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            ShowAutoComplete(autoOpening: true, text: e.Text);
        }

        /// <summary>
        /// Handles the TextEntering event of the TextArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs"/> instance containing the event data.</param>
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            // See if we want to do the inserting of currently selected item in auto complete popup window
            if (e.Text.Length > 0 && autoCompletePopup != null && !char.IsLetterOrDigit(e.Text.Last()))
            {
                autoCompletePopup.CompletionList.RequestInsertion(e);
            }
        }

        private void ShowAutoComplete(bool autoOpening, string text)
        {
            if (autoCompletePopup == null)
            {
                IEnumerable<CompletionData> completionData = null;
                int startPosition, wordLength = 0;
                string word = string.Empty;

                try
                {
                    string filename = "filename.csx";
                    int offset = CaretOffset + ScriptStart.Length;
                    string newSourceCode = CorrectSource(Text);
                    var document = new ICSharpCode.NRefactory.Editor.ReadOnlyDocument(new ICSharpCode.NRefactory.Editor.StringTextSource(newSourceCode), filename);
                    var syntaxTree = new ICSharpCode.NRefactory.CSharp.CSharpParser().Parse(document, document.FileName);
                    syntaxTree.Freeze();
                    var unresolvedFile = syntaxTree.ToTypeSystem();
                    var projectContent = this.projectContent.AddOrUpdateFiles(unresolvedFile);
                    var compilation = projectContent.CreateCompilation();
                    var location = document.GetLocation(offset);
                    var resolver = unresolvedFile.GetResolver(compilation, location);
                    var typeResolveContextAtCaret = unresolvedFile.GetTypeResolveContext(compilation, location);
                    var completionContextProvider = new ICSharpCode.NRefactory.CSharp.Completion.DefaultCompletionContextProvider(document, unresolvedFile);
                    var completionDataFactory = new CompletionDataFactory(tooltipTextColor);
                    var cce = new ICSharpCode.NRefactory.CSharp.Completion.CSharpCompletionEngine(document, completionContextProvider, completionDataFactory, projectContent, typeResolveContextAtCaret);
                    cce.EolMarker = Environment.NewLine;
                    cce.FormattingPolicy = ICSharpCode.NRefactory.CSharp.FormattingOptionsFactory.CreateSharpDevelop();
                    var completionChar = document.GetCharAt(offset - 1);

                    if (!autoOpening)
                    {
                        if (!cce.TryGetCompletionWord(offset, out startPosition, out wordLength))
                        {
                            startPosition = offset;
                            wordLength = 0;
                        }

                        completionData = cce.GetCompletionData(startPosition, true).Cast<CompletionData>();
                    }
                    else
                    {
                        startPosition = offset;
                        if (char.IsLetterOrDigit(completionChar) || completionChar == '_')
                        {
                            if (!(startPosition > 1 && char.IsLetterOrDigit(document.GetCharAt(startPosition - 2))))
                            {
                                completionData = cce.GetCompletionData(startPosition, false).Cast<CompletionData>();
                                startPosition--;
                                wordLength = 1;
                            }
                        }
                        else
                        {
                            completionData = cce.GetCompletionData(startPosition, false).Cast<CompletionData>();
                            wordLength = 0;
                        }
                    }

                    if (wordLength > 0)
                    {
                        word = document.GetText(offset - wordLength, wordLength);
                    }

                    if (functionCallPopup == null)
                    {
                        var parameterCompletionDataFactory = new ParameterCompletionDataFactory(tooltipTextColor);
                        var pce = new ICSharpCode.NRefactory.CSharp.Completion.CSharpParameterCompletionEngine(
                            document,
                            completionContextProvider,
                            parameterCompletionDataFactory,
                            projectContent,
                            typeResolveContextAtCaret
                        );
                        var parameterDataProvider = pce.GetParameterDataProvider(offset, completionChar);

                        if (functionCallPopup == null && parameterDataProvider != null)
                        {
                            var ppd = parameterDataProvider as ParameterDataProvider;

                            if (ppd.Methods.Length > 0)
                            {
                                functionCallPopup = new OverloadInsightWindow(TextArea);
                                functionCallPopup.Provider = ppd;
                                if (tooltipTextBackground != null)
                                {
                                    functionCallPopup.Background = tooltipTextBackground;
                                }
                                if (tooltipTextColor != null)
                                {
                                    functionCallPopup.Foreground = tooltipTextColor;
                                }
                                functionCallPopup.Show();
                                functionCallPopup.Closed += (o, args) => functionCallPopup = null;
                            }

                            return;
                        }
                    }
                }
                catch
                {
                }

                if (completionData != null && completionData.Any())
                {
                    autoCompletePopup = new CompletionWindow(TextArea);
                    System.Windows.Shell.WindowChrome.SetWindowChrome(
                        autoCompletePopup,
                        new System.Windows.Shell.WindowChrome()
                        {
                            CaptionHeight = 0,
                        });
                    if (completionTextBackground != null)
                    {
                        autoCompletePopup.CompletionList.ListBox.Background = completionTextBackground;
                    }
                    if (completionTextColor != null)
                    {
                        autoCompletePopup.CompletionList.ListBox.Foreground = completionTextColor;
                    }
                    autoCompletePopup.ResizeMode = System.Windows.ResizeMode.NoResize;
                    autoCompletePopup.Width = 0;
                    autoCompletePopup.MinWidth = 300;
                    autoCompletePopup.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
                    autoCompletePopup.CloseWhenCaretAtBeginning = true;
                    autoCompletePopup.StartOffset -= wordLength;
                    var data = autoCompletePopup.CompletionList.CompletionData;

                    if (completionData != null)
                    {
                        foreach (var cd in completionData.GroupBy(c => c.Content).Select(g => g.OrderBy(c => c.CompletionText).First()).OrderBy(c => c.Content))
                        {
                            data.Add(cd);
                        }

                        if (wordLength > 0)
                        {
                            autoCompletePopup.CompletionList.SelectItem(word);
                        }
                    }
                    autoCompletePopup.Show();
                    autoCompletePopup.Closed += (o, args) => autoCompletePopup = null;
                }
            }

            if (!string.IsNullOrEmpty(text) && functionCallPopup != null)
            {
                var provider = functionCallPopup.Provider as ParameterDataProvider;
                if (provider != null)
                {
                    string filename = "filename.csx";
                    int offset = CaretOffset + ScriptStart.Length;
                    string newSourceCode = CorrectSource(Text);
                    var document = new ICSharpCode.NRefactory.Editor.ReadOnlyDocument(new ICSharpCode.NRefactory.Editor.StringTextSource(newSourceCode), filename);
                    var syntaxTree = new ICSharpCode.NRefactory.CSharp.CSharpParser().Parse(document, document.FileName);
                    syntaxTree.Freeze();
                    var unresolvedFile = syntaxTree.ToTypeSystem();
                    var projectContent = this.projectContent.AddOrUpdateFiles(unresolvedFile);
                    var compilation = projectContent.CreateCompilation();
                    var location = document.GetLocation(offset);
                    var typeResolveContextAtCaret = unresolvedFile.GetTypeResolveContext(compilation, location);
                    var completionContextProvider = new ICSharpCode.NRefactory.CSharp.Completion.DefaultCompletionContextProvider(document, unresolvedFile);
                    var parameterCompletionDataFactory = new ParameterCompletionDataFactory(tooltipTextColor);
                    var completionChar = document.GetCharAt(offset - 1);
                    var pce = new ICSharpCode.NRefactory.CSharp.Completion.CSharpParameterCompletionEngine(
                        document,
                        completionContextProvider,
                        parameterCompletionDataFactory,
                        projectContent,
                        typeResolveContextAtCaret
                    );
                    int parameterIndex = pce.GetCurrentParameterIndex(provider.StartOffset, offset);
                    if (parameterIndex < 0)
                    {
                        functionCallPopup.Close();
                        functionCallPopup = null;
                    }
                    else
                    {
                        provider.CurrentParameter = parameterIndex - 1;
                    }
                }
            }
        }

        public string ScriptStart { get; protected set; }

        public string ScriptEnd { get; protected set; }

        private string CorrectSource(string code)
        {
            return ScriptStart + code + ScriptEnd;
        }
    }
}
