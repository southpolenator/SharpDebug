using CsDebugScript.CLR;
using System.Linq;
using Xunit;
using ClrString = CsDebugScript.CommonUserTypes.CLR.System.String;

namespace CsDebugScript.Tests.CLR
{
    [Collection("CLR NestedException")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class StackTests
    {
        [Fact]
        public void ObjectArgumentAndLocalTest()
        {
            IClrThread clrThread = Thread.Current.ClrThread;
            StackFrame frame = clrThread.ClrStackTrace.Frames.Where(f => f.FunctionNameWithoutModule.StartsWith("Program.Main(")).Single();
            Variable args = frame.Arguments.Single();

            Assert.NotNull(clrThread.Runtime);
            Assert.NotNull(clrThread.AppDomain);
            Assert.NotNull(args);
            Assert.Equal("System.String[]", args.GetCodeType().Name);
            Assert.Equal("args", args.GetName());

            Variable foo = frame.Locals.Single();

            Assert.NotNull(foo);
            Assert.False(foo.IsNull());
            Assert.Equal("Foo", foo.GetCodeType().Name);
            Assert.Equal("foo", foo.GetName());

            Assert.Equal(8.4, (double)foo.GetField("d"));
            Assert.Equal("Foo string", new ClrString(foo.GetField("FooString")).Text);
        }
    }
}
