using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DIA
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CAB72C48-443B-48F5-9B0B-42F0820AB29A"), DefaultMember("Item")]
    public interface IDiaEnumSymbols
    {
        [DispId(-4)]
        object _NewEnum
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        [DispId(1)]
        int count { get; }

        [return: MarshalAs(UnmanagedType.Interface)]
        IDiaSymbol Item(
            [In] uint index);

        void Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol rgelt,
            [Out] out uint pceltFetched);

        void Skip(
            [In] uint celt);

        void Reset();

        void Clone(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppenum);
    }
}
