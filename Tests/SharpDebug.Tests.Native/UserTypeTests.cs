using SharpDebug.CommonUserTypes;
using SharpDebug.CommonUserTypes.NativeTypes.Windows;
using System;
using Xunit;

namespace SharpDebug.Tests.Native
{
    [Collection("NativeDumpTest.x64.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class UserTypeTests : DumpTestBase
    {
        public UserTypeTests(NativeDumpTest_x64_dmp_Initialization initialization)
            : base(initialization)
        {
        }

        [Fact]
        public void TestNakedPointer()
        {
            NakedPointer nakedPointer = new NakedPointer(0);

            Assert.True(nakedPointer.IsNull);
            Assert.True(nakedPointer.ToCodePointer<int>().IsNull);
        }

        [Fact]
        public void TestPEB()
        {
            ProcessEnvironmentBlock peb = new ProcessEnvironmentBlock();

            try
            {
                Console.WriteLine(peb.BeingDebugged);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessHeap.GetPointerAddress());
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessHeaps.Length);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessParameters.CommandLine);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessParameters.EnvironmentVariables.Length);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }

            try
            {
                Console.WriteLine(peb.ProcessParameters.ImagePathName);
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }
        }

        [Fact]
        public void TestTEB()
        {
            ThreadEnvironmentBlock teb = new ThreadEnvironmentBlock(Thread.Current.TEB);

            try
            {
                Console.WriteLine(teb.PEB.GetPointerAddress());
            }
            catch (InvalidSymbolsException)
            {
                // Ignore this exception
            }
        }
    }
}
