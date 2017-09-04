namespace DIA
{
    /// <summary>
    /// The checksum type is a value that can be mapped to a checksum algorithm.
    /// For example, the standard PDB file format can typically have one of the following values:
    /// </summary>
    public enum ChecksumAlgorithmType : uint
    {
        /// <summary>
        /// No checksum present.
        /// </summary>
        None = 0,

        /// <summary>
        /// Checksum generated with the MD5 hashing algorithm.
        /// </summary>
        Md5 = 1,

        /// <summary>
        /// Checksum generated with the SHA1 hashing algorithm.
        /// </summary>
        Sha1 = 2,
    }
}
