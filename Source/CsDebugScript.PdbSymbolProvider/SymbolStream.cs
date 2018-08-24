using CsDebugScript.Engine.Utility;
using CsDebugScript.PdbSymbolProvider.SymbolRecords;
using CsDebugScript.PdbSymbolProvider.Utility;
using System;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Represents PDB symbol stream.
    /// </summary>
    public class SymbolStream
    {
        /// <summary>
        /// Internal symbol reference structure.
        /// </summary>
        private struct SymbolReference
        {
            /// <summary>
            /// Offset of the symbol record data in the stream.
            /// </summary>
            public uint DataOffset;

            /// <summary>
            /// Symbol record data length in bytes.
            /// </summary>
            public ushort DataLen;

            /// <summary>
            /// Symbol record kind.
            /// </summary>
            public SymbolRecordKind Kind;
        }

        /// <summary>
        /// List of all symbol references in this stream.
        /// </summary>
        private List<SymbolReference> references;

        /// <summary>
        /// Dictionary cache of symbols by its kind.
        /// </summary>
        private DictionaryCache<SymbolRecordKind, SymbolRecord[]> symbolsByKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolStream"/> class.
        /// </summary>
        /// <param name="stream">PDB symbol stream.</param>
        public SymbolStream(PdbStream stream)
        {
            IBinaryReader reader = stream.Reader;

            Reader = reader;
            references = new List<SymbolReference>();

            long position = reader.Position, end = reader.Length;

            while (position < end)
            {
                RecordPrefix prefix = RecordPrefix.Read(reader);

                if (prefix.RecordLength < 2)
                    throw new Exception("CV corrupt record");

                SymbolRecordKind kind = (SymbolRecordKind)prefix.RecordKind;
                ushort dataLen = prefix.DataLen;

                references.Add(new SymbolReference
                {
                    DataOffset = (uint)position + RecordPrefix.Size,
                    Kind = kind,
                    DataLen = dataLen,
                });
                position += dataLen + RecordPrefix.Size;
                reader.ReadFake(dataLen);
            }

            symbolsByKind = new DictionaryCache<SymbolRecordKind, SymbolRecord[]>(GetSymbolsByKind);
        }

        /// <summary>
        /// Gets the PDB stream.
        /// </summary>
        public PdbStream Stream { get; private set; }

        /// <summary>
        /// Gets the stream binary reader.
        /// </summary>
        public IBinaryReader Reader { get; private set; }

        /// <summary>
        /// Indexing operator for getting all symbols of the given kind.
        /// </summary>
        /// <param name="kind">Symbol record kind that should be parsed from this symbol stream.</param>
        /// <returns>Array of symbol record for the specified symbol record kind.</returns>
        public SymbolRecord[] this[SymbolRecordKind kind] => symbolsByKind[kind];

        /// <summary>
        /// Parses all symbols of the specified symbol record kind.
        /// </summary>
        /// <param name="kind">Symbol record kind.</param>
        /// <returns>Array of symbol record for the specified symbol record kind.</returns>
        private SymbolRecord[] GetSymbolsByKind(SymbolRecordKind kind)
        {
            List<SymbolRecord> symbols = new List<SymbolRecord>();

            for (int i = 0; i < references.Count; i++)
                if (references[i].Kind == kind)
                    symbols.Add(GetSymbol(i));
            return symbols.ToArray();
        }

        /// <summary>
        /// Reads symbol record from symbol references for the specified index.
        /// </summary>
        /// <param name="index">Index of the symbol record.</param>
        private SymbolRecord GetSymbol(int index)
        {
            // Since DictionaryCache is allowing only single thread to call this function, we don't need to lock reader here.
            SymbolReference reference = references[index];

            Reader.Position = reference.DataOffset;
            switch (reference.Kind)
            {
                case SymbolRecordKind.S_GPROC32:
                case SymbolRecordKind.S_LPROC32:
                case SymbolRecordKind.S_GPROC32_ID:
                case SymbolRecordKind.S_LPROC32_ID:
                case SymbolRecordKind.S_LPROC32_DPC:
                case SymbolRecordKind.S_LPROC32_DPC_ID:
                    return ProcedureSymbol.Read(Reader, reference.Kind);
                case SymbolRecordKind.S_PUB32:
                    return Public32Symbol.Read(Reader, reference.Kind);
                case SymbolRecordKind.S_CONSTANT:
                case SymbolRecordKind.S_MANCONSTANT:
                    return ConstantSymbol.Read(Reader, reference.Kind);
                case SymbolRecordKind.S_LDATA32:
                case SymbolRecordKind.S_GDATA32:
                case SymbolRecordKind.S_LMANDATA:
                case SymbolRecordKind.S_GMANDATA:
                    return DataSymbol.Read(Reader, reference.Kind);
                case SymbolRecordKind.S_PROCREF:
                case SymbolRecordKind.S_LPROCREF:
                    return ProcedureReferenceSymbol.Read(Reader, reference.Kind);
                case SymbolRecordKind.S_UDT:
                case SymbolRecordKind.S_COBOLUDT:
                    return UdtSymbol.Read(Reader, reference.Kind);
                case SymbolRecordKind.S_LTHREAD32:
                case SymbolRecordKind.S_GTHREAD32:
                    return ThreadLocalDataSymbol.Read(Reader, reference.Kind);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
