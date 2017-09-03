using System;

namespace DIA
{
    /// <summary>
    /// Control how undecorated names are returned.
    /// </summary>
    [Flags]
    public enum UndecoratedNameOptions : uint
    {
        /// <summary>
        /// Enables full undecoration.
        /// </summary>
        Complete = 0x0000,

        /// <summary>
        /// Removes leading underscores from Microsoft extended keywords.
        /// </summary>
        NoLeadingUnderscores = 0x0001,

        /// <summary>
        /// Disables expansion of Microsoft extended keywords.
        /// </summary>
        NoMsKeywords = 0x0002,

        /// <summary>
        /// Disables expansion of return type for primary declaration.
        /// </summary>
        NoFunctionReturns = 0x0004,

        /// <summary>
        /// Disables expansion of the declaration model.
        /// </summary>
        NoAllocationModel = 0x0008,

        /// <summary>
        /// Disables expansion of the declaration language specifier.
        /// </summary>
        NoAllocationLanguage = 0x0010,

        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved1 = 0x0020,

        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved2 = 0x0040,

        /// <summary>
        /// Disables all modifiers on the <c>this</c> type.
        /// </summary>
        NoThisType = 0x0060,

        /// <summary>
        /// Disables expansion of access specifiers for members.
        /// </summary>
        NoAccessSpecifiers = 0x0080,

        /// <summary>
        /// Disables expansion of "throw-signatures" for functions and pointers to functions.
        /// </summary>
        NoThrowSignatures = 0x0100,

        /// <summary>
        /// Disables expansion of <c>static</c> or <c>virtual</c> members.
        /// </summary>
        NoMemberType = 0x0200,

        /// <summary>
        /// Disables expansion of the Microsoft model for UDT returns.
        /// </summary>
        NoReturnUdtModel = 0x0400,

        /// <summary>
        /// Undecorates 32-bit decorated names.
        /// </summary>
        _32bitDecode = 0x0800,

        /// <summary>
        /// Gets only the name for primary declaration; returns just [scope::]name. Expands template params.
        /// </summary>
        NameOnly = 0x1000,

        /// <summary>
        /// Input is just a type encoding; composes an abstract declarator.
        /// </summary>
        TypeOnly = 0x2000,

        /// <summary>
        /// The real template parameters are available.
        /// </summary>
        HaveParameters = 0x4000,

        /// <summary>
        /// Suppresses enum/class/struct/union.
        /// </summary>
        NoEscu = 0x8000,

        /// <summary>
        /// Suppresses check for valid identifier characters.
        /// </summary>
        NoIdentCharCheck = 0x10000,

        /// <summary>
        /// Does not include ptr64 in output.
        /// </summary>
        NoPtr64 = 0x20000,
    }
}
