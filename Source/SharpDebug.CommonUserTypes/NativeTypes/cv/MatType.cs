using SharpDebug.Engine;
using System;

namespace SharpDebug.CommonUserTypes.NativeTypes.cv
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
            BuiltinType builtinType = BuiltinType.Void;
            Type type = null;

            switch (fieldType)
            {
                case ElementType.Byte:
                    bits = 8;
                    type = typeof(byte);
                    builtinType = BuiltinType.UInt8;
                    break;
                case ElementType.SByte:
                    bits = 8;
                    type = typeof(sbyte);
                    builtinType = BuiltinType.Int8;
                    break;
                case ElementType.UShort:
                    bits = 16;
                    type = typeof(ushort);
                    builtinType = BuiltinType.UInt16;
                    break;
                case ElementType.Short:
                    bits = 16;
                    type = typeof(short);
                    builtinType = BuiltinType.Int16;
                    break;
                case ElementType.Int:
                    bits = 32;
                    type = typeof(int);
                    builtinType = BuiltinType.Int32;
                    break;
                case ElementType.Float:
                    bits = 32;
                    type = typeof(float);
                    builtinType = BuiltinType.Float32;
                    break;
                case ElementType.Double:
                    bits = 64;
                    type = typeof(double);
                    builtinType = BuiltinType.Float64;
                    break;
                case ElementType.UserType:
                    bits = -1;
                    type = typeof(object);
                    builtinType = BuiltinType.NoType;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return new MatType()
            {
                Bits = bits,
                Channels = channels + 1,
                BuiltinType = builtinType,
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
        /// Built-in type of the single value.
        /// </summary>
        public BuiltinType BuiltinType;

        /// <summary>
        /// Type of the single value.
        /// </summary>
        public Type Type;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Channels} ({Type.Name})";
        }
    }
}
