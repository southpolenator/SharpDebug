using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable fooObject = frame.Locals["containsnullref"];

            Assert.AreEqual(42, (int)fooObject.GetField("SetValue").GetField("i"));
            Assert.IsTrue(fooObject.GetField("NullValue").IsNullPointer());
        }

        [TestMethod]
        public void PrimitiveVariableConversionTest()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame;

            frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Inner(")).Single();
            Assert.IsTrue((bool)frame.Locals["b"]);
            Assert.AreEqual('c', (char)frame.Locals["c"]);
            Assert.AreEqual("hello world", new ClrString(frame.Locals["s"]).Text);
            Assert.AreEqual(42, (int)frame.Locals["st"].GetField("i"));

            frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Middle(")).Single();
            Assert.AreEqual(0x42, (byte)frame.Locals["b"]);
            Assert.AreEqual(0x43, (sbyte)frame.Locals["sb"]);
            Assert.AreEqual(0x4242, (short)frame.Locals["sh"]);
            Assert.AreEqual(0x4243, (ushort)frame.Locals["ush"]);
            Assert.AreEqual(0x42424242, (int)frame.Locals["i"]);
            Assert.AreEqual(0x42424243u, (uint)frame.Locals["ui"]);

            frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Outer(")).Single();
            Assert.AreEqual(42.0f, (float)frame.Locals["f"]);
            Assert.AreEqual(43.0, (double)frame.Locals["d"]);
            Assert.AreEqual((ulong)0x42424242, (ulong)frame.Locals["ptr"]);
            Assert.AreEqual((ulong)0x43434343, (ulong)frame.Locals["uptr"]);
        }
    }
}
