using CsDebugScript.Drawing.Interfaces;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Implements circle as drawing object.
    /// </summary>
    internal class Circle : Ellipse, ICircle
    {
        /// <summary>
        /// Gets the circle radius.
        /// </summary>
        public double Radius => Width;

        /// <summary>
        /// Gets the center X coordinate.
        /// </summary>
        public double CenterX => Left + Radius;

        /// <summary>
        /// Gets the center Y coordinate.
        /// </summary>
        public double CenterY => Top + Radius;

        /// <summary>
        /// Initializes a new instance of the <see cref="Circle" /> class.
        /// </summary>
        /// <param name="pen">Pen used to draw the line.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        /// <param name="centerX">Circle center X coordinate.</param>
        /// <param name="centerY">Circle center Y coordinate.</param>
        /// <param name="radius">Circle radius.</param>
        public Circle(IPen pen, IBrush fillBrush, double centerX, double centerY, double radius)
            : base(pen, fillBrush, centerX - radius / 2, centerY - radius / 2, radius, radius, 0)
        {
        }
    }
}
