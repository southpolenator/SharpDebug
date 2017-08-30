using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ClrString = CsDebugScript.CommonUserTypes.CLR.System.String;

namespace DbgEngTest.CLR
{
    [TestClass]
    [DeploymentItem(@"CLR\Apps\LocalVariables.cs", @"CLR\Apps")]
    [DeploymentItem(@"CLR\Apps\SharedLibrary.cs", @"CLR\Apps")]
    public class ValueTests : ClrTestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            SyncStart();
            CompileAndInitialize(ClrTestApps.LocalVariables);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestMethod]
        [TestCategory("CLR")]
        public void CheckObjectArgsAndLocals()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Inner(")).Single();
            Variable i = frame.Arguments["i"];
            Variable d = frame.Locals["d"];

            Assert.AreEqual(0x42424242, (int)i);
            Assert.AreEqual(43.0, (double)d);
        }

        [TestMethod]
        [TestCategory("CLR")]
        public void NullValueOkTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable fooObject = frame.Locals["containsnullref"];

            Assert.AreEqual(42, (int)fooObject.GetField("SetValue").GetField("i"));
            Assert.IsTrue(fooObject.GetField("NullValue").IsNullPointer());
            Assert.IsTrue(clrThread.EnumerateStackObjects().Contains(fooObject));
        }

        [TestMethod]
        [TestCategory("CLR")]
        public void PrimitiveVariableConversionTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame;

            foreach (var f in clrThread.ClrStackTrace.Frames)
                foreach (var variable in f.Locals)
                    System.Console.WriteLine(" {2} = ({0}) {1:X}", variable.GetCodeType(), variable.GetPointerAddress(), variable.GetName());

            foreach (Variable variable in clrThread.EnumerateStackObjects())
                System.Console.WriteLine("   ({0}) {1:X}", variable.GetCodeType(), variable.GetPointerAddress());

            frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Inner(")).Single();
            Assert.IsTrue((bool)frame.Locals["b"]);
            Assert.AreEqual('c', (char)frame.Locals["c"]);
            Assert.AreEqual("hello world", new ClrString(frame.Locals["s"]).Text);
            Assert.AreEqual(42, (int)frame.Locals["st"].GetField("i"));
            Assert.IsTrue(clrThread.EnumerateStackObjects().Contains(frame.Locals["s"]));

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
        [TestCategory("CLR")]
        public void ObjectFieldTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.IsTrue(clrThread.EnumerateStackObjects().Contains(foo));
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
        [TestCategory("CLR")]
        public void ObjectLocalVariableTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.IsNotNull(foo);
            Assert.IsFalse(foo.IsNullPointer());
            Assert.AreEqual("Foo", foo.GetCodeType().Name);
        }

        [TestMethod]
        [TestCategory("CLR")]
        public void GetFieldTests()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.AreEqual("Foo", foo.GetCodeType().Name);
            Assert.IsTrue((bool)foo.GetField("b"));

            Variable inner = foo.GetField("st").GetField("middle").GetField("inner");
            Assert.IsTrue((bool)inner.GetField("b"));
        }

        [TestMethod]
        [TestCategory("CLR")]
        public void StructVariableTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
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
        [TestCategory("CLR")]
        public void InteriorStructTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
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
