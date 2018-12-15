using std = CsDebugScript.CommonUserTypes.NativeTypes.std;
using Xunit;

namespace CsDebugScript.Tests
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
