using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Provides access to the records in a debug data stream.
    /// </summary>
    [ComImport, Guid("486943E8-D187-4A6B-A3C4-291259FFF60D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumDebugStreamData
    {
        /// <summary>
        /// Gets the enumerator. Internally, marshals the COM IEnumVARIANT interface to the .NET Framework <see cref="IEnumerator"/> interface, and vice versa.
        /// </summary>
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Retrieves the number of records in the debug data stream.
        /// </summary>
        [DispId(1)]
        int count { get; }

        /// <summary>
        /// Retrieves the name of the debug data stream.
        /// </summary>
        [DispId(2)]
        string name
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the specified record.
        /// </summary>
        /// <param name="index">Index of the record to be retrieved. The index is in the range 0 to count-1, where count is returned by <see cref="IDiaEnumDebugStreamData.count"/>.</param>
        /// <param name="cbData">Size of the data buffer, in bytes.</param>
        /// <param name="pcbData">Returns the number of bytes returned. If data is <c>null</c>, then pcbData contains the total number of bytes of data available in the specified record.</param>
        /// <param name="pbData">A buffer that is filled in with the debug stream record data.</param>
        [DispId(0)]
        void Item(
            [In] uint index,
            [In] uint cbData,
            [Out] out uint pcbData,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbData);

        /// <summary>
        /// Retrieves a specified number of records in the enumerated sequence.
        /// </summary>
        /// <param name="celt">The number of records to be retrieved.</param>
        /// <param name="cbData">Size of the data buffer, in bytes.</param>
        /// <param name="pcbData">Returns the number of bytes returned. If data is <c>null</c>, then pcbData contains the total number of bytes of data available for all requested records.</param>
        /// <param name="pbData">A buffer that is to be filled with the debug stream record data.</param>
        /// <param name="pceltFetched">Returns the number of records in data.</param>
        void Next(
            [In] uint celt,
            [In] uint cbData,
            [Out] out uint pcbData,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbData,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Skips a specified number of records in an enumerated sequence.
        /// </summary>
        /// <param name="celt">The number of records to skip in the enumerated sequence.</param>
        void Skip(
            [In] uint celt);

        /// <summary>
        /// Resets to the beginning of an enumerated sequence.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates an enumerator that contains the same enumerated sequence as the current enumerator.
        /// </summary>
        /// <returns>Returns an <see cref="IDiaEnumDebugStreamData"/> object that contains the duplicated sequence of debug data stream records.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumDebugStreamData Clone();
    }
}
