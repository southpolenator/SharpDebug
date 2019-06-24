namespace SharpDebug.CLR
{
    /// <summary>
    /// CLR code AppDomain interface. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public interface IClrAppDomain
    {
        /// <summary>
        /// Gets the runtime associated with this AppDomain.
        /// </summary>
        IClrRuntime Runtime { get; }

        /// <summary>
        /// Gets the array of modules loaded into this AppDomain.
        /// </summary>
        IClrModule[] Modules { get; }

        /// <summary>
        /// Gets the base directory for this AppDomain. This may return null if the targeted
        /// runtime does not support enumerating this information.
        /// </summary>
        string ApplicationBase { get; }

        /// <summary>
        /// Gets the configuration file used for the AppDomain. This may be null if there was
        /// no configuration file loaded, or if the targeted runtime does not support enumerating that data.
        /// </summary>
        string ConfigurationFile { get; }

        /// <summary>
        /// Gets the AppDomain's ID.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the name of the AppDomain, as specified when the domain was created.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the address of the AppDomain
        /// </summary>
        ulong Address { get; }
    }
}
