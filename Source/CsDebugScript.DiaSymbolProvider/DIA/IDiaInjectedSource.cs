using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Accesses injected source code stored in the DIA data source.
    /// </summary>
    [ComImport, Guid("AE605CDC-8105-4A23-B710-3259F1E26112"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaInjectedSource
    {
        /// <summary>
        /// Retrieves a cyclic redundancy check (CRC) calculated from the bytes of the source code.
        /// </summary>
        [DispId(1)]
        uint crc { get; }

        /// <summary>
        /// Retrieves the number of bytes of code.
        /// </summary>
        [DispId(2)]
        ulong length { get; }

        /// <summary>
        /// Retrieves the file name for the source.
        /// </summary>
        [DispId(3)]
        string fileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the object file name to which the source was compiled.
        /// </summary>
        [DispId(4)]
        string objectFileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the name given to non-file source code; that is, code that was injected.
        /// </summary>
        [DispId(5)]
        string virtualFilename
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the indicator of the source compression used.
        /// </summary>
        [DispId(6)]
        uint sourceCompression { get; }

        /// <summary>
        /// Retrieves the source code bytes.
        /// </summary>
        /// <param name="cbData">The number of bytes that represents the size of the data buffer.</param>
        /// <param name="pcbData">Returns the number of bytes that represents the bytes returned. If data is <c>null</c>, then pcbData is the total number of bytes of data available.</param>
        /// <param name="pbData">A buffer that is to be filled in with the source bytes.</param>
        void get_source(
            [In] uint cbData,
            [Out] out uint pcbData,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] pbData);
    }
}
