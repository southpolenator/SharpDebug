using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerates the various line numbers contained in the data source.
    /// </summary>
    [ComImport, Guid("FE30E878-54AC-44F1-81BA-39DE940F6052"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumLineNumbers
    {
        /// <summary>
        /// Gets the enumerator. Internally, marshals the COM IEnumVARIANT interface to the .NET Framework <see cref="IEnumerator"/> interface, and vice versa.
        /// </summary>
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Retrieves the number of line numbers.
        /// </summary>
        [DispId(1)]
        int count { get; }

        /// <summary>
        /// Retrieves an line number by means of an index.
        /// </summary>
        /// <param name="index">Index of the <see cref="IDiaLineNumber"/> object to be retrieved. The index is the range 0 to count-1, where count is returned by the <see cref="IDiaEnumLineNumbers.count"/> property.</param>
        /// <returns>Returns an <see cref="IDiaLineNumber"/> object representing the line number.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaLineNumber Item(
            [In] uint index);

        /// <summary>
        /// Retrieves a specified number of line numbers in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of line numbers in the enumerator to be retrieved.</param>
        /// <param name="rgelt">Returns an array of <see cref="IDiaLineNumber"/> objects that represents the desired line numbers.</param>
        /// <param name="pceltFetched">Returns the number of line numbers in the fetched enumerator.</param>
        void Next(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaLineNumber[] rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Skips a specified number of line numbers in an enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of line numbers in the enumeration sequence to skip.</param>
        void Skip(
            [In] uint celt);

        /// <summary>
        /// Resets an enumeration sequence to the beginning.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates an enumerator that contains the same enumeration state as the current enumerator.
        /// </summary>
        /// <returns>Returns an <see cref="IDiaEnumLineNumbers"/> object that contains a duplicate of the enumerator. The line numbers are not duplicated, only the enumerator.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumLineNumbers Clone();
    }
}
