using CsDebugScript.ClrMdProvider;
using System.Linq;
using ClrException = CsDebugScript.CommonUserTypes.CLR.System.Exception;
using Xunit;

namespace CsDebugScript.Tests.CLR
{
    [Collection("CLR NestedException")]
    [Trait("Run", "x64,x86")]
    public class DataReaderTests
    {
        [Fact]
        public void CodeCoverageTest()
        {
            DataReader dataReader = new DataReader(Process.Current);
            Thread thread = Thread.Current;
            int contextLength = thread.ThreadContext.Bytes.Length;
            Microsoft.Diagnostics.Runtime.VirtualQueryData vq;

            Assert.False(dataReader.CanReadAsync);
            Assert.NotEqual(0, dataReader.EnumerateAllThreads().Count());
            Assert.True(dataReader.GetThreadContext(thread.SystemId, 0, (uint)contextLength, new byte[contextLength]));
            Assert.Equal(thread.TebAddress, dataReader.GetThreadTeb(thread.SystemId));
            Assert.Equal((uint)0, dataReader.ReadDwordUnsafe(0));
            Assert.Equal((ulong)0, dataReader.ReadPointerUnsafe(0));
            Assert.False(dataReader.VirtualQuery(0, out vq));

            Debugger.DontUseDumpReader = true;
            ClrException exception = Variable.CastAs<ClrException>(thread.ClrThread.LastThrownException);
            Assert.Equal("IOE Message", exception.Message);
            Debugger.DontUseDumpReader = false;

            dataReader.Flush();
            dataReader.Close();
        }
    }
}
