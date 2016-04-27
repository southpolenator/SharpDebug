using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbgEngTest.CLR
{
    [TestClass]
    public class ValueTests : ClrTestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            CompileAndInitialize("LocalVariables");
        }

        [TestMethod]
        public void NullValueOkTest()
        {
            //var frame = GetFrame("LocalVariables!Program.Main");
            //ClrRuntime runtime = dt.ClrVersions.Single().CreateRuntime();
            //ClrValue fooObject = runtime.GetMainThread().GetFrame("Main").GetLocal("containsnullref");

            //Assert.AreEqual(42, fooObject.GetObject("SetValue").GetInt32("i"));
            //Assert.IsTrue(fooObject.GetObject("NullValue").IsNull);
            //Assert.IsNull(fooObject.GetObject("NullValue").GetInt32OrNull("i"));
        }
    }
}
