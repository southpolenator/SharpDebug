#pragma warning disable 1591

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    public enum VirtualFunctionTableSlotKind : byte
    {
        Near16 = 0x00,
        Far16 = 0x01,
        This = 0x02,
        Outer = 0x03,
        Meta = 0x04,
        Near = 0x05,
        Far = 0x06
    }
}
