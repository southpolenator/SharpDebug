namespace SharpDebug.Drawing.Interfaces
{
    /// <summary>
    /// Enumeration that represents style of the font.
    /// </summary>
    public enum FontStyle
    {
        /// <summary>
        /// A normal font.
        /// </summary>
        Normal,

        /// <summary>
        /// An oblique font.
        /// </summary>
        Oblique,

        /// <summary>
        /// An italic font.
        /// </summary>
        Italic,
    }

    /// <summary>
    /// Enumeration that represents weight of the font.
    /// </summary>
    public enum FontWeight
    {
        /// <summary>
        /// Specifies a "black" font weight.
        /// </summary>
        Black = 900,

        /// <summary>
        /// Specifies a "bold" font weight.
        /// </summary>
        Bold = 700,

        /// <summary>
        /// Specifies a "demi-bold" font weight.
        /// </summary>
        DemiBold = 600,

        /// <summary>
        /// Specifies an "extra black" font weight.
        /// </summary>
        ExtraBlack = 950,

        /// <summary>
        /// Specifies an "extra bold" font weight.
        /// </summary>
        ExtraBold = 800,

        /// <summary>
        /// Specifies an "extra light" font weight.
        /// </summary>
        ExtraLight = 200,

        /// <summary>
        /// Specifies a "heavy" font weight.
        /// </summary>
        Heavy = 900,

        /// <summary>
        /// Specifies a "light" font weight.
        /// </summary>
        Light = 300,

        /// <summary>
        /// Specifies a "medium" font weight.
        /// </summary>
        Medium = 500,

        /// <summary>
        /// Specifies a "normal" font weight.
        /// </summary>
        Normal = 400,

        /// <summary>
        /// Specifies a "regular" font weight.
        /// </summary>
        Regular = 400,

        /// <summary>
        /// Specifies a "semi-bold" font weight.
        /// </summary>
        SemiBold = 600,

        /// <summary>
        /// Specifies a "thin" font weight.
        /// </summary>
        Thin = 100,

        /// <summary>
        /// Specifies an "ultra black" font weight.
        /// </summary>
        UltraBlack = 950,

        /// <summary>
        /// Specifies an "ultra bold" font weight.
        /// </summary>
        UltraBold = 800,

        /// <summary>
        /// Specifies an "ultra light" font weight.
        /// </summary>
        UltraLight = 200,
    }

    /// <summary>
    /// Interface that represents the font object.
    /// </summary>
    public interface IFont
    {
        /// <summary>
        /// Gets the font family.
        /// </summary>
        string Family { get; }

        /// <summary>
        /// Gets the font size.
        /// </summary>
        double Size { get; }

        /// <summary>
        /// Gets the font style.
        /// </summary>
        FontStyle Style { get; }

        /// <summary>
        /// Gets the font weight.
        /// </summary>
        FontWeight Weight { get; }
    }
}
