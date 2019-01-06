namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Interface that represents ellipse as drawing object.
    /// </summary>
    public interface IEllipse : IDrawing
    {
        /// <summary>
        /// Gets the pen used to draw the circle.
        /// </summary>
        IPen Pen { get; }

        /// <summary>
        /// Gets the brush used to fill the content.
        /// </summary>
        IBrush FillBrush { get; }

        /// <summary>
        /// Gets the left coordinate of top left corner.
        /// </summary>
        double Left { get; }

        /// <summary>
        /// Gets the top coordinate of top left corner.
        /// </summary>
        double Top { get; }

        /// <summary>
        /// Gets the ellipse rectangle width.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Gets the ellipse rectangle height.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Gets the ellipse clockwise rotation in radians.
        /// </summary>
        double Rotation { get; }
    }
}
