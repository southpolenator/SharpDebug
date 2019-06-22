using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, Guid("D98ADA1F-29E9-4EF5-A6C0-E53349883212"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugDataSpaces4 : IDebugDataSpaces3
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugDataSpaces
        void ReadVirtual(
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WriteVirtual(
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        ulong SearchVirtual(
            [In] ulong Offset,
            [In] ulong Length,
            [In] IntPtr Pattern,
            [In] uint PatternSize,
            [In] uint PatternGranularity);

        void ReadVirtualUncached(
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WriteVirtualUncached(
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        ulong ReadPointersVirtual(
            [In] uint Count,
            [In] ulong Offset);

        void WritePointersVirtual(
            [In] uint Count,
            [In] ulong Offset,
            [In] ref ulong Ptrs);

        void ReadPhysical(
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WritePhysical(
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        void ReadControl(
            [In] uint Processor,
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WriteControl(
            [In] uint Processor,
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        void ReadIo(
            [In] uint InterfaceType,
            [In] uint BusNumber,
            [In] uint AddressSpace,
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WriteIo(
            [In] uint InterfaceType,
            [In] uint BusNumber,
            [In] uint AddressSpace,
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        ulong ReadMsr(
            [In] uint Msr);

        void WriteMsr(
            [In] uint Msr,
            [In] ulong Value);

        void ReadBusData(
            [In] uint BusDataType,
            [In] uint BusNumber,
            [In] uint SlotNumber,
            [In] uint Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WriteBusData(
            [In] uint BusDataType,
            [In] uint BusNumber,
            [In] uint SlotNumber,
            [In] uint Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        void CheckLowMemory();

        void ReadDebuggerData(
            [In] uint Index,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);

        void ReadProcessorSystemData(
            [In] uint Processor,
            [In] uint Index,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);
        #endregion

        #region IDebugDataSpaces2
        ulong VirtualToPhysical(
            [In] ulong Virtual);

        void GetVirtualTranslationPhysicalOffsets(
            [In] ulong Virtual,
            [Out] out ulong Offsets,
            [In] uint OffsetsSize,
            [Out] out uint Levels);

        void ReadHandleData(
            [In] ulong Handle,
            [In] uint DataType,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);

        void FillVirtual(
            [In] ulong Start,
            [In] uint Size,
            [In] IntPtr Pattern,
            [In] uint PatternSize,
            [Out] out uint Filled);

        void FillPhysical(
            [In] ulong Start,
            [In] uint Size,
            [In] IntPtr Pattern,
            [In] uint PatternSize,
            [Out] out uint Filled);

        _MEMORY_BASIC_INFORMATION64 QueryVirtual(
            [In] ulong Offset);
        #endregion

        #region IDebugDataSpaces3
        _IMAGE_NT_HEADERS64 ReadImageNtHeaders(
            [In] ulong ImageBase);

        void ReadTagged(
            [In] ref Guid Tag,
            [In] uint Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint TotalSize);

        ulong StartEnumTagged();

        void GetNextTagged(
            [In] ulong Handle,
            out Guid Tag,
            out uint Size);

        void EndEnumTagged(
            [In] ulong Handle);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        void GetOffsetInformation(
            [In] uint Space,
            [In] uint Which,
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint InfoSize);

        ulong GetNextDifferentlyValidOffsetVirtual(
            [In] ulong Offset);

        void GetValidRegionVirtual(
            [In] ulong Base,
            [In] uint Size,
            [Out] out ulong ValidBase,
            [Out] out uint ValidSize);

        ulong SearchVirtual2(
            [In] ulong Offset,
            [In] ulong Length,
            [In] uint Flags,
            [In] IntPtr Pattern,
            [In] uint PatternSize,
            [In] uint PatternGranularity);

        void ReadMultiByteStringVirtual(
            [In] ulong Offset,
            [In] uint MaxBytes,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringBytes);

        void ReadMultiByteStringVirtualWide(
            [In] ulong Offset,
            [In] uint MaxBytes,
            [In] uint CodePage,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringBytes);

        void ReadUnicodeStringVirtual(
            [In] ulong Offset,
            [In] uint MaxBytes,
            [In] uint CodePage,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringBytes);

        void ReadUnicodeStringVirtualWide(
            [In] ulong Offset,
            [In] uint MaxBytes,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringBytes);

        void ReadPhysical2(
            [In] ulong Offset,
            [In] uint Flags,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WritePhysical2(
            [In] ulong Offset,
            [In] uint Flags,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);
    }
}
