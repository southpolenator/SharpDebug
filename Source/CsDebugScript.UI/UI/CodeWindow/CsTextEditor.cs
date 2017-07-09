using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace CsDebugScript.UI.CodeWindow
{
    internal class CsTextEditor : TextEditor
    {
        protected IProjectContent projectContent;
        private CompletionWindow autoCompletePopup;
        private OverloadInsightWindow functionCallPopup;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsTextEditor"/> class.
        /// </summary>
        public CsTextEditor()
        {
            // Adjust appearance
            FontFamily = new System.Windows.Media.FontFamily("Consolas");
            FontSize = 14;
            Options.IndentationSize = 4;
            Options.HighlightCurrentLine = true;
            SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");

            var indentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(Options);
            indentationStrategy.IndentationString = "    ";
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
            var completionDataFactory = new CompletionDataFactory();
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

            var parameterCompletionDataFactory = new ParameterCompletionDataFactory();
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

        private void TextArea_ExecuteCSharpScript(object sender, ExecutedRoutedEventArgs e)
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
                    var completionDataFactory = new CompletionDataFactory();
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
                        var parameterCompletionDataFactory = new ParameterCompletionDataFactory();
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
                    var parameterCompletionDataFactory = new ParameterCompletionDataFactory();
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
