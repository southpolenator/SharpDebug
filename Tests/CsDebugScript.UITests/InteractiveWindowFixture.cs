using CsDebugScript.Tests;
using System;
using TestStack.White;
using TestStack.White.Factory;
using Xunit;

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

    public class DumpInteractiveWindowFixture : InteractiveWindowFixture
    {
        private NativeDumpTest_x64_dmp_Initialization dumpInitialization;

        public DumpInteractiveWindowFixture()
        {
            dumpInitialization = new NativeDumpTest_x64_dmp_Initialization();
        }
    }

    [CollectionDefinition("UI with dump")]
    public class UiWithDumpCollectionFixture : ICollectionFixture<DumpInteractiveWindowFixture>
    {
    }
}
