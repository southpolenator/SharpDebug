using CsDebugScript.Drawing.Interfaces;
using System;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Implements ellipse as drawing object.
    /// </summary>
    internal class Ellipse : Drawing, IEllipse
    {
        /// <summary>
        /// Gets the pen used to draw the circle.
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
        /// Gets the ellipse rectangle width.
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Gets the ellipse rectangle height.
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// UI object used to represent ellipse.
        /// </summary>
        public System.Windows.Shapes.Ellipse UIEllipse { get; private set; }

        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        public override object UIObject => UIEllipse;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ellipse" /> class.
        /// </summary>
        /// <param name="pen">Pen used to draw the line.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Ellipse rectangle width.</param>
        /// <param name="height">Ellipse rectangle height.</param>
        /// <param name="rotation">Ellipse clockwise rotation in radians.</param>
        public Ellipse(IPen pen, IBrush fillBrush, double left, double top, double width, double height, double rotation)
        {
            Pen = pen;
            FillBrush = fillBrush;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            UIEllipse = new System.Windows.Shapes.Ellipse()
            {
                Width = width,
                Height = height,
                Fill = (fillBrush as Brush)?.UIBrush,
            };
            if (rotation != 0)
            {
                UIEllipse.RenderTransform = new System.Windows.Media.RotateTransform(rotation * 180 / Math.PI);
                UIEllipse.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            }
            System.Windows.Controls.Canvas.SetLeft(UIEllipse, left);
            System.Windows.Controls.Canvas.SetTop(UIEllipse, top);
            UIEllipse.SetPen(pen);
        }
    }
}
