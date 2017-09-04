using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerate the various injected sources contained in the data source.
    /// </summary>
    [ComImport, Guid("D5612573-6925-4468-8883-98CDEC8C384A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumInjectedSources
    {
        /// <summary>
        /// Gets the enumerator. Internally, marshals the COM IEnumVARIANT interface to the .NET Framework <see cref="IEnumerator"/> interface, and vice versa.
        /// </summary>
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Retrieves the number of injected sources.
        /// </summary>
        [DispId(1)]
        int count { get; }

        /// <summary>
        /// Retrieves an injected source by means of an index.
        /// </summary>
        /// <param name="index">Index of the <see cref="IDiaInjectedSource"/> object to be retrieved. The index is the range 0 to count-1, where count is returned by the <see cref="IDiaEnumInjectedSources.count"/> property.</param>
        /// <returns>Returns an <see cref="IDiaInjectedSource"/> object representing the injected source.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaInjectedSource Item(
            [In] uint index);

        /// <summary>
        /// Retrieves a specified number of injected sources in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of injected sources in the enumerator to be retrieved.</param>
        /// <param name="rgelt">Returns an array of <see cref="IDiaInjectedSource"/> objects that represents the desired injected sources.</param>
        /// <param name="pceltFetched">Returns the number of injected sources in the fetched enumerator.</param>
        void Next(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaInjectedSource[] rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Skips a specified number of injected sources in an enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of injected sources in the enumeration sequence to skip.</param>
        void Skip(
            [In] uint celt);

        /// <summary>
        /// Resets an enumeration sequence to the beginning.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates an enumerator that contains the same enumeration state as the current enumerator.
        /// </summary>
        /// <returns>Returns an <see cref="IDiaEnumInjectedSources"/> object that contains a duplicate of the enumerator. The injected sources are not duplicated, only the enumerator.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumInjectedSources Clone();
    }
}
