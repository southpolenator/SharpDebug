using CsDebugScript.PdbSymbolProvider.Utility;
using System;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents field list type record.
    /// </summary>
    public class FieldListRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_FIELDLIST
        };

        /// <summary>
        /// Gets the fields in this list.
        /// </summary>
        public IReadOnlyList<TypeRecord> Fields { get; private set; }

        /// <summary>
        /// Reads <see cref="FieldListRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static FieldListRecord Read(IBinaryReader reader, TypeLeafKind kind, uint dataLength)
        {
            long positionEnd = reader.Position + dataLength;
            List<TypeRecord> fields = new List<TypeRecord>();

            while (reader.Position < positionEnd)
            {
                // Read leaf
                TypeRecord record;
                TypeLeafKind leaf = (TypeLeafKind)reader.ReadUshort();

                switch (leaf)
                {
                    case TypeLeafKind.LF_ENUMERATE:
                        record = EnumeratorRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_MEMBER:
                        record = DataMemberRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_NESTTYPE:
                        record = NestedTypeRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_ONEMETHOD:
                        record = OneMethodRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_METHOD:
                        record = OverloadedMethodRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_BCLASS:
                    case TypeLeafKind.LF_INTERFACE:
                        record = BaseClassRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_VFUNCTAB:
                        record = VirtualFunctionPointerRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_STMEMBER:
                        record = StaticDataMemberRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_VBCLASS:
                    case TypeLeafKind.LF_IVBCLASS:
                        record = VirtualBaseClassRecord.Read(reader, leaf);
                        break;
                    case TypeLeafKind.LF_INDEX:
                        record = ListContinuationRecord.Read(reader, leaf);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                fields.Add(record);

                // Read padding
                if (reader.Position < positionEnd)
                {
                    byte padding = reader.ReadByte();

                    if (padding > 0xf0)
                    {
                        byte offset = (byte)((padding & 0x0f) - 1);

                        if (offset > 0)
                            reader.Position += offset;
                    }
                    else
                        reader.Position--;
                }
            }

            return new FieldListRecord
            {
                Kind = kind,
                Fields = fields,
            };
        }
    }
}
