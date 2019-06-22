using CsDebugScript.Drawing.Interfaces;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Implements the font object.
    /// </summary>
    internal class Font : IFont
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Font" /> class.
        /// </summary>
        /// <param name="family">The font family.</param>
        /// <param name="size">The font size.</param>
        /// <param name="style">The font style.</param>
        /// <param name="weight">The font weight.</param>
        public Font(string family, double size, FontStyle style, FontWeight weight)
        {
            Family = family;
            Size = size;
            Style = style;
            Weight = weight;
        }

        /// <summary>
        /// Gets the font family.
        /// </summary>
        public string Family { get; private set; }

        /// <summary>
        /// Gets the font size.
        /// </summary>
        public double Size { get; private set; }

        /// <summary>
        /// Gets the font style.
        /// </summary>
        public FontStyle Style { get; private set; }

        /// <summary>
        /// Gets the font weight.
        /// </summary>
        public FontWeight Weight { get; private set; }
    }
}
