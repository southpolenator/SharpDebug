using DbgEngManaged;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CsScriptManaged
{
    /// <summary>
    /// The EXT_TDOP enumeration is used in the Operation member of the EXT_TYPED_DATA structure to
    /// specify which suboperation the DEBUG_REQUEST_EXT_TYPED_DATA_ANSI Request operation will perform.
    /// </summary>
    public enum ExtTdop : uint
    {
        Copy,
        Release,
        SetFromExpr,
        SetFromU64Expr,
        GetField,
        Evaluate,
        GetTypeName,
        OutputTypeName,
        OutputSimpleValue,
        OutputFullValue,
        HasField,
        GetFieldOffset,
        GetArrayElement,
        GetDereference,
        GetTypeSize,
        OutputTypeDefinition,
        GetPointerTo,
        SetFromTypeIdAndU64,
        SetPtrFromTypeIdAndU64,
        Count,
    }

    /// <summary>
    /// Specifies the type of symbol.
    /// </summary>
    public enum SymTag : uint
    {
        Null,
        Exe,
        Compiland,
        CompilandDetails,
        CompilandEnv,
        Function,
        Block,
        Data,
        Annotation,
        Label,
        PublicSymbol,
        UDT,
        Enum,
        FunctionType,
        PointerType,
        ArrayType,
        BaseType,
        Typedef,
        BaseClass,
        Friend,
        FunctionArgType,
        FuncDebugStart,
        FuncDebugEnd,
        UsingNamespace,
        VTableShape,
        VTable,
        Custom,
        Thunk,
        CustomType,
        ManagedType,
        Dimension,
        CallSite,
        Max,
    }

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
        /// DEBUG_TYPED_DATA_IS_IN_MEMORY              The data is in the target's memory and is available.
        /// DEBUG_TYPED_DATA_PHYSICAL_DEFAULT          Offset is a physical memory address, and the physical memory at Offset uses the default memory caching.
        /// DEBUG_TYPED_DATA_PHYSICAL_CACHED           Offset is a physical memory address, and the physical memory at Offset is cached.
        /// DEBUG_TYPED_DATA_PHYSICAL_UNCACHED         Offset is a physical memory address, and the physical memory at Offset is uncached.
        /// DEBUG_TYPED_DATA_PHYSICAL_WRITE_COMBINED   Offset is a physical memory address, and the physical memory at Offset is write-combined.
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

    /// <summary>
    /// The EXT_TYPED_DATA structure is passed to and returned from the DEBUG_REQUEST_EXT_TYPED_DATA_ANSI
    /// Request operation. It contains the input and output parameters for the operation as well as specifying
    /// which particular suboperation to perform.
    /// </summary>
    public struct EXT_TYPED_DATA
    {
        /// <summary>
        /// Specifies which suboperation the DEBUG_REQUEST_EXT_TYPED_DATA_ANSI Request operation should perform.
        /// The interpretation of some of the other members depends on Operation. For a list of possible suboperations, see EXT_TDOP.
        /// </summary>
        public ExtTdop Operation;

        /// <summary>
        /// Specifies the bit flags describing the target's memory in which the data resides. If no flags are present, the data is
        /// considered to be in virtual memory. One of the following flags may be present:
        /// EXT_TDF_PHYSICAL_DEFAULT          The typed data is in physical memory, and this physical memory uses the default memory caching.
        /// EXT_TDF_PHYSICAL_CACHED           The typed data is in physical memory, and this physical memory is cached.
        /// EXT_TDF_PHYSICAL_UNCACHED         The typed data is in physical memory, and this physical memory is uncached.
        /// EXT_TDF_PHYSICAL_WRITE_COMBINED   The typed data is in physical memory, and this physical memory is write-combined.
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Specifies typed data to be used as input to the operation.For details about this structure, see DEBUG_TYPED_DATA.
        /// The interpretation of InData depends on the value of Operation.
        /// </summary>
        public DEBUG_TYPED_DATA InData;

        /// <summary>
        /// Receives typed data as output from the operation.Any suboperation that returns typed data to OutData initially
        /// copies the contents of InData to OutData, then modifies OutData in place, so that the input parameters in InData
        /// are also present in OutData.For details about this structure, see DEBUG_TYPED_DATA.
        /// The interpretation of OutData depends on the value of Operation.
        /// </summary>
        public DEBUG_TYPED_DATA OutData;

        /// <summary>
        /// Specifies the position of an ANSI string to be used as input to the operation.InStrIndex can be zero to indicate
        /// that the input parameters do not include an ANSI string.
        /// The position of the string is relative to the base address of this EXT_TYPED_DATA structure. The string must follow
        /// this structure, so InStrIndex must be greater than the size of this structure.The string is part of the input to the
        /// operation and InStrIndex must be smaller than InBufferSize, the size of the input buffer passed to Request.
        /// The interpretation of the string depends on the value of Operation.
        /// </summary>
        public uint InStrIndex;

        /// <summary>
        /// Specifies a 32-bit parameter to be used as input to the operation.
        /// The interpretation of In32 depends on the value of Operation.
        /// </summary>
        public uint In32;

        /// <summary>
        /// Receives a 32-bit value as output from the operation.
        /// The interpretation of Out32 depends on the value of Operation.
        /// </summary>
        public uint Out32;

        /// <summary>
        /// Specifies a 64-bit parameter to be used as input to the operation.
        /// The interpretation of In64 depends on the value of Operation.
        /// </summary>
        public ulong In64;

        /// <summary>
        /// Receives a 64-bit value as output from the operation.
        /// The interpretation of Out64 depends on the value of Operation.
        /// </summary>
        public ulong Out64;

        /// <summary>
        /// Specifies the position to return an ANSI string as output from the operation. StrBufferIndex can be zero
        /// if no ANSI string is to be received from the operation.
        /// The position of the string is relative to the base address of the returned EXT_TYPED_DATA structure.
        /// The string must follow the structure, so StrBufferIndex must be greater than the size of this structure.
        /// The string is part of the output from the suboperation, and StrBufferIndex plus StrBufferChars must be
        /// smaller than OutBufferSize, the size of the output buffer passed to Request.
        /// The interpretation of the string depends on the value of Operation.
        /// </summary>
        public uint StrBufferIndex;

        /// <summary>
        /// Specifies the size in characters of the ANSI string buffer specified by StrBufferIndex.
        /// </summary>
        public uint StrBufferChars;

        /// <summary>
        /// Receives the number of characters needed by the string buffer specified by StrBufferIndex.
        /// </summary>
        public uint StrCharsNeeded;

        /// <summary>
        /// Set to zero.
        /// </summary>
        public uint DataBufferIndex;

        /// <summary>
        /// Set to zero.
        /// </summary>
        public uint DataBufferBytes;

        /// <summary>
        /// Set to zero.
        /// </summary>
        public uint DataBytesNeeded;

        /// <summary>
        /// Receives the status code returned by the operation. This is the same value returned by Request.
        /// </summary>
        public int Status; // HRESULT
        
        // Set to zero.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Reserved;
    }

    /// <summary>
    /// Extensions for debugger interfaces for easier manipulation
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Requests the specified request type.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static EXT_TYPED_DATA Request(this IDebugAdvanced3 client, DebugRequest requestType, EXT_TYPED_DATA request)
        {
            using (var requestNative = new MarshalStructure<EXT_TYPED_DATA>())
            {
                uint outSize;

                requestNative.Structure = request;
                Context.Advanced.Request((uint)requestType, requestNative.Pointer, requestNative.USize, requestNative.Pointer, requestNative.USize, out outSize);
                return requestNative.Structure;
            }
        }

        /// <summary>
        /// Requests the specified request type with extended structure.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="request">The request.</param>
        /// <param name="extended">The extended string.</param>
        public static EXT_TYPED_DATA RequestExtended(this IDebugAdvanced3 client, DebugRequest requestType, EXT_TYPED_DATA request, string extended)
        {
            using (var requestNative = new MarshalStructureExtendedWithAnsiString<EXT_TYPED_DATA>())
            {
                uint outSize;

                requestNative.Extended = extended;
                requestNative.Structure = request;
                Context.Advanced.Request((uint)requestType, requestNative.Pointer, requestNative.USize, requestNative.Pointer, requestNative.USize, out outSize);
                return requestNative.Structure;
            }
        }

        /// <summary>
        /// Gets the name of the current process executable.
        /// </summary>
        /// <param name="systemObjects">The system objects.</param>
        public static string GetCurrentProcessExecutableName(this IDebugSystemObjects4 systemObjects)
        {
            uint exeSize;
            StringBuilder sb = new StringBuilder(Constants.MaxFileName);

            systemObjects.GetCurrentProcessExecutableNameWide(sb, (uint)sb.Capacity, out exeSize);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Exported native methods
    /// </summary>
    public static class NativeMethods
    {
        /// <summary>
        /// Copies a block of memory from one location to another.
        /// </summary>
        /// <param name="destination">The destination address.</param>
        /// <param name="source">The source address.</param>
        /// <param name="count">The number of bytes.</param>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint count);
    }
}
