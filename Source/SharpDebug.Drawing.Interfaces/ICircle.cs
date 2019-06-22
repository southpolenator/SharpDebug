namespace SharpDebug.Drawing.Interfaces
{
    /// <summary>
    /// Interface that reprensents circle as drawing object.
    /// </summary>
    public interface ICircle : IEllipse
    {
        /// <summary>
        /// Gets the circle radius.
        /// </summary>
        double Radius { get; }

        /// <summary>
        /// Gets the center X coordinate.
        /// </summary>
        double CenterX { get; }

        /// <summary>
        /// Gets the center Y coordinate.
        /// </summary>
        double CenterY { get; }
    }
}
