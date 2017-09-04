using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerates a DIA data source table.
    /// </summary>
    /// <seealso cref="DIA.IEnumUnknown" />
    [ComImport, Guid("4A59FB77-ABAC-469B-A30B-9ECC85BFEF14"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaTable : IEnumUnknown
    {
        /// <summary>
        /// Gets the enumerator. Internally, marshals the COM IEnumVARIANT interface to the .NET Framework <see cref="IEnumerator"/> interface, and vice versa.
        /// </summary>
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
        IEnumerator GetEnumerator();

        /// <summary>
        /// Retrieves the name of the table.
        /// </summary>
        [DispId(1)]
        string name
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Retrieves the number of items in the table.
        /// </summary>
        [DispId(2)]
        int count { get; }

        /// <summary>
        /// Retrieves a reference to the specified entry in the table.
        /// </summary>
        /// <param name="index">The index of the table entry in the range 0 to count-1, where count is returned by the <see cref="IDiaTable.count"/> method.</param>
        /// <returns>Returns an IUnknown object that represents the specified table entry.</returns>
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object Item(
            [In] uint index);
    }
}
