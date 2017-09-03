using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DIA
{
    [ComImport, DefaultMember("Item"), Guid("FE30E878-54AC-44F1-81BA-39DE940F6052"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumLineNumbers
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
        IDiaLineNumber Item(
            [In] uint index);

        void Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaLineNumber rgelt,
            [Out] out uint pceltFetched);

        void Skip(
            [In] uint celt);

        void Reset();

        void Clone(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppenum);
    }
}
