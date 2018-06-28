namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Interface for rectangle as drawing object.
    /// </summary>
    public interface IRectangle : IDrawing
    {
        /// <summary>
        /// Gets the pen used to draw edges.
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
        /// Gets the rectangle width.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Gets the rectangle height.
        /// </summary>
        double Height { get; }
    }
}
