using SharpDebug.Drawing.Interfaces;

namespace SharpDebug.UI.Drawing
{
    /// <summary>
    /// Implements brush object.
    /// </summary>
    internal class Brush : IBrush
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Brush" /> class.
        /// </summary>
        /// <param name="uiBrush">UI object used to represent brush.</param>
        /// <param name="opacity">Brush opacity.</param>
        public Brush(System.Windows.Media.Brush uiBrush, double opacity)
        {
            Opacity = opacity;
            UIBrush = uiBrush;
            uiBrush.Opacity = opacity;
        }

        /// <summary>
        /// Gets the opacity of the brush.
        /// </summary>
        public double Opacity { get; private set; }

        /// <summary>
        /// UI object used to represent brush.
        /// </summary>
        public System.Windows.Media.Brush UIBrush { get; private set; }
    }
}
