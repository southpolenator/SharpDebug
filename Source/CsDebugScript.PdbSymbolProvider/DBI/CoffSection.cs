using CsDebugScript.PdbSymbolProvider.Utility;
using System.Text;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// COFF section header.
    /// </summary>
    public struct CoffSectionHeader
    {
        /// <summary>
        /// Size of <see cref="CoffSectionHeader"/> structure in bytes.
        /// </summary>
        public const int Size = 40;

        /// <summary>
        /// Gets array of bytes that represents ASCII name of COFF section.
        /// </summary>
        public byte[] NameBytes { get; private set; }

        /// <summary>
        /// Gets the number of bytes needed when this section is loaded into memory.
        /// </summary>
        public uint VirtualSize { get; private set; }

        /// <summary>
        /// Gets the address at which the section data should be loaded into memory.
        /// </summary>
        public uint VirtualAddress { get; private set; }

        /// <summary>
        /// Gets the number of bytes of data stored in the file for this section.
        /// </summary>
        public uint SizeOfRawData { get; private set; }

        /// <summary>
        /// Gets the file offset of the section data.
        /// </summary>
        public uint PointerToRawData { get; private set; }

        /// <summary>
        /// Gets the file offset of the relocation entries for this section.
        /// </summary>
        public uint PointerToRelocations { get; private set; }

        /// <summary>
        /// Gets the file offset of the line number entries for this section.
        /// </summary>
        public uint PointerToLinenumbers { get; private set; }

        /// <summary>
        /// Gets the number of relocation entries for this section.
        /// </summary>
        /// <remarks>
        /// Beware files with more than 65535 entries; this field truncates the value with no other way to get the "real" value.
        /// </remarks>
        public ushort NumberOfRelocations { get; private set; }

        /// <summary>
        /// The number of line number entries for this section.
        /// </summary>
        /// <remarks>
        /// Beware files with more than 65535 entries; this field truncates the value with no other way to get the "real" value.
        /// </remarks>
        public ushort NumberOfLinenumbers { get; private set; }

        /// <summary>
        /// Gets the flags provide additional information for each section.
        /// </summary>
        public ImageSectionCharacteristics Characteristics { get; private set; }

        /// <summary>
        /// Gets name of the COFF section (<see cref="NameBytes"/> converted to ASCII string).
        /// </summary>
        public string Name
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < NameBytes.Length; i++)
                    if (NameBytes[i] == 0)
                        return sb.ToString();
                    else
                        sb.Append((char)NameBytes[i]);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Reads <see cref="CoffSectionHeader"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static CoffSectionHeader Read(IBinaryReader reader)
        {
            return new CoffSectionHeader
            {
                NameBytes = reader.ReadByteArray(8),
                VirtualSize = reader.ReadUint(),
                VirtualAddress = reader.ReadUint(),
                SizeOfRawData = reader.ReadUint(),
                PointerToRawData = reader.ReadUint(),
                PointerToRelocations = reader.ReadUint(),
                PointerToLinenumbers = reader.ReadUint(),
                NumberOfRelocations = reader.ReadUshort(),
                NumberOfLinenumbers = reader.ReadUshort(),
                Characteristics = (ImageSectionCharacteristics)reader.ReadUint(),
            };
        }
    }
}
