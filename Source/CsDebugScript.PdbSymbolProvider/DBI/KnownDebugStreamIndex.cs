#pragma warning disable 1591

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// Known stream indexes comming from DBI optional debug stream header stream.
    /// </summary>
    public enum KnownDebugStreamIndex : ushort
    {
        FPO,
        Exception,
        Fixup,
        OmapToSrc,
        OmapFromSrc,
        SectionHdr,
        TokenRidMap,
        Xdata,
        Pdata,
        NewFPO,
        SectionHdrOrig,
        Max
    }
}
