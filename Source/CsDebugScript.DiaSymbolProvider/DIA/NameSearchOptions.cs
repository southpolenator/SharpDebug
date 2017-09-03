using System;

namespace DIA
{
    /// <summary>
    /// Specifies the search options for symbol and file names.
    /// </summary>
    [Flags]
    public enum NameSearchOptions : uint
    {
        /// <summary>
        /// No options are specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Applies a case-sensitive name match.
        /// </summary>
        CaseSensitive = 0x1,

        /// <summary>
        /// Applies a case-insensitive name match.
        /// </summary>
        CaseInsensitive = 0x2,

        /// <summary>
        /// Treats names as paths and applies a filename.ext name match.
        /// </summary>
        FNameExt = 0x4,

        /// <summary>
        /// Applies a case-sensitive name match using asterisks (*) and question marks (?) as wildcards.
        /// </summary>
        RegularExpression = 0x8,

        /// <summary>
        /// Applies only to symbols that have both undecorated and decorated names.
        /// </summary>
        UndecoratedName = 0x10,
    }
}
