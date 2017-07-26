using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Windows.Input;
using System;

namespace DbgEngTest
{
    [CodedUITest]
    public class CodedUITests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;
        private UIMap map;

        public CodedUITests()
        {
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            InitializeDump(DefaultDumpFile, DefaultSymbolPath);
        }

        [TestMethod, Timeout(30000)]
        [TestCategory("UI")]
        public void ExecuteDebuggerCommand()
        {
            OpenInteractiveWindow();

            SendInput("{#}dbg{Space}k{Enter}");
            WaitForExecutionState();
            WaitForReadyState();
            UIMap.AssertInteractiveWindowReady();

            CloseInteractiveWindow();
        }

        [TestMethod, Timeout(60000)]
        [TestCategory("UI")]
        public void MultiLineCode()
        {
            OpenInteractiveWindow();

            SendInput("for {(}int i = 0; i < 5; i{+}{+}{)}{Enter}Console.WriteLine{(}i{)};{Enter}");

            WaitForExecutionState();
            WaitForReadyState();
            UIMap.AssertInteractiveWindowReady();

            CloseInteractiveWindow();
        }

        [TestMethod, Timeout(60000)]
        [TestCategory("UI")]
        public void SimpleEndToEnd()
        {
            OpenInteractiveWindow();

            SendInput("var{Space}a{Space}={Space}new{Space}{[}{]}{Space}{RShiftKey}{{}");
            SendInput("{Space}", ModifierKeys.Shift);
            SendInput("1,{Space}2,{Space}3,{Space}4,{Space}5,{Space}6,{Space}7,{Space}{RShiftKey}{}};{Enter}");

            WaitForExecutionState();
            WaitForReadyState();
            UIMap.AssertInteractiveWindowReady();

            SendInput("writeln{RShiftKey}{(}a.{RShiftKey}Len{Enter}{RShiftKey}{)};{Enter}");

            WaitForExecutionState();
            WaitForReadyState();
            UIMap.AssertInteractiveWindowReady();

            SendInput("a{Enter}{Enter}");

            WaitForExecutionState();
            WaitForReadyState();
            UIMap.AssertInteractiveWindowReady();

            CloseInteractiveWindow();
        }

        /// <summary>
        /// Gets or sets the test context which provides information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        public WpfCustom CodeInput
        {
            get
            {
                return UIMap.InteractiveWindow.InteractiveWindowContent.ResultContainer.PromptLabel.CodeInput;
            }
        }

        private void OpenInteractiveWindow()
        {
            Executor.ShowInteractiveWindow(false);

            WaitForReadyState(10000);
            UIMap.AssertInteractiveWindowReady();
        }

        private void CloseInteractiveWindow(int millisecondsTimeout = 5000)
        {
            SendInput("q{Space}{Enter}");
            UIMap.InteractiveWindow.WaitForControlNotExist(millisecondsTimeout);
        }

        private void WaitForReadyState(int millisecondsTimeout = 5000)
        {
            PromptLabel label = UIMap.InteractiveWindow.InteractiveWindowContent.ResultContainer.PromptLabel;
            bool happened = label.WaitForControlCondition(c => c.GetProperty(WpfText.PropertyNames.DisplayText).ToString() == "C#> ", millisecondsTimeout);

#if false
            if (!happened)
            {
                System.Windows.Forms.MessageBox.Show($"'{label.DisplayText}'");
            }
#endif
            Assert.IsTrue(happened);
        }

        private void WaitForExecutionState(int millisecondsTimeout = 5000)
        {
            PromptLabel label = UIMap.InteractiveWindow.InteractiveWindowContent.ResultContainer.PromptLabel;
            bool happened = label.WaitForControlCondition(c => c.GetProperty(WpfText.PropertyNames.DisplayText).ToString() == "...> ", millisecondsTimeout);

#if false
            if (!happened)
            {
                System.Windows.Forms.MessageBox.Show($"'{label.DisplayText}'");
            }
#endif
            Assert.IsTrue(happened);
        }

        private void SendInput(string input, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            Keyboard.SendKeys(UIMap.InteractiveWindow, input, modifierKeys);
        }
    }
}
