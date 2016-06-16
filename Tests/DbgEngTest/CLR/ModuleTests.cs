using CsDebugScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DbgEngTest.CLR
{
    [TestClass]
    public class ModuleTests : ClrTestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            CompileAndInitialize(ClrTestApps.Types);
        }

        [TestMethod]
        public void TestGetTypeByName()
        {
            Module types = Process.Current.Modules.Where(m => m.Name == "Types").Single();

            Assert.IsNotNull(CodeType.Create("Foo", types));
            try
            {
                CodeType.Create("Foo2", types);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Type 'Foo2' wasn't found", ex.Message);
            }
        }
    }
}
