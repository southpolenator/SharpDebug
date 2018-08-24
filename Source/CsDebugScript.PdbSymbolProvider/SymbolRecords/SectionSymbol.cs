using CsDebugScript.PdbSymbolProvider.DBI;
using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.SymbolRecords
{
    /// <summary>
    /// Represents section symbol record.
    /// </summary>
    public class SectionSymbol : SymbolRecord
    {
        /// <summary>
        /// Array of <see cref="SymbolRecordKind"/> that this class can read.
        /// </summary>
        public static readonly SymbolRecordKind[] Kinds = new SymbolRecordKind[]
        {
            SymbolRecordKind.S_SECTION
        };

        /// <summary>
        /// Gets the section number.
        /// </summary>
        public ushort SectionNumber { get; private set; }

        /// <summary>
        /// Gets the alignment.
        /// </summary>
        public byte Alignment { get; private set; }

        /// <summary>
        /// Padding to round next number to 4-bytes offset.
        /// </summary>
        public byte Padding { get; private set; }

        /// <summary>
        /// Gets the relative virtual address.
        /// </summary>
        public uint RelativeVirtualAddress { get; private set; }

        /// <summary>
        /// Gets the length in bytes.
        /// </summary>
        public uint Length { get; private set; }

        /// <summary>
        /// Gets the section characteristics.
        /// </summary>
        public ImageSectionCharacteristics Characteristics { get; private set; }

        /// <summary>
        /// Gets the section name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="SectionSymbol"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Symbol record kind.</param>
        public static SectionSymbol Read(IBinaryReader reader, SymbolRecordKind kind)
        {
            return new SectionSymbol
            {
                Kind = kind,
                SectionNumber = reader.ReadUshort(),
                Alignment = reader.ReadByte(),
                Padding = reader.ReadByte(),
                RelativeVirtualAddress = reader.ReadUint(),
                Length = reader.ReadUint(),
                Characteristics = (ImageSectionCharacteristics)reader.ReadUint(),
                Name = reader.ReadCString(),
            };
        }
    }
}
