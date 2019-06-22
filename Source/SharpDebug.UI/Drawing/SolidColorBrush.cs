using SharpDebug.Drawing.Interfaces;

namespace SharpDebug.UI.Drawing
{
    /// <summary>
    /// Implements solid color brush object.
    /// </summary>
    internal class SolidColorBrush : Brush, ISolidColorBrush
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolidColorBrush" /> class.
        /// </summary>
        /// <param name="color">Brush solid color.</param>
        /// <param name="opacity">Brush opacity.</param>
        public SolidColorBrush(Color color, double opacity)
            : base(new System.Windows.Media.SolidColorBrush(color.ToWpfColor()), opacity)
        {
            Color = color;
        }

        /// <summary>
        /// Gets the brush solid color.
        /// </summary>
        public Color Color { get; private set; }
    }
}
