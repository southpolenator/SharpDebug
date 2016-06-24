using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DbgEngTest.CLR
{
    [TestClass]
    public class HeapTests : ClrTestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            SyncStart();
            CompileAndInitialize(ClrTestApps.Types);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestMethod]
        public void HeapSize()
        {
            Runtime runtime = Process.Current.ClrRuntimes.Single();
            Heap heap = runtime.Heap;

            Assert.IsTrue(heap.TotalHeapSize > 0);

            ulong generationSizes = 0;

            for (int generation = 0; generation <= 3; generation++)
                generationSizes += heap.GetSizeByGeneration(generation);
            Assert.AreEqual(heap.TotalHeapSize, generationSizes);
        }

        [TestMethod]
        public void HeapEnumeration()
        {
            Runtime runtime = Process.Current.ClrRuntimes.Single();
            Heap heap = runtime.Heap;
            int count = 0;

            Assert.IsTrue(heap.CanWalkHeap);
            foreach (Variable variable in heap.EnumerateObjects())
            {
                Assert.IsNotNull(variable);
                count++;
            }

            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public void ServerSegmentTests()
        {
            Environment.SetEnvironmentVariable("COMPlus_BuildFlavor", "svr");
            CompileAndInitialize(ClrTestApps.Types, customDumpName: "TypesServerGC.mdmp");
            Runtime runtime = Process.Current.ClrRuntimes.Single();

            Assert.IsTrue(runtime.ServerGC);
        }

        [TestMethod]
        public void WorkstationSegmentTests()
        {
            Environment.SetEnvironmentVariable("COMPlus_BuildFlavor", "");
            CompileAndInitialize(ClrTestApps.Types, customDumpName: "TypesWorkstation.mdmp");
            Runtime runtime = Process.Current.ClrRuntimes.Single();

            Assert.IsFalse(runtime.ServerGC);
        }
    }
}
