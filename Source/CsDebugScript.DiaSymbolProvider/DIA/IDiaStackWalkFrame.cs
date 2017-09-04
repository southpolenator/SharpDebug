using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Maintains stack context between invocations of the <see cref="IDiaFrameData.execute"/> method.
    /// </summary>
    [ComImport, Guid("07C590C1-438D-4F47-BDCD-4397BC81AD75"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaStackWalkFrame
    {
        /// <summary>
        /// Gets or sets the value of a register.
        /// </summary>
        [DispId(1)]
        ulong registerValue { get; set; }

        /// <summary>
        /// Reads memory from image.
        /// </summary>
        /// <param name="type">One of the <see cref="MemoryTypeEnum"/> enumeration values that specifies the kind of memory to access.</param>
        /// <param name="va">Virtual address location in image to begin reading.</param>
        /// <param name="cbData">Size of the data buffer, in bytes.</param>
        /// <param name="pcbData">Returns the number of bytes returned. If data is <c>null</c>, then pcbData contains the total number of bytes of data available.</param>
        /// <param name="pbData">A buffer that is to be filled in with data from the specified location.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int readMemory(
            [In] MemoryTypeEnum type,
            [In] ulong va,
            [In] uint cbData,
            [Out] out uint pcbData,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ref byte[] pbData);

        /// <summary>
        /// Searches the specified stack frame for the nearest function return address.
        /// </summary>
        /// <param name="frame">An <see cref="IDiaFrameData"/> object that represents the current stack frame.</param>
        /// <param name="returnAddress">Returns the nearest function return address.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int searchForReturnAddress(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaFrameData frame,
            [Out] out ulong returnAddress);

        /// <summary>
        /// Searches the specified stack frame for a return address at or near the specified address.
        /// </summary>
        /// <param name="frame">An <see cref="IDiaFrameData"/> object that represents the current stack frame.</param>
        /// <param name="startAddress">A virtual memory address from which to begin searching.</param>
        /// <param name="returnAddress">Returns the nearest function return address to startAddress.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int searchForReturnAddressStart(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaFrameData frame,
            [In] ulong startAddress,
            [Out] out ulong returnAddress);
    }
}
