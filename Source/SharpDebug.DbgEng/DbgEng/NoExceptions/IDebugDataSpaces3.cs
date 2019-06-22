using System;
using System.Runtime.InteropServices;

namespace DbgEng.NoExceptions
{
    [ComImport, Guid("23F79D6C-8AAF-4F7C-A607-9995F5407E63"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugDataSpaces3 : IDebugDataSpaces2
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugDataSpaces
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
        #endregion

        #region IDebugDataSpaces2
        [PreserveSig]
        int VirtualToPhysical(
            [In] ulong Virtual,
            [Out] out ulong Physical);

        [PreserveSig]
        int GetVirtualTranslationPhysicalOffsets(
            [In] ulong Virtual,
            [Out] out ulong Offsets,
            [In] uint OffsetsSize,
            [Out] out uint Levels);

        [PreserveSig]
        int ReadHandleData(
            [In] ulong Handle,
            [In] uint DataType,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint DataSize);

        [PreserveSig]
        int FillVirtual(
            [In] ulong Start,
            [In] uint Size,
            [In] IntPtr Pattern,
            [In] uint PatternSize,
            [Out] out uint Filled);

        [PreserveSig]
        int FillPhysical(
            [In] ulong Start,
            [In] uint Size,
            [In] IntPtr Pattern,
            [In] uint PatternSize,
            [Out] out uint Filled);

        [PreserveSig]
        int QueryVirtual(
            [In] ulong Offset,
            [Out] out _MEMORY_BASIC_INFORMATION64 Information);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int ReadImageNtHeaders(
            [In] ulong ImageBase,
            [Out] out _IMAGE_NT_HEADERS64 Headers);

        [PreserveSig]
        int ReadTagged(
            [In] ref Guid Tag,
            [In] uint Offset,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint TotalSize);

        [PreserveSig]
        int StartEnumTagged(
            [Out] out ulong Handle);

        [PreserveSig]
        int GetNextTagged(
            [In] ulong Handle,
            [Out] out Guid Tag,
            [Out] out uint Size);

        [PreserveSig]
        int EndEnumTagged(
            [In] ulong Handle);
    }
}
