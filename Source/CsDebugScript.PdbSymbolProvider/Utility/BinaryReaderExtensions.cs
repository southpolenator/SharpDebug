using CsDebugScript.PdbSymbolProvider.TypeRecords;
using System;

namespace CsDebugScript.PdbSymbolProvider.Utility
{
    /// <summary>
    /// Extension methods for <see cref="IBinaryReader"/>.
    /// </summary>
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads <c>byte[]</c> from the stream.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        /// <param name="length">Number of elements to be read.</param>
        public static byte[] ReadByteArray(this IBinaryReader reader, int length)
        {
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
                result[i] = reader.ReadByte();
            return result;
        }

        /// <summary>
        /// Reads <c>ushort[]</c> from the stream.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        /// <param name="length">Number of elements to be read.</param>
        public static ushort[] ReadUshortArray(this IBinaryReader reader, int length)
        {
            ushort[] result = new ushort[length];

            for (int i = 0; i < length; i++)
                result[i] = reader.ReadUshort();
            return result;
        }

        /// <summary>
        /// Reads <c>uint[]</c> from the stream.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        /// <param name="length">Number of elements to be read.</param>
        public static uint[] ReadUintArray(this IBinaryReader reader, int length)
        {
            uint[] result = new uint[length];

            for (int i = 0; i < length; i++)
                result[i] = reader.ReadUint();
            return result;
        }

        /// <summary>
        /// Creates substream of the specified length from the current position and moves position by the substream length.
        /// </summary>
        /// <typeparam name="TStream">Type of the parent stream.</typeparam>
        /// <param name="reader">Parent binary stream.</param>
        /// <param name="length">Substream length in bytes.</param>
        public static IBinaryReader ReadSubstream<TStream>(this TStream reader, long length = -1)
            where TStream : IBinaryReader
        {
            if (length < 0)
                length = reader.Length - reader.Position;
            IBinaryReader result = new BinarySubstreamReader<TStream>(reader, reader.Position, length);

            reader.Position += length;
            return result;
        }

        /// <summary>
        /// Reads encoded integer from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <returns>Integer, but also carries type info, so it is returned as object.</returns>
        public static object ReadEncodedInteger(this IBinaryReader reader)
        {
            ushort type = reader.ReadUshort();

            if (type < (ushort)TypeLeafKind.LF_NUMERIC)
                return type;

            switch ((TypeLeafKind)type)
            {
                case TypeLeafKind.LF_CHAR:
                    return (sbyte)reader.ReadByte();
                case TypeLeafKind.LF_SHORT:
                    return reader.ReadShort();
                case TypeLeafKind.LF_USHORT:
                    return reader.ReadUshort();
                case TypeLeafKind.LF_LONG:
                    return reader.ReadInt();
                case TypeLeafKind.LF_ULONG:
                    return reader.ReadUint();
                case TypeLeafKind.LF_QUADWORD:
                    return reader.ReadLong();
                case TypeLeafKind.LF_UQUADWORD:
                    return reader.ReadUlong();
            }

            throw new NotImplementedException();
        }
    }
}
