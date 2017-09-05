using System;
using TestStack.White;
using TestStack.White.Factory;

namespace CsDebugScript.UITests
{
    public class InteractiveWindowFixture : IDisposable
    {
        public InteractiveWindowFixture()
        {
            UI.InteractiveWindow.ShowWindow();
            Application = Application.Attach(System.Diagnostics.Process.GetCurrentProcess());
            InteractiveWindow = new InteractiveWindowWrapper(Application.GetWindow(UI.InteractiveWindow.WindowTitle, InitializeOption.NoCache));
        }

        public Application Application { get; private set; }

        public InteractiveWindowWrapper InteractiveWindow { get; private set; }

        public void Dispose()
        {
            InteractiveWindow.MainWindow?.Close();
        }
    }
}
