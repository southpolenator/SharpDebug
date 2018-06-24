using CsDebugScript.Drawing.Interfaces;
using System.Windows.Threading;

namespace CsDebugScript.UI.Drawing
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
    }
}
