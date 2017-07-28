using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Windows.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using DbgEngManaged;
using CsDebugScript.Engine.Utility;
using System.IO;

namespace DbgEngTest
{
    [CodedUITest]
    public class CodedUITests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;
        private const string DefaultPrompt = "C#> ";
        private const string DbgPrompt = "dbg> ";
        private const string ExecutingPrompt = "...> ";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private UIMap map;

        [TestInitialize()]
        public void TestInitialize()
        {
            InitializeDump(DefaultDumpFile, DefaultSymbolPath);
            OpenInteractiveWindow();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //CloseInteractiveWindow();
        }

        [TestMethod, Timeout(60000)]
        [TestCategory("UI")]
        public void ExecuteDebuggerCommand()
        {
            SendInput("{#}dbg{Space}k{Enter}");
            WaitForExecutionEnd(DefaultTimeout);
            VerifyOutput("#dbg k");
        }

        [TestMethod, Timeout(60000)]
        [TestCategory("UI")]
        public void MultiLineCode()
        {
            SendInput("for {(}int i = 0; i < 1; i{+}{+}{)}{Enter}Console.WriteLine{(}i{)};{Enter}");

            WaitForExecutionEnd(DefaultTimeout);
            VerifyOutput(@"for (int i = 0; i < 1; i++)
    Console.WriteLine(i);");
        }

        [TestMethod, Timeout(60000)]
        [TestCategory("UI")]
        public void SimpleEndToEnd()
        {
            SendInput("var{Space}a{Space}={Space}new{Space}{[}{]}{Space}{RShiftKey}{{}");
            SendInput("{Space}", ModifierKeys.Shift);
            SendInput("1,{Space}2,{Space}3,{Space}4,{Space}5,{Space}6,{Space}7,{Space}{RShiftKey}{}};{Enter}");
            WaitForExecutionEnd(DefaultTimeout);
            VerifyOutput("var a = new [] { 1, 2, 3, 4, 5, 6, 7, };");

            SendInput("writeln{RShiftKey}{(}a.{RShiftKey}Len{Enter}{RShiftKey}{)};{Enter}");
            WaitForExecutionEnd(DefaultTimeout);
            VerifyOutput("writeln(a.Length);");

            SendInput("a{Enter}{Enter}");
            WaitForExecutionEnd(DefaultTimeout);
            // TODO: Verify that output is correct
        }

        public UIMap UIMap
        {
            get
            {
                if (map == null)
                {
                    map = new UIMap();
                }

                return map;
            }
        }

        private UITestControl[] GetVisualControls()
        {
            var children = UIMap.InteractiveWindow.InteractiveWindowContent.ResultContainer.GetChildren();

            return children.Where(c => c?.ClassName != "Uia.ScrollBar").ToArray();
        }

        private void OpenInteractiveWindow()
        {
            InteractiveWindow window = UIMap.InteractiveWindow;

            if (!window.TryFind())
            {
                Executor.ShowInteractiveWindow(false);

                window.WaitForControlExist();
                WaitForExecutionEnd(DefaultTimeout);
                UIMap.AssertInteractiveWindowReady();
            }
        }

        private void CloseInteractiveWindow(int millisecondsTimeout = 5000)
        {
            SendInput("q{Space}{Enter}");
            UIMap.InteractiveWindow.WaitForControlNotExist(millisecondsTimeout);
        }

        private void WaitForExecutionEnd(TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool awaited = false;

            while (stopwatch.Elapsed < timeout)
            {
                ExecutionEntry[] executions = GetExecutions().Take(2).ToArray();

                if (executions.Length > 0)
                {
                    ExecutionEntry currentEntry = executions[0];

                    if (currentEntry.Code == "")
                    {
                        awaited = true;
                        break;
                    }

                    if (currentEntry.Prompt == DefaultPrompt && executions.Length > 1)
                    {
                        ExecutionEntry lastExecution = executions[1];

                        if (lastExecution.Code == currentEntry.Code)
                        {
                            awaited = true;
                            break;
                        }
                    }
                }

                System.Threading.Thread.Sleep(100);
            }

            Assert.IsTrue(awaited, "Command didn't execute");
        }

        private void VerifyOutput(string code)
        {
            ExecutionEntry[] executions = GetExecutions().Take(2).ToArray();

            Assert.AreEqual(executions.Length, 2);

            ExecutionEntry currentEntry = executions[0];
            ExecutionEntry lastExecution = executions[1];

            Assert.AreNotEqual(currentEntry.Code, lastExecution.Code, $"Execution error happened. Code:\n{currentEntry.Code}\n\nError: {lastExecution.ResultText}");
            Assert.AreEqual(currentEntry.Code, "", $"Current entry should be empty, but it isn't. Code:\n{currentEntry.Code}");

            // Capture code output
            string expectedOutput;
            string expectedError;

            using (StringWriter outputWriter = new StringWriter())
            using (StringWriter errorWriter = new StringWriter())
            {
                TextWriter originalOutput = Console.Out;
                TextWriter originalError = Console.Error;

                try
                {
                    Console.SetOut(outputWriter);
                    Console.SetError(errorWriter);

                    DebugOutput captureFlags = DebugOutput.Normal | DebugOutput.Error | DebugOutput.Warning | DebugOutput.Verbose
                        | DebugOutput.Prompt | DebugOutput.PromptRegisters | DebugOutput.ExtensionWarning | DebugOutput.Debuggee
                        | DebugOutput.DebuggeePrompt | DebugOutput.Symbols | DebugOutput.Status;
                    var callbacks = DebuggerOutputToTextWriter.Create(outputWriter, captureFlags);
                    using (OutputCallbacksSwitcher switcher = OutputCallbacksSwitcher.Create(callbacks))
                    {
                        new InteractiveExecution().Interpret(code);
                    }

                    expectedOutput = outputWriter.GetStringBuilder().ToString();
                    expectedError = errorWriter.GetStringBuilder().ToString();
                }
                finally
                {
                    Console.SetOut(originalOutput);
                    Console.SetError(originalError);
                }
            }

            // Verify that output is the same
            if (currentEntry.Code == lastExecution.Code)
            {
                Assert.AreEqual(FixOutput(lastExecution.ResultText), FixOutput(expectedError));
            }
            else
            {
                Assert.AreEqual(FixOutput(lastExecution.ResultText), FixOutput(expectedOutput));
            }
        }

        private static string FixOutput(string output)
        {
            return output.Trim().Replace("\r\n", "\n").Replace("\r", "");
        }

        private void SendInput(string input, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            Microsoft.VisualStudio.TestTools.UITesting.Keyboard.SendKeys(UIMap.InteractiveWindow, input, modifierKeys);
        }

        class ExecutionEntry
        {
            public string Code { get; set; }

            public string Prompt { get; set; }

            public UITestControl[] Result { get; set; }

            public string ResultText
            {
                get
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (UITestControl control in Result)
                    {
                        WpfEdit textControl = control as WpfEdit;

                        if (textControl != null)
                        {
                            sb.AppendLine(textControl.Text);
                        }
                    }

                    return sb.ToString();
                }
            }
        }

        private static int FindPromptIndex(UITestControl[] controls, int startingIndex)
        {
            int promptIndex = startingIndex;

            while (promptIndex >= 0)
            {
                if (controls[promptIndex].ClassName == "Uia.TextBlock")
                {
                    WpfText textBlock = controls[promptIndex] as WpfText;

                    if (textBlock != null && (textBlock.DisplayText == DefaultPrompt || textBlock.DisplayText == DbgPrompt || textBlock.DisplayText == ExecutingPrompt))
                    {
                        break;
                    }
                }

                promptIndex--;
            }

            return promptIndex;
        }

        private IEnumerable<ExecutionEntry> GetExecutions()
        {
            UITestControl[] controls = GetVisualControls();
            int promptIndex = FindPromptIndex(controls, controls.Length - 1);
            int nextPromptIndex = controls.Length;

            while (promptIndex >= 0)
            {
                WpfCustom codeInput = controls[promptIndex + 1] as WpfCustom;
                var automationElement = codeInput?.NativeElement as System.Windows.Automation.AutomationElement;
                var valuePattern = automationElement?.GetSupportedPatterns()?.FirstOrDefault(p => p.ProgrammaticName.Contains("Value"));
                var value = automationElement?.GetCurrentPattern(valuePattern) as System.Windows.Automation.ValuePattern;

                string code = value?.Current.Value;
                string prompt = (controls[promptIndex] as WpfText)?.DisplayText;

                UITestControl[] result = new UITestControl[nextPromptIndex - promptIndex - 2];
                for (int i = promptIndex + 2, k = 0; i < nextPromptIndex; i++, k++)
                {
                    result[k] = controls[i];
                }

                yield return new ExecutionEntry()
                {
                    Code = code,
                    Prompt = prompt,
                    Result = result,
                };

                nextPromptIndex = promptIndex;
                promptIndex = FindPromptIndex(controls, promptIndex - 1);
            }
        }
    }
}
