namespace SharpDebug.Drawing.Interfaces
{
    /// <summary>
    /// Interface that tells visualizer that this object can be visualized as a drawing.
    /// </summary>
    public interface IDrawingVisualizerObject
    {
        /// <summary>
        /// Cheks if data is correct and object can be visualized as a drawing.
        /// </summary>
        /// <returns><c>true</c> if data is correct and object can be visualized as a drawing.</returns>
        bool CanVisualize();

        /// <summary>
        /// Creates drawing that should be visualized.
        /// </summary>
        /// <param name="graphics">Graphics object used to create drawings.</param>
        /// <returns>Drawing object that should be visualized.</returns>
        IDrawing CreateDrawing(IGraphics graphics);
    }
}
