#pragma warning disable 1591

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    // Part of member attribute flags. (CV_methodprop_e)
    public enum MethodKind : byte
    {
        Vanilla = 0x00,
        Virtual = 0x01,
        Static = 0x02,
        Friend = 0x03,
        IntroducingVirtual = 0x04,
        PureVirtual = 0x05,
        PureIntroducingVirtual = 0x06
    }
}
