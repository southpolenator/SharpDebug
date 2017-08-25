using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// Simple Portable Executable image reader.
    /// </summary>
    /// <seealso cref="CsDebugScript.DwarfSymbolProvider.IDwarfImage" />
    internal class PeImage : IDwarfImage
    {
        /// <summary>
        /// The DOS header
        /// </summary>
        private IMAGE_DOS_HEADER dosHeader;

        /// <summary>
        /// The NT headers for 32-bit image
        /// </summary>
        private IMAGE_NT_HEADERS32 ntHeaders32;

        /// <summary>
        /// The NT headers for 64-bit image
        /// </summary>
        private IMAGE_NT_HEADERS64 ntHeaders64;

        /// <summary>
        /// The file header
        /// </summary>
        private IMAGE_FILE_HEADER fileHeader;

        /// <summary>
        /// Image Section headers. Number of sections is in the file header.
        /// </summary>
        private IMAGE_SECTION_HEADER[] imageSectionHeaders;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeImage"/> class.
        /// </summary>
        /// <param name="path">The image path.</param>
        public PeImage(string path)
            : this(File.ReadAllBytes(path))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeImage"/> class.
        /// </summary>
        /// <param name="data">The image data.</param>
        public PeImage(byte[] data)
        {
            ParseData(data);
        }

        /// <summary>
        /// Gets the debug data.
        /// </summary>
        public byte[] DebugData { get; private set; }

        /// <summary>
        /// Gets the debug data description.
        /// </summary>
        public byte[] DebugDataDescription { get; private set; }

        /// <summary>
        /// Gets the debug data strings.
        /// </summary>
        public byte[] DebugDataStrings { get; private set; }

        /// <summary>
        /// Gets the debug line.
        /// </summary>
        public byte[] DebugLine { get; private set; }

        /// <summary>
        /// Gets the debug frame.
        /// </summary>
        public byte[] DebugFrame { get; private set; }

        /// <summary>
        /// Gets the code segment offset.
        /// </summary>
        public ulong CodeSegmentOffset { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDwarfImage" /> is 64 bit image.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is 64 bit image; otherwise, <c>false</c>.
        /// </value>
        public bool Is64bit { get; private set; }

        /// <summary>
        /// Gets the public symbols.
        /// </summary>
        public IReadOnlyList<PublicSymbol> PublicSymbols { get; private set; }

        /// <summary>
        /// Parses the specified data.
        /// </summary>
        /// <param name="data">The PE image data.</param>
        private void ParseData(byte[] data)
        {
            using (DwarfMemoryReader reader = new DwarfMemoryReader(data))
            {
                dosHeader = reader.ReadStructure<IMAGE_DOS_HEADER>();
                if (dosHeader.e_magic != IMAGE_DOS_SIGNATURE)
                {
                    throw new ArgumentException($"Invalid IMAGE_DOS_HEADER magic constant. Expected: 0x{IMAGE_DOS_SIGNATURE:X}, Got: 0x{dosHeader.e_magic:X}");
                }

                reader.Position = (int)dosHeader.e_lfanew;
                ntHeaders32 = reader.ReadStructure<IMAGE_NT_HEADERS32>();
                if (ntHeaders32.Signature != IMAGE_NT_SIGNATURE)
                {
                    throw new ArgumentException($"Invalid optional header signature. Expected: 0x{IMAGE_NT_SIGNATURE:X}, Got: 0x{ntHeaders32.Signature:X}");
                }
                if (ntHeaders32.FileHeader.Machine == IMAGE_FILE_MACHINE_AMD64
                    || ntHeaders32.FileHeader.Machine == IMAGE_FILE_MACHINE_IA64)
                {
                    reader.Position = (int)dosHeader.e_lfanew;
                    ntHeaders64 = reader.ReadStructure<IMAGE_NT_HEADERS64>();
                    Is64bit = true;
                    fileHeader = ntHeaders64.FileHeader;
                    reader.Position += ntHeaders64.FileHeader.SizeOfOptionalHeader - Marshal.SizeOf<IMAGE_OPTIONAL_HEADER64>();
                    CodeSegmentOffset = ntHeaders64.OptionalHeader.ImageBase;
                }
                else
                {
                    Is64bit = false;
                    fileHeader = ntHeaders32.FileHeader;
                    reader.Position += ntHeaders32.FileHeader.SizeOfOptionalHeader - Marshal.SizeOf<IMAGE_OPTIONAL_HEADER32>();
                    CodeSegmentOffset = ntHeaders32.OptionalHeader.ImageBase;
                }

                // Load image section headers
                uint stringTablePosition = fileHeader.PointerToSymbolTable + fileHeader.NumberOfSymbols * IMAGE_SIZEOF_SYMBOL;

                imageSectionHeaders = new IMAGE_SECTION_HEADER[fileHeader.NumberOfSections];
                for (int section = 0; section < imageSectionHeaders.Length; section++)
                {
                    IMAGE_SECTION_HEADER imageSectionHeader = reader.ReadStructure<IMAGE_SECTION_HEADER>();
                    imageSectionHeaders[section] = imageSectionHeader;
                    string name = imageSectionHeader.Section;
                    if (imageSectionHeader.Name[0] == '/')
                    {
                        uint position = stringTablePosition + uint.Parse(imageSectionHeader.Section.Substring(1));

                        name = reader.ReadString((int)position);
                    }

                    switch (name)
                    {
                        case ".debug_info":
                            DebugData = reader.ReadBlock(imageSectionHeader.SizeInImage, (int)imageSectionHeader.PointerToRawData);
                            break;
                        case ".debug_abbrev":
                            DebugDataDescription = reader.ReadBlock(imageSectionHeader.SizeInImage, (int)imageSectionHeader.PointerToRawData);
                            break;
                        case ".debug_line":
                            DebugLine = reader.ReadBlock(imageSectionHeader.SizeInImage, (int)imageSectionHeader.PointerToRawData);
                            break;
                        case ".debug_frame":
                            DebugFrame = reader.ReadBlock(imageSectionHeader.SizeInImage, (int)imageSectionHeader.PointerToRawData);
                            break;
                        case ".debug_str":
                            DebugDataStrings = reader.ReadBlock(imageSectionHeader.SizeInImage, (int)imageSectionHeader.PointerToRawData);
                            break;
                    }
                }

                // Load image symbols
                List<PublicSymbol> publicSymbols = new List<PublicSymbol>();
                byte toSkip = 0;

                reader.Position = (int)fileHeader.PointerToSymbolTable;
                for (uint i = 0; i < fileHeader.NumberOfSymbols; i++)
                {
                    int position = reader.Position;
                    IMAGE_SYMBOL symbol = reader.ReadStructure<IMAGE_SYMBOL>();

                    if (toSkip == 0)
                    {
                        string name = symbol.SymbolName;

                        if (string.IsNullOrEmpty(name))
                        {
                            int stringPosition = (int)reader.ReadUint(position);
                            stringPosition = (int)reader.ReadUint(position + 4);

                            name = reader.ReadString((int)stringTablePosition + stringPosition);
                        }


                        if (symbol.SectionNumber > 0 && symbol.SectionNumber <= imageSectionHeaders.Length)
                        {
                            uint sectionAddress = imageSectionHeaders[symbol.SectionNumber - 1].VirtualAddress;
                            sectionAddress += symbol.Value;

                            publicSymbols.Add(new PublicSymbol(name, sectionAddress));
                        }

                        toSkip = symbol.NumberOfAuxSymbols;
                    }
                    else
                    {
                        toSkip--;
                    }
                }
                PublicSymbols = publicSymbols;
            }
        }

        #region File Header Structures
        public const ushort IMAGE_DOS_SIGNATURE = 0x5A4D;
        public const uint IMAGE_NT_SIGNATURE = 0x00004550;
        public const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
        public const ushort IMAGE_FILE_MACHINE_IA64 = 0x0200;
        public const uint IMAGE_SIZEOF_SYMBOL = 18;

        public struct IMAGE_DOS_HEADER
        {
            public UInt16 e_magic;              // Magic number
            public UInt16 e_cblp;               // Bytes on last page of file
            public UInt16 e_cp;                 // Pages in file
            public UInt16 e_crlc;               // Relocations
            public UInt16 e_cparhdr;            // Size of header in paragraphs
            public UInt16 e_minalloc;           // Minimum extra paragraphs needed
            public UInt16 e_maxalloc;           // Maximum extra paragraphs needed
            public UInt16 e_ss;                 // Initial (relative) SS value
            public UInt16 e_sp;                 // Initial SP value
            public UInt16 e_csum;               // Checksum
            public UInt16 e_ip;                 // Initial IP value
            public UInt16 e_cs;                 // Initial (relative) CS value
            public UInt16 e_lfarlc;             // File address of relocation table
            public UInt16 e_ovno;               // Overlay number
            public UInt16 e_res_0;              // Reserved words
            public UInt16 e_res_1;              // Reserved words
            public UInt16 e_res_2;              // Reserved words
            public UInt16 e_res_3;              // Reserved words
            public UInt16 e_oemid;              // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo;            // OEM information; e_oemid specific
            public UInt16 e_res2_0;             // Reserved words
            public UInt16 e_res2_1;             // Reserved words
            public UInt16 e_res2_2;             // Reserved words
            public UInt16 e_res2_3;             // Reserved words
            public UInt16 e_res2_4;             // Reserved words
            public UInt16 e_res2_5;             // Reserved words
            public UInt16 e_res2_6;             // Reserved words
            public UInt16 e_res2_7;             // Reserved words
            public UInt16 e_res2_8;             // Reserved words
            public UInt16 e_res2_9;             // Reserved words
            public UInt32 e_lfanew;             // File address of new exe header
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_NT_HEADERS64
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_NT_HEADERS32
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public UInt16 Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;
            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;

            public IMAGE_DATA_DIRECTORY ExportTable;
            public IMAGE_DATA_DIRECTORY ImportTable;
            public IMAGE_DATA_DIRECTORY ResourceTable;
            public IMAGE_DATA_DIRECTORY ExceptionTable;
            public IMAGE_DATA_DIRECTORY CertificateTable;
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;
            public IMAGE_DATA_DIRECTORY Debug;
            public IMAGE_DATA_DIRECTORY Architecture;
            public IMAGE_DATA_DIRECTORY GlobalPtr;
            public IMAGE_DATA_DIRECTORY TLSTable;
            public IMAGE_DATA_DIRECTORY LoadConfigTable;
            public IMAGE_DATA_DIRECTORY BoundImport;
            public IMAGE_DATA_DIRECTORY IAT;
            public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
            public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
            public IMAGE_DATA_DIRECTORY Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public UInt16 Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt64 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt64 SizeOfStackReserve;
            public UInt64 SizeOfStackCommit;
            public UInt64 SizeOfHeapReserve;
            public UInt64 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;

            public IMAGE_DATA_DIRECTORY ExportTable;
            public IMAGE_DATA_DIRECTORY ImportTable;
            public IMAGE_DATA_DIRECTORY ResourceTable;
            public IMAGE_DATA_DIRECTORY ExceptionTable;
            public IMAGE_DATA_DIRECTORY CertificateTable;
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;
            public IMAGE_DATA_DIRECTORY Debug;
            public IMAGE_DATA_DIRECTORY Architecture;
            public IMAGE_DATA_DIRECTORY GlobalPtr;
            public IMAGE_DATA_DIRECTORY TLSTable;
            public IMAGE_DATA_DIRECTORY LoadConfigTable;
            public IMAGE_DATA_DIRECTORY BoundImport;
            public IMAGE_DATA_DIRECTORY IAT;
            public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
            public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
            public IMAGE_DATA_DIRECTORY Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_FILE_HEADER
        {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }

        [StructLayout(LayoutKind.Explicit, Size = 18)]
        public struct IMAGE_SYMBOL
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public char[] Name;
            [FieldOffset(8)]
            public UInt32 Value;
            [FieldOffset(12)]
            public Int16 SectionNumber;
            [FieldOffset(14)]
            public UInt16 Type;
            [FieldOffset(16)]
            public byte StorageClass;
            [FieldOffset(17)]
            public byte NumberOfAuxSymbols;

            public string SymbolName
            {
                get
                {
                    int count = 0;

                    while (count < Name.Length && Name[count] != 0)
                    {
                        count++;
                    }
                    return new string(Name, 0, count);
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_SECTION_HEADER
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public char[] Name;
            [FieldOffset(8)]
            public UInt32 VirtualSize;
            [FieldOffset(12)]
            public UInt32 VirtualAddress;
            [FieldOffset(16)]
            public UInt32 SizeOfRawData;
            [FieldOffset(20)]
            public UInt32 PointerToRawData;
            [FieldOffset(24)]
            public UInt32 PointerToRelocations;
            [FieldOffset(28)]
            public UInt32 PointerToLinenumbers;
            [FieldOffset(32)]
            public UInt16 NumberOfRelocations;
            [FieldOffset(34)]
            public UInt16 NumberOfLinenumbers;
            [FieldOffset(36)]
            public DataSectionFlags Characteristics;

            public string Section
            {
                get
                {
                    int count = 0;

                    while (count < Name.Length && Name[count] != 0)
                    {
                        count++;
                    }
                    return new string(Name, 0, count);
                }
            }

            public uint SizeInImage
            {
                get
                {
                    if (VirtualSize == 0)
                    {
                        return SizeOfRawData;
                    }
                    return SizeOfRawData < VirtualSize ? SizeOfRawData : VirtualSize;
                }
            }
        }

        [Flags]
        public enum DataSectionFlags : uint
        {
            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeReg = 0x00000000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeDsect = 0x00000001,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeNoLoad = 0x00000002,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeGroup = 0x00000004,

            /// <summary>
            /// The section should not be padded to the next boundary. This flag is obsolete and is replaced by IMAGE_SCN_ALIGN_1BYTES. This is valid only for object files.
            /// </summary>
            TypeNoPadded = 0x00000008,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeCopy = 0x00000010,

            /// <summary>
            /// The section contains executable code.
            /// </summary>
            ContentCode = 0x00000020,

            /// <summary>
            /// The section contains initialized data.
            /// </summary>
            ContentInitializedData = 0x00000040,

            /// <summary>
            /// The section contains uninitialized data.
            /// </summary>
            ContentUninitializedData = 0x00000080,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            LinkOther = 0x00000100,

            /// <summary>
            /// The section contains comments or other information. The .drectve section has this type. This is valid for object files only.
            /// </summary>
            LinkInfo = 0x00000200,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            TypeOver = 0x00000400,

            /// <summary>
            /// The section will not become part of the image. This is valid only for object files.
            /// </summary>
            LinkRemove = 0x00000800,

            /// <summary>
            /// The section contains COMDAT data. For more information, see section 5.5.6, COMDAT Sections (Object Only). This is valid only for object files.
            /// </summary>
            LinkComDat = 0x00001000,

            /// <summary>
            /// Reset speculative exceptions handling bits in the TLB entries for this section.
            /// </summary>
            NoDeferSpecExceptions = 0x00004000,

            /// <summary>
            /// The section contains data referenced through the global pointer (GP).
            /// </summary>
            RelativeGP = 0x00008000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            MemPurgeable = 0x00020000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            Memory16Bit = 0x00020000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            MemoryLocked = 0x00040000,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            MemoryPreload = 0x00080000,

            /// <summary>
            /// Align data on a 1-byte boundary. Valid only for object files.
            /// </summary>
            Align1Bytes = 0x00100000,

            /// <summary>
            /// Align data on a 2-byte boundary. Valid only for object files.
            /// </summary>
            Align2Bytes = 0x00200000,

            /// <summary>
            /// Align data on a 4-byte boundary. Valid only for object files.
            /// </summary>
            Align4Bytes = 0x00300000,

            /// <summary>
            /// Align data on an 8-byte boundary. Valid only for object files.
            /// </summary>
            Align8Bytes = 0x00400000,

            /// <summary>
            /// Align data on a 16-byte boundary. Valid only for object files.
            /// </summary>
            Align16Bytes = 0x00500000,

            /// <summary>
            /// Align data on a 32-byte boundary. Valid only for object files.
            /// </summary>
            Align32Bytes = 0x00600000,

            /// <summary>
            /// Align data on a 64-byte boundary. Valid only for object files.
            /// </summary>
            Align64Bytes = 0x00700000,

            /// <summary>
            /// Align data on a 128-byte boundary. Valid only for object files.
            /// </summary>
            Align128Bytes = 0x00800000,

            /// <summary>
            /// Align data on a 256-byte boundary. Valid only for object files.
            /// </summary>
            Align256Bytes = 0x00900000,

            /// <summary>
            /// Align data on a 512-byte boundary. Valid only for object files.
            /// </summary>
            Align512Bytes = 0x00A00000,

            /// <summary>
            /// Align data on a 1024-byte boundary. Valid only for object files.
            /// </summary>
            Align1024Bytes = 0x00B00000,

            /// <summary>
            /// Align data on a 2048-byte boundary. Valid only for object files.
            /// </summary>
            Align2048Bytes = 0x00C00000,

            /// <summary>
            /// Align data on a 4096-byte boundary. Valid only for object files.
            /// </summary>
            Align4096Bytes = 0x00D00000,

            /// <summary>
            /// Align data on an 8192-byte boundary. Valid only for object files.
            /// </summary>
            Align8192Bytes = 0x00E00000,

            /// <summary>
            /// The section contains extended relocations.
            /// </summary>
            LinkExtendedRelocationOverflow = 0x01000000,

            /// <summary>
            /// The section can be discarded as needed.
            /// </summary>
            MemoryDiscardable = 0x02000000,

            /// <summary>
            /// The section cannot be cached.
            /// </summary>
            MemoryNotCached = 0x04000000,

            /// <summary>
            /// The section is not pageable.
            /// </summary>
            MemoryNotPaged = 0x08000000,

            /// <summary>
            /// The section can be shared in memory.
            /// </summary>
            MemoryShared = 0x10000000,

            /// <summary>
            /// The section can be executed as code.
            /// </summary>
            MemoryExecute = 0x20000000,

            /// <summary>
            /// The section can be read.
            /// </summary>
            MemoryRead = 0x40000000,

            /// <summary>
            /// The section can be written to.
            /// </summary>
            MemoryWrite = 0x80000000
        }
        #endregion File Header Structures
    }
}
