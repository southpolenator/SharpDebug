using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Facilitates walking the stack using the program debug database (.pdb) file.
    /// </summary>
    [ComImport, Guid("21F81B1B-C5BB-42A3-BC4F-CCBAA75B9F19"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaStackWalkHelper
    {
        /// <summary>
        /// Gets or sets the value of a register.
        /// </summary>
        [DispId(1)]
        ulong registerValue { get; set; }

        /// <summary>
        /// Reads a block of data from the executable's image in memory.
        /// </summary>
        /// <param name="type">A value from the <see cref="MemoryTypeEnum"/> enumeration specifying the type of memory to read.</param>
        /// <param name="va">Virtual address in the image from which to begin reading.</param>
        /// <param name="cbData">The size of the data buffer in bytes.</param>
        /// <param name="pcbData">Returns the number of bytes actually read. If pbData is <c>null</c>, then this is the total number of bytes of data available.</param>
        /// <param name="pbData">A buffer that is filled in with the memory read.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int readMemory(
            [In] MemoryTypeEnum type,
            [In] ulong va,
            [In] uint cbData,
            [Out] out uint pcbData,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbData);

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
        /// Searches the specified stack frame for a return address at or near the specified stack address.
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

        /// <summary>
        /// Retrieves the stack frame that contains the specified virtual address.
        /// </summary>
        /// <param name="va">The virtual address for the frame data.</param>
        /// <param name="ppFrame">An <see cref="IDiaFrameData"/> object that represents the stack frame at the specified address.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int frameForVA(
            [In] ulong va,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaFrameData ppFrame);

        /// <summary>
        /// Retrieves the symbol that contains the specified virtual address.
        /// </summary>
        /// <param name="va">The virtual address that is contained in the requested symbol. The symbol must be a <see cref="SymTagEnum.FunctionType"/>.</param>
        /// <param name="ppSymbol">An <see cref="IDiaSymbol"/> object that represents the symbol at the specified address.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int symbolForVA(
            [In] ulong va,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol);

        /// <summary>
        /// Returns the PDATA data block associated with the virtual address.
        /// </summary>
        /// <param name="va">Specifies the virtual address of the data to obtain. </param>
        /// <param name="cbData">The size of data in bytes to obtain.</param>
        /// <param name="pcbData">Returns the actual size of data in bytes that was obtained.</param>
        /// <param name="pbData">A buffer that is filled in with the requested data. Cannot be <c>null</c>.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int pdataForVA(
            [In] ulong va,
            [In] uint cbData,
            [Out] out uint pcbData,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbData);

        /// <summary>
        /// Returns the start of an executable's image in memory given a virtual address somewhere in the executable's memory space.
        /// </summary>
        /// <param name="vaContext">The virtual address that lies somewhere in the executable's space.</param>
        /// <param name="pvaImageStart">Returns the starting virtual address of the executable's image.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int imageForVA(
            [In] ulong vaContext,
            [Out] out ulong pvaImageStart);

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        /// <param name="va">Specifies the virtual address of the address to obtain.</param>
        /// <param name="pISect">Returns the section component of the address.</param>
        /// <param name="pOffset">Returns the offset component of the address.</param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int addressForVA(
            [In] ulong va,
            [Out] out uint pISect,
            [Out] out uint pOffset);

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        /// <param name="vaFunc"></param>
        /// <param name="cbFunc"></param>
        /// <param name="pNumFragments"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int numberOfFunctionFragmentsForVA(
            [In] ulong vaFunc,
            [In] uint cbFunc,
            [Out] out uint pNumFragments);

        /// <summary>
        /// Undocumented on MSDN.
        /// </summary>
        /// <param name="vaFunc"></param>
        /// <param name="cbFunc"></param>
        /// <param name="cFragments"></param>
        /// <param name="pVaFragment"></param>
        /// <param name="pLenFragment"></param>
        /// <returns>If successful, returns S_OK; otherwise, returns an error code.</returns>
        [PreserveSig]
        int functionFragmentsForVA(
            [In] ulong vaFunc,
            [In] uint cbFunc,
            [In] uint cFragments,
            [Out] out ulong pVaFragment,
            [Out] out uint pLenFragment);
    }
}
