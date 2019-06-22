using CsDebugScript.Drawing.Interfaces;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Drawing object that should be visualized.
    /// </summary>
    internal abstract class Drawing : IDrawing
    {
        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        public abstract object UIObject { get; }

        /// <summary>
        /// Canvas object that contains this object or <c>null</c> if it is standalone.
        /// </summary>
        public ICanvas Container { get; internal set; }
    }
}
