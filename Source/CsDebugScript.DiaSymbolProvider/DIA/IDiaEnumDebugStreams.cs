using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerates the various debug streams contained in the data source.
    /// </summary>
    [ComImport, Guid("08CBB41E-47A6-4F87-92F1-1C9C87CED044"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumDebugStreams
    {
        /// <summary>
        /// Gets the enumerator. Internally, marshals the COM IEnumVARIANT interface to the .NET Framework <see cref="IEnumerator"/> interface, and vice versa.
        /// </summary>
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Retrieves the number of debug streams.
        /// </summary>
        [DispId(1)]
        int count { get; }

        /// <summary>
        /// Retrieves a debug stream by means of an index or name.
        /// </summary>
        /// <param name="index">Index or name of the debug stream to be retrieved. If an integer variant is used, it must be in the range 0 to count-1, where count is as returned by the <see cref="IDiaEnumDebugStreams.count"/> property.</param>
        /// <returns>Returns an <see cref="IDiaEnumDebugStreamData"/> object representing the specified debug stream.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumDebugStreamData Item(
            [In, MarshalAs(UnmanagedType.Struct)] object index);

        /// <summary>
        /// Retrieves a specified number of debug streams in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of debug streams in the enumerator to be retrieved.</param>
        /// <param name="rgelt">Returns an array of <see cref="IDiaEnumDebugStreamData"/> objects that represents the debug streams being retrieved.</param>
        /// <param name="pceltFetched">Returns the number of debug streams returned.</param>
        void Next(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaEnumDebugStreamData rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Skips a specified number of debug streams in an enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of debug streams in the enumeration sequence to skip.</param>
        void Skip(
            [In] uint celt);

        /// <summary>
        /// Resets an enumeration sequence to the beginning.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates an enumerator that contains the same enumeration state as the current enumerator.
        /// </summary>
        /// <returns>Returns an <see cref="IDiaEnumDebugStreams"/> object that contains a duplicate of the enumerator. The streams are not duplicated, only the enumerator.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumDebugStreams Clone();
    }
}
