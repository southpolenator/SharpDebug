using SharpDebug.CLR;
using System.Linq;
using Xunit;
using ClrString = SharpDebug.CommonUserTypes.CLR.System.String;

namespace SharpDebug.Tests.CLR
{
    public abstract class TypeTests
    {
        [Fact]
        public void IntegerObjectClrType()
        {
            IClrHeap heap = Process.Current.ClrRuntimes.Single().Heap;
            Module typesModule = Module.All.Single(m => m.Name == "Types");
            Variable s_i = typesModule.GetVariable("Types.s_i");

            Assert.Equal(42, (int)s_i);
            Assert.Contains(s_i, heap.EnumerateObjects());
        }

        [Fact]
        public void FieldNameAndValueTests()
        {
            IClrHeap heap = Process.Current.ClrRuntimes.Single().Heap;
            Module sharedLibraryModule = Module.All.Single(m => m.Name == "SharedLibrary");
            Module typesModule = Module.All.Single(m => m.Name == "Types");
            CodeType fooType = CodeType.Create("Foo", sharedLibraryModule);
            Variable s_foo = typesModule.GetVariable("Types.s_foo");

            Assert.Equal(fooType, s_foo.GetCodeType());
            Assert.Equal(42, (int)s_foo.GetField("i"));
            Assert.Equal("string", new ClrString(s_foo.GetField("s")).Text);
            Assert.True((bool)s_foo.GetField("b"));
            Assert.Equal(4.2f, (float)s_foo.GetField("f"));
            Assert.Equal(8.4, (double)s_foo.GetField("d"));
            Assert.Contains(s_foo, heap.EnumerateObjects());
        }

        [Fact]
        public void ArrayTests()
        {
            Module typesModule = Module.All.Single(m => m.Name == "Types");
            CodeType fooType = CodeType.Create("Types", typesModule);
            Variable s_array = fooType.GetStaticField("s_array");
            Variable s_one = fooType.GetStaticField("s_one");
            Variable s_two = fooType.GetStaticField("s_two");
            Variable s_three = fooType.GetStaticField("s_three");

            CodeArray<Variable> codeArray = new CodeArray<Variable>(s_array);
            Variable[] expected = new Variable[] { s_one, s_two, s_three };

            Assert.Equal(expected.Length, codeArray.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], codeArray[i]);
            }
        }
    }

    #region Test configurations
    [Collection("CLR Types")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class TypeTestsWindows : TypeTests
    {
    }

#if ENABLE_CLR_CORE_TESTS
    [Collection("CLR Types Windows Core")]
    [Trait("x64", "true")]
    public class TypeTestsWindowsCore : TypeTests
    {
    }

    [Collection("CLR Types Linux Core")]
    [Trait("x64", "true")]
    public class TypeTestsLinuxCore : TypeTests
    {
    }
#endif
    #endregion
}
