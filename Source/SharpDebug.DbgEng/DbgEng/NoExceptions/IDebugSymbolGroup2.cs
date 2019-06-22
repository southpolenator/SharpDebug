using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("6A7CCC5F-FB5E-4DCC-B41C-6C20307BCCC7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbolGroup2 : IDebugSymbolGroup
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugSymbolGroup
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
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int AddSymbolWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In, Out] ref uint Index);

        [PreserveSig]
        int RemoveSymbolByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name);

        [PreserveSig]
        int GetSymbolNameWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int WriteSymbolWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Value);

        [PreserveSig]
        int OutputAsTypeWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Type);

        [PreserveSig]
        int GetSymbolTypeName(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetSymbolTypeNameWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetSymbolSize(
            [In] uint Index,
            [Out] out uint Size);

        [PreserveSig]
        int GetSymbolOffset(
            [In] uint Index,
            [Out] out ulong Offset);

        [PreserveSig]
        int GetSymbolRegister(
            [In] uint Index,
            [Out] out uint Register);

        [PreserveSig]
        int GetSymbolValueText(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetSymbolValueTextWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetSymbolEntryInformation(
            [In] uint Index,
            [Out] out _DEBUG_SYMBOL_ENTRY Entry);
    }
}
