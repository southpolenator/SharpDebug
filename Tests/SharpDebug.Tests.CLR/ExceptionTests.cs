﻿using SharpDebug.CLR;
using System;
using System.Collections;
using System.IO;
using ClrException = SharpDebug.CommonUserTypes.CLR.System.Exception;
using Xunit;

namespace SharpDebug.Tests.CLR
{
    public abstract class ExceptionTests
    {
        [Fact]
        public void ExceptionPropertyTest()
        {
            IClrThread thread = Thread.Current.ClrThread;

            Assert.False(thread.IsFinalizerThread);

            ClrException exception = Variable.CastAs<ClrException>(thread.LastThrownException);

            Assert.NotNull(exception);
            Assert.Equal("IOE Message", exception.Message);
            Assert.Equal("System.InvalidOperationException", exception.GetCodeType().Name);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("FNF Message", exception.InnerException.Message);
            Assert.Equal("System.IO.FileNotFoundException", exception.InnerException.GetCodeType().Name);

            CodeFunction[] stackTrace = exception.StackTrace;

            Assert.Equal(4, stackTrace.Length);
            Assert.Equal("Program.Inner()", stackTrace[0].FunctionNameWithoutModule);
            Assert.Equal("Program.Middle()", stackTrace[1].FunctionNameWithoutModule);
            Assert.Equal("Program.Outer()", stackTrace[2].FunctionNameWithoutModule);
            Assert.Equal("Program.Main(System.String[])", stackTrace[3].FunctionNameWithoutModule);
            Assert.Equal((uint)11, stackTrace[3].SourceFileLine);
            Assert.Equal("nestedexception.cs", Path.GetFileName(stackTrace[3].SourceFileName).ToLowerInvariant());

            Assert.Equal(0x80131509, (uint)exception.HResult);
            Assert.Null(exception.HelpLink);
            Assert.Null(exception.Source);
            Assert.Null(exception.StackTraceString);

            try
            {
                IDictionary data = exception.Data;
                throw new Exception("This should never happen");
            }
            catch (NotImplementedException)
            {
            }
        }
    }

    #region Test configurations
    [Collection("CLR NestedException")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class ExceptionTestsWindows : ExceptionTests
    {
    }

    [Collection("CLR NestedException Windows Core")]
    [Trait("x64", "true")]
    public class ExceptionTestsWindowsCore : ExceptionTests
    {
    }

#if ENABLE_LINUX_CLR_CORE_TESTS
    [Collection("CLR NestedException Linux Core")]
    [Trait("x64", "true")]
    public class ExceptionTestsLinuxCore : ExceptionTests
    {
    }
#endif
    #endregion
}
