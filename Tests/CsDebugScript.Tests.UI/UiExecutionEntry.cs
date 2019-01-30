using System.Text;
using TestStack.White.UIItems;

namespace CsDebugScript.Tests.UI
{
    public class UiExecutionEntry
    {
        public string Code { get; set; }

        public string Prompt { get; set; }

        public IUIItem[] Result { get; set; }

        public string ResultText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                foreach (IUIItem item in Result)
                {
                    if (item is WPFLabel label)
                    {
                        sb.AppendLine(label.Text);
                    }
                    else if (item is TextBox textBox)
                    {
                        sb.AppendLine(textBox.Text);
                    }
                }

                return sb.ToString();
            }
        }
    }
}
