using CsDebugScript.UI.CodeWindow;
using CsDebugScript.Engine.Utility;
using DbgEng;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CsDebugScript.Engine.Debuggers;

namespace CsDebugScript.UI
{
    internal delegate void CommandExecutedHandler(bool csharpCode, string textOutput, IEnumerable<object> objectOutput);

    internal delegate void CommandFailedHandler(bool csharpCode, string textOutput, string errorOutput);

    internal delegate void ExecutingHandler(bool started);

    internal class InteractiveCodeEditor : CsTextEditor
    {
        private class ObjectWriter : IObjectWriter
        {
            public InteractiveCodeEditor InteractiveCodeEditor { get; set; }

            public object Output(object obj)
            {
                if (obj != null)
                {
                    InteractiveCodeEditor.AddResult(obj);
                }

                return null;
            }
        }

        private delegate void BackgroundExecuteDelegate(string documentText, out string textOutput, out string errorOutput, out IEnumerable<object> result);

        private InteractiveExecution interactiveExecution;
        private Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        private List<object> results = new List<object>();
        private System.Threading.ManualResetEvent initializedEvent = new System.Threading.ManualResetEvent(false);

        private void AddResult(object obj)
        {
            results.Add(obj);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveCodeEditor" /> class.
        /// </summary>
        /// <param name="objectWriter">Interactive result visualizer object writer.</param>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="indentationSize">Size of the indentation.</param>
        /// <param name="highlightingColors">The highlighting colors.</param>
        public InteractiveCodeEditor(InteractiveResultVisualizer objectWriter, string fontFamily, double fontSize, int indentationSize, params ICSharpCode.AvalonEdit.Highlighting.HighlightingColor[] highlightingColors)
            : base(fontFamily, fontSize, indentationSize, highlightingColors)
        {
            interactiveExecution = new InteractiveExecution();
            interactiveExecution.scriptBase._InternalObjectWriter_ = new ObjectWriter()
            {
                InteractiveCodeEditor = this,
            };
            interactiveExecution.scriptBase.ObjectWriter = objectWriter;

            // Run initialization of the window in background task
            IsEnabled = false;
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        IsEnabled = true;
                        Executing?.Invoke(false);
                    });
                    Initialize();
                    Dispatcher.InvokeAsync(() =>
                    {
                        InitializationFinished?.Invoke();
                    });
                }
                catch (ExitRequestedException)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        CloseRequested?.Invoke();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });
        }

        public event CommandExecutedHandler CommandExecuted;

        public event CommandFailedHandler CommandFailed;

        public event ExecutingHandler Executing;

        public event Action InitializationFinished;

        public event Action CloseRequested;

        protected new void Initialize()
        {
            UpdateScriptCode();
            base.Initialize();
            interactiveExecution.UnsafeInterpret("null");
            initializedEvent.Set();
        }

        protected override void OnExecuteCSharpScript()
        {
            BackgroundExecute((string _documentText, out string _textOutput, out string _errorOutput, out IEnumerable<object> _result) =>
            {
                BackgroundExecuteDelegate scriptExecution = (string documentText, out string textOutput, out string errorOutput, out IEnumerable<object> result) =>
                {
                    // Setting results
                    textOutput = "";
                    errorOutput = "";

                    // Execution code
                    var oldOut = Console.Out;
                    var oldError = Console.Error;

                    try
                    {
                        using (StringWriter writer = new StringWriter())
                        {
                            Console.SetOut(writer);
                            Console.SetError(writer);

                            DebugOutput captureFlags = DebugOutput.Normal | DebugOutput.Error | DebugOutput.Warning | DebugOutput.Verbose
                                | DebugOutput.Prompt | DebugOutput.PromptRegisters | DebugOutput.ExtensionWarning | DebugOutput.Debuggee
                                | DebugOutput.DebuggeePrompt | DebugOutput.Symbols | DebugOutput.Status;
                            var callbacks = DebuggerOutputToTextWriter.Create(Console.Out, captureFlags);

                            interactiveExecution.scriptBase._UiActionExecutor_ = (action) => Dispatcher.Invoke(action);
                            using (OutputCallbacksSwitcher switcher = OutputCallbacksSwitcher.Create(callbacks))
                            {
                                interactiveExecution.UnsafeInterpret(documentText);
                                writer.Flush();
                                textOutput = writer.GetStringBuilder().ToString();
                            }
                        }

                        UpdateScriptCode();
                    }
                    catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine("Compile errors:");
                        foreach (var error in ex.Diagnostics)
                        {
                            sb.AppendLine(error.ToString());
                        }

                        errorOutput = sb.ToString();
                    }
                    catch (ExitRequestedException)
                    {
                        throw;
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException is ExitRequestedException)
                        {
                            throw ex.InnerException;
                        }
                        errorOutput = ex.ToString();
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException is ExitRequestedException)
                        {
                            throw ex.InnerException;
                        }
                        errorOutput = ex.InnerException.ToString();
                    }
                    catch (Exception ex)
                    {
                        errorOutput = ex.ToString();
                    }
                    finally
                    {
                        Console.SetError(oldError);
                        Console.SetOut(oldOut);
                        result = results;
                        results = new List<object>();
                        interactiveExecution.scriptBase._UiActionExecutor_ = null;
                    }
                };

                // Check if we should execute script code in STA thread
                if (interactiveExecution.scriptBase.ForceStaExecution)
                {
                    string tempTextOutput = null;
                    string tempErrorOutput = null;
                    IEnumerable<object> tempResult = null;

                    InteractiveWindow.ExecuteInSTA(() =>
                    {
                        scriptExecution(_documentText, out tempTextOutput, out tempErrorOutput, out tempResult);
                    });
                    _textOutput = tempTextOutput;
                    _errorOutput = tempErrorOutput;
                    _result = tempResult;
                }
                else
                {
                    scriptExecution(_documentText, out _textOutput, out _errorOutput, out _result);
                }

            }, true);
        }

        protected override void OnExecuteWinDbgCommand()
        {
            BackgroundExecute((string documentText, out string textOutput, out string errorOutput, out IEnumerable<object> result) =>
            {
                // Setting results
                textOutput = "";
                errorOutput = "";

                try
                {
                    textOutput = DbgEngDll.ExecuteAndCapture(documentText);
                }
                catch (Exception ex)
                {
                    errorOutput = ex.ToString();
                }
                result = results;
                results = new List<object>();
            }, false);
        }

        private void BackgroundExecute(BackgroundExecuteDelegate action, bool csharpCode)
        {
            string documentText = Document.Text;

            IsEnabled = false;
            if (Executing != null)
            {
                Executing(true);
            }
            Task.Run(() =>
            {
                initializedEvent.WaitOne();

                try
                {
                    string textOutput, errorOutput;
                    IEnumerable<object> objectOutput;

                    action(documentText, out textOutput, out errorOutput, out objectOutput);
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            if (CommandFailed != null)
                            {
                                CommandFailed(csharpCode, textOutput, errorOutput);
                            }
                        }
                        else
                        {
                            if (CommandExecuted != null)
                            {
                                CommandExecuted(csharpCode, textOutput, objectOutput);
                            }
                            Document.Text = "";
                        }

                        IsEnabled = true;
                        if (Executing != null)
                        {
                            Executing(false);
                        }
                    });
                }
                catch (ExitRequestedException)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (CloseRequested != null)
                        {
                            CloseRequested();
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });
        }

        private void UpdateScriptCode()
        {
            string scriptStart, scriptEnd;
            string[] loadedReferences = interactiveExecution.GetScriptHelperCode(out scriptStart, out scriptEnd).ToArray();

            if (loadedReferences.Length != loadedAssemblies.Count)
            {
                var newAssemblies = new List<IUnresolvedAssembly>();

                foreach (var assemblyPath in loadedReferences)
                    if (!loadedAssemblies.ContainsKey(assemblyPath))
                    {
                        try
                        {
                            var loader = new CecilLoader();
                            loader.DocumentationProvider = GetXmlDocumentation(assemblyPath);
                            newAssemblies.Add(loader.LoadAssemblyFile(assemblyPath));
                        }
                        catch (Exception)
                        {
                        }
                    }

                if (newAssemblies.Count > 0)
                {
                    projectContent = projectContent.AddAssemblyReferences(newAssemblies);
                }
            }

            Regex lineRegex = new Regex("#line[^\n]*", RegexOptions.Compiled);

            ScriptStart = lineRegex.Replace(scriptStart, "");
            ScriptEnd = lineRegex.Replace(scriptEnd, "");
        }

        private static XmlDocumentationProvider GetXmlDocumentation(string dllPath)
        {
            if (!string.IsNullOrEmpty(dllPath))
            {
                var documentationFile = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".xml");

                if (File.Exists(documentationFile))
                {
                    return new XmlDocumentationProvider(documentationFile);
                }

                string[] paths = new string[]
                    {
                        @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\",
                        @"C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\",
                    };
                string[] versions = new string[]
                    {
                        @"v4.7",
                        @"v4.6.2",
                        @"v4.6.1",
                        @"v4.6",
                        @"v4.5.2",
                        @"v4.5.1",
                        @"v4.5",
                        @"v4.0",
                        @"v3.5",
                    };

                foreach (var path in paths)
                {
                    foreach (var version in versions)
                    {
                        documentationFile = Path.Combine(path, version, Path.GetFileNameWithoutExtension(dllPath) + ".xml");
                        if (File.Exists(documentationFile))
                        {
                            return new XmlDocumentationProvider(documentationFile);
                        }
                    }
                }
            }

            return null;
        }
    }
}
