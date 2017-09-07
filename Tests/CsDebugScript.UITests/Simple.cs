using Xunit;

namespace CsDebugScript.UITests
{
    [Trait("x64", "true")]
    public class Simple : UiTestBase, IClassFixture<InteractiveWindowFixture>
    {
        public Simple(InteractiveWindowFixture interactiveWindow)
            : base(interactiveWindow)
        {
        }

        [Fact]
        public void ConsoleWriteLine()
        {
            Window.ExecuteCommand("Console.WriteLine(42);");
            Assert.Equal("42", Window.LastExecution.ResultText.Trim());
        }

        [Fact]
        public void ExpressionInterpret()
        {
            Window.ExecuteCommand("3+1");
            Assert.Equal("4", Window.LastExecution.ResultText.Trim());
        }

        [Fact]
        public void VariableSaving()
        {
            Window.ExecuteCommand("var a = 3+1;");
            Window.ExecuteCommand("a{Return}");
            Assert.Equal("4", Window.LastExecution.ResultText.Trim());
        }
    }
}
