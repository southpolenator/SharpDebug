using CsDebugScript.PdbSymbolProvider.Utility;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents method list type record.
    /// </summary>
    public class MethodOverloadListRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_METHODLIST
        };

        /// <summary>
        /// Gets the list of methods.
        /// </summary>
        public IReadOnlyList<OneMethodRecord> Methods { get; private set; }

        /// <summary>
        /// Reads <see cref="MethodOverloadListRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static MethodOverloadListRecord Read(IBinaryReader reader, TypeLeafKind kind, uint dataLength)
        {
            long endPosition = reader.Position + dataLength;
            List<OneMethodRecord> methods = new List<OneMethodRecord>();

            while (reader.Position < endPosition)
                methods.Add(OneMethodRecord.Read(reader, TypeLeafKind.LF_ONEMETHOD, isFromOverloadedList: true));
            return new MethodOverloadListRecord
            {
                Kind = kind,
                Methods = methods,
            };
        }
    }
}
