using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// Simple ELF image reader.
    /// </summary>
    /// <seealso cref="CsDebugScript.DwarfSymbolProvider.IDwarfImage" />
    internal class ElfImage : IDwarfImage
    {
        /// <summary>
        /// The ELF interface
        /// </summary>
        private IELF elf;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElfImage"/> class.
        /// </summary>
        /// <param name="path">The image path.</param>
        public ElfImage(string path)
        {
            elf = ELFReader.Load(path);
        }

        /// <summary>
        /// Gets the code segment offset.
        /// </summary>
        public ulong CodeSegmentOffset
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the debug data.
        /// </summary>
        public byte[] DebugData
        {
            get
            {
                return LoadSection(".debug_info");
            }
        }

        /// <summary>
        /// Gets the debug data description.
        /// </summary>
        public byte[] DebugDataDescription
        {
            get
            {
                return LoadSection(".debug_abbrev");
            }
        }

        /// <summary>
        /// Gets the debug data strings.
        /// </summary>
        public byte[] DebugDataStrings
        {
            get
            {
                return LoadSection(".debug_str");
            }
        }

        /// <summary>
        /// Gets the debug frame.
        /// </summary>
        public byte[] DebugFrame
        {
            get
            {
                return LoadSection(".debug_frame");
            }
        }

        /// <summary>
        /// Gets the debug line.
        /// </summary>
        public byte[] DebugLine
        {
            get
            {
                return LoadSection(".debug_line");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDwarfImage" /> is 64 bit image.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is 64 bit image; otherwise, <c>false</c>.
        /// </value>
        public bool Is64bit
        {
            get
            {
                return elf.Class == Class.Bit64;
            }
        }

        /// <summary>
        /// Loads the section bytes specified by the name.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <returns>Section bytes.</returns>
        private byte[] LoadSection(string sectionName)
        {
            foreach (ISection section in elf.Sections)
            {
                if (section.Name == sectionName)
                {
                    return section.GetContents();
                }
            }

            return new byte[0];
        }
    }
}
