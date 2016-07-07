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
        private static string TestProcessPath;

        private const string TestProcessPathx64 = "NativeDumpTest.x64.exe";
        private const string TestProcessPathx86 = "NativeDumpTest.x86.exe";

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

            Debug.WriteLine($"Process {TestProcessPath} started.");

            int lastStackDepth = -1;
            for (int i = 0; i < 5; i++)
            {
                Context.Debugger.ContinueExecution();
                Debug.WriteLine($"Process continue iteration {i}.");

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

            Debug.WriteLine($"Process {TestProcessPath} started.");

            int lastArgument = -1;

            for (int i = 0; i < 5; i++)
            {
                Context.Debugger.ContinueExecution();
                Debug.WriteLine($"Process continue iteration {i}.");

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

        /// <summary>
        /// Tests running multiple processes.
        /// </summary>
        static void GoBreakMultipleProcessesBody()
        {
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);

            var ps = CsDebugScript.Process.All;
            Context.Debugger.ContinueExecution(ps[1]);
            Context.Debugger.ContinueExecution(ps[0]);

            foreach (var process in CsDebugScript.Process.All)
            {
                Context.Debugger.ContinueExecution(process);
            }

            System.Threading.Thread.Sleep(1000);

            foreach (var process in CsDebugScript.Process.All)
            {
                Context.Debugger.BreakExecution(process);
            }
        }

        /// <summary>
        /// Wrapper around actual tests which runs them in separate MTA thread
        /// in order to avoid problems with COM object sharing.
        /// </summary>
        /// <param name="test"></param>
        static void ContinousTestExecutionWrapper(Action test)
        {
            Action cleanup = () =>
            {
                Debug.WriteLine("Issuing process terminate for all the processes.");
                foreach (var process in CsDebugScript.Process.All)
                    Context.Debugger.Terminate(process);
            };

            var testWithCleanup = test + cleanup;

            TestProcessPath = Environment.Is64BitProcess ? TestProcessPathx64 : TestProcessPathx86;

            var testTask = System.Threading.Tasks.Task.Factory.StartNew(testWithCleanup);
            testTask.Wait();
            Assert.IsTrue(testTask.Exception == null, "Exception happened while running the test");
        }

        [TestMethod, Timeout(30000)]
        public void GoBreakContinuosTestDepth()
        {
            ContinousTestExecutionWrapper(GoBreakContinuosTestDepthBody);
        }

        [TestMethod, Timeout(30000)]
        public void GoBreakContinousVariablesChange()
        {
            ContinousTestExecutionWrapper(GoBreakContinousVariablesChangeBody);
        }

        [TestMethod]
        public void MultipleProcesses()
        {
            // Not yet implemented.
            // ContinousTestExecutionWrapper(GoBreakMultipleProcessesBody);
        }

    }
}
