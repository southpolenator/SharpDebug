using System;
using System.Linq;
using Xunit;

namespace CsDebugScript.Tests.CLR
{
    [Collection("CLR Types")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class ModuleTests
    {
        [Fact]
        public void TestGetTypeByName()
        {
            Module types = Process.Current.Modules.Where(m => m.Name == "Types").Single();

            Assert.NotNull(CodeType.Create("Foo", types));
            try
            {
                CodeType.Create("Foo2", types);
                throw new Exception("This should never happen");
            }
            catch (Exception ex)
            {
                Assert.Equal("Type 'Foo2' wasn't found", ex.Message);
            }
        }
    }
}
