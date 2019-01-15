using CsDebugScript.Engine;
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

        public static string GetAbsoluteBinPathRecursive1(string path)
        {
            if (Path.IsPathRooted(path))
                return path;

            string parent = Path.GetDirectoryName(GetBinFolder());
            string[] directories = Directory.GetDirectories(parent);

            foreach (string directory in directories)
            {
                string file = Path.GetFullPath(Path.Combine(directory, path));

                if (File.Exists(file))
                    return file;
            }
            return GetAbsoluteBinPath(path);
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
    }

    public class DumpTestBase : TestBase
    {
        private static object autoCastLock = new object();

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

        protected void Execute_AutoCast(Action action)
        {
            lock (autoCastLock)
            {
                var originalUserTypeMetadata = Context.UserTypeMetadata;

                try
                {
                    Context.SetUserTypeMetadata(ScriptCompiler.ExtractMetadata(new[]
                        {
                            typeof(CsDebugScript.CommonUserTypes.NativeTypes.std.@string).Assembly,
                            GetType().Assembly,
                        }));

                    action();
                }
                finally
                {
                    Context.SetUserTypeMetadata(originalUserTypeMetadata);
                }
            }
        }
    }
}
