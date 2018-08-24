using CsDebugScript.Engine.Utility;
using CsDebugScript.PdbSymbolProvider.TypeRecords;
using CsDebugScript.PdbSymbolProvider.Utility;
using System;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider.TPI
{
    /// <summary>
    /// Represents TPI stream from PDB file.
    /// </summary>
    public class TpiStream
    {
        /// <summary>
        /// Minimum TPI stream hash buckets.
        /// </summary>
        public const uint MinTpiHashBuckets = 0x1000;

        /// <summary>
        /// Maximum TPI stream hash buckets.
        /// </summary>
        public const uint MaxTpiHashBuckets = 0x40000;

        /// <summary>
        /// Value for which stream index is invalid (stream doesn't exist).
        /// </summary>
        public const ushort InvalidStreamIndex = 0xFFFF;

        /// <summary>
        /// Internal type record reference structure.
        /// </summary>
        private struct RecordReference
        {
            public uint DataOffset;
            public ushort DataLen;
            public TypeLeafKind Kind;
        }

        /// <summary>
        /// List of all type record references in this stream.
        /// </summary>
        private List<RecordReference> references;

        /// <summary>
        /// Dictionary cache of all types by index.
        /// </summary>
        private DictionaryCache<int, TypeRecord> typesCache;

        /// <summary>
        /// Dictionary cache of type records by its kind.
        /// </summary>
        private DictionaryCache<TypeLeafKind, TypeRecord[]> typesByKindCache;

        /// <summary>
        /// Cache for <see cref="HashValues"/>.
        /// </summary>
        private SimpleCacheStruct<uint[]> hashValuesCache;

        /// <summary>
        /// Cache for <see cref="TypeIndexOffsets"/>.
        /// </summary>
        private SimpleCacheStruct<TypeIndexOffset[]> typeIndexOffsetsCache;

        /// <summary>
        /// Cache for <see cref="HashAdjusters"/>.
        /// </summary>
        private SimpleCacheStruct<HashTable> hashAdjustersCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbStream"/> class.
        /// </summary>
        /// <param name="stream">PDB symbol stream.</param>
        public TpiStream(PdbStream stream)
        {
            Stream = stream;
            if (stream.Reader.BytesRemaining < TpiStreamHeader.Size)
                throw new Exception("TPI Stream does not contain a header.");
            Header = TpiStreamHeader.Read(stream.Reader);

            if (Header.Version != PdbTpiVersion.V80)
                throw new Exception("Unsupported TPI Version.");

            if (Header.HeaderSize != TpiStreamHeader.Size)
                throw new Exception("Corrupt TPI Header size.");

            if (Header.HashKeySize != 4) // 4 = sizeof(uint)
                throw new Exception("TPI Stream expected 4 byte hash key size.");

            if (Header.HashBucketsCount < MinTpiHashBuckets || Header.HashBucketsCount > MaxTpiHashBuckets)
                throw new Exception("TPI Stream Invalid number of hash buckets.");

            // The actual type records themselves come from this stream
            TypeRecordsSubStream = Stream.Reader.ReadSubstream(Header.TypeRecordBytes);

            IBinaryReader reader = TypeRecordsSubStream;
            long position = reader.Position, end = reader.Length;

            references = new List<RecordReference>();
            while (position < end)
            {
                RecordPrefix prefix = RecordPrefix.Read(reader);

                if (prefix.RecordLength < 2)
                    throw new Exception("CV corrupt record");

                TypeLeafKind kind = (TypeLeafKind)prefix.RecordKind;
                ushort dataLen = prefix.DataLen;

                references.Add(new RecordReference
                {
                    DataOffset = (uint)position + RecordPrefix.Size,
                    Kind = kind,
                    DataLen = dataLen,
                });
                position += dataLen + RecordPrefix.Size;
                reader.ReadFake(dataLen);
            }
            typesCache = new DictionaryCache<int, TypeRecord>(ReadType);
            typesByKindCache = new DictionaryCache<TypeLeafKind, TypeRecord[]>(GetTypesByKind);

            // Hash indices, hash values, etc come from the hash stream.
            if (Header.HashStreamIndex != InvalidStreamIndex)
            {
                if (Header.HashStreamIndex >= Stream.File.Streams.Count)
                    throw new Exception("Invalid TPI hash stream index.");

                HashSubstream = Stream.File.Streams[Header.HashStreamIndex].Reader;
            }

            hashValuesCache = SimpleCache.CreateStruct(() =>
            {
                if (HashSubstream != null)
                {
                    // There should be a hash value for every type record, or no hashes at all.
                    uint numHashValues = Header.HashValueBuffer.Length / 4; // 4 = sizeof(uint)
                    if (numHashValues != references.Count && numHashValues != 0)
                        throw new Exception("TPI hash count does not match with the number of type records.");

                    HashSubstream.Position = Header.HashValueBuffer.Offset;
                    return HashSubstream.ReadUintArray(references.Count);
                }
                return null;
            });
            typeIndexOffsetsCache = SimpleCache.CreateStruct(() =>
            {
                if (HashSubstream != null)
                {
                    HashSubstream.Position = Header.IndexOffsetBuffer.Offset;
                    uint numTypeIndexOffsets = Header.IndexOffsetBuffer.Length / TypeIndexOffset.Size;
                    TypeIndexOffset[] typeIndexOffsets = new TypeIndexOffset[numTypeIndexOffsets];
                    for (uint i = 0; i < typeIndexOffsets.Length; i++)
                        typeIndexOffsets[i] = TypeIndexOffset.Read(HashSubstream);
                    return typeIndexOffsets;
                }
                return null;
            });
            hashAdjustersCache = SimpleCache.CreateStruct(() =>
            {
                if (HashSubstream != null && Header.HashAdjustersBuffer.Length > 0)
                {
                    HashSubstream.Position = Header.HashAdjustersBuffer.Offset;
                    return new HashTable(HashSubstream);
                }
                return null;
            });
        }

        /// <summary>
        /// Gets the PDB TPI stream.
        /// </summary>
        public PdbStream Stream { get; private set; }

        /// <summary>
        /// Gets the TPI stream header.
        /// </summary>
        public TpiStreamHeader Header { get; private set; }

        /// <summary>
        /// Gets the type records substream.
        /// </summary>
        public IBinaryReader TypeRecordsSubStream { get; private set; }

        /// <summary>
        /// Gets the hash substream.
        /// </summary>
        public IBinaryReader HashSubstream { get; private set; }

        /// <summary>
        /// Gets the type records hash values.
        /// </summary>
        public uint[] HashValues => hashValuesCache.Value;

        /// <summary>
        /// Gets the type info offsets.
        /// </summary>
        public TypeIndexOffset[] TypeIndexOffsets => typeIndexOffsetsCache.Value;

        /// <summary>
        /// Gets the hash adjusters.
        /// </summary>
        public HashTable HashAdjusters => hashAdjustersCache.Value;

        /// <summary>
        /// Gets the number of type records in the stream.
        /// </summary>
        public int TypeRecordCount => references.Count;

        /// <summary>
        /// Gets the <see cref="TypeRecord"/> associated with the <see cref="TypeIndex"/>.
        /// </summary>
        /// <param name="index">Type index to be retrieved.</param>
        public TypeRecord this[TypeIndex index] => typesCache[(int)index.ArrayIndex];

        /// <summary>
        /// Gets all <see cref="TypeRecord"/> entries associated with the specified <see cref="TypeLeafKind"/>.
        /// </summary>
        /// <param name="kind">Type record kind.</param>
        public TypeRecord[] this[TypeLeafKind kind] => typesByKindCache[kind];

        /// <summary>
        /// Gets all type indexes for the specified type record kind.
        /// </summary>
        /// <param name="kind">Type record kind.</param>
        public IEnumerable<TypeIndex> GetIndexes(TypeLeafKind kind)
        {
            for (int i = 0; i < references.Count; i++)
                if (references[i].Kind == kind)
                    yield return TypeIndex.FromArrayIndex(i);
        }

        /// <summary>
        /// Gets all <see cref="TypeRecord"/> entries associated with the specified <see cref="TypeLeafKind"/>.
        /// </summary>
        /// <param name="kind">Type record kind.</param>
        private TypeRecord[] GetTypesByKind(TypeLeafKind kind)
        {
            List<TypeRecord> records = new List<TypeRecord>();

            for (int i = 0; i < references.Count; i++)
                if (references[i].Kind == kind)
                    records.Add(typesCache[i]);
            return records.ToArray();
        }

        /// <summary>
        /// Reads type record for the specified type index.
        /// </summary>
        /// <param name="typeIndex">Type record index in <see cref="references"/> list.</param>
        /// <returns></returns>
        private TypeRecord ReadType(int typeIndex)
        {
            // Since DictionaryCache is allowing only single thread to call this function, we don't need to lock reader here.
            RecordReference reference = references[typeIndex];
            IBinaryReader reader = TypeRecordsSubStream;
            TypeRecord typeRecord;
            long dataEnd = reference.DataOffset + reference.DataLen;

            reader.Position = reference.DataOffset;
            switch (reference.Kind)
            {
                case TypeLeafKind.LF_MODIFIER:
                    typeRecord = ModifierRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_PROCEDURE:
                    typeRecord = ProcedureRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_MFUNCTION:
                    typeRecord = MemberFunctionRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_LABEL:
                    typeRecord = LabelRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_MFUNC_ID:
                    typeRecord = MemberFunctionIdRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_ARGLIST:
                    typeRecord = ArgumentListRecord.Read(reader, reference.Kind, reference.DataLen);
                    break;
                case TypeLeafKind.LF_SUBSTR_LIST:
                    typeRecord = StringListRecord.Read(reader, reference.Kind, reference.DataLen);
                    break;
                case TypeLeafKind.LF_POINTER:
                    typeRecord = PointerRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_NESTTYPE:
                    typeRecord = NestedTypeRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_FIELDLIST:
                    typeRecord = FieldListRecord.Read(reader, reference.Kind, reference.DataLen);
                    break;
                case TypeLeafKind.LF_ENUM:
                    typeRecord = EnumRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_CLASS:
                case TypeLeafKind.LF_STRUCTURE:
                case TypeLeafKind.LF_INTERFACE:
                    typeRecord = ClassRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_ARRAY:
                    typeRecord = ArrayRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_BITFIELD:
                    typeRecord = BitFieldRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_METHODLIST:
                    typeRecord = MethodOverloadListRecord.Read(reader, reference.Kind, reference.DataLen);
                    break;
                case TypeLeafKind.LF_UNION:
                    typeRecord = UnionRecord.Read(reader, reference.Kind);
                    break;
                case TypeLeafKind.LF_VTSHAPE:
                    typeRecord = VirtualFunctionTableShapeRecord.Read(reader, reference.Kind);
                    break;
                default:
                    throw new NotImplementedException($"Unknown reference kind: {reference.Kind}");
            }
            if (reader.Position + 4 < dataEnd || reader.Position > dataEnd)
                throw new Exception("Parsing went wrong");
            return typeRecord;
        }
    }
}
