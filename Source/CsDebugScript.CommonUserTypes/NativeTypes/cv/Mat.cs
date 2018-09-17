using CsDebugScript.Drawing.Interfaces;
using CsDebugScript.Exceptions;
using System;
using System.Linq;

namespace CsDebugScript.CommonUserTypes.NativeTypes.cv
{
    /// <summary>
    /// Implementation of cv::Mat class.
    /// </summary>
    [UserType(TypeName = "cv::Mat")]
    public class Mat : DynamicSelfUserType, IDrawingVisualizerObject
    {
        /// <summary>
        /// Magic value used to verify that cv::Mat flags is correct.
        /// </summary>
        public const int MAGIC_VAL = 0x42FF0000;

        /// <summary>
        /// Mask used to extract magic value.
        /// </summary>
        public const uint MAGIC_MASK = 0xFFFF0000;

        /// <summary>
        /// Flag to check if matrix is continuous.
        /// </summary>
        public const int CONTINUOUS_FLAG = 16384;

        /// <summary>
        /// Flag to check if matrix is submatrix.
        /// </summary>
        public const int SUBMATRIX_FLAG = 32768;

        /// <summary>
        /// Mask to extract matrix element type.
        /// </summary>
        public const int TYPE_MASK = 0x00000FFF;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType" /> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public Mat(Variable variable)
            : base(variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "cv::Mat");
            }
        }

        /// <summary>
        /// Checks whether cv::Mat is initialized correctly.
        /// </summary>
        public bool IsCorrect => ((int)self.flags & MAGIC_MASK) == MAGIC_VAL;

        /// <summary>
        /// The matrix dimensionality, >= 2
        /// </summary>
        public int Dimensions => (int)self.dims;

        /// <summary>
        /// Array of integers specifying an n-dimensional array shape.
        /// </summary>
        public int[] Sizes => ReadIntArray(self.size.p, Dimensions);

        /// <summary>
        /// Gets the per dimension step (or stride) in bytes.
        /// </summary>
        public int[] Steps => ReadIntArray(self.step.p, Dimensions);

        /// <summary>
        /// Gets matrix element type.
        /// </summary>
        public MatType Type => MatType.ConvertNativeType((int)self.flags & TYPE_MASK);

        /// <summary>
        /// Number of bytes needed to store one element of the matrix.
        /// </summary>
        public int ElementSize => Steps.LastOrDefault();

        /// <summary>
        /// Reports whether the matrix is continuous or not.
        /// </summary>
        public bool IsContinuous => ((int)self.flags & CONTINUOUS_FLAG) != 0;

        /// <summary>
        /// <c>true</c> if the matrix is a submatrix of another matrix.
        /// </summary>
        public bool IsSubmatrix => ((int)self.flags & SUBMATRIX_FLAG) != 0;

        /// <summary>
        /// void* pointer where elements are located.
        /// </summary>
        public new NakedPointer Data => new NakedPointer(GetCodeType().Module.Process, self.data.GetPointerAddress());

        /// <summary>
        /// Reads integer array.
        /// </summary>
        /// <param name="array">Variable that represents array.</param>
        /// <param name="length">Lenght of the array</param>
        private static int[] ReadIntArray(Variable array, int length)
        {
            int[] result = new int[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (int)array.GetArrayElement(i);
            }
            return result;
        }

        /// <summary>
        /// Cheks if data is correct and object can be visualized as a drawing.
        /// </summary>
        /// <returns><c>true</c> if data is correct and object can be visualized as a drawing.</returns>
        public bool CanVisualize()
        {
            return IsCorrect && Dimensions == 2 && Sizes[0] > 0 && Sizes[1] > 0 && !Data.IsNull;
        }

        /// <summary>
        /// Creates drawing that should be visualized.
        /// </summary>
        /// <param name="graphics">Graphics object used to create drawings.</param>
        /// <returns>Drawing object that should be visualized.</returns>
        public IDrawing CreateDrawing(IGraphics graphics)
        {
            return CreateDrawing(graphics, Sizes[1], Sizes[0], Data, Steps[0], Type);
        }

        /// <summary>
        /// Creates drawing that should be visualized based on cv::Mat data.
        /// </summary>
        /// <param name="graphics">Graphics object used to create drawings.</param>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="data">Pointer to where start of the data is.</param>
        /// <param name="stride">Row stride in bytes.</param>
        /// <param name="type">Matrix element type.</param>
        /// <returns>Drawing object that should be visualized.</returns>
        internal static IDrawing CreateDrawing(IGraphics graphics, int width, int height, NakedPointer data, int stride, MatType type)
        {
            ChannelType[] channels;

            if (type.Channels == 3)
            {
                channels = Channels.BGR;
            }
            else if (type.Channels == 1)
            {
                channels = Channels.Grayscale;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (type.Type == typeof(byte))
            {
                return graphics.CreateBitmap(width, height, channels, ReadPixels<byte>(width, height, data, stride, type));
            }
            else if (type.Type == typeof(sbyte))
            {
                return graphics.CreateBitmap(width, height, channels, ReadPixels<sbyte>(width, height, data, stride, type));
            }
            else if (type.Type == typeof(ushort))
            {
                return graphics.CreateBitmap(width, height, channels, ReadPixels<ushort>(width, height, data, stride, type));
            }
            else if (type.Type == typeof(short))
            {
                return graphics.CreateBitmap(width, height, channels, ReadPixels<short>(width, height, data, stride, type));
            }
            else if (type.Type == typeof(int))
            {
                return graphics.CreateBitmap(width, height, channels, ReadPixels<int>(width, height, data, stride, type));
            }
            else if (type.Type == typeof(float))
            {
                return graphics.CreateBitmap(width, height, channels, ReadPixels<float>(width, height, data, stride, type));
            }
            else if (type.Type == typeof(double))
            {
                return graphics.CreateBitmap(width, height, channels, ReadPixels<double>(width, height, data, stride, type));
            }

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Reads pixels data into single array of pixels.
        /// </summary>
        /// <typeparam name="T">Type of the pixel element.</typeparam>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="data">Pointer to where start of the data is.</param>
        /// <param name="stride">Row stride in bytes.</param>
        /// <param name="type">Matrix element type.</param>
        /// <returns>Array of pixels.</returns>
        internal static T[] ReadPixels<T>(int width, int height, NakedPointer data, int stride, MatType type)
        {
            if (stride == type.Bits * type.Channels * width / 8)
            {
                return new CodeArray<T>(data, width * height * type.Channels).ToArray();
            }
            else
            {
                T[] result = new T[width * height * type.Channels];
                int rowElements = width * type.Channels;

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

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private bool VerifyCodeType(CodeType codeType)
        {
            // We want to have this kind of hierarchy
            // flags
            // dims
            // size
            // | p
            // step
            // | p
            // data
            CodeType flags, dims, size, sizep, step, stepp, data;

            var fields = codeType.GetFieldTypes();

            if (!fields.TryGetValue("flags", out flags) || !fields.TryGetValue("dims", out dims) || !fields.TryGetValue("size", out size) || !fields.TryGetValue("step", out step) || !fields.TryGetValue("data", out data))
                return false;

            if (!size.GetFieldTypes().TryGetValue("p", out sizep) || !step.GetFieldTypes().TryGetValue("p", out stepp))
                return false;

            return true;
        }
    }
}
