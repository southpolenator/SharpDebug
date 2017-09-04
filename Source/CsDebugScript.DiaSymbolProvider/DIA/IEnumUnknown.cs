using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// This interface implements the IEnumUnknown enumeration methods in the Microsoft.VisualStudio.OLE.Interop namespace.
    /// </summary>
    [ComImport, Guid("00000100-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumUnknown
    {
        /// <summary>
        /// Retrieves a specified number of records in the enumerated sequence.
        /// </summary>
        /// <param name="celt">The number of records to be retrieved.</param>
        /// <param name="rgelt">A buffer that is to be filled with the debug stream record data.</param>
        /// <param name="pceltFetched">Returns the number of records in data.</param>
        void RemoteNext(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 0)] ref object[] rgelt,
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
        /// <returns>Returns an <see cref="IEnumUnknown"/> object that contains the duplicated sequence of debug data stream records.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumUnknown Clone();
    }
}
