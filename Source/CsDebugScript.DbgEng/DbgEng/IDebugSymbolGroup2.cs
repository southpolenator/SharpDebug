using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("6A7CCC5F-FB5E-4DCC-B41C-6C20307BCCC7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbolGroup2 : IDebugSymbolGroup
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugSymbolGroup
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
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        uint AddSymbolWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name);

        void RemoveSymbolByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name);

        void GetSymbolNameWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        void WriteSymbolWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Value);

        void OutputAsTypeWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Type);

        void GetSymbolTypeName(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        void GetSymbolTypeNameWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        uint GetSymbolSize(
            [In] uint Index);

        ulong GetSymbolOffset(
            [In] uint Index);

        uint GetSymbolRegister(
            [In] uint Index);

        void GetSymbolValueText(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        void GetSymbolValueTextWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        _DEBUG_SYMBOL_ENTRY GetSymbolEntryInformation(
            [In] uint Index);
    }
}
