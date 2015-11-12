using DbgEngManaged;
using System;
using System.Runtime.InteropServices;

namespace CsScriptManaged
{
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

    public struct DEBUG_TYPED_DATA
    {
        public ulong ModBase;
        public ulong Offset;
        public ulong EngineHandle;
        public ulong Data;
        public uint Size;
        public uint Flags;
        public uint TypeId;
        public uint BaseTypeId;
        public SymTag Tag;
        public uint Register;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public ulong[] Internal;
    }

    public struct EXT_TYPED_DATA
    {
        public ExtTdop Operation;
        public uint Flags;
        public DEBUG_TYPED_DATA InData;
        public DEBUG_TYPED_DATA OutData;
        public uint InStrIndex;
        public uint In32;
        public uint Out32;
        public ulong In64;
        public ulong Out64;
        public uint StrBufferIndex;
        public uint StrBufferChars;
        public uint StrCharsNeeded;
        public uint DataBufferIndex;
        public uint DataBufferBytes;
        public uint DataBytesNeeded;
        public int Status; // HRESULT
        // Must be zeroed.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Reserved;
    }

    public static class Extensions
    {
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
    }

    public static class NativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }
}
