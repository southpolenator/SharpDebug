using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerate the various tables contained in the data source.
    /// </summary>
    [ComImport, Guid("C65C2B0A-1150-4D7A-AFCC-E05BF3DEE81E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumTables
    {
        /// <summary>
        /// Gets the enumerator. Internally, marshals the COM IEnumVARIANT interface to the .NET Framework <see cref="IEnumerator"/> interface, and vice versa.
        /// </summary>
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Retrieves the number of tables.
        /// </summary>
        [DispId(1)]
        int count { get; }

        /// <summary>
        /// Retrieves an table by means of an index.
        /// </summary>
        /// <param name="index">Index of the <see cref="IDiaTable"/> object to be retrieved. The index is the range 0 to count-1, where count is returned by the <see cref="IDiaEnumTables.count"/> property.</param>
        /// <returns>Returns an <see cref="IDiaTable"/> object representing the table.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaTable Item(
            [In] uint index);

        /// <summary>
        /// Retrieves a specified number of tables in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of tables in the enumerator to be retrieved.</param>
        /// <param name="rgelt">Returns an array of <see cref="IDiaTable"/> objects that represents the desired tables.</param>
        /// <param name="pceltFetched">Returns the number of tables in the fetched enumerator.</param>
        void Next(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaTable[] rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Skips a specified number of tables in an enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of tables in the enumeration sequence to skip.</param>
        void Skip(
            [In] uint celt);

        /// <summary>
        /// Resets an enumeration sequence to the beginning.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates an enumerator that contains the same enumeration state as the current enumerator.
        /// </summary>
        /// <returns>Returns an <see cref="IDiaEnumTables"/> object that contains a duplicate of the enumerator. The tables are not duplicated, only the enumerator.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumTables Clone();
    }
}
