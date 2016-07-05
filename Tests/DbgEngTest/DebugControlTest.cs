using CsDebugScript;
using CsDebugScript.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace DbgEngTest
{
    /// <summary>
    /// Tests for interactive debugging control.
    /// Note that every test has to run in a MTA initialized thread.
    /// </summary>
    [TestClass]
    public class DebugControlTest : TestBase
    {
        // TODO: check in test app.
        // IDEA: Write a test where variable just gets incremented and insure it trully does get incremented.
        //
        private const string TestProcessPath = @"NativeDumpTest.x64.exe";

        private const string DefaultSymbolPath = @".\";

        /// <summary>
        /// Test case id to be run.
        /// </summary>
        private const string ProcessArguments = @"1";

        static Thread FindThreadHostingMain()
        {
            return Thread.All.First(thread => thread.StackTrace.Frames.Any(frame => frame.FunctionName.Contains("!main")));
        }

        /// <summary>
        ///  Start process in inactive mode, release it, break it, print state in the loop.
        ///  Test runs a sample exe which recursivly calls the same function and increments the argument.
        ///  Test checks whether stack trully grows between the continue/break calls.
        /// </summary>
        static void GoBreakContinuosTestDepthBody()
        {
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);

            Debug.WriteLine("Process {0} started.", TestProcessPath);

            int lastStackDepth = -1;
            for (int i = 0; i < 5; i++)
            {
                Context.Debugger.ContinueExecution();
                System.Threading.Thread.Sleep(1000);
                Context.Debugger.BreakExecution();

                int depthOfMainThread = FindThreadHostingMain().StackTrace.Frames.Length;

                // Ensure that thread depth grows between the executions.
                //
                Assert.IsTrue(depthOfMainThread > lastStackDepth, "Stack did not grow between the executions");
                lastStackDepth = depthOfMainThread;
            }
        }

        /// <summary>
        /// Start process in inactive mode, release it, break it, print state in the loop.
        /// Test checks whether argument on stack changes.
        /// Also test asserts whether argument matches the stack lenght.
        /// </summary>
        static void GoBreakContinousVariablesChangeBody()
        {
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);

            int lastArgument = -1;

            for (int i = 0; i < 5; i++)
            {
                Context.Debugger.ContinueExecution();
                System.Threading.Thread.Sleep(1000);
                Context.Debugger.BreakExecution();

                Thread mainThread = FindThreadHostingMain();

                var functionNames = mainThread.StackTrace.Frames.Select(f => f.FunctionName).ToArray();

                var lastFrame = mainThread.StackTrace.Frames.First(frame => frame.FunctionName.Contains("InfiniteRecursionTestCase"));
                var functionArgument = lastFrame.Arguments.First(arg => arg.GetName() == "arg");

                Assert.IsTrue((int)functionArgument > lastArgument, "Argument did not increase");
                lastArgument = (int)functionArgument;

                int depthOfMainThread = mainThread.StackTrace.Frames.Where(frame => frame.FunctionName.Contains("InfiniteRecursionTestCase")).Count();

                Assert.AreEqual(depthOfMainThread, lastArgument + 1);

            }
        }

        static void MultipleProcessesBody()
        {
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);

            var ps = CsDebugScript.Process.All;
        }

        /// <summary>
        /// Wrapper around actual tests which runs them in separate MTA thread
        /// in order to avoid problems with COM object sharing.
        /// </summary>
        /// <param name="test"></param>
        static void ContinousTestExecutionWrapper(Action test)
        {
            Action cleanup = () => Context.Debugger.Terminate();
            var testWithCleanup = test + cleanup;

            System.Threading.Thread testThread = new System.Threading.Thread(() => testWithCleanup());
            testThread.SetApartmentState(System.Threading.ApartmentState.MTA);

            testThread.Start();
            testThread.Join();
        }

        [TestMethod]
        public void GoBreakContinuosTestDepth()
        {
            ContinousTestExecutionWrapper(GoBreakContinuosTestDepthBody);
        }

        [TestMethod]
        public void GoBreakContinousVariablesChange()
        {
            ContinousTestExecutionWrapper(GoBreakContinousVariablesChangeBody);
        }

        [TestMethod]
        public void MultipleProcesses()
        {

            ContinousTestExecutionWrapper(MultipleProcessesBody);
        }

    }
}
