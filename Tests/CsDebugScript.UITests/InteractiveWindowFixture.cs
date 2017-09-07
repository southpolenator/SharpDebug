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
            NativeDumpTest_x64_dmp_Initialization initialization = new NativeDumpTest_x64_dmp_Initialization();
            Application = Application.Launch(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = TestBase.GetAbsoluteBinPath("CsDebugScript.UI.App.exe"),
                Arguments = $"-d \"{initialization.DumpPath}\" -p \"{initialization.SymbolPath}\"",
            });
            InteractiveWindow = new InteractiveWindowWrapper(Application.GetWindow(UI.InteractiveWindow.WindowTitle, InitializeOption.NoCache));
        }

        public Application Application { get; private set; }

        public InteractiveWindowWrapper InteractiveWindow { get; private set; }

        public void Dispose()
        {
            InteractiveWindow.MainWindow?.Close();
            Application.Kill();
        }
    }
}
