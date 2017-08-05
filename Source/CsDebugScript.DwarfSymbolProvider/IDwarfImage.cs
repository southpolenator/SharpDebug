namespace CsDebugScript.DwarfSymbolProvider
{
    internal interface IDwarfImage
    {
        byte[] DebugData { get; }

        byte[] DebugDataDescription { get; }

        byte[] DebugDataStrings { get; }

        byte[] DebugLine { get; }

        byte[] DebugFrame { get; }

        ulong CodeSegmentOffset { get; }

        bool Is64bit { get; }
    }
}
