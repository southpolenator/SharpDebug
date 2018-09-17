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
    internal class Bitmap : Drawing, IBitmap
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
            Initialize(channels, pixels, dpiX, dpiY);
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
            : this(width, height, channels, ConvertToByte(pixels), dpiX, dpiY)
        {
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
            Initialize(channels, pixels, dpiX, dpiY);
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
            : this(width, height, channels, ConvertToUshort(pixels), dpiX, dpiY)
        {
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
            : this(width, height, channels, ConvertToUshort(pixels), dpiX, dpiY)
        {
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
            : this(width, height, channels, ConvertToUshort(pixels), dpiX, dpiY)
        {
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
            : this(width, height, channels, ConvertToUshort(pixels), dpiX, dpiY)
        {
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
        public override object UIObject => image;

        /// <summary>
        /// Initializes bitmap object with the specified parameters.
        /// </summary>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        private void Initialize(ChannelType[] channels, byte[] pixels, double dpiX, double dpiY)
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
                format = PixelFormats.Bgra32;
                pixels = BGRA2RGBA(pixels);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(channels), $"Unsupported channels: {string.Join(", ", channels)}");
            }

            Initialize(dpiX, dpiY, format, pixels, Width * channels.Length * 1);
        }

        /// <summary>
        /// Initializes bitmap object with the specified parameters.
        /// </summary>
        /// <param name="channels">Array of channels</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        private void Initialize(ChannelType[] channels, ushort[] pixels, double dpiX, double dpiY)
        {
            PixelFormat format;

            // Check format
            if (Channels.AreSame(channels, Channels.Grayscale))
            {
                format = PixelFormats.Gray16;
            }
            else if (Channels.AreSame(channels, Channels.BGR))
            {
                format = PixelFormats.Rgb48;
                pixels = BGR2RGB(pixels);
            }
            else if (Channels.AreSame(channels, Channels.RGB))
            {
                format = PixelFormats.Rgb48;
            }
            else if (Channels.AreSame(channels, Channels.BGRA))
            {
                format = PixelFormats.Rgba64;
                pixels = BGRA2RGBA(pixels);
            }
            else if (Channels.AreSame(channels, Channels.RGBA))
            {
                format = PixelFormats.Rgba64;
            }
            else if (Channels.AreSame(channels, Channels.CMYK))
            {
                Initialize(channels, ConvertToByte(pixels), dpiX, dpiY);
                return;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(channels), $"Unsupported channels: {string.Join(", ", channels)}");
            }

            Initialize(dpiX, dpiY, format, pixels, Width * channels.Length * 2);
        }

        /// <summary>
        /// Converts data from BGR channels to RGB channels.
        /// </summary>
        /// <typeparam name="T">Type of the pixel channel.</typeparam>
        /// <param name="pixels">Buffer with pixels</param>
        /// <returns>Converted pixel data.</returns>
        private T[] BGR2RGB<T>(T[] pixels)
        {
            T[] result = new T[pixels.Length];

            for (int i = 0; i < pixels.Length; i += 3)
            {
                result[i] = pixels[i + 2];
                result[i + 1] = pixels[i + 1];
                result[i + 2] = pixels[i];
            }
            return result;
        }

        /// <summary>
        /// Converts data from BGRA channels to RGBA channels.
        /// </summary>
        /// <typeparam name="T">Type of the pixel channel.</typeparam>
        /// <param name="pixels">Buffer with pixels</param>
        /// <returns>Converted pixel data.</returns>
        private T[] BGRA2RGBA<T>(T[] pixels)
        {
            T[] result = new T[pixels.Length];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                result[i] = pixels[i + 2];
                result[i + 1] = pixels[i + 1];
                result[i + 2] = pixels[i];
                result[i + 3] = pixels[i + 3];
            }
            return result;
        }

        /// <summary>
        /// Initializes bitmap object with the specified parameters.
        /// </summary>
        /// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
        /// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
        /// <param name="format">Pixel format.</param>
        /// <param name="pixels">Buffer with pixels</param>
        /// <param name="stride">Stride in bytes.</param>
        private void Initialize(double dpiX, double dpiY, PixelFormat format, Array pixels, int stride)
        {
            imageSource = BitmapSource.Create(Width, Height, dpiX, dpiY, format, null, pixels, stride);
            image = new Image()
            {
                Width = Width,
                Height = Height,
                Source = imageSource,
            };
        }

        /// <summary>
        /// Converts pixels buffer to byte array.
        /// </summary>
        /// <param name="pixels">Buffer with pixels</param>
        private static byte[] ConvertToByte(sbyte[] pixels)
        {
            byte[] result = new byte[pixels.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(pixels[i] ^ 0x80);
            }

            return result;
        }

        /// <summary>
        /// Converts pixels buffer to byte array.
        /// </summary>
        /// <param name="pixels">Buffer with pixels</param>
        private static byte[] ConvertToByte(ushort[] pixels)
        {
            byte[] result = new byte[pixels.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(pixels[i] >> 8);
            }

            return result;
        }

        /// <summary>
        /// Converts pixels buffer to ushort array.
        /// </summary>
        /// <param name="pixels">Buffer with pixels</param>
        private static ushort[] ConvertToUshort(short[] pixels)
        {
            ushort[] result = new ushort[pixels.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (ushort)(pixels[i] ^ 0x8000);
            }

            return result;
        }

        /// <summary>
        /// Converts pixels buffer to ushort array.
        /// </summary>
        /// <param name="pixels">Buffer with pixels</param>
        private static ushort[] ConvertToUshort(int[] pixels)
        {
            ushort[] result = new ushort[pixels.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (ushort)((uint)(pixels[i] ^ 0x80000000) >> 16);
            }

            return result;
        }

        /// <summary>
        /// Converts pixels buffer to ushort array.
        /// </summary>
        /// <param name="pixels">Buffer with pixels</param>
        private static ushort[] ConvertToUshort(float[] pixels)
        {
            ushort[] result = new ushort[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                result[i] = (ushort)(pixels[i] * (ushort.MaxValue - 1));
            }

            return result;
        }

        /// <summary>
        /// Converts pixels buffer to ushort array.
        /// </summary>
        /// <param name="pixels">Buffer with pixels</param>
        private static ushort[] ConvertToUshort(double[] pixels)
        {
            ushort[] result = new ushort[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                result[i] = (ushort)(pixels[i] * (ushort.MaxValue - 1));
            }

            return result;
        }
    }
}
