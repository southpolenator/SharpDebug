using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("F2528316-0F1A-4431-AEED-11D096E1E2AB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbolGroup
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugSymbolGroup
        // ---------------------------------------------------------------------------------------------

        uint GetNumberSymbols();

        uint AddSymbol(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name);

        void RemoveSymbolByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name);

        void RemoveSymbolByIndex(
            [In] uint Index);

        void GetSymbolName(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        IntPtr GetSymbolParameters(
            [In] uint Start,
            [In] uint Count);

        void ExpandSymbol(
            [In] uint Index,
            [In] int Expand);

        void OutputSymbols(
            [In] uint OutputControl,
            [In] uint Flags,
            [In] uint Start,
            [In] uint Count);

        void WriteSymbol(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Value);

        void OutputAsType(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Type);
    }
}
