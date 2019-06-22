namespace SharpDebug.CLR
{
    /// <summary>
    /// CLR code module interface. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public interface IClrModule
    {
        /// <summary>
        /// Gets the base of the image loaded into memory. This may be 0 if there is not a physical file backing it.
        /// </summary>
        ulong ImageBase { get; }

        /// <summary>
        /// Gets the native module.
        /// </summary>
        Module Module { get; }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the name of the PDB file.
        /// </summary>
        string PdbFileName { get; }

        /// <summary>
        /// Gets the size of the image in memory.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// Attempts to obtain a <see cref="IClrType"/> based on the name of the type.
        /// Note this is a "best effort" due to the way that the DAC handles types.
        /// This function will fail for Generics, and types which have never been constructed in the target process.
        /// Please be sure to null-check the return value of this function.
        /// </summary>
        /// <param name="typeName">The name of the type. (This would be the EXACT value returned by <see cref="IClrType.Name"/>).</param>
        IClrType GetTypeByName(string typeName);
    }
}
