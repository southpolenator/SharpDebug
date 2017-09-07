using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [ComImport, Guid("7A5E852F-96E9-468F-AC1B-0B3ADDC4A049"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugDataSpaces2 : IDebugDataSpaces
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

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

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
    }
}
