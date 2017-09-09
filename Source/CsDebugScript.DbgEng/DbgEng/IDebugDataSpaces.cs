using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [ComImport, Guid("88F7DFAB-3EA7-4C3A-AEFB-C4E8106173AA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugDataSpaces
    {
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
    }
}
