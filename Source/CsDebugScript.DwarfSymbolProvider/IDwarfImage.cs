namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// Interface that defines image format that contains DWARF data.
    /// </summary>
    internal interface IDwarfImage
    {
        /// <summary>
        /// Gets the debug data.
        /// </summary>
        byte[] DebugData { get; }

        /// <summary>
        /// Gets the debug data description.
        /// </summary>
        byte[] DebugDataDescription { get; }

        /// <summary>
        /// Gets the debug data strings.
        /// </summary>
        byte[] DebugDataStrings { get; }

        /// <summary>
        /// Gets the debug line.
        /// </summary>
        byte[] DebugLine { get; }

        /// <summary>
        /// Gets the debug frame.
        /// </summary>
        byte[] DebugFrame { get; }

        /// <summary>
        /// Gets the code segment offset.
        /// </summary>
        ulong CodeSegmentOffset { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDwarfImage"/> is 64 bit image.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is 64 bit image; otherwise, <c>false</c>.
        /// </value>
        bool Is64bit { get; }
    }
}
