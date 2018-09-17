using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Used to chain two large <see cref="FieldListRecord"/> or <see cref="MethodOverloadListRecord"/> records
    /// together. The first will end in an <see cref="ListContinuationRecord"/> that points to the next.
    /// </summary>
    public class ListContinuationRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_INDEX
        };

        /// <summary>
        /// Padding or unknown data?
        /// </summary>
        public ushort Padding { get; private set; }

        /// <summary>
        /// Gets the continuation type index. This field is emitted by the compiler when a complex list
        /// needs to be split during writing.
        /// </summary>
        public TypeIndex ContinuationIndex { get; private set; }

        /// <summary>
        /// Reads <see cref="ListContinuationRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static ListContinuationRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new ListContinuationRecord
            {
                Kind = kind,
                Padding = reader.ReadUshort(),
                ContinuationIndex = TypeIndex.Read(reader),
            };
        }
    }
}
