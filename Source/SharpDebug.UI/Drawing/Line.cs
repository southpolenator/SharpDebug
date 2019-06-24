using SharpDebug.Drawing.Interfaces;

namespace SharpDebug.UI.Drawing
{
    /// <summary>
    /// Implements line as drawing object.
    /// </summary>
    internal class Line : Drawing, ILine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Line" /> class.
        /// </summary>
        /// <param name="pen">Pen that should be used to draw the line.</param>
        /// <param name="x1">First point X coordinate.</param>
        /// <param name="y1">First point Y coordinate.</param>
        /// <param name="x2">Second point X coordinate.</param>
        /// <param name="y2">Second point Y coordinate.</param>
        public Line(IPen pen, double x1, double y1, double x2, double y2)
        {
            Pen = pen;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            UILine = new System.Windows.Shapes.Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
            };
            UILine.SetPen(pen);
        }

        /// <summary>
        /// Gets the pen used to draw the line.
        /// </summary>
        public IPen Pen { get; private set; }

        /// <summary>
        /// Gets first point X coordinate.
        /// </summary>
        public double X1 { get; private set; }

        /// <summary>
        /// Gets first point Y coordinate.
        /// </summary>
        public double Y1 { get; private set; }

        /// <summary>
        /// Gets second point X coordinate.
        /// </summary>
        public double X2 { get; private set; }

        /// <summary>
        /// Gets second point Y coordinate.
        /// </summary>
        public double Y2 { get; private set; }

        /// <summary>
        /// UI object used to represent line.
        /// </summary>
        public System.Windows.Shapes.Line UILine { get; private set; }

        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        public override object UIObject => UILine;
    }
}
