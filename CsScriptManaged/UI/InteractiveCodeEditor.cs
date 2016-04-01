using CsScriptManaged.UI.CodeWindow;
using CsScriptManaged.Utility;
using DbgEngManaged;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CsScriptManaged.UI
{
    internal delegate void CommandExecutedHandler(string textOutput, object objectOutput);

    internal delegate void CommandFailedHandler(string textOutput, string errorOutput);

    internal delegate void ExecutingHandler(bool started);

    internal class InteractiveCodeEditor : CsTextEditor
    {
        private delegate void BackgroundExecuteDelegate(string documentText, out string textOutput, out string errorOutput, out object result);

        private InteractiveExecution interactiveExecution;
        private Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

        public InteractiveCodeEditor()
        {
            interactiveExecution = Context.InteractiveExecution;
            UpdateScriptCode();
        }

        public event CommandExecutedHandler CommandExecuted;

        public event CommandFailedHandler CommandFailed;

        public event ExecutingHandler Executing;

        public event Action CloseRequested;

        protected override void OnExecuteCSharpScript()
        {
            BackgroundExecute((string documentText, out string textOutput, out string errorOutput, out object result) =>
            {
                // Setting results
                textOutput = "";
                errorOutput = "";
                result = null;

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

                        using (OutputCallbacksSwitcher switcher = OutputCallbacksSwitcher.Create(callbacks))
                        {
                            interactiveExecution.UnsafeInterpret(documentText);
                            writer.Flush();
                            textOutput = writer.GetStringBuilder().ToString();
                        }
                    }

                    UpdateScriptCode();
                }
                catch (CompileException ex)
                {
                    CompileError[] errors = ex.Errors;

                    if (errors[0].FileName.EndsWith(InteractiveExecution.InteractiveScriptName) || errors.Count(e => !e.FileName.EndsWith(InteractiveExecution.InteractiveScriptName)) == 0)
                    {
                        CompileError e = errors[0];

                        errorOutput = e.FullMessage;
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine("Compile errors:");
                        foreach (var error in errors)
                        {
                            sb.AppendLine(error.FullMessage);
                        }

                        errorOutput = sb.ToString();
                    }
                }
                catch (ExitRequestedException)
                {
                    throw;
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is ExitRequestedException)
                        throw ex.InnerException;
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
                }
            });
        }

        protected override void OnExecuteWinDbgCommand()
        {
            BackgroundExecute((string documentText, out string textOutput, out string errorOutput, out object result) =>
            {
                // Setting results
                textOutput = "";
                errorOutput = "";
                result = null;

                try
                {
                    textOutput = CsScripts.Debugger.ExecuteAndCapture(documentText);
                }
                catch (Exception ex)
                {
                    errorOutput = ex.ToString();
                }
            });
        }

        private void BackgroundExecute(BackgroundExecuteDelegate action)
        {
            string documentText = Document.Text;

            IsEnabled = false;
            if (Executing != null)
                Executing(true);
            Task.Run(() =>
            {
                try
                {
                    string textOutput, errorOutput;
                    object objectOutput;

                    action(documentText, out textOutput, out errorOutput, out objectOutput);
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            if (CommandFailed != null)
                                CommandFailed(textOutput, errorOutput);
                        }
                        else
                        {
                            if (CommandExecuted != null)
                                CommandExecuted(textOutput, objectOutput);
                            Document.Text = "";
                        }

                        IsEnabled = true;
                        if (Executing != null)
                            Executing(false);
                    });
                }
                catch (ExitRequestedException)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (CloseRequested != null)
                            CloseRequested();
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

            ScriptStart = scriptStart;
            ScriptEnd = scriptEnd;
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
            }

            return null;
        }
    }
}
