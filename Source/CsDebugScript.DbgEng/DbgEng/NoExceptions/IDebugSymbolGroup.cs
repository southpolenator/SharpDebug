using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("F2528316-0F1A-4431-AEED-11D096E1E2AB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbolGroup
    {
        [PreserveSig]
        int GetNumberSymbols(
            [Out] out uint Number);

        [PreserveSig]
        int AddSymbol(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [In, Out] ref uint Index);

        [PreserveSig]
        int RemoveSymbolByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name);

        [PreserveSig]
        int RemoveSymbolByIndex(
            [In] uint Index);

        [PreserveSig]
        int GetSymbolName(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetSymbolParameters(
            [In] uint Start,
            [In] uint Count,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_SYMBOL_PARAMETERS[] Params);

        [PreserveSig]
        int ExpandSymbol(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.Bool)] bool Expand);

        [PreserveSig]
        int OutputSymbols(
            [In] uint OutputControl,
            [In] uint Flags,
            [In] uint Start,
            [In] uint Count);

        [PreserveSig]
        int WriteSymbol(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Value);

        [PreserveSig]
        int OutputAsType(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Type);
    }
}
