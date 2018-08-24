namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Pointer type of the simple built-in type.
    /// </summary>
    public enum SimpleTypeMode : uint
    {
        /// <summary>
        /// Not a pointer
        /// </summary>
        Direct = 0x00000000,

        /// <summary>
        /// Near pointer
        /// </summary>
        NearPointer = 0x00000100,

        /// <summary>
        /// Far pointer
        /// </summary>
        FarPointer = 0x00000200,

        /// <summary>
        /// Huge pointer
        /// </summary>
        HugePointer = 0x00000300,

        /// <summary>
        /// 32 bit near pointer
        /// </summary>
        NearPointer32 = 0x00000400,

        /// <summary>
        /// 32 bit far pointer
        /// </summary>
        FarPointer32 = 0x00000500,

        /// <summary>
        /// 64 bit near pointer
        /// </summary>
        NearPointer64 = 0x00000600,

        /// <summary>
        /// 128 bit near pointer
        /// </summary>
        NearPointer128 = 0x00000700,
    }
}
