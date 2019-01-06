using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// These symbols are used for data declared with the thread storage attribute that is not exported
    /// from a module. In C and C++, thread symbols that are declared static are emitted as Local
    /// Thread Storage 16:32 symbols. Symbols that are emitted as Local Thread Storage 16:32 cannot
    /// be moved by the CVPACK utility into the global symbol table for the executable file.
    /// </summary>
    public class ThreadLocalDataSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_LTHREAD32, SymbolRecordKind.S_GTHREAD32
        };

        /// <summary>
        /// Gets the symbol type.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the offset into thread local storage.
        /// </summary>
        public uint Offset { get; private set; }

        /// <summary>
        /// Gets the segment of thread local storage.
        /// </summary>
        public ushort Segment { get; private set; }

        /// <summary>
        /// Gets the data symbol name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="ThreadLocalDataSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static ThreadLocalDataSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new ThreadLocalDataSymbol
            {
                Kind = kind,
                Type = TypeIndex.Read(reader),
                Offset = reader.ReadUint(),
                Segment = reader.ReadUshort(),
                Name = reader.ReadCString(),
            };
        }
    }
}
