using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace CsDebugScript.Tests
{
    public class TestBase
    {
        public static Assembly GetTestsAssembly()
        {
            return typeof(TestBase).GetTypeInfo().Assembly;
        }

        public static string GetBinFolder()
        {
            Assembly assembly = GetTestsAssembly();
            Uri codeBaseUrl = new Uri(assembly.CodeBase);
            string codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);

            return Path.GetDirectoryName(codeBasePath);
        }

        public static string GetAbsoluteBinPath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(GetBinFolder(), path));
            }

            return path;
        }

        public static StackFrame GetFrame(string functionName)
        {
            foreach (var frame in Thread.Current.StackTrace.Frames)
            {
                try
                {
                    if (frame.FunctionName == functionName)
                    {
                        return frame;
                    }
                }
                catch (Exception)
                {
                    // Ignore exception for getting source file name for frames where we don't have PDBs
                }
            }

            throw new Exception($"Frame not found '{functionName}'");
        }

        public static void CompareArrays<T>(T[] array1, T[] array2)
        {
            Assert.Equal(array1.Length, array2.Length);
            for (int i = 0; i < array1.Length; i++)
            {
                Assert.Contains(array1[i], array2);
            }
        }

        public static void ExecuteMTA(Action action)
        {
            if (System.Threading.Thread.CurrentThread.GetApartmentState() != System.Threading.ApartmentState.MTA)
            {
                System.Threading.Thread thread = new System.Threading.Thread(() => action());
                thread.SetApartmentState(System.Threading.ApartmentState.MTA);
                thread.Start();
                thread.Join();
            }
            else
            {
                action();
            }
        }
    }

    public class DumpTestBase : TestBase
    {
        public DumpTestBase(DumpInitialization dumpInitialization)
        {
            DumpInitialization = dumpInitialization;
        }

        public DumpInitialization DumpInitialization { get; private set; }

        public string DefaultModuleName
        {
            get
            {
                return DumpInitialization.DefaultModuleName;
            }
        }

        public Module DefaultModule
        {
            get
            {
                return Module.All.Single(module => module.Name == DefaultModuleName);
            }
        }
    }
}
