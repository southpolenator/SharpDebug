using DbgEngManaged;
using System.Text;

namespace CsScriptManaged
{
    public class DebuggerOutputSaver : IDebugOutputCallbacksWide
    {
        private StringBuilder sb = new StringBuilder();

        public string Text
        {
            get
            {
                return sb.ToString();
            }
        }

        public void Output(uint Mask, string Text)
        {
            sb.Append(Text);
        }
    }
}
