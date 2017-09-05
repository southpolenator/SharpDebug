using CsDebugScript.Engine.Utility;
using DbgEng;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using TestStack.White.InputDevices;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Custom;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.WindowsAPI;
using Xunit;

namespace CsDebugScript.UITests
{
    public class InteractiveWindowWrapper
    {
        private const string DefaultPrompt = "C#> ";
        private const string DbgPrompt = "dbg> ";
        private const string ExecutingPrompt = "...> ";

        public InteractiveWindowWrapper(Window mainWindow)
        {
            MainWindow = mainWindow;
            Assert.NotNull(MainWindow);
            ContentPanel = MainWindow.Get<Panel>(SearchCriteria.ByClassName("ScrollViewer"));
            Assert.NotNull(ContentPanel);
        }

        public Window MainWindow { get; private set; }

        public Panel ContentPanel { get; private set; }

        public CustomUIItem CodeInput
        {
            get
            {
                UIItemCollection items = ContentPanel.Items;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (items[i] is CustomUIItem codeInput)
                    {
                        return codeInput;
                    }
                }

                return null;
            }
        }

        public UiExecutionEntry LastExecution
        {
            get
            {
                return GetExecutions().Take(2).Last();
            }
        }

        public void WaitForExecutionEnd()
        {
            WaitForExecutionEnd(TimeSpan.FromSeconds(5));
        }

        public void WaitForExecutionEnd(TimeSpan timeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool awaited = false;

            while (stopwatch.Elapsed < timeout)
            {
                UiExecutionEntry[] executions = GetExecutions().Take(2).ToArray();

                if (executions.Length > 0)
                {
                    UiExecutionEntry currentEntry = executions[0];

                    if (currentEntry.Code == "")
                    {
                        awaited = true;
                        break;
                    }

                    if (currentEntry.Prompt == DefaultPrompt && executions.Length > 1)
                    {
                        UiExecutionEntry lastExecution = executions[1];

                        if (lastExecution.Code == currentEntry.Code)
                        {
                            awaited = true;
                            break;
                        }
                    }
                }

                System.Threading.Thread.Sleep(100);
            }

            Assert.True(awaited, "Command didn't execute");
        }

        public void ExecuteCommand(string command)
        {
            SendInput($"{command}{{Return}}");
            WaitForExecutionEnd();
        }

        public void SendInput(string input)
        {
            int index = 0;

            CodeInput.Focus();
            while (index < input.Length)
            {
                int tokenStart = input.IndexOf('{', index);

                if (tokenStart < 0)
                {
                    break;
                }

                if (tokenStart > index)
                {
                    Keyboard.Instance.Enter(input.Substring(index, tokenStart - index));
                }

                int tokenEnd = input.IndexOf('}', tokenStart);
                Assert.True(tokenEnd >= 0);
                string token = input.Substring(tokenStart + 1, tokenEnd - tokenStart - 1);

                var values = Enum.GetValues(typeof(KeyboardInput.SpecialKeys));

                foreach (var value in values)
                {
                    if (value.ToString().ToLowerInvariant() == token.ToLowerInvariant())
                    {
                        Keyboard.Instance.PressSpecialKey((KeyboardInput.SpecialKeys)value);
                    }
                }
                index = tokenEnd + 1;
            }

            if (index < input.Length)
            {
                Keyboard.Instance.Enter(input.Substring(index));
            }
        }

        public IEnumerable<UiExecutionEntry> GetExecutions()
        {
            UIItemCollection controls = ContentPanel.Items;
            int promptIndex = FindPromptIndex(controls, controls.Count - 1);
            int nextPromptIndex = controls.Count;

            while (promptIndex >= 0)
            {
                CustomUIItem codeInput = controls[promptIndex + 1] as CustomUIItem;
                AutomationElement automationElement = codeInput?.AutomationElement;
                var valuePattern = automationElement?.GetSupportedPatterns()?.FirstOrDefault(p => p.ProgrammaticName.Contains("Value"));
                var value = automationElement?.GetCurrentPattern(valuePattern) as ValuePattern;

                string code = value?.Current.Value;
                string prompt = (controls[promptIndex] as WPFLabel)?.Text;

                IUIItem[] result = new IUIItem[nextPromptIndex - promptIndex - 2];
                for (int i = promptIndex + 2, k = 0; i < nextPromptIndex; i++, k++)
                {
                    result[k] = controls[i];
                }

                yield return new UiExecutionEntry()
                {
                    Code = code,
                    Prompt = prompt,
                    Result = result,
                };

                nextPromptIndex = promptIndex;
                promptIndex = FindPromptIndex(controls, promptIndex - 1);
            }
        }

        public void VerifyOutput(string code)
        {
            UiExecutionEntry[] executions = GetExecutions().Take(2).ToArray();

            Assert.Equal(2, executions.Length);

            UiExecutionEntry currentEntry = executions[0];
            UiExecutionEntry lastExecution = executions[1];

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
                Assert.Equal(FixOutput(expectedError), FixOutput(lastExecution.ResultText));
            }
            else
            {
                Assert.Equal(FixOutput(expectedOutput), FixOutput(lastExecution.ResultText));
            }
        }

        private static string FixOutput(string output)
        {
            return output.Trim().Replace("\r\n", "\n").Replace("\r", "");
        }

        private static int FindPromptIndex(UIItemCollection controls, int startingIndex)
        {
            int promptIndex = startingIndex;

            while (promptIndex >= 0)
            {
                WPFLabel textBlock = controls[promptIndex] as WPFLabel;

                if (textBlock != null && (textBlock.Text == DefaultPrompt || textBlock.Text == DbgPrompt || textBlock.Text == ExecutingPrompt))
                {
                    break;
                }

                promptIndex--;
            }

            return promptIndex;
        }
    }
}
