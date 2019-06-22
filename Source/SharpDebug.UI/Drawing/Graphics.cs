using SharpDebug.Drawing.Interfaces;
using System.Collections.Generic;
using System.Windows.Threading;

namespace SharpDebug.UI.Drawing
{
    /// <summary>
    /// Implementation of <see cref="IGraphics"/> for WPF.
    /// </summary>
    internal class Graphics : IGraphics
    {
        /// <summary>
        /// Dispatcher object which helps us to create UI objects on correct thread.
        /// </summary>
        private Dispatcher dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics" /> class.
        /// </summary>
        /// <param name="dispatcher">Dispatcher object which helps us to create UI objects on correct thread.</param>
        public Graphics(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// Creates canvas object that contains the specified list of drawings.
        /// </summary>
        /// <param name="drawings">Drawing objects.</param>
        /// <returns>Canvas object.</returns>
        public ICanvas CreateCanvas(IEnumerable<IDrawing> drawings)
        {
            return dispatcher.Invoke(() => new Canvas(drawings));
        }

        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        public IBitmap CreateBitmap(int width, int height, ChannelType[] channels, byte[] pixels)
        {
            return dispatcher.Invoke(() => new Bitmap(width, height, channels, pixels));
        }

        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        public IBitmap CreateBitmap(int width, int height, ChannelType[] channels, sbyte[] pixels)
        {
            return dispatcher.Invoke(() => new Bitmap(width, height, channels, pixels));
        }

        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        public IBitmap CreateBitmap(int width, int height, ChannelType[] channels, ushort[] pixels)
        {
            return dispatcher.Invoke(() => new Bitmap(width, height, channels, pixels));
        }

        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        public IBitmap CreateBitmap(int width, int height, ChannelType[] channels, short[] pixels)
        {
            return dispatcher.Invoke(() => new Bitmap(width, height, channels, pixels));
        }

        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        public IBitmap CreateBitmap(int width, int height, ChannelType[] channels, int[] pixels)
        {
            return dispatcher.Invoke(() => new Bitmap(width, height, channels, pixels));
        }

        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        public IBitmap CreateBitmap(int width, int height, ChannelType[] channels, float[] pixels)
        {
            return dispatcher.Invoke(() => new Bitmap(width, height, channels, pixels));
        }

        /// <summary>
        /// Creates bitmap from parameters. Pixels are bytes.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="channels">Number of channels per pixel.</param>
        /// <param name="pixels">Array of pixel data.</param>
        /// <returns>Bitmap object.</returns>
        public IBitmap CreateBitmap(int width, int height, ChannelType[] channels, double[] pixels)
        {
            return dispatcher.Invoke(() => new Bitmap(width, height, channels, pixels));
        }

        /// <summary>
        /// Creates solid color brush.
        /// </summary>
        /// <param name="color">Brush solid color.</param>
        /// <param name="opacity">Brush opacity.</param>
        /// <returns>Solid color brush object.</returns>
        public ISolidColorBrush CreateSolidColorBrush(Color color, double opacity)
        {
            return dispatcher.Invoke(() => new SolidColorBrush(color, opacity));
        }

        /// <summary>
        /// Creates pen object.
        /// </summary>
        /// <param name="brush">Brush object.</param>
        /// <param name="thickness">Pen thickness.</param>
        /// <returns>Pen object.</returns>
        public IPen CreatePen(IBrush brush, double thickness = 1)
        {
            return dispatcher.Invoke(() => new Pen(brush, thickness));
        }

        /// <summary>
        /// Creates font object.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="fontStyle">The font style.</param>
        /// <param name="fontWeight">The font weight.</param>
        /// <returns>Font object.</returns>
        public IFont CreateFont(string fontFamily, double fontSize, FontStyle fontStyle = FontStyle.Normal, FontWeight fontWeight = FontWeight.Normal)
        {
            return new Font(fontFamily, fontSize, fontStyle, fontWeight);
        }

        /// <summary>
        /// Creates text as drawing object.
        /// </summary>
        /// <param name="text">Text that will be drawn.</param>
        /// <param name="font">Font that will be used to draw the text.</param>
        /// <param name="foreground">Brush that will be used to paint the text.</param>
        /// <param name="left">Left position of the text.</param>
        /// <param name="top">Top position of the text.</param>
        /// <param name="width">Virtual text box width. If value is less than 0, it will be automatically computed.</param>
        /// <param name="height">Virtual text box height. If value is less than 0, it will be automatically computed.</param>
        /// <param name="horizontalAlignment">Text horizontal alignment.</param>
        /// <param name="verticalAlignment">Text vertical alignment.</param>
        /// <param name="wrapping">Text wrapping.</param>
        /// <param name="rotation">Text clockwise rotation in radians.</param>
        /// <returns>Text as drawing object.</returns>
        public IText CreateText(string text, IFont font, IBrush foreground, double left = 0, double top = 0, double width = -1, double height = -1, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left, TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top, TextWrapping wrapping = TextWrapping.Wrap, double rotation = 0)
        {
            return CreateText(text, font, foreground, null, left, top, width, height, horizontalAlignment, verticalAlignment, wrapping, rotation);
        }

        /// <summary>
        /// Creates text as drawing object.
        /// </summary>
        /// <param name="text">Text that will be drawn.</param>
        /// <param name="font">Font that will be used to draw the text.</param>
        /// <param name="foreground">Brush that will be used to paint the text.</param>
        /// <param name="background">Brush that will be used to paint the text's background.</param>
        /// <param name="left">Left position of the text.</param>
        /// <param name="top">Top position of the text.</param>
        /// <param name="width">Virtual text box width. If value is less than 0, it will be automatically computed.</param>
        /// <param name="height">Virtual text box height. If value is less than 0, it will be automatically computed.</param>
        /// <param name="horizontalAlignment">Text horizontal alignment.</param>
        /// <param name="verticalAlignment">Text vertical alignment.</param>
        /// <param name="wrapping">Text wrapping.</param>
        /// <param name="rotation">Text clockwise rotation in radians.</param>
        /// <returns>Text as drawing object.</returns>
        public IText CreateText(string text, IFont font, IBrush foreground, IBrush background, double left = 0, double top = 0, double width = -1, double height = -1, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left, TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Top, TextWrapping wrapping = TextWrapping.Wrap, double rotation = 0)
        {
            return dispatcher.Invoke(() => new Text(text, font, foreground, background, left, top, width, height, horizontalAlignment, verticalAlignment, wrapping, rotation));
        }

        /// <summary>
        /// Creates line as drawing object.
        /// </summary>
        /// <param name="pen">Pen that should be used to draw the line.</param>
        /// <param name="x1">First point X coordinate.</param>
        /// <param name="y1">First point Y coordinate.</param>
        /// <param name="x2">Second point X coordinate.</param>
        /// <param name="y2">Second point Y coordinate.</param>
        /// <returns>Line as drawing object.</returns>
        public ILine CreateLine(IPen pen, double x1, double y1, double x2, double y2)
        {
            return dispatcher.Invoke(() => new Line(pen, x1, y1, x2, y2));
        }

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
        public IRectangle CreateRectangle(IPen pen, double left, double top, double width, double height, IBrush fillBrush = null)
        {
            return dispatcher.Invoke(() => new Rectangle(pen, fillBrush, left, top, width, height));
        }

        /// <summary>
        /// Creates ellipse as drawing object.
        /// </summary>
        /// <param name="pen">Pen used to draw the line.</param>
        /// <param name="left">Left coordinate of top left corner.</param>
        /// <param name="top">Top coordinate of top left corner.</param>
        /// <param name="width">Ellipse rectangle width.</param>
        /// <param name="height">Ellipse rectangle height.</param>
        /// <param name="rotation">Ellipse clockwise rotation in radians.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        /// <returns>Ellipse as drawing object.</returns>
        public IEllipse CreateEllipse(IPen pen, double left, double top, double width, double height, double rotation, IBrush fillBrush = null)
        {
            return dispatcher.Invoke(() => new Ellipse(pen, fillBrush, left, top , width, height, rotation));
        }

        /// <summary>
        /// Creates circle as drawing object.
        /// </summary>
        /// <param name="pen">Pen used to draw the line.</param>
        /// <param name="centerX">Circle center X coordinate.</param>
        /// <param name="centerY">Circle center Y coordinate.</param>
        /// <param name="radius">Circle radius.</param>
        /// <param name="fillBrush">Brush used to fill the content.</param>
        /// <returns>Circle as drawing object.</returns>
        public ICircle CreateCircle(IPen pen, double centerX, double centerY, double radius, IBrush fillBrush = null)
        {
            return dispatcher.Invoke(() => new Circle(pen, fillBrush, centerX, centerY, radius));
        }
    }
}
