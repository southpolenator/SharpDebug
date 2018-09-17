using CsDebugScript.Drawing.Interfaces;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Implements rectangle as drawing object.
    /// </summary>
    internal class Rectangle : Drawing, IRectangle
    {
        /// <summary>
        /// Gets the pen used to draw edges.
        /// </summary>
        public IPen Pen { get; private set; }

        /// <summary>
        /// Gets the brush used to fill the content.
        /// </summary>
        public IBrush FillBrush { get; private set; }

        /// <summary>
        /// Gets the left coordinate of top left corner.
        /// </summary>
        public double Left { get; private set; }

        /// <summary>
        /// Gets the top coordinate of top left corner.
        /// </summary>
        public double Top { get; private set; }

        /// <summary>
        /// Gets the rectangle width.
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Gets the rectangle height.
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// UI object used to represent rectangle.
        /// </summary>
        public System.Windows.Shapes.Rectangle UIRectangle { get; private set; }

        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        public override object UIObject => UIRectangle;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle" /> class.
        /// </summary>
        /// <param name="pen">Pen used to draw edges.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Rectangle width.</param>
        /// <param name="height">Rectangle height.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        public Rectangle(IPen pen, IBrush fillBrush, double left, double top, double width, double height)
        {
            Pen = pen;
            FillBrush = fillBrush;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            UIRectangle = new System.Windows.Shapes.Rectangle()
            {
                Width = width,
                Height = height,
                Fill = (fillBrush as Brush)?.UIBrush,
            };
            System.Windows.Controls.Canvas.SetLeft(UIRectangle, left);
            System.Windows.Controls.Canvas.SetTop(UIRectangle, top);
            UIRectangle.SetPen(pen);
        }
    }
}
