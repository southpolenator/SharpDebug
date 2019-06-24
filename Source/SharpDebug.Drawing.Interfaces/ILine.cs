namespace SharpDebug.Drawing.Interfaces
{
    /// <summary>
    /// Interface that represents line as drawing object.
    /// </summary>
    public interface ILine : IDrawing
    {
        /// <summary>
        /// Gets the pen used to draw the line.
        /// </summary>
        IPen Pen { get; }

        /// <summary>
        /// Gets first point X coordinate.
        /// </summary>
        double X1 { get; }

        /// <summary>
        /// Gets first point Y coordinate.
        /// </summary>
        double Y1 { get; }

        /// <summary>
        /// Gets second point X coordinate.
        /// </summary>
        double X2 { get; }

        /// <summary>
        /// Gets second point Y coordinate.
        /// </summary>
        double Y2 { get; }
    }
}
