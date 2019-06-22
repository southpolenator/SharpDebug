namespace SharpDebug.Drawing.Interfaces
{
    /// <summary>
    /// Defines how text is horizontally aligned.
    /// </summary>
    public enum TextHorizontalAlignment
    {
        /// <summary>
        /// The text is left-aligned.
        /// </summary>
        Left,

        /// <summary>
        /// The text is centered.
        /// </summary>
        Center,

        /// <summary>
        /// The text is right-aligned.
        /// </summary>
        Right,
    }

    /// <summary>
    /// Defines how text is vertically aligned.
    /// </summary>
    public enum TextVerticalAlignment
    {
        /// <summary>
        /// The text is top-aligned.
        /// </summary>
        Top = 1,

        /// <summary>
        /// The text is centered.
        /// </summary>
        Center,

        /// <summary>
        /// The text is bottom-aligned.
        /// </summary>
        Bottom,
    }

    /// <summary>
    /// Controls the wrapping mode of text.
    /// </summary>
    public enum TextWrapping
    {
        /// <summary>
        /// Text should not wrap.
        /// </summary>
        NoWrap,

        /// <summary>
        /// Text can wrap.
        /// </summary>
        Wrap,
    }

    /// <summary>
    /// Interface that represents text as drawing object.
    /// </summary>
    public interface IText : IDrawing
    {
        /// <summary>
        /// Gets the font used to draw the text.
        /// </summary>
        IFont Font { get; }

        /// <summary>
        /// Gets a brush used to paint the text's background.
        /// </summary>
        IBrush Background { get; }

        /// <summary>
        /// Gets a brush used to paint the text.
        /// </summary>
        IBrush Foreground { get; }

        /// <summary>
        /// Gets the label.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the text horizontal alignment.
        /// </summary>
        TextHorizontalAlignment HorizontalAlignment { get; }

        /// <summary>
        /// Gets the text vertical alignment.
        /// </summary>
        TextVerticalAlignment VerticalAlignment { get; }

        /// <summary>
        /// Gets the text wrapping.
        /// </summary>
        TextWrapping Wrapping { get; }

        /// <summary>
        /// Gets the left position of the text.
        /// </summary>
        double Left { get; }

        /// <summary>
        /// Gets the top position of the text.
        /// </summary>
        double Top { get; }

        /// <summary>
        /// Gets the virtual text box width.
        /// If text is smaller than the box width, <see cref="HorizontalAlignment"/> will be used; if it is bigger, <see cref="Wrapping"/> will be used.
        /// If value is less than 0, it will be automatically computed.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Gets the virtual text box height.
        /// If text is smaller that the box height, <see cref="VerticalAlignment"/> will be used.
        /// If value is less than 0, it will be automatically computed.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Gets the text clockwise rotation in radians.
        /// </summary>
        double Rotation { get; }
    }
}
