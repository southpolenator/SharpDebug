using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// DBI Build number value extractor structure.
    /// </summary>
    public struct DbiBuildNumber
    {
        /// <summary>
        /// Mask for getting build minor version.
        /// </summary>
        private const ushort BuildMinorMask = 0x00FF;

        /// <summary>
        /// Shift for build minor version.
        /// </summary>
        private const ushort BuildMinorShift = 0;

        /// <summary>
        /// Mask for getting build major version.
        /// </summary>
        private const ushort BuildMajorMask = 0x7F00;

        /// <summary>
        /// Shift for build major version.
        /// </summary>
        private const ushort BuildMajorShift = 8;

        /// <summary>
        /// Bit mask for getting new verion format flag.
        /// </summary>
        private const ushort NewVersionFormatFlag = 0x8000;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbiBuildNumber"/> class.
        /// </summary>
        /// <param name="value">DBI build number value.</param>
        public DbiBuildNumber(ushort value)
        {
            Value = value;
        }

        /// <summary>
        /// DBI build number value.
        /// </summary>
        public ushort Value { get; private set; }

        /// <summary>
        /// Gets minor build number.
        /// </summary>
        public ushort Minor => (ushort)((Value & BuildMinorMask) >> BuildMinorShift);

        /// <summary>
        /// Gets major build number.
        /// </summary>
        public ushort Major => (ushort)((Value & BuildMajorMask) >> BuildMajorShift);

        /// <summary>
        /// Is new version format flag set?
        /// </summary>
        public bool NewVersionFormat => (Value & NewVersionFormatFlag) != 0;

        /// <summary>
        /// Reads <see cref="DbiBuildNumber"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public static DbiBuildNumber Read(IBinaryReader reader)
        {
            return new DbiBuildNumber(reader.ReadUshort());
        }
    }
}
