using DbgEngManaged;
using System.Text;

namespace CsScriptManaged
{
    public class DebuggerOutputSaver : IDebugOutputCallbacksWide
    {
        private StringBuilder sb = new StringBuilder();

        public DebuggerOutputSaver(DebugOutput captureFlags)
        {
            CaptureFlags = captureFlags;
        }

        public DebugOutput CaptureFlags { get; set; }

        public string Text
        {
            get
            {
                return sb.ToString();
            }
        }

        public void Output(uint mask, string text)
        {
            if ((mask & (uint)CaptureFlags) != 0)
            {
                sb.Append(text);
            }
        }
    }
}
