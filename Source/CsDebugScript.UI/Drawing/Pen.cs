using CsDebugScript.Drawing.Interfaces;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Implements the pen object.
    /// </summary>
    internal class Pen : IPen
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pen" /> class.
        /// </summary>
        /// <param name="brush">Brush object.</param>
        /// <param name="thickness">Pen thickness.</param>
        public Pen(IBrush brush, double thickness)
        {
            System.Windows.Media.Brush uiBrush = (brush as SolidColorBrush)?.UIBrush;
            UIPen = new System.Windows.Media.Pen(uiBrush, thickness);
            Brush = brush;
            Thickness = thickness;
        }

        /// <summary>
        /// Gets the brush used to draw the stroke.
        /// </summary>
        public IBrush Brush { get; private set; }

        /// <summary>
        /// Gets the stroke thickness.
        /// </summary>
        public double Thickness { get; private set; }

        /// <summary>
        /// UI object used to represent pen.
        /// </summary>
        public System.Windows.Media.Pen UIPen { get; private set; }
    }
}
