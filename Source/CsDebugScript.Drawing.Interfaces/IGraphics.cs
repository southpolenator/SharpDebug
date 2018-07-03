using System.Collections.Generic;

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
        /// Gray shades from black to white. See https://en.wikipedia.org/wiki/Grayscale.
        /// </summary>
        public static readonly ChannelType[] Grayscale = new[] { ChannelType.Grayscale };

        /// <summary>
        /// Cyan-Magenta-Yellow-Key. See https://en.wikipedia.org/wiki/CMYK_color_model.
        /// </summary>
        public static readonly ChannelType[] CMYK = new[] { ChannelType.Cyan, ChannelType.Magenta, ChannelType.Yellow, ChannelType.Key };

        /// <summary>
        /// Checks whether two channel arrays are the same.
        /// </summary>
        /// <param name="channels1">First array of channels</param>
        /// <param name="channels2">Second array of channels</param>
        /// <returns><c>true</c> if two channel arrays are the same; <c>false</c> otherwise.</returns>
        public static bool AreSame(ChannelType[] channels1, ChannelType[] channels2)
        {
            if (channels1.Length != channels2.Length)
            {
                return false;
            }

            for (int i = 0; i < channels1.Length; i++)
            {
                if (channels1[i] != channels2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Interface that defines object that can create drawing objects.
    /// </summary>
    public interface IGraphics
    {
        /// <summary>
        /// Creates canvas object that contains the specified list of drawings.
        /// </summary>
        /// <param name="drawings">Drawing objects.</param>
        /// <returns>Canvas object.</returns>
        ICanvas CreateCanvas(IEnumerable<IDrawing> drawings = null);

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

        /// <summary>
        /// Creates solid color brush.
        /// </summary>
        /// <param name="color">Brush solid color.</param>
        /// <param name="opacity">Brush opacity.</param>
        /// <returns>Solid color brush object.</returns>
        ISolidColorBrush CreateSolidColorBrush(Color color, double opacity = 1);

        /// <summary>
        /// Creates pen object.
        /// </summary>
        /// <param name="brush">Brush object.</param>
        /// <param name="thickness">Pen thickness.</param>
        /// <returns>Pen object.</returns>
        IPen CreatePen(IBrush brush, double thickness = 1);

        /// <summary>
        /// Creates line as drawing object.
        /// </summary>
        /// <param name="pen">Pen that should be used to draw the line.</param>
        /// <param name="x1">First point X coordinate.</param>
        /// <param name="y1">First point Y coordinate.</param>
        /// <param name="x2">Second point X coordinate.</param>
        /// <param name="y2">Second point Y coordinate.</param>
        /// <returns>Line as drawing object.</returns>
        ILine CreateLine(IPen pen, double x1, double y1, double x2, double y2);

        /// <summary>
        /// Creates rectangle as drawing object.
        /// </summary>
        /// <param name="pen">Pen used to draw edges.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Rectangle width.</param>
        /// <param name="height">Rectangle height.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        /// <returns>Rectangle as drawing object.</returns>
        IRectangle CreateRectangle(IPen pen, double left, double top, double width, double height, IBrush fillBrush = null);

        /// <summary>
        /// Creates ellipse as drawing object.
        /// </summary>
        /// <param name="pen">Pen used to draw the line.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Ellipse rectangle width.</param>
        /// <param name="height">Ellipse rectangle height.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        /// <param name="rotation">Ellipse clockwise rotation in radians.</param>
        /// <returns>Ellipse as drawing object.</returns>
        IEllipse CreateEllipse(IPen pen, double left, double top, double width, double height, double rotation, IBrush fillBrush = null);

        /// <summary>
        /// Creates circle as drawing object.
        /// </summary>
        /// <param name="pen">Pen used to draw the line.</param>
        /// <param name="centerX">Circle center X coordinate.</param>
        /// <param name="centerY">Circle center Y coordinate.</param>
        /// <param name="radius">Circle radius.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        /// <returns>Circle as drawing object.</returns>
        ICircle CreateCircle(IPen pen, double centerX, double centerY, double radius, IBrush fillBrush = null);
    }
}
