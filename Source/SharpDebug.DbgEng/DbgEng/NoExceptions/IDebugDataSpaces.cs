using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("88F7DFAB-3EA7-4C3A-AEFB-C4E8106173AA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugDataSpaces
    {
        [PreserveSig]
        int ReadVirtual(
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WriteVirtual(
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int SearchVirtual(
            [In] ulong Offset,
            [In] ulong Length,
            [In] IntPtr Pattern,
            [In] uint PatternSize,
            [In] uint PatternGranularity,
            [Out] out ulong Address);

        [PreserveSig]
        int ReadVirtualUncached(
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WriteVirtualUncached(
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int ReadPointersVirtual(
            [In] uint Count,
            [In] ulong Offset,
            [Out] out ulong Address);

        [PreserveSig]
        int WritePointersVirtual(
            [In] uint Count,
            [In] ulong Offset,
            [In] ref ulong Ptrs);

        [PreserveSig]
        int ReadPhysical(
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WritePhysical(
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int ReadControl(
            [In] uint Processor,
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WriteControl(
            [In] uint Processor,
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int ReadIo(
            [In] uint InterfaceType,
            [In] uint BusNumber,
            [In] uint AddressSpace,
            [In] ulong Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WriteIo(
            [In] uint InterfaceType,
            [In] uint BusNumber,
            [In] uint AddressSpace,
            [In] ulong Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int ReadMsr(
            [In] uint Msr,
            [Out] out ulong Value);

        [PreserveSig]
        int WriteMsr(
            [In] uint Msr,
            [In] ulong Value);

        [PreserveSig]
        int ReadBusData(
            [In] uint BusDataType,
            [In] uint BusNumber,
            [In] uint SlotNumber,
            [In] uint Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WriteBusData(
            [In] uint BusDataType,
            [In] uint BusNumber,
            [In] uint SlotNumber,
            [In] uint Offset,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int CheckLowMemory();

        [PreserveSig]
        int ReadDebuggerData(
            [In] uint Index,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);

        [PreserveSig]
        int ReadProcessorSystemData(
            [In] uint Processor,
            [In] uint Index,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);
    }
}
