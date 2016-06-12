using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsDebugScript;
using DbgEngManaged;
using System.Threading.Tasks;

namespace DbgEngTest
{
    [CodedUITest]
    public class CodedUITests : TestBase
    {
        private const string DefaultDumpFile = NativeDumpTest64.DefaultDumpFile;
        private const string DefaultSymbolPath = NativeDumpTest64.DefaultSymbolPath;

        public CodedUITests()
        {
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            Task mtaTask = new Task(() =>
            {
                //Initialize(DefaultDumpFile, DefaultSymbolPath);
                new Executor().InitializeContext(DebugClient.DebugCreateEx(0));
            });

            mtaTask.Start();
            mtaTask.Wait();
        }

        [TestMethod, Timeout(60000)]
        public void SimpleEndToEnd()
        {
            // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
            Executor.ShowInteractiveWindow(false);
            UIMap.AssertMethod1();
            UIMap.RecordedMethod1();
            System.Threading.Thread.Sleep(1000);
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

        private UIMap map;
    }
}
