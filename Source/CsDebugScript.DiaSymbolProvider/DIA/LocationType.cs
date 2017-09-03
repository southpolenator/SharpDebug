namespace DIA
{
    /// <summary>
    /// Indicates the kind of location information contained in a symbol.
    /// </summary>
    public enum LocationType : uint
    {
        /// <summary>
        /// Location information is unavailable.
        /// </summary>
        Null,

        /// <summary>
        /// Location is static.
        /// </summary>
        Static,

        /// <summary>
        /// Location is in thread local storage.
        /// </summary>
        TLS,

        /// <summary>
        /// Location is register-relative.
        /// </summary>
        RegRel,

        /// <summary>
        /// Location is this-relative.
        /// </summary>
        ThisRel,

        /// <summary>
        /// Location is in a register.
        /// </summary>
        Enregistered,

        /// <summary>
        /// Location is in a bit field.
        /// </summary>
        BitField,

        /// <summary>
        /// Location is a Microsoft Intermediate Language (MSIL) slot.
        /// </summary>
        Slot,

        /// <summary>
        /// Location is MSIL-relative.
        /// </summary>
        IlRel,

        /// <summary>
        /// Location is in metadata.
        /// </summary>
        MetaData,

        /// <summary>
        /// Location is in a constant value.
        /// </summary>
        Constant,

        /// <summary>
        /// The number of location types in this enumeration.
        /// </summary>
        TypeMax,
    };
}
