using CsDebugScript.Engine;
using System;
using Diagnostics = System.Diagnostics;
using System.Linq;
using Xunit;
using DbgEng;
using CsDebugScript.Engine.Debuggers;
using System.IO;
using Xunit.Abstractions;

namespace CsDebugScript.Tests
{
    /// <summary>
    /// Tests for interactive debugging control.
    /// Note that every test has to run in a MTA initialized thread.
    /// </summary>
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    [Collection("Sequential")]
    public class DebugControlTest : TestBase
    {
        private TimeSpan DefaultTimeout
        {
            get
            {
                if (Diagnostics.Debugger.IsAttached)
                {
                    return TimeSpan.FromSeconds(10 * 1000);
                }
                else
                {
                    return TimeSpan.FromSeconds(10);
                }
            }
        }

        private const string TestProcessPathx64 = "NativeDumpTest.x64.exe";
        private const string TestProcessPathx86 = "NativeDumpTest.x86.exe";

        private const string DefaultSymbolPath = DumpInitialization.DefaultDumpPath;
        private readonly ITestOutputHelper testOutputHelper;

        /// <summary>
        /// Test case id to be run.
        /// </summary>
        private const string ProcessArguments = @"1";

        private static string TestProcessPath
        {
            get
            {
                return Path.Combine(DumpInitialization.DefaultDumpPath, Environment.Is64BitProcess ? TestProcessPathx64 : TestProcessPathx86);
            }
        }

        public DebugControlTest(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

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

            Diagnostics.Debug.WriteLine($"Process {TestProcessPath} started.");

            int lastStackDepth = -1;
            for (int i = 0; i < 5; i++)
            {
                Debugger.ContinueExecution();
                Diagnostics.Debug.WriteLine($"Process continue iteration {i}.");

                System.Threading.Thread.Sleep(1000);
                Debugger.BreakExecution();

                int depthOfMainThread = FindThreadHostingMain().StackTrace.Frames.Length;

                // Ensure that thread depth grows between the executions.
                //
                Assert.True(depthOfMainThread > lastStackDepth, "Stack did not grow between the executions");
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

            Diagnostics.Debug.WriteLine($"Process {TestProcessPath} started.");

            int lastArgument = -1;

            for (int i = 0; i < 5; i++)
            {
                Debugger.ContinueExecution();
                Diagnostics.Debug.WriteLine($"Process continue iteration {i}.");

                System.Threading.Thread.Sleep(1000);
                Debugger.BreakExecution();

                Thread mainThread = FindThreadHostingMain();

                var functionNames = mainThread.StackTrace.Frames.Select(f => f.FunctionName).ToArray();

                var lastFrame = mainThread.StackTrace.Frames.First(frame => frame.FunctionName.Contains("InfiniteRecursionTestCase"));
                var functionArgument = lastFrame.Arguments.First(arg => arg.GetName() == "arg");

                Assert.True((int)functionArgument > lastArgument, "Argument did not increase");
                lastArgument = (int)functionArgument;

                int depthOfMainThread = mainThread.StackTrace.Frames.Where(frame => frame.FunctionName.Contains("InfiniteRecursionTestCase")).Count();

                Diagnostics.Debug.WriteLine($"Depth of main thread is {depthOfMainThread}");
                Assert.Equal(depthOfMainThread, lastArgument + 1);
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
                Debugger.ContinueExecution(process);
            }

            System.Threading.Thread.Sleep(1000);

            foreach (var process in CsDebugScript.Process.All)
            {
                Debugger.BreakExecution(process);
            }
        }

        static void BreakpointSanityTestBody()
        {
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);
            Diagnostics.Debug.WriteLine($"Process {TestProcessPath} started.");

            System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);

            Debugger.AddBreakpoint("NativeDumpTest_x64!InfiniteRecursionTestCase", () =>
            {
                are.Set();
            });

            Debugger.ContinueExecution();
            are.WaitOne();
        }
        static void BreakpointBreakAndContinueTestBody()
        {
            InitializeProcess(TestProcessPath, ProcessArguments, DefaultSymbolPath);
            Diagnostics.Debug.WriteLine($"Process {TestProcessPath} started.");

            System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
            int breakpointHitCount = 0;

            Debugger.AddBreakpoint("NativeDumpTest_x64!InfiniteRecursionTestCase", () =>
            {
                Thread mainThread = FindThreadHostingMain();
                int recursionDepthCount = 
                    mainThread.StackTrace.Frames.
                    Where(frame => frame.FunctionName.Contains("InfiniteRecursionTestCase")).Count();

                breakpointHitCount++;
                Assert.Equal(breakpointHitCount, recursionDepthCount);

                if (recursionDepthCount == 10)
                {
                    are.Set();
                }
            });

            Debugger.ContinueExecution();
            are.WaitOne();
        }

        /// <summary>
        /// Wrapper around actual tests which runs them in separate MTA thread
        /// in order to avoid problems with COM object sharing.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="timeout"></param>
        void ContinousTestExecutionWrapper(Action test, TimeSpan timeout)
        {
            Action cleanup = () =>
            {
                Diagnostics.Debug.WriteLine("Issuing process terminate for all the processes.");
                foreach (var process in Process.All)
                    Debugger.Terminate(process);
            };

            Action testWithCleanup = () =>
            {
                try
                {
                    test();
                    testOutputHelper.WriteLine("Test completed without exceptions.");
                }
                finally
                {
                    testOutputHelper.WriteLine("Starting cleanup.");
                    cleanup();
                    testOutputHelper.WriteLine("Cleanup completed without exceptions.");
                }
            };
            
            var testTask = System.Threading.Tasks.Task.Factory.StartNew(testWithCleanup);

            bool waitForTaskCompleteSuccess = testTask.Wait(timeout);
            if (!waitForTaskCompleteSuccess)
            {
                // break here
                testOutputHelper.WriteLine("Wait for task complete failure.");
            }

            Assert.True(waitForTaskCompleteSuccess, "Test timeout");
            Assert.True(testTask.Exception == null, "Exception happened while running the test");
        }

        [Fact]
        public void GoBreakContinuosTestDepth()
        {
            ContinousTestExecutionWrapper(GoBreakContinuosTestDepthBody, DefaultTimeout);
        }

        [Fact]
        public void GoBreakContinousVariablesChange()
        {
            ContinousTestExecutionWrapper(GoBreakContinousVariablesChangeBody, DefaultTimeout);
        }

        [Fact]
        public void BreakpointSanityTest() => ContinousTestExecutionWrapper(BreakpointSanityTestBody, DefaultTimeout);

        [Fact]
        public void BreakpointBreakAndContinue() => ContinousTestExecutionWrapper(BreakpointBreakAndContinueTestBody, DefaultTimeout);

        [Fact]
        public void MultipleProcesses()
        {
            // Not yet implemented.
            // ContinousTestExecutionWrapper(GoBreakMultipleProcessesBody, DefaultTimeout);
        }

        /// <summary>
        /// Initializes the test class with the specified process file.
        /// </summary>
        /// <param name="processPath">Path to the process to be started.</param>
        /// <param name="processArguments">Arguments for process to be started.</param>
        /// <param name="symbolPath">Symbol path.</param>
        /// <param name="addSymbolServer">if set to <c>true</c> symbol server will be added to the symbol path.</param>
        /// <param name="debugEngineOptions">Debug create options. Default is to start in break mode, and break on process exit.</param>
        protected static void InitializeProcess(string processPath, string processArguments, string symbolPath, bool addSymbolServer = true, uint debugEngineOptions = (uint)(Defines.DebugEngoptInitialBreak | Defines.DebugEngoptFinalBreak))
        {
            processPath = GetAbsoluteBinPath(processPath);
            symbolPath = GetAbsoluteBinPath(symbolPath);
            if (addSymbolServer)
            {
                symbolPath += ";srv*";
            }

            // Disable caching.
            //
            Context.EnableUserCastedVariableCaching = false;
            Context.EnableVariableCaching = false;

            IDebugClient client = DebugClient.OpenProcess(processPath, processArguments, symbolPath, debugEngineOptions);
            DbgEngDll.InitializeContext(client);
        }
    }
}
