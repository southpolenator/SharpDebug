using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerate the various symbols contained in the data source.
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CAB72C48-443B-48F5-9B0B-42F0820AB29A")]
    public interface IDiaEnumSymbols
    {
        /// <summary>
        /// Gets the enumerator. Internally, marshals the COM IEnumVARIANT interface to the .NET Framework <see cref="IEnumerator"/> interface, and vice versa.
        /// </summary>
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Retrieves the number of symbols.
        /// </summary>
        [DispId(1)]
        int count { get; }

        /// <summary>
        /// Retrieves an symbol by means of an index.
        /// </summary>
        /// <param name="index">Index of the <see cref="IDiaSymbol"/> object to be retrieved. The index is the range 0 to count-1, where count is returned by the <see cref="IDiaEnumSymbols.count"/> property.</param>
        /// <returns>Returns an <see cref="IDiaSymbol"/> object representing the symbol.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaSymbol Item(
            [In] uint index);

        /// <summary>
        /// Retrieves a specified number of symbols in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of symbols in the enumerator to be retrieved.</param>
        /// <param name="rgelt">Returns an array of <see cref="IDiaSymbol"/> objects that represents the desired symbols.</param>
        /// <param name="pceltFetched">Returns the number of symbols in the fetched enumerator.</param>
        void Next(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaSymbol[] rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Skips a specified number of symbols in an enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of symbols in the enumeration sequence to skip.</param>
        void Skip(
            [In] uint celt);

        /// <summary>
        /// Resets an enumeration sequence to the beginning.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates an enumerator that contains the same enumeration state as the current enumerator.
        /// </summary>
        /// <returns>Returns an <see cref="IDiaEnumSymbols"/> object that contains a duplicate of the enumerator. The symbols are not duplicated, only the enumerator.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbols Clone();
    }
}
