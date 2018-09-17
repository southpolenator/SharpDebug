using System;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// The characteristics of the image.
    /// </summary>
    [Flags]
    public enum ImageSectionCharacteristics : uint
    {
        /// <summary>
        /// IMAGE_SCN_TYPE_NO_PAD - The section should not be padded to the next boundary. This flag is obsolete and is replaced by IMAGE_SCN_ALIGN_1BYTES.
        /// </summary>
        TypeNoPadding = 0x00000008,

        /// <summary>
        /// IMAGE_SCN_CNT_CODE - The section contains executable code.
        /// </summary>
        ContainsCode = 0x00000020,

        /// <summary>
        /// IMAGE_SCN_CNT_INITIALIZED_DATA - The section contains initialized data.
        /// </summary>
        ContainsInitializedData = 0x00000040,

        /// <summary>
        /// IMAGE_SCN_CNT_UNINITIALIZED_DATA - The section contains uninitialized data.
        /// </summary>
        ContainsUninitializedData = 0x00000080,

        /// <summary>
        /// IMAGE_SCN_LNK_OTHER - Reserved,
        /// </summary>
        LinkerOther = 0x00000100,

        /// <summary>
        /// IMAGE_SCN_LNK_INFO - The section contains comments or other information. This is valid only for object files.
        /// </summary>
        LinkerInfo = 0x00000200,

        /// <summary>
        /// IMAGE_SCN_LNK_REMOVE - The section will not become part of the image. This is valid only for object files.
        /// </summary>
        LinkerRemove = 0x00000800,

        /// <summary>
        /// IMAGE_SCN_LNK_COMDAT - The section contains COMDAT data. This is valid only for object files.
        /// </summary>
        LinkerComdat = 0x00001000,

        /// <summary>
        /// IMAGE_SCN_NO_DEFER_SPEC_EXC - Reset speculative exceptions handling bits in the TLB entries for this section.
        /// </summary>
        NoDeferSpeculativeExceptions = 0x00004000,

        /// <summary>
        /// IMAGE_SCN_GPREL - The section contains data referenced through the global pointer.
        /// </summary>
        GlobalPointerRelative = 0x00008000,

        /// <summary>
        /// IMAGE_SCN_MEM_PURGEABLE - Reserved,
        /// </summary>
        MemoryPurgable = 0x00020000,

        /// <summary>
        /// IMAGE_SCN_MEM_LOCKED - Reserved,
        /// </summary>
        MemoryLocked = 0x00040000,

        /// <summary>
        /// IMAGE_SCN_MEM_PRELOAD - Reserved,
        /// </summary>
        MemoryPreload = 0x00080000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_1BYTES - Align data on a 1-byte boundary. This is valid only for object files.
        /// </summary>
        Align1Bytes = 0x00100000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_2BYTES - Align data on a 2-byte boundary. This is valid only for object files.
        /// </summary>
        Align2Bytes = 0x00200000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_4BYTES - Align data on a 4-byte boundary. This is valid only for object files.
        /// </summary>
        Align4Bytes = 0x00300000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_8BYTES - Align data on a 8-byte boundary. This is valid only for object files.
        /// </summary>
        Align8Bytes = 0x00400000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_16BYTES - Align data on a 16-byte boundary. This is valid only for object files.
        /// </summary>
        Align16Bytes = 0x00500000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_32BYTES - Align data on a 32-byte boundary. This is valid only for object files.
        /// </summary>
        Align32Bytes = 0x00600000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_64BYTES - Align data on a 64-byte boundary. This is valid only for object files.
        /// </summary>
        Align64Bytes = 0x00700000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_128BYTES - Align data on a 128-byte boundary. This is valid only for object files.
        /// </summary>
        Align128Bytes = 0x00800000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_256BYTES - Align data on a 256-byte boundary. This is valid only for object files.
        /// </summary>
        Align256Bytes = 0x00900000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_512BYTES - Align data on a 512-byte boundary. This is valid only for object files.
        /// </summary>
        Align512Bytes = 0x00A00000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_1024BYTES - Align data on a 1024-byte boundary. This is valid only for object files.
        /// </summary>
        Align1024Bytes = 0x00B00000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_2048BYTES - Align data on a 2048-byte boundary. This is valid only for object files.
        /// </summary>
        Align2048Bytes = 0x00C00000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_4096BYTES - Align data on a 4096-byte boundary. This is valid only for object files.
        /// </summary>
        Align4096Bytes = 0x00D00000,

        /// <summary>
        /// IMAGE_SCN_ALIGN_8192BYTES - Align data on a 8192-byte boundary. This is valid only for object files.
        /// </summary>
        Align8192Bytes = 0x00E00000,

        /// <summary>
        /// IMAGE_SCN_LNK_NRELOC_OVFL - The section contains extended relocations. The count of relocations for
        /// the section exceeds the 16 bits that is reserved for it in the section header. If the NumberOfRelocations
        /// field in the section header is 0xffff, the actual relocation count is stored in the VirtualAddress field
        /// of the first relocation. It is an error if IMAGE_SCN_LNK_NRELOC_OVFL is set and there are fewer than
        /// 0xffff relocations in the section.
        /// </summary>
        LinkerExtendedRelocations = 0x01000000,

        /// <summary>
        /// IMAGE_SCN_MEM_DISCARDABLE - The section can be discarded as needed.
        /// </summary>
        MemoryDiscardable = 0x02000000,

        /// <summary>
        /// IMAGE_SCN_MEM_NOT_CACHED - The section cannot be cached.
        /// </summary>
        MemoryNotCached = 0x04000000,

        /// <summary>
        /// IMAGE_SCN_MEM_NOT_PAGED - The section cannot be paged.
        /// </summary>
        MemoryNotPaged = 0x08000000,

        /// <summary>
        /// IMAGE_SCN_MEM_SHARED - The section can be shared in memory.
        /// </summary>
        MemoryShared = 0x10000000,

        /// <summary>
        /// IMAGE_SCN_MEM_EXECUTE - The section can be executed as code.
        /// </summary>
        MemoryExecute = 0x20000000,

        /// <summary>
        /// IMAGE_SCN_MEM_READ - The section can be read.
        /// </summary>
        MemoryRead = 0x40000000,

        /// <summary>
        /// IMAGE_SCN_MEM_WRITE - The section can be written to.
        /// </summary>
        MemoryWrite = 0x80000000,
    }
}
