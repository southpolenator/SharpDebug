using CsDebugScript.UI;
using System.Windows.Controls;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Interaction logic for VSInteractiveWindowControl.
    /// </summary>
    public partial class VSInteractiveWindowControl : UserControl
    {
        private InteractiveWindowContent contentControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSInteractiveWindowControl"/> class.
        /// </summary>
        public VSInteractiveWindowControl()
        {
            this.InitializeComponent();

            Grid grid = new Grid();
            contentControl = new InteractiveWindowContent();
            grid.Children.Add(contentControl);
            this.Content = grid;

            MakeEnabled(VSContext.CurrentDebugMode == EnvDTE.dbgDebugMode.dbgBreakMode);
            VSContext.DebuggerEnteredBreakMode += () => MakeEnabled(true);
            VSContext.DebuggerEnteredDesignMode += () => MakeEnabled(false);
            VSContext.DebuggerEnteredRunMode += () => MakeEnabled(false);
        }

        private void MakeEnabled(bool enabled)
        {
            contentControl.IsEnabled = enabled;
        }
    }
}
