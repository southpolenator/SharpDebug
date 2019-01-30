using System;
using TestStack.White;
using TestStack.White.Factory;

namespace CsDebugScript.Tests.UI
{
    public class InteractiveWindowFixture : IDisposable
    {
        public InteractiveWindowFixture()
        {
            DumpInitialization initialization = new DumpInitialization("NativeDumpTest.x64.mdmp", "", FixSymbolPath(DumpInitialization.DefaultDumpPath));
            Application = Application.Launch(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = TestBase.GetAbsoluteBinPath("CsDebugScript.UI.App.exe"),
                Arguments = $"-d \"{initialization.DumpPath}\" -p \"{initialization.SymbolPath}\"",
            });
            InteractiveWindow = new InteractiveWindowWrapper(Application.GetWindow(CsDebugScript.UI.InteractiveWindow.WindowTitle, InitializeOption.NoCache));
        }

        public Application Application { get; private set; }

        public InteractiveWindowWrapper InteractiveWindow { get; private set; }

        public void Dispose()
        {
            InteractiveWindow.MainWindow?.Close();
            Application.Kill();
        }

        private static string FixSymbolPath(string symbolPath, bool addSymbolServer = true)
        {
            symbolPath = TestBase.GetAbsoluteBinPath(symbolPath);
            if (addSymbolServer)
            {
                symbolPath += ";srv*";
            }
            return symbolPath;
        }
    }
}
