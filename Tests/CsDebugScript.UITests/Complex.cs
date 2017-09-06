using Xunit;

namespace CsDebugScript.UITests
{
    [Collection("UI with dump")]
    [Trait("Run", "x64")]
    public class Complex : UiTestBase
    {
        public Complex(DumpInteractiveWindowFixture interactiveWindowFixture)
            : base(interactiveWindowFixture)
        {
        }

        [Fact]
        public void ExecuteDebuggerCommand()
        {
            Window.ExecuteCommand("#dbg k");
            Window.VerifyOutput("#dbg k");
        }

        [Fact]
        public void MultiLineCode()
        {
            Window.ExecuteCommand("for (int i = 0; i < 1; i++){Return}Console.WriteLine(i);");
            Window.VerifyOutput(@"for (int i = 0; i < 1; i++)
    Console.WriteLine(i);");
        }

        [Fact]
        public void SimpleEndToEnd()
        {
            Window.ExecuteCommand("var a = new [] {{} 1, 2, 3, 4, 5, 6, 7, {Shift}{}};");
            Window.VerifyOutput("var a = new [] { 1, 2, 3, 4, 5, 6, 7, };");

            Window.ExecuteCommand("writeln(a.Len{Return});");
            Window.VerifyOutput("writeln(a.Length);");

            Window.ExecuteCommand("a{Return}");
            // TODO: Verify that output is correct
        }
    }
}
