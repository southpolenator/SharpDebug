namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Bitmap object as drawing.
    /// </summary>
    public interface IBitmap : IDrawing
    {
        /// <summary>
        /// Bitmap width.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Bitmap height.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Number of channels.
        /// </summary>
        int ChannelsCount { get; }
    }
}
