namespace CsDebugScript.Drawing.Interfaces
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
    }
}
