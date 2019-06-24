namespace SharpDebug.Drawing.Interfaces
{
    /// <summary>
    /// Drawing object that should be visualized.
    /// </summary>
    public interface IDrawing
    {
        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        object UIObject { get; }

        /// <summary>
        /// Canvas object that contains this object or <c>null</c> if it is standalone.
        /// </summary>
        ICanvas Container { get; }
    }
}
