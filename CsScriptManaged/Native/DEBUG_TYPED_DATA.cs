using System.Runtime.InteropServices;

namespace CsScriptManaged.Native
{
    /// <summary>
    /// The DEBUG_TYPED_DATA structure describes typed data in the memory of the target.
    /// </summary>
    public struct DEBUG_TYPED_DATA
    {
        /// <summary>
        /// The base address of the module, in the target's virtual address space, that contains the typed data.
        /// </summary>
        public ulong ModBase;

        /// <summary>
        /// The location of the typed data in the target's memory. Offset is a virtual memory address unless there
        /// are flags present in Flags that specify that Offset is a physical memory address.
        /// </summary>
        public ulong Offset;

        /// <summary>
        /// Set to zero.
        /// </summary>
        public ulong EngineHandle;

        /// <summary>
        /// The data cast to a ULONG64. If Flags does not contain the DEBUG_TYPED_DATA_IS_IN_MEMORY flag, the data
        /// is not available and Data is set to zero.
        /// </summary>
        public ulong Data;

        /// <summary>
        /// The size, in bytes, of the data.
        /// </summary>
        public uint Size;

        /// <summary>
        /// The flags describing the target's memory in which the data resides. The following bit flags can be set. Flag Description:
        /// <para>DEBUG_TYPED_DATA_IS_IN_MEMORY              The data is in the target's memory and is available.</para>
        /// <para>DEBUG_TYPED_DATA_PHYSICAL_DEFAULT          Offset is a physical memory address, and the physical memory at Offset uses the default memory caching.</para>
        /// <para>DEBUG_TYPED_DATA_PHYSICAL_CACHED           Offset is a physical memory address, and the physical memory at Offset is cached.</para>
        /// <para>DEBUG_TYPED_DATA_PHYSICAL_UNCACHED         Offset is a physical memory address, and the physical memory at Offset is uncached.</para>
        /// <para>DEBUG_TYPED_DATA_PHYSICAL_WRITE_COMBINED   Offset is a physical memory address, and the physical memory at Offset is write-combined.</para>
        /// </summary>
        public uint Flags;

        /// <summary>
        /// The type ID for the data's type.
        /// </summary>
        public uint TypeId;

        /// <summary>
        /// For generated types, the type ID of the type on which the data's type is based. For example, if the typed data
        /// represents a pointer (or an array), BaseTypeId is the type of the object pointed to (or held in the array).
        /// For other types, BaseTypeId is the same as TypeId.
        /// </summary>
        public uint BaseTypeId;

        /// <summary>
        /// The symbol tag of the typed data. This is a value from the SymTagEnum enumeration. For descriptions of the values, see the
        /// DbgHelp API documentation.
        /// </summary>
        public SymTag Tag;

        /// <summary>
        /// The index of the processor's register containing the data, or zero if the data is not contained in a register.
        /// (Note that the zero value can represent either that the data is not in a register or that it is in the register
        /// whose index is zero.) 
        /// </summary>
        public uint Register;

        /// <summary>
        /// Internal debugger engine data.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public ulong[] Internal;
    }
}
