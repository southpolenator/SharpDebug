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
        public void CheckObjectArgsAndLocals()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Inner(")).Single();
            Variable i = frame.Arguments["i"];
            Variable d = frame.Locals["d"];

            Assert.AreEqual(0x42424242, (int)i);
            Assert.AreEqual(43.0, (double)d);
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
        public void ArrayElementTests()
        {
            // TODO: ArrayElementTests test is not converted
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

        [TestMethod]
        public void ObjectFieldTest()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.AreEqual(42, (int)foo.GetField("i"));
            Assert.AreEqual(0x42u, (uint)foo.GetField("ui"));
            Assert.AreEqual("string", new ClrString(foo.GetField("s")).Text);
            Assert.AreEqual(true, (bool)foo.GetField("b"));
            Assert.AreEqual(4.2f, (float)foo.GetField("f"));
            Assert.AreEqual(8.4, (double)foo.GetField("d"));
            Assert.AreEqual('c', (char)foo.GetField("c"));
            Assert.AreEqual(0x12, (byte)foo.GetField("by"));
            Assert.AreEqual((sbyte)0x13, (sbyte)foo.GetField("sby"));
            Assert.AreEqual((short)0x4242, (short)foo.GetField("sh"));
            Assert.AreEqual((ushort)0x4343, (ushort)foo.GetField("ush"));
            Assert.AreEqual(0x424242ul, (ulong)foo.GetField("ulng"));
            Assert.AreEqual(0x434343L, (long)foo.GetField("lng"));
        }

        [TestMethod]
        public void ObjectLocalVariableTest()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.IsNotNull(foo);
            Assert.IsFalse(foo.IsNullPointer());
            Assert.AreEqual("Foo", foo.GetCodeType().Name);
        }

        [TestMethod]
        public void GetFieldTests()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.AreEqual("Foo", foo.GetCodeType().Name);
            Assert.IsTrue((bool)foo.GetField("b"));

            Variable inner = foo.GetField("st").GetField("middle").GetField("inner");
            Assert.IsTrue((bool)inner.GetField("b"));
        }

        [TestMethod]
        public void StructVariableTest()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable s = frame.Locals["s"];

            Assert.AreEqual("Struct", s.GetCodeType().Name);
            CheckStruct(s);
        }

        private static void CheckStruct(Variable s)
        {
            Assert.AreEqual(42, (int)s.GetField("i"));
            Assert.AreEqual("string", new ClrString(s.GetField("s")).Text);
            Assert.AreEqual(true, (bool)s.GetField("b"));
            Assert.AreEqual(4.2f, (float)s.GetField("f"));
            Assert.AreEqual(8.4, (double)s.GetField("d"));
            Assert.AreEqual("System.Object", s.GetField("o").GetCodeType().Name);
        }

        [TestMethod]
        public void InteriorStructTest()
        {
            ClrThread clrThread = Thread.Current.FindClrThread();
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable s = frame.Locals["s"];

            Assert.AreEqual("Struct", s.GetCodeType().Name);
            CheckStruct(s);

            s = s.GetField("middle");
            Assert.AreEqual("MiddleStruct", s.GetCodeType().Name);
            CheckStruct(s);

            s = s.GetField("inner");
            Assert.AreEqual("InnerStruct", s.GetCodeType().Name);
            CheckStruct(s);
        }
    }
}
