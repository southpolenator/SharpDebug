using System;

namespace CsDebugScript.CommonUserTypes.NativeTypes.cv
{
    /// <summary>
    /// Type of the matrix.
    /// </summary>
    public struct MatType
    {
        /// <summary>
        /// Converts native type flags into readable .NET structure.
        /// </summary>
        /// <param name="nativeType">cv::Mat::TYPE_MASK bits of cv::Mat::flags field.</param>
        public static MatType ConvertNativeType(int nativeType)
        {
            ElementType fieldType = (ElementType)(nativeType & (int)ElementType.CoverageMask);
            int channels = nativeType / ((int)ElementType.CoverageMask + 1);
            int bits = 0;
            Type type = null;

            switch (fieldType)
            {
                case ElementType.Byte:
                    bits = 8;
                    type = typeof(byte);
                    break;
                case ElementType.SByte:
                    bits = 8;
                    type = typeof(sbyte);
                    break;
                case ElementType.UShort:
                    bits = 16;
                    type = typeof(ushort);
                    break;
                case ElementType.Short:
                    bits = 16;
                    type = typeof(short);
                    break;
                case ElementType.Int:
                    bits = 32;
                    type = typeof(int);
                    break;
                case ElementType.Float:
                    bits = 32;
                    type = typeof(float);
                    break;
                case ElementType.Double:
                    bits = 64;
                    type = typeof(double);
                    break;
                case ElementType.UserType:
                    bits = -1;
                    type = typeof(object);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return new MatType()
            {
                Bits = bits,
                Channels = channels + 1,
                Type = type,
            };
        }

        /// <summary>
        /// Number of bits used to save single value.
        /// </summary>
        public int Bits;

        /// <summary>
        /// Number of values (channels) in one matrix element.
        /// </summary>
        public int Channels;

        /// <summary>
        /// Type of the single value.
        /// </summary>
        public Type Type;
    }
}
