using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerates by address the various symbols contained in the data source.
    /// </summary>
    [ComImport, Guid("624B7D9C-24EA-4421-9D06-3B577471C1FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumSymbolsByAddr
    {
        /// <summary>
        /// Positions the enumerator by performing a lookup by image section number and offset.
        /// </summary>
        /// <param name="isect">Image section number.</param>
        /// <param name="offset">Offset in section.</param>
        /// <returns>Returns an <see cref="IDiaSymbol"/> object representing the symbol found. </returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaSymbol symbolByAddr(
            [In] uint isect,
            [In] uint offset);

        /// <summary>
        /// Positions the enumerator by performing a lookup by relative virtual address (RVA).
        /// </summary>
        /// <param name="relativeVirtualAddress">Address relative to start of image.</param>
        /// <returns>Returns an <see cref="IDiaSymbol"/> object representing the symbol found. </returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaSymbol symbolByRVA(
            [In] uint relativeVirtualAddress);

        /// <summary>
        /// Positions the enumerator by performing a lookup by virtual address (VA).
        /// </summary>
        /// <param name="virtualAddress">Virtual address.</param>
        /// <returns>Returns an <see cref="IDiaSymbol"/> object representing the symbol found. </returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaSymbol symbolByVA([
            In] ulong virtualAddress);

        /// <summary>
        /// Retrieves the next symbols in order by address.
        /// </summary>
        /// <param name="celt">The number of symbols in the enumerator to be retrieved.</param>
        /// <param name="rgelt">An array that is to be filled in with the <see cref="IDiaSymbol"/> object that represent the desired symbols.</param>
        /// <param name="pceltFetched">Returns the number of symbols in the fetched enumerator.</param>
        void Next(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaSymbol[] rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Retrieves the previous symbols in order by address.
        /// </summary>
        /// <param name="celt">The number of symbols in the enumerator to be retrieved.</param>
        /// <param name="rgelt">An array that is to be filled in with the <see cref="IDiaSymbol"/> object that represent the desired symbols.</param>
        /// <param name="pceltFetched">Returns the number of symbols in the fetched enumerator.</param>
        void Prev(
            [In] uint celt,
            [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] ref IDiaSymbol[] rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Makes a copy of an object.
        /// </summary>
        /// <returns>Returns an <see cref="IDiaEnumSymbolsByAddr"/> object that contains a duplicate of the enumerator. The symbols are not duplicated, only the enumerator.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaEnumSymbolsByAddr Clone();
    }
}
