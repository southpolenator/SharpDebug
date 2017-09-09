using CsDebugScript.CLR;
using System.Linq;
using Xunit;
using ClrString = CsDebugScript.CommonUserTypes.CLR.System.String;

namespace CsDebugScript.Tests.CLR
{
    [Collection("CLR LocalVariables")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class ValueTests
    {
        [Fact]
        public void CheckObjectArgsAndLocals()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Inner(")).Single();
            Variable i = frame.Arguments["i"];
            Variable d = frame.Locals["d"];

            Assert.Equal(0x42424242, (int)i);
            Assert.Equal(43.0, (double)d);
        }

        [Fact]
        public void NullValueOkTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable fooObject = frame.Locals["containsnullref"];

            Assert.Equal(42, (int)fooObject.GetField("SetValue").GetField("i"));
            Assert.True(fooObject.GetField("NullValue").IsNullPointer());
            Assert.True(clrThread.EnumerateStackObjects().Contains(fooObject));
        }

        [Fact]
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
            Assert.True((bool)frame.Locals["b"]);
            Assert.Equal('c', (char)frame.Locals["c"]);
            Assert.Equal("hello world", new ClrString(frame.Locals["s"]).Text);
            Assert.Equal(42, (int)frame.Locals["st"].GetField("i"));
            Assert.True(clrThread.EnumerateStackObjects().Contains(frame.Locals["s"]));

            frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Middle(")).Single();
            Assert.Equal(0x42, (byte)frame.Locals["b"]);
            Assert.Equal(0x43, (sbyte)frame.Locals["sb"]);
            Assert.Equal(0x4242, (short)frame.Locals["sh"]);
            Assert.Equal(0x4243, (ushort)frame.Locals["ush"]);
            Assert.Equal(0x42424242, (int)frame.Locals["i"]);
            Assert.Equal(0x42424243u, (uint)frame.Locals["ui"]);

            frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Outer(")).Single();
            Assert.Equal(42.0f, (float)frame.Locals["f"]);
            Assert.Equal(43.0, (double)frame.Locals["d"]);
            Assert.Equal((ulong)0x42424242, (ulong)frame.Locals["ptr"]);
            Assert.Equal((ulong)0x43434343, (ulong)frame.Locals["uptr"]);
        }

        [Fact]
        public void ObjectFieldTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.True(clrThread.EnumerateStackObjects().Contains(foo));
            Assert.Equal(42, (int)foo.GetField("i"));
            Assert.Equal(0x42u, (uint)foo.GetField("ui"));
            Assert.Equal("string", new ClrString(foo.GetField("s")).Text);
            Assert.Equal(true, (bool)foo.GetField("b"));
            Assert.Equal(4.2f, (float)foo.GetField("f"));
            Assert.Equal(8.4, (double)foo.GetField("d"));
            Assert.Equal('c', (char)foo.GetField("c"));
            Assert.Equal(0x12, (byte)foo.GetField("by"));
            Assert.Equal((sbyte)0x13, (sbyte)foo.GetField("sby"));
            Assert.Equal((short)0x4242, (short)foo.GetField("sh"));
            Assert.Equal((ushort)0x4343, (ushort)foo.GetField("ush"));
            Assert.Equal(0x424242ul, (ulong)foo.GetField("ulng"));
            Assert.Equal(0x434343L, (long)foo.GetField("lng"));
        }

        [Fact]
        public void ObjectLocalVariableTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.NotNull(foo);
            Assert.False(foo.IsNullPointer());
            Assert.Equal("Foo", foo.GetCodeType().Name);
        }

        [Fact]
        public void GetFieldTests()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable foo = frame.Locals["foo"];

            Assert.Equal("Foo", foo.GetCodeType().Name);
            Assert.True((bool)foo.GetField("b"));

            Variable inner = foo.GetField("st").GetField("middle").GetField("inner");
            Assert.True((bool)inner.GetField("b"));
        }

        [Fact]
        public void StructVariableTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable s = frame.Locals["s"];

            Assert.Equal("Struct", s.GetCodeType().Name);
            CheckStruct(s);
        }

        private static void CheckStruct(Variable s)
        {
            Assert.Equal(42, (int)s.GetField("i"));
            Assert.Equal("string", new ClrString(s.GetField("s")).Text);
            Assert.Equal(true, (bool)s.GetField("b"));
            Assert.Equal(4.2f, (float)s.GetField("f"));
            Assert.Equal(8.4, (double)s.GetField("d"));
            Assert.Equal("System.Object", s.GetField("o").GetCodeType().Name);
        }

        [Fact]
        public void InteriorStructTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable s = frame.Locals["s"];

            Assert.Equal("Struct", s.GetCodeType().Name);
            CheckStruct(s);

            s = s.GetField("middle");
            Assert.Equal("MiddleStruct", s.GetCodeType().Name);
            CheckStruct(s);

            s = s.GetField("inner");
            Assert.Equal("InnerStruct", s.GetCodeType().Name);
            CheckStruct(s);
        }
    }
}
