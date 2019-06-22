using SharpDebug.ClrMdProvider;
using System.Linq;
using ClrException = SharpDebug.CommonUserTypes.CLR.System.Exception;
using Xunit;

namespace SharpDebug.Tests.CLR
{
    public abstract class DataReaderTests
    {
        [Fact]
        public void CodeCoverageTest()
        {
            DataReader dataReader = new DataReader(Process.Current);
            Thread thread = Thread.Current;
            int contextLength = thread.ThreadContext.Bytes.Length;
            Microsoft.Diagnostics.Runtime.VirtualQueryData vq;

            Assert.False(dataReader.CanReadAsync);
            Assert.NotEmpty(dataReader.EnumerateAllThreads());
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

    #region Test configurations
    [Collection("CLR NestedException")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class DataReaderTestsWindows : DataReaderTests
    {
    }

    [Collection("CLR NestedException Windows Core")]
    [Trait("x64", "true")]
    public class DataReaderTestsWindowsCore : DataReaderTests
    {
    }

#if ENABLE_LINUX_CLR_CORE_TESTS
    [Collection("CLR NestedException Linux Core")]
    [Trait("x64", "true")]
    public class DataReaderTestsLinuxCore : DataReaderTests
    {
    }
#endif
    #endregion
}
