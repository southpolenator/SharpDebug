using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This is used for assembler labels where there is no typing information about the label.
    /// </summary>
    public class LabelRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_LABEL
        };

        /// <summary>
        /// Gets the label type.
        /// </summary>
        public LabelType Mode { get; private set; }

        /// <summary>
        /// Reads <see cref="LabelRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static LabelRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new LabelRecord
            {
                Kind = kind,
                Mode = (LabelType)reader.ReadUshort(),
            };
        }
    }
}
