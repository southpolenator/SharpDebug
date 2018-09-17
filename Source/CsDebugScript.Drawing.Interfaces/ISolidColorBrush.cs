namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Interface that represents solid color brush object.
    /// </summary>
    public interface ISolidColorBrush : IBrush
    {
        /// <summary>
        /// Gets the brush solid color.
        /// </summary>
        Color Color { get; }
    }
}
