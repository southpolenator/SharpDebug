using SharpDebug.Drawing.Interfaces;
using SharpDebug.Engine;
using System;

namespace SharpDebug.CommonUserTypes
{
    /// <summary>
    /// Helper class for easier implementation of bimap user types.
    /// </summary>
    public class BitmapUserType : DynamicSelfUserType, IDrawingVisualizerObject
    {
        /// <summary>
        /// Type of the single pixel channel value.
        /// </summary>
        private BuiltinType dataType;

        /// <summary>
        /// Channels in the bitmap.
        /// </summary>
        private ChannelType[] channels;

        /// <summary>
        /// Address of the pixels data.
        /// </summary>
        private NakedPointer data;

        /// <summary>
        /// Bitmap data stride.
        /// </summary>
        private int stride;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapUserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public BitmapUserType(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the bitmap width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the bitmap height.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Cheks if data is correct and object can be visualized as a drawing.
        /// </summary>
        /// <returns><c>true</c> if data is correct and object can be visualized as a drawing.</returns>
        public virtual bool CanVisualize()
        {
            return !data.IsNull() && Width > 0 && Height > 0 && stride >= Width;
        }

        /// <summary>
        /// Creates drawing that should be visualized.
        /// </summary>
        /// <param name="graphics">Graphics object used to create drawings.</param>
        /// <returns>Drawing object that should be visualized.</returns>
        public virtual IDrawing CreateDrawing(IGraphics graphics)
        {
            return CreateDrawing(graphics, Width, Height, data, channels, dataType, stride);
        }

        /// <summary>
        /// Creates drawing that should be visualized.
        /// </summary>
        /// <param name="graphics">Graphics object used to create drawings.</param>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="data">Address of the pixels data.</param>
        /// <param name="channels">Channels in the bitmap.</param>
        /// <param name="dataType">Type of the single pixel channel value.</param>
        /// <param name="stride">Bitmap data stride.</param>
        /// <returns>Drawing object that should be visualized.</returns>
        public static IDrawing CreateDrawing(IGraphics graphics, int width, int height, NakedPointer data, ChannelType[] channels, BuiltinType dataType = BuiltinType.UInt8, int stride = 0)
        {
            switch (dataType)
            {
                case BuiltinType.Float32:
                    return graphics.CreateBitmap(width, height, channels, ReadPixels<float>(width, height, data, stride, channels.Length));
                case BuiltinType.Float64:
                    return graphics.CreateBitmap(width, height, channels, ReadPixels<double>(width, height, data, stride, channels.Length));
                case BuiltinType.Int8:
                    return graphics.CreateBitmap(width, height, channels, ReadPixels<sbyte>(width, height, data, stride, channels.Length));
                case BuiltinType.Int16:
                    return graphics.CreateBitmap(width, height, channels, ReadPixels<short>(width, height, data, stride, channels.Length));
                case BuiltinType.Int32:
                    return graphics.CreateBitmap(width, height, channels, ReadPixels<int>(width, height, data, stride, channels.Length));
                case BuiltinType.NoType:
                case BuiltinType.Char8:
                case BuiltinType.Void:
                case BuiltinType.UInt8:
                    return graphics.CreateBitmap(width, height, channels, ReadPixels<byte>(width, height, data, stride, channels.Length));
                case BuiltinType.UInt16:
                    return graphics.CreateBitmap(width, height, channels, ReadPixels<ushort>(width, height, data, stride, channels.Length));
                default:
                    throw new NotImplementedException($"Unknown image data type: {dataType}");
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!CanVisualize())
                return "[empty]";
            return $"{Width} x {Height} x {channels.Length} ({dataType})";
        }

        /// <summary>
        /// Initializes bitmap for visualization.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="dataAddress">Address of the pixels data.</param>
        /// <param name="channels">Channels in the bitmap.</param>
        /// <param name="dataType">Type of the single pixel channel value.</param>
        /// <param name="stride">Bitmap data stride.</param>
        protected void Initialize(int width, int height, ulong dataAddress, ChannelType[] channels, BuiltinType dataType = BuiltinType.UInt8, int stride = 0)
        {
            Width = width;
            Height = height;
            this.stride = stride;
            this.channels = channels;
            this.dataType = dataType;
            data = dataAddress != 0 ? new NakedPointer(GetCodeType().Module.Process, dataAddress) : null;
        }

        /// <summary>
        /// Reads pixels data into single array of pixels with new stride equal to <paramref name="width"/>.
        /// </summary>
        /// <typeparam name="T">Type of the pixel.</typeparam>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="data">Pointer to start of bitmap data.</param>
        /// <param name="stride">Row stride in bytes.</param>
        /// <param name="channels">Number of channels in the image.</param>
        /// <returns>Array of image pixels.</returns>
        private static T[] ReadPixels<T>(int width, int height, NakedPointer data, int stride, int channels)
        {
            int pixelByteSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();

            if (stride <= 0 || stride == pixelByteSize * channels * width)
            {
                return new CodeArray<T>(data, width * height * channels).ToArray();
            }
            else
            {
                T[] result = new T[width * height * channels];
                int rowElements = width * channels;

                for (int y = 0, j = 0; y < height; y++)
                {
                    CodeArray<T> array = new CodeArray<T>(data.AdjustPointer(stride * y), rowElements);

                    for (int x = 0; x < rowElements; x++, j++)
                    {
                        result[j] = array[x];
                    }
                }

                return result;
            }
        }
    }
}
