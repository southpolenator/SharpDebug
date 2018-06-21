namespace CsDebugScript.Drawing.Interfaces
{
    /// <summary>
    /// Channel type used when creating bitmap.
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// Transparency channel. See https://en.wikipedia.org/wiki/Alpha_compositing.
        /// </summary>
        Alpha,

        /// <summary>
        /// Red color channel. See https://en.wikipedia.org/wiki/RGB_color_model.
        /// </summary>
        Red,

        /// <summary>
        /// Green color channel. See https://en.wikipedia.org/wiki/RGB_color_model.
        /// </summary>
        Green,

        /// <summary>
        /// Blue color channel. See https://en.wikipedia.org/wiki/RGB_color_model.
        /// </summary>
        Blue,

        /// <summary>
        /// Cyan color channel. See https://en.wikipedia.org/wiki/CMYK_color_model.
        /// </summary>
        Cyan,

        /// <summary>
        /// Magenta color channel. See https://en.wikipedia.org/wiki/CMYK_color_model.
        /// </summary>
        Magenta,

        /// <summary>
        /// Yellow color channel. See https://en.wikipedia.org/wiki/CMYK_color_model.
        /// </summary>
        Yellow,

        /// <summary>
        /// Black color channel. See https://en.wikipedia.org/wiki/CMYK_color_model.
        /// </summary>
        Key,

        /// <summary>
        /// Grayscale channel, intensity maps to white present in black-white image. See https://en.wikipedia.org/wiki/Grayscale.
        /// </summary>
        Grayscale,
    }

    /// <summary>
    /// Predefined arrays of channels.
    /// </summary>
    public static class Channels
    {
        /// <summary>
        /// Red-Green-Blue. See https://en.wikipedia.org/wiki/RGB_color_model.
        /// </summary>
        public static readonly ChannelType[] RGB = new[] { ChannelType.Red, ChannelType.Green, ChannelType.Blue };

        /// <summary>
        /// Red-Green-Blue-Alpha. See https://en.wikipedia.org/wiki/RGBA_color_space.
        /// </summary>
        public static readonly ChannelType[] RGBA = new[] { ChannelType.Red, ChannelType.Green, ChannelType.Blue, ChannelType.Alpha };

        /// <summary>
        /// Blue-Green-Red. See https://en.wikipedia.org/wiki/RGB_color_model.
        /// </summary>
        public static readonly ChannelType[] BGR = new[] { ChannelType.Blue, ChannelType.Green, ChannelType.Red };

        /// <summary>
        /// Blue-Green-Red-Alpha. See https://en.wikipedia.org/wiki/RGBA_color_space.
        /// </summary>
        public static readonly ChannelType[] BGRA = new[] { ChannelType.Blue, ChannelType.Green, ChannelType.Red, ChannelType.Alpha };

        /// <summary>
        /// Alpha-Blue-Green-Red. See https://en.wikipedia.org/wiki/RGBA_color_space.
        /// </summary>
        public static readonly ChannelType[] ABGR = new[] { ChannelType.Alpha, ChannelType.Blue, ChannelType.Green, ChannelType.Red };

        /// <summary>
        /// Gray shades from black to white. See https://en.wikipedia.org/wiki/Grayscale.
        /// </summary>
        public static readonly ChannelType[] Grayscale = new[] { ChannelType.Grayscale };

        /// <summary>
        /// Cyan-Magenta-Yellow-Key. See https://en.wikipedia.org/wiki/CMYK_color_model.
        /// </summary>
        public static readonly ChannelType[] CMYK = new[] { ChannelType.Cyan, ChannelType.Magenta, ChannelType.Yellow, ChannelType.Key };
    }

    /// <summary>
    /// Interface that defines object that can create drawing objects.
    /// </summary>
    public interface IGraphics
    {
        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        IBitmap CreateBitmap(int width, int height, ChannelType[] channels, byte[] pixels);

        /// <summary>
        /// Creates bitmap from parameters. Pixels are sbytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        IBitmap CreateBitmap(int width, int height, ChannelType[] channels, sbyte[] pixels);

        /// <summary>
        /// Creates bitmap from parameters. Pixels are ushorts.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        IBitmap CreateBitmap(int width, int height, ChannelType[] channels, ushort[] pixels);

        /// <summary>
        /// Creates bitmap from parameters. Pixels are shorts.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        IBitmap CreateBitmap(int width, int height, ChannelType[] channels, short[] pixels);

        /// <summary>
        /// Creates bitmap from parameters. Pixels are ints.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        IBitmap CreateBitmap(int width, int height, ChannelType[] channels, int[] pixels);

        /// <summary>
        /// Creates bitmap from parameters. Pixels are floats.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        IBitmap CreateBitmap(int width, int height, ChannelType[] channels, float[] pixels);

        /// <summary>
        /// Creates bitmap from parameters. Pixels are doubles.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        IBitmap CreateBitmap(int width, int height, ChannelType[] channels, double[] pixels);
    }
}
