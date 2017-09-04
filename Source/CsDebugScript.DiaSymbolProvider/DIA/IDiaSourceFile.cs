using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Represents a source file.
    /// </summary>
    [ComImport, Guid("A2EF5353-F5A8-4EB3-90D2-CB526ACB3CDD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaSourceFile
    {
        /// <summary>
        /// Retrieves a simple integer key value that is unique for this image.
        /// </summary>
        [DispId(2)]
        uint uniqueId { get; }

        /// <summary>
        /// Retrieves the source file name.
        /// </summary>
        [DispId(3)]
        string fileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the checksum type.
        /// </summary>
        [DispId(4)]
        ChecksumAlgorithmType checksumType { get; }

        /// <summary>
        /// Retrieves an enumerator of the compilands with line numbers referencing this file.
        /// </summary>
        [DispId(5)]
        IDiaEnumSymbols compilands
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        /// <summary>
        /// Retrieves the checksum bytes.
        /// </summary>
        /// <param name="cbData">Size of the data buffer, in bytes.</param>
        /// <param name="pcbData">Returns the number of checksum bytes. This parameter cannot be <c>null</c>.</param>
        /// <param name="pbData">A buffer that is filled with the checksum bytes. If this parameter is <c>null</c>, then pcbData returns the number of bytes required.</param>
        void get_checksum(
            [In] uint cbData,
            [Out] out uint pcbData,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ref byte[] pbData);
    }
}
