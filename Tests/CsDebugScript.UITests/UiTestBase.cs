using Xunit;

namespace CsDebugScript.UITests
{
    public class UiTestBase : IClassFixture<InteractiveWindowFixture>
    {
        public UiTestBase(InteractiveWindowFixture interactiveWindowFixture)
        {
            Window = interactiveWindowFixture.InteractiveWindow;
        }

        public InteractiveWindowWrapper Window { get; private set; }
    }
}
