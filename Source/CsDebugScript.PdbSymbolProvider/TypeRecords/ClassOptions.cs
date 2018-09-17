#pragma warning disable 1591

using System;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents class options for <see cref="TagRecord"/>.
    /// </summary>
    [Flags]
    public enum ClassOptions : ushort
    {
        None = 0x0000,
        Packed = 0x0001,
        HasConstructorOrDestructor = 0x0002,
        HasOverloadedOperator = 0x0004,
        Nested = 0x0008,
        ContainsNestedClass = 0x0010,
        HasOverloadedAssignmentOperator = 0x0020,
        HasConversionOperator = 0x0040,
        ForwardReference = 0x0080,
        Scoped = 0x0100,
        HasUniqueName = 0x0200,
        Sealed = 0x0400,
        Intrinsic = 0x2000
    }
}
