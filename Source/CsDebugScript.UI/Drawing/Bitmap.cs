using CsDebugScript.Drawing.Interfaces;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CsDebugScript.UI.Drawing
{
    /// <summary>
    /// Implementation of <see cref="IBitmap"/> for WPF.
    /// </summary>
    internal class Bitmap : IBitmap
    {
        /// <summary>
        /// Image source for this bitmap.
        /// </summary>
        private ImageSource imageSource;

        /// <summary>
        /// Image UI object for this bitmap.
        /// </summary>
        private Image image;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Number of channels in the image</param>
        private Bitmap(int width, int height, int channels)
        {
            Width = width;
            Height = height;
            ChannelsCount = channels;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        public Bitmap(int width, int height, ChannelType[] channels, byte[] pixels, double dpiX = 96, double dpiY = 96)
            : this(width, height, channels.Length)
        {
            PixelFormat format;

            // Check format
            if (Channels.AreSame(channels, Channels.BGR))
            {
                format = PixelFormats.Bgr24;
            }
            else if (Channels.AreSame(channels, Channels.BGRA))
            {
                format = PixelFormats.Bgra32;
            }
            else if (Channels.AreSame(channels, Channels.CMYK))
            {
                format = PixelFormats.Cmyk32;
            }
            else if (Channels.AreSame(channels, Channels.Grayscale))
            {
                format = PixelFormats.Gray8;
            }
            else if (Channels.AreSame(channels, Channels.RGB))
            {
                format = PixelFormats.Rgb24;
            }
            else if (Channels.AreSame(channels, Channels.RGBA))
            {
                // TODO:
                //format = PixelFormats.Rgba32;
                throw new NotImplementedException();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(channels), $"Unsupported channels: {string.Join(", ", channels)}");
            }

            imageSource = BitmapSource.Create(width, height, dpiX, dpiY, format, null, pixels, width * channels.Length * 1);
            image = new Image()
            {
                Width = width,
                Height = height,
                Source = imageSource,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        public Bitmap(int width, int height, ChannelType[] channels, sbyte[] pixels, double dpiX = 96, double dpiY = 96)
            : this(width, height, channels.Length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        public Bitmap(int width, int height, ChannelType[] channels, ushort[] pixels, double dpiX = 96, double dpiY = 96)
            : this(width, height, channels.Length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        public Bitmap(int width, int height, ChannelType[] channels, short[] pixels, double dpiX = 96, double dpiY = 96)
            : this(width, height, channels.Length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        public Bitmap(int width, int height, ChannelType[] channels, int[] pixels, double dpiX = 96, double dpiY = 96)
            : this(width, height, channels.Length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        public Bitmap(int width, int height, ChannelType[] channels, float[] pixels, double dpiX = 96, double dpiY = 96)
            : this(width, height, channels.Length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        public Bitmap(int width, int height, ChannelType[] channels, double[] pixels, double dpiX = 96, double dpiY = 96)
            : this(width, height, channels.Length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Bitmap width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Bitmap height.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Number of channels.
        /// </summary>
        public int ChannelsCount { get; private set; }

        /// <summary>
        /// UI object that should be added to visualization window.
        /// </summary>
        public object UIObject { get { return image; } }
    }
}
