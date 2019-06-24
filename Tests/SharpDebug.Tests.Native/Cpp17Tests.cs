using System.Linq;
using Xunit;
using std = SharpDebug.CommonUserTypes.NativeTypes.std;

namespace SharpDebug.Tests.Native
{
    public abstract class Cpp17Tests : DumpTestBase
    {
        private const string MainSourceFileName = "cpp17.cpp";

        public Cpp17Tests(DumpInitialization dumpInitialization, bool executeCodeGen = true)
            : base(dumpInitialization)
        {
        }

        public bool LinuxDump { get; set; }

        public bool NoRtti { get; set; }

        [Fact]
        public void StdAny()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestAny");
            VariableCollection locals = defaultTestCaseFrame.Locals;

            // int
            std.any a_int = new std.any(locals["a_int"]);
            Assert.True(a_int.HasValue);
            Assert.Equal("int", a_int.Value.GetCodeType().Name);
            Assert.Equal(1729, (int)a_int.Value);

            // double
            std.any a_double = new std.any(locals["a_double"]);
            Assert.True(a_double.HasValue);
            Assert.Equal("double", a_double.Value.GetCodeType().Name);
            Assert.Equal(3.14, (double)a_double.Value);

            // std::string
            std.any a_string = new std.any(locals["a_string"]);
            Assert.True(a_string.HasValue);
            std.@string s = new std.@string(a_string.Value);
            Assert.Equal("meow", s.Text);

            // std::vector<int>
            std.any a_vector = new std.any(locals["a_vector"]);
            Assert.True(a_vector.HasValue);
            std.vector<int> v = new std.vector<int>(a_vector.Value);
            Assert.Empty(v);

            // std::list<int>
            std.any a_list = new std.any(locals["a_list"]);
            Assert.True(a_list.HasValue);
            std.list<int> l = new std.list<int>(a_list.Value);
            Assert.Empty(l);

            // std::map<int, int>
            std.any a_map = new std.any(locals["a_map"]);
            Assert.True(a_map.HasValue);
            std.map<int, int> m = new std.map<int, int>(a_map.Value);
            Assert.Empty(m);

            // BigTest
            std.any a_bigTest = new std.any(locals["a_bigTest"]);
            Assert.True(a_bigTest.HasValue);
            Assert.Equal("BigTest", a_bigTest.Value.GetCodeType().Name);
            Assert.Equal("meow meow meow", a_bigTest.Value.GetField("text").ToString());

            // std::pair<std::string, double>
            std.any a_pair = new std.any(locals["a_pair"]);
            Assert.True(a_pair.HasValue);
            std.pair<std::@string, double> p = new std.pair<std::@string, double>(a_pair.Value);
            Assert.Equal("meow", p.First.Text);
            Assert.Equal(3.14, p.Second);

            // std::shared_ptr<std::any>
            std.any a_shared = new std.any(locals["a_shared"]);
            Assert.True(a_shared.HasValue);
            std.shared_ptr<std.any> sp = new std.shared_ptr<std.any>(a_shared.Value);
            Assert.False(sp.IsEmpty);
            std.any inner = sp.Element;
            Assert.True(inner.HasValue);
            p = new std.pair<std::@string, double>(inner.Value);
            Assert.Equal("meow", p.First.Text);
            Assert.Equal(3.14, p.Second);

            // empty
            std.any a_empty = new std.any(locals["a_empty"]);
            Assert.False(a_empty.HasValue);
        }

        [Fact]
        public void StdAny_AutoCast()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestAny");
                VariableCollection locals = defaultTestCaseFrame.Locals;

                // std::string
                std.any a_string = new std.any(locals["a_string"]);
                Assert.True(a_string.HasValue);
                Assert.IsType<std.basic_string>(a_string.Value);

                // std::vector<int>
                std.any a_vector = new std.any(locals["a_vector"]);
                Assert.True(a_vector.HasValue);
                Assert.IsType<std.vector>(a_vector.Value);

                // std::list<int>
                std.any a_list = new std.any(locals["a_list"]);
                Assert.True(a_list.HasValue);
                Assert.IsType<std.list>(a_list.Value);

                // std::map<int, int>
                std.any a_map = new std.any(locals["a_map"]);
                Assert.True(a_map.HasValue);
                Assert.IsType<std.map>(a_map.Value);

                // std::pair<std::string, double>
                std.any a_pair = new std.any(locals["a_pair"]);
                Assert.True(a_pair.HasValue);
                Assert.IsType<std.pair>(a_pair.Value);
                std.pair p = (std.pair)a_pair.Value;
                Assert.IsType<std.basic_string>(p.First);

                // std::shared_ptr<std::any>
                std.any a_shared = new std.any(locals["a_shared"]);
                Assert.True(a_shared.HasValue);
                Assert.IsType<std.shared_ptr>(a_shared.Value);
                std.shared_ptr sp = (std.shared_ptr)a_shared.Value;
                Assert.IsType<std.any>(sp.Element);
                std.any inner = (std.any)sp.Element;
                Assert.IsType<std.pair>(inner.Value);
                p = (std.pair)a_pair.Value;
                Assert.IsType<std.basic_string>(p.First);
            });
        }

        [Fact]
        public void StdOptional()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestOptional");
            VariableCollection locals = defaultTestCaseFrame.Locals;

            // int
            std.optional<int> i = new std.optional<int>(locals["i"]);
            Assert.True(i.HasValue);
            Assert.Equal(5, i.Value);
            std.optional<int> emptyInt = new std.optional<int>(locals["emptyInt"]);
            Assert.False(emptyInt.HasValue);

            // bool
            std.optional<bool> bFalse = new std.optional<bool>(locals["bFalse"]);
            Assert.True(bFalse.HasValue);
            Assert.False(bFalse.Value);
            std.optional<bool> bTrue = new std.optional<bool>(locals["bTrue"]);
            Assert.True(bTrue.HasValue);
            Assert.True(bTrue.Value);
            std.optional<bool> bEmpty = new std.optional<bool>(locals["bEmpty"]);
            Assert.False(bEmpty.HasValue);
        }

        [Fact]
        public void StdOptional_AutoCast()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestOptional");
                VariableCollection locals = defaultTestCaseFrame.Locals;

                // int
                Assert.IsType<std.optional>(locals["i"]);
                std.optional i = (std.optional)locals["i"];
                Assert.True(i.HasValue);
                Assert.Equal("int", i.Value.GetCodeType().Name);
                Assert.Equal(5, (int)i.Value);

                Assert.IsType<std.optional>(locals["emptyInt"]);
                std.optional emptyInt = (std.optional)locals["emptyInt"];
                Assert.False(emptyInt.HasValue);

                // bool
                Assert.IsType<std.optional>(locals["bFalse"]);
                std.optional bFalse = (std.optional)locals["bFalse"];
                Assert.True(bFalse.HasValue);
                Assert.Equal("bool", bFalse.Value.GetCodeType().Name);
                Assert.False((bool)bFalse.Value);

                Assert.IsType<std.optional>(locals["bTrue"]);
                std.optional bTrue = (std.optional)locals["bTrue"];
                Assert.True(bTrue.HasValue);
                Assert.Equal("bool", bTrue.Value.GetCodeType().Name);
                Assert.True((bool)bTrue.Value);

                Assert.IsType<std.optional>(locals["bEmpty"]);
                std.optional bEmpty = (std.optional)locals["bEmpty"];
                Assert.False(bEmpty.HasValue);
            });
        }

        [Fact]
        public void StdOptional_Dynamic()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestOptional");
                VariableCollection locals = defaultTestCaseFrame.Locals;

                // int
                dynamic i = locals["i"];
                Assert.True(i.HasValue);
                Assert.Equal(5, (int)i.Value);

                dynamic emptyInt = locals["emptyInt"];
                Assert.False(emptyInt.HasValue);

                // bool
                dynamic bFalse = locals["bFalse"];
                Assert.True(bFalse.HasValue);
                Assert.False((bool)bFalse.Value);

                dynamic bTrue = locals["bTrue"];
                Assert.True(bTrue.HasValue);
                Assert.True((bool)bTrue.Value);

                dynamic bEmpty = locals["bEmpty"];
                Assert.False(bEmpty.HasValue);
            });
        }

        [Fact]
        public void StdFilesystemPath()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestPath");
            VariableCollection locals = defaultTestCaseFrame.Locals;

            char pathSeparator = (char)locals["pathSeparator"];
            std.filesystem.path root = new std.filesystem.path(locals["root"]);
            Assert.Equal("/my/test", root.Path);
            std.filesystem.path child = new std.filesystem.path(locals["child"]);
            Assert.Equal($"/my/test{pathSeparator}child", child.Path);
            std.filesystem.path wroot = new std.filesystem.path(locals["wroot"]);
            Assert.Equal("/my/test", wroot.Path);
            std.filesystem.path wchild = new std.filesystem.path(locals["wchild"]);
            Assert.Equal($"/my/test{pathSeparator}child", wchild.Path);
        }

        [Fact]
        public void StdFilesystemPath_AutoCast()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestPath");
                VariableCollection locals = defaultTestCaseFrame.Locals;
                char pathSeparator = (char)locals["pathSeparator"];

                Assert.IsType<std.filesystem.path>(locals["root"]);
                std.filesystem.path root = (std.filesystem.path)locals["root"];
                Assert.Equal("/my/test", root.Path);

                Assert.IsType<std.filesystem.path>(locals["child"]);
                std.filesystem.path child = (std.filesystem.path)locals["child"];
                Assert.Equal($"/my/test{pathSeparator}child", child.Path);

                Assert.IsType<std.filesystem.path>(locals["wroot"]);
                std.filesystem.path wroot = (std.filesystem.path)locals["wroot"];
                Assert.Equal("/my/test", wroot.Path);

                Assert.IsType<std.filesystem.path>(locals["wchild"]);
                std.filesystem.path wchild = (std.filesystem.path)locals["wchild"];
                Assert.Equal($"/my/test{pathSeparator}child", wchild.Path);
            });
        }

        [Fact]
        public void StdFilesystemPath_Dynamic()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestPath");
                VariableCollection locals = defaultTestCaseFrame.Locals;
                char pathSeparator = (char)locals["pathSeparator"];

                dynamic root = locals["root"];
                Assert.Equal("/my/test", root.Path);

                dynamic child = locals["child"];
                Assert.Equal($"/my/test{pathSeparator}child", child.Path);

                dynamic wroot = locals["wroot"];
                Assert.Equal("/my/test", wroot.Path);

                dynamic wchild = locals["wchild"];
                Assert.Equal($"/my/test{pathSeparator}child", wchild.Path);
            });
        }

        [Fact]
        public void BoolContainers()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestBoolContainers");
            VariableCollection locals = defaultTestCaseFrame.Locals;
            bool[] expectedArray = new bool[100];
            for (int i = 0, j = 1; i < expectedArray.Length; i += j, j++)
                for (int k = 0; k < j && i < expectedArray.Length; k++, i++)
                    expectedArray[i] = true;

            // C style array
            CodeArray<bool> carray = new CodeArray<bool>(locals["carray"]);
            Assert.Equal(expectedArray, carray);

            // std::array
            std.array<bool> array = new std.array<bool>(locals["array"]);
            Assert.Equal(expectedArray, array);

            // std::vector
            std.vector<bool> vector = new std.vector<bool>(locals["vector"]);
            Assert.True(expectedArray.Length < vector.Reserved);
            Assert.Equal(expectedArray, vector);
            Assert.Equal(expectedArray, vector.ToArray());
        }

        [Fact]
        public void BoolContainers_AutoCast()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestBoolContainers");
                VariableCollection locals = defaultTestCaseFrame.Locals;
                bool[] expectedArray = new bool[100];
                for (int i = 0, j = 1; i < expectedArray.Length; i += j, j++)
                    for (int k = 0; k < j && i < expectedArray.Length; k++, i++)
                        expectedArray[i] = true;

                // std::array
                std.array array = (std.array)locals["array"];
                Assert.Equal(expectedArray, array.Select(v => (bool)v));

                // std::vector
                std.vector vector = (std.vector)locals["vector"];
                Assert.True(expectedArray.Length < vector.Reserved);
                Assert.Equal(expectedArray, vector.Select(v => (bool)v));
                Assert.Equal(expectedArray, vector.ToArray().Select(v => (bool)v).ToArray());
            });
        }

        [Fact]
        public void StdVariant()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestVariant");
            VariableCollection locals = defaultTestCaseFrame.Locals;

            // int, float
            std.variant<int, float> if1 = new std.variant<int, float>(locals["if1"]);
            Assert.IsType<int>(if1.Value);
            Assert.Equal(42, if1.Get<int>());
            std.variant<int, float> if2 = new std.variant<int, float>(locals["if2"]);
            Assert.IsType<float>(if2.Value);
            Assert.Equal(42.0f, if2.Get<float>());

            // std::string, std::wstring
            std.variant<std.@string, std.wstring> s1 = new std.variant<std.@string, std.wstring>(locals["s1"]);
            Assert.IsType<std.@string>(s1.Value);
            Assert.Equal("ansiFoo", s1.Get<std.@string>().Text);
            std.variant<std.@string, std.wstring> s2 = new std.variant<std.@string, std.wstring>(locals["s2"]);
            Assert.IsType<std.wstring>(s2.Value);
            Assert.Equal("Foo", s2.Get<std.wstring>().Text);
        }

        [Fact]
        public void StdVariant_AutoCast()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestVariant");
                VariableCollection locals = defaultTestCaseFrame.Locals;

                // int, float
                std.variant if1 = (std.variant)locals["if1"];
                Assert.Equal("int", if1.Value.GetCodeType().Name);
                Assert.Equal(42, (int)if1.Value);
                std.variant if2 = (std.variant)locals["if2"];
                Assert.Equal("float", if2.Value.GetCodeType().Name);
                Assert.Equal(42.0f, (float)if2.Value);

                // std::string, std::wstring
                std.variant s1 = (std.variant)locals["s1"];
                Assert.Equal("ansiFoo", ((std.basic_string)s1.Value).Text);
                std.variant s2 = (std.variant)locals["s2"];
                Assert.Equal("Foo", ((std.basic_string)s2.Value).Text);
            });
        }

        [Fact]
        public void StdVariant_Dynamic()
        {
            Execute_AutoCast(() =>
            {
                StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestVariant");
                VariableCollection locals = defaultTestCaseFrame.Locals;

                // int, float
                dynamic if1 = locals["if1"];
                Assert.Equal(42, (int)if1.Value);
                dynamic if2 = locals["if2"];
                Assert.Equal(42.0f, (float)if2.Value);

                // std::string, std::wstring
                dynamic s1 = locals["s1"];
                Assert.Equal("ansiFoo", s1.Value.Text);
                dynamic s2 = locals["s2"];
                Assert.Equal("Foo", s2.Value.Text);
            });
        }
    }

    #region Test configurations
    [Collection("Cpp17.x64.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x64 : Cpp17Tests
    {
        public Cpp17Tests_x64(Cpp17Tests_x64_dmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("Cpp17.x64.Release.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x64_Release : Cpp17Tests
    {
        public Cpp17Tests_x64_Release(Cpp17Tests_x64_Release_dmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("Cpp17.x86.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x86 : Cpp17Tests
    {
        public Cpp17Tests_x86(Cpp17Tests_x86_dmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("Cpp17.x86.Release.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x86_Release : Cpp17Tests
    {
        public Cpp17Tests_x86_Release(Cpp17Tests_x86_Release_dmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("Cpp17.linux.x86.gcc.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x86_Linux_gcc : Cpp17Tests
    {
        public Cpp17Tests_x86_Linux_gcc(Cpp17Tests_linux_x86_gcc_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
        }
    }

    [Collection("Cpp17.linux.x64.gcc.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x64_Linux_gcc : Cpp17Tests
    {
        public Cpp17Tests_x64_Linux_gcc(Cpp17Tests_linux_x64_gcc_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
        }
    }

    [Collection("Cpp17.linux.x64.clang.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x64_Linux_clang : Cpp17Tests
    {
        public Cpp17Tests_x64_Linux_clang(Cpp17Tests_linux_x64_clang_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
        }
    }

    [Collection("Cpp17.linux.x86.gcc.nortti.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x86_Linux_gcc_nortti : Cpp17Tests
    {
        public Cpp17Tests_x86_Linux_gcc_nortti(Cpp17Tests_linux_x86_gcc_nortti_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
            NoRtti = true;
        }
    }

    [Collection("Cpp17.linux.x64.gcc.nortti.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x64_Linux_gcc_nortti : Cpp17Tests
    {
        public Cpp17Tests_x64_Linux_gcc_nortti(Cpp17Tests_linux_x64_gcc_nortti_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
            NoRtti = true;
        }
    }

    [Collection("Cpp17.linux.x64.clang.nortti.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class Cpp17Tests_x64_Linux_clang_nortti : Cpp17Tests
    {
        public Cpp17Tests_x64_Linux_clang_nortti(Cpp17Tests_linux_x64_clang_nortti_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
            NoRtti = true;
        }
    }
    #endregion
}
