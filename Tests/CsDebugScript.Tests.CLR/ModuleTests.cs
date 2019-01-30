using System;
using System.Linq;
using Xunit;

namespace CsDebugScript.Tests.CLR
{
    public abstract class ModuleTests
    {
        [Fact]
        public void TestGetTypeByName()
        {
            var modules = Process.Current.Modules;
            Module types = modules.Where(m => m.Name == "Types").Single();

            Exception ex = Assert.Throws<Exception>(() => CodeType.Create("Foo", types));
            Assert.Equal("Type 'Foo' wasn't found", ex.Message);

            Module sharedLibrary = modules.Where(m => m.Name == "SharedLibrary").Single();
            Assert.NotNull(CodeType.Create("Foo", sharedLibrary));
            ex = Assert.Throws<Exception>(() => CodeType.Create("Foo2", sharedLibrary));
            Assert.Equal("Type 'Foo2' wasn't found", ex.Message);
        }
    }

    #region Test configurations
    [Collection("CLR Types")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class ModuleTestsWindows : ModuleTests
    {
    }

    [Collection("CLR Types Windows Core")]
    [Trait("x64", "true")]
    public class ModuleTestsWindowsCore : ModuleTests
    {
    }

#if ENABLE_LINUX_CLR_CORE_TESTS
    [Collection("CLR Types Linux Core")]
    [Trait("x64", "true")]
    public class ModuleTestsLinuxCore : ModuleTests
    {
    }
#endif
    #endregion
}
