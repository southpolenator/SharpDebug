using System.Runtime.InteropServices;

namespace CsDebugScript.Native
{
    /// <summary>
    /// The DEBUG_MODULE_PARAMETERS structure contains most of the parameters for describing a module.
    /// </summary>
    internal struct DEBUG_MODULE_PARAMETERS
    {
        /// <summary>
        /// The location in the target's virtual address space of the module's base. If the value of Base is DEBUG_INVALID_OFFSET, the structure is invalid.
        /// </summary>
        public ulong Base;

        /// <summary>
        /// The size, in bytes, of the memory range that is occupied by the module.
        /// </summary>
        public uint Size;

        /// <summary>
        /// The date and time stamp of the module's executable file. This is the number of seconds elapsed since midnight (00:00:00), January 1, 1970
        /// Coordinated Universal Time (UTC) as stored in the image file header.
        /// </summary>
        public uint TimeDateStamp;

        /// <summary>
        /// The checksum of the image. This value can be zero.
        /// </summary>
        public uint Checksum;

        /// <summary>
        /// A bit-set that contains the module's flags. The bit-flags that can be present are as follows. Value Description:
        /// <para>DEBUG_MODULE_UNLOADED           The module was unloaded.</para>
        /// <para>DEBUG_MODULE_USER_MODE          The module is a user-mode module.</para>
        /// <para>DEBUG_MODULE_SYM_BAD_CHECKSUM   The checksum in the symbol file did not match the checksum for the module image.</para>
        /// </summary>
        public uint Flags;

        /// <summary>
        /// The type of symbols that are loaded for the module. This member can have one of the following values. Value Description:
        /// <para>DEBUG_SYMTYPE_NONE        No symbols are loaded.</para>
        /// <para>DEBUG_SYMTYPE_COFF        The symbols are in common object file format(COFF).</para>
        /// <para>DEBUG_SYMTYPE_CODEVIEW    The symbols are in Microsoft CodeView format.</para>
        /// <para>DEBUG_SYMTYPE_PDB         Symbols in PDB format have been loaded through the pre-Debug Interface Access(DIA) interface.</para>
        /// <para>DEBUG_SYMTYPE_EXPORT      No actual symbol files were found; symbol information was extracted from the binary file's export table.</para>
        /// <para>DEBUG_SYMTYPE_DEFERRED    The module was loaded, but the engine has deferred its loading of the symbols.</para>
        /// <para>DEBUG_SYMTYPE_SYM         Symbols in SYM format have been loaded.</para>
        /// <para>DEBUG_SYMTYPE_DIA         Symbols in PDB format have been loaded through the DIA interface.</para>
        /// </summary>
        public uint SymbolType;

        /// <summary>
        /// The size of the file name for the module. The size is measured in characters, including the terminator.
        /// </summary>
        public uint ImageNameSize;

        /// <summary>
        /// The size of the module name of the module. The size is measured in characters, including the terminator.
        /// </summary>
        public uint ModuleNameSize;

        /// <summary>
        /// The size of the loaded image name for the module. The size is measured in characters, including the terminator.
        /// </summary>
        public uint LoadedImageNameSize;

        /// <summary>
        /// The size of the symbol file name for the module. The size is measured in characters, including the terminator.
        /// </summary>
        public uint SymbolFileNameSize;

        /// <summary>
        /// The size of the mapped image name of the module. The size is measured in characters, including the terminator.
        /// </summary>
        public uint MappedImageNameSize;

        /// <summary>
        /// Reserved for system use.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ulong[] Reserved;
    }
}
