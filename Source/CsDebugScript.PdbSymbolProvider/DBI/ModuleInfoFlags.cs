using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// Structure for extracting module info flags.
    /// </summary>
    public struct ModuleInfoFlags
    {
        /// <summary>
        /// Mask for getting <see cref="IsWritten"/> property.
        /// </summary>
        private const ushort WrittenFlagMask = 0x01;

        /// <summary>
        /// Mask for getting <see cref="IsECEnabled"/> property.
        /// </summary>
        private const ushort ECFlagMask = 0x02;

        /// <summary>
        /// Mask for getting <see cref="TypeServerIndex"/> property.
        /// </summary>
        private const ushort TypeServerIndexMask = 0xFF00;

        /// <summary>
        /// Bit shift for getting <see cref="TypeServerIndex"/> property.
        /// </summary>
        private const ushort TypeServerIndexShift = 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleInfoFlags"/> class.
        /// </summary>
        /// <param name="flags">Module info flags to be extracted.</param>
        public ModuleInfoFlags(ushort flags)
        {
            Flags = flags;
        }

        /// <summary>
        /// Original flags field.
        /// </summary>
        public ushort Flags { get; private set; }

        /// <summary>
        /// True if <see cref="DbiModuleDescriptor"/> is dirty
        /// </summary>
        public bool IsWritten => (Flags & WrittenFlagMask) != 0;

        /// <summary>
        /// Is EC symbolic info present?  (What is EC?)
        /// </summary>
        public bool IsECEnabled => (Flags & ECFlagMask) != 0;

        /// <summary>
        /// Type Server Index for this module
        /// </summary>
        public byte TypeServerIndex => (byte)((Flags & TypeServerIndexMask) >> TypeServerIndexShift);

        /// <summary>
        /// Reads <see cref="ModuleInfoFlags"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static ModuleInfoFlags Read(IBinaryReader reader)
        {
            return new ModuleInfoFlags(reader.ReadUshort());
        }
    }
}
