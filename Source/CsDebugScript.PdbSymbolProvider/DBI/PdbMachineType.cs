#pragma warning disable 1591

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// Enumeration of machine types (architectures) used in PDB.
    /// </summary>
    public enum PdbMachineType
    {
        /// <summary>
        /// Unknown architecture.
        /// </summary>
        Unknown = 0x0,

        Am33 = 0x13,

        /// <summary>
        /// AMD64bit architecture. Also known as extended x86 to 64 bit.
        /// </summary>
        Amd64 = 0x8664,

        /// <summary>
        /// ARM architecture.
        /// </summary>
        Arm = 0x1C0,

        ArmNT = 0x1C4,

        Ebc = 0xEBC,

        /// <summary>
        /// Intel x86 architecture.
        /// </summary>
        x86 = 0x14C,

        /// <summary>
        /// Intel Itanium arhitecture.
        /// </summary>
        Ia64 = 0x200,

        M32R = 0x9041,

        Mips16 = 0x266,

        MipsFpu = 0x366,

        MipsFpu16 = 0x466,

        PowerPC = 0x1F0,

        PowerPCFP = 0x1F1,

        R4000 = 0x166,

        SH3 = 0x1A2,

        SH3DSP = 0x1A3,

        SH4 = 0x1A6,

        SH5 = 0x1A8,

        Thumb = 0x1C2,

        WceMipsV2 = 0x169,

        /// <summary>
        /// Invalid architecture.
        /// </summary>
        Invalid = 0xffff,
    }
}
