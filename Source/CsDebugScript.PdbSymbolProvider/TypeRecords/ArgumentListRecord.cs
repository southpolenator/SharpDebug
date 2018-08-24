using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents function argument list type record.
    /// </summary>
    public class ArgumentListRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_ARGLIST
        };

        /// <summary>
        /// List of type indices for describing the formal parameters for a function or method.
        /// </summary>
        public TypeIndex[] Arguments { get; private set; }

        /// <summary>
        /// Reads <see cref="ArgumentListRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        /// <param name="dataLength">Length of the record data.</param>
        public static ArgumentListRecord Read(IBinaryReader reader, TypeLeafKind kind, uint dataLength)
        {
            uint count = dataLength / TypeIndex.Size;
            TypeIndex[] arguments = new TypeIndex[count];

            for (int i = 0; i < arguments.Length; i++)
                arguments[i] = TypeIndex.Read(reader);
            return new ArgumentListRecord
            {
                Kind = kind,
                Arguments = arguments,
            };
        }
    }
}
