using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("2F609EE1-D1C8-4E24-8288-3326BADCD211")]
    public interface IDiaSession
    {
        [DispId(1)]
        ulong loadAddress { get; set; }

        [DispId(2)]
        IDiaSymbol globalScope
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        void getEnumTables(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumTables ppEnumTables);

        void getSymbolsByAddr(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbolsByAddr ppEnumbyAddr);

        void findChildren(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findChildrenEx(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findChildrenExByAddr(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [In] uint isect,
            [In] uint offset,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findChildrenExByVA(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [In] ulong va,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findChildrenExByRVA(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] SymTagEnum symTag,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [In] uint rva,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findSymbolByAddr(
            [In] uint isect,
            [In] uint offset,
            [In] SymTagEnum symTag,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol);

        void findSymbolByRVA(
            [In] uint rva,
            [In] SymTagEnum symTag,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol);

        void findSymbolByVA(
            [In] ulong va,
            [In] SymTagEnum symTag,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol);

        void findSymbolByToken(
            [In] uint token,
            [In] SymTagEnum symTag,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol);

        void symsAreEquiv(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol symbolA,
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol symbolB);

        void symbolById(
            [In] uint id,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol);

        void findSymbolByRVAEx(
            [In] uint rva,
            [In] SymTagEnum symTag,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol,
            [Out] out int displacement);

        void findSymbolByVAEx(
            [In] ulong va,
            [In] SymTagEnum symTag,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSymbol ppSymbol,
            [Out] out int displacement);

        void findFile(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol pCompiland,
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint compareFlags,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSourceFiles ppResult);

        void findFileById(
            [In] uint uniqueId,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSourceFile ppResult);

        void findLines(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol compiland,
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSourceFile file,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findLinesByAddr(
            [In] uint seg,
            [In] uint offset,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findLinesByRVA(
            [In] uint rva,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findLinesByVA(
            [In] ulong va,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findLinesByLinenum(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol compiland,
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSourceFile file,
            [In] uint linenum,
            [In] uint column,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInjectedSource(
            [In, MarshalAs(UnmanagedType.LPWStr)] string srcFile,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumInjectedSources ppResult);

        void getEnumDebugStreams(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumDebugStreams ppEnumDebugStreams);

        void findInlineFramesByAddr(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] uint isect,
            [In] uint offset,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findInlineFramesByRVA(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] uint rva,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findInlineFramesByVA(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] ulong va,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findInlineeLines(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineeLinesByAddr(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] uint isect,
            [In] uint offset,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineeLinesByRVA(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] uint rva,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineeLinesByVA(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol parent,
            [In] ulong va,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineeLinesByLinenum(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol compiland,
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSourceFile file,
            [In] uint linenum,
            [In] uint column,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInlineesByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] uint option,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void addressForVA(
            [In] ulong va,
            [Out] out uint pISect,
            [Out] out uint pOffset);

        void addressForRVA(
            [In] uint rva,
            [Out] out uint pISect,
            [Out] out uint pOffset);

        void findILOffsetsByAddr(
            [In] uint isect,
            [In] uint offset,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findILOffsetsByRVA(
            [In] uint rva,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findILOffsetsByVA(
            [In] ulong va,
            [In] uint length,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumLineNumbers ppResult);

        void findInputAssemblyFiles(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumInputAssemblyFiles ppResult);

        void findInputAssembly(
            [In] uint index,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaInputAssemblyFile ppResult);

        void findInputAssemblyById([In] uint uniqueId,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaInputAssemblyFile ppResult);

        void getFuncMDTokenMapSize(
            [Out] out uint pcb);

        void getFuncMDTokenMap(
            [In] uint cb,
            [Out] out uint pcb,
            [Out] out byte pb);

        void getTypeMDTokenMapSize(
            [Out] out uint pcb);

        void getTypeMDTokenMap(
            [In] uint cb,
            [Out] out uint pcb,
            [Out] out byte pb);

        void getNumberOfFunctionFragments_VA(
            [In] ulong vaFunc,
            [In] uint cbFunc,
            [Out] out uint pNumFragments);

        void getNumberOfFunctionFragments_RVA(
            [In] uint rvaFunc,
            [In] uint cbFunc,
            [Out] out uint pNumFragments);

        void getFunctionFragments_VA(
            [In] ulong vaFunc,
            [In] uint cbFunc,
            [In] uint cFragments,
            [Out] out ulong pVaFragment,
            [Out] out uint pLenFragment);

        void getFunctionFragments_RVA(
            [In] uint rvaFunc,
            [In] uint cbFunc,
            [In] uint cFragments,
            [Out] out uint pRvaFragment,
            [Out] out uint pLenFragment);

        void getExports(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void getHeapAllocationSites(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbols ppResult);

        void findInputAssemblyFile(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaSymbol pSymbol,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaInputAssemblyFile ppResult);
    }
}
