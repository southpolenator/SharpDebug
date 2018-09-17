using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents function options used in type records.
    /// </summary>
    [Flags]
    public enum FunctionOptions : byte
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Function returns cxx user defined type.
        /// </summary>
        CxxReturnUdt = 0x01,

        /// <summary>
        /// Function is a constructor.
        /// </summary>
        Constructor = 0x02,

        /// <summary>
        /// Function is a constructor with virtual bases.
        /// </summary>
        ConstructorWithVirtualBases = 0x04,
    }
}
