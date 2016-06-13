using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DbgEngTest.CLR
{
    [TestClass]
    public class HeapTests : ClrTestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            CompileAndInitialize("Types");
        }

        [TestMethod]
        public void HeapEnumeration()
        {
            Runtime runtime = Process.Current.ClrRuntimes.Single();
            Heap heap = runtime.Heap;
            int count = 0;

            foreach (Variable variable in heap.EnumerateObjects())
            {
                Assert.IsNotNull(variable);
                count++;
            }

            Assert.IsTrue(count > 0);
        }
    }
}
