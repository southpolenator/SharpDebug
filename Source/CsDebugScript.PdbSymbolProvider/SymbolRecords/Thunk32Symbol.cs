using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// This record is used to specify any piece of code that exists outside a procedure. It is followed by
    /// an End record.The thunk  record is intended for small code fragments and a two byte length
    /// field is sufficient for its intended purpose.
    /// </summary>
    public class Thunk32Symbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_THUNK32
        };

        /// <summary>
        /// Used in local procedures, global procedures, thunk start, with start, and
        /// block start symbols. If the scope is not enclosed by another lexical scope,
        /// then <see cref="Parent"/> is zero. Otherwise, the parent of this scope is the symbol
        /// within this module that opens the outer scope that encloses this scope but
        /// encloses no other scope that encloses this scope. The <see cref="Parent"/> field contains
        /// the offset from the beginning of the module's symbol table of the symbol
        /// that opens the enclosing lexical scope.
        /// </summary>
        public uint Parent { get; private set; }

        /// <summary>
        /// Used in start search local procedures, global procedures, and thunk start
        /// symbols. The <see cref="Next"/> field, along with the start search symbol, defines a
        /// group of lexically scoped symbols within a symbol table that is contained
        /// within a code segment or PE section. For each segment or section
        /// represented in the symbol table, there is a start search symbol that contains
        /// the offset from the start of the symbols for this module to the first procedure
        /// or thunk contained in the segment. Each outermost lexical scope symbol
        /// has a next field containing the next outermost scope symbol contained in the
        /// segment. The last outermost scope in the symbol table for each segment has
        /// a next field of zero.
        /// </summary>
        public uint End { get; private set; }

        /// <summary>
        /// This field is defined for local procedures, global procedures, thunk, block,
        /// and with symbols.The end field contains the offset from the start of the
        /// symbols for this module to the matching block end symbol that terminates
        /// the lexical scope.
        /// </summary>
        public uint Next { get; private set; }

        /// <summary>
        /// Gets the offset portion of the thunk address.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the segment portion of the thunk address.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the length in bytes of this thunk.
        /// </summary>
        public ushort Length { get; private set; }

        /// <summary>
        /// Gets the ordinal specifying the type of thunk.
        /// </summary>
        public ThunkOrdinal Ordinal { get; private set; }

        /// <summary>
        /// Gets the name of thunk.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Variant field, depending on value of <see cref="Ordinal"/>. If
        /// <see cref="Ordinal"/> is <see cref="ThunkOrdinal.Standard"/>, there is no variant field. If
        /// <see cref="Ordinal"/> is <see cref="ThunkOrdinal.ThisAdjustor"/>, the variant field is a two-byte
        /// signed value specifying the delta to be added to the <c>this</c> pointer, followed by the
        /// length-prefixed name of the target function. If <see cref="Ordinal"/> is <see cref="ThunkOrdinal.VirtualCall"/>,
        /// then the variant field is a two-byte signed displacement into the virtual table. If <see cref="Ordinal"/>
        /// is <see cref="ThunkOrdinal.PCode"/>, the variant is the segment:offset of the pcode entry point.
        /// </summary>
        public byte[] VariantData { get; private set; }

        /// <summary>
        /// Reads <see cref="Thunk32Symbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        /// <param name="dataLength">Record data length.</param>
        public static Thunk32Symbol Read(IBinaryReader reader, SymbolRecordKind kind, uint dataLength)
        {
            long start = reader.Position;
            return new Thunk32Symbol
            {
                Kind = kind,
                Parent = reader.ReadUint(),
                End = reader.ReadUint(),
                Next = reader.ReadUint(),
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Length = reader.ReadUshort(),
                Ordinal = (ThunkOrdinal)reader.ReadByte(),
                Name = reader.ReadCString(),
                VariantData = reader.ReadByteArray((int)(dataLength - (reader.Position - start))),
            };
        }
    }
}
