using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("C65FA83E-1E69-475E-8E0E-B5D79E9CC17E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbols5 : IDebugSymbols4
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugSymbols
        // ---------------------------------------------------------------------------------------------

        uint GetSymbolOptions();

        void AddSymbolOptions(
            [In] uint Options);

        void RemoveSymbolOptions(
            [In] uint Options);

        void SetSymbolOptions(
            [In] uint Options);

        void GetNameByOffset(
            [In] ulong Offset,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        ulong GetOffsetByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol);

        void GetNearNameByOffset(
            [In] ulong Offset,
            [In] int Delta,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        void GetLineByOffset(
            [In] ulong Offset,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        ulong GetOffsetByLine(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPStr)] string File);

        void GetNumberModules(
            [Out] out uint Loaded,
            [Out] out uint Unloaded);

        ulong GetModuleByIndex(
            [In] uint Index);

        void GetModuleByModuleName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [In] uint StartIndex,
            [Out] out uint Index,
            [Out] out ulong Base);

        void GetModuleByOffset(
            [In] ulong Offset,
            [In] uint StartIndex,
            [Out] out uint Index,
            [Out] out ulong Base);

        void GetModuleNames(
            [In] uint Index,
            [In] ulong Base,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder ImageNameBuffer,
            [In] uint ImageNameBufferSize,
            [Out] out uint ImageNameSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder ModuleNameBuffer,
            [In] uint ModuleNameBufferSize,
            [Out] out uint ModuleNameSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder LoadedImageNameBuffer,
            [In] uint LoadedImageNameBufferSize,
            [Out] out uint LoadedImageNameSize);

        void GetModuleParameters(
            [In] uint Count,
            [In] ref ulong Bases,
            [In] uint Start = default(uint),
            [Out] IntPtr Params = default(IntPtr));

        ulong GetSymbolModule(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol);

        void GetTypeName(
            [In] ulong Module,
            [In] uint TypeId,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        uint GetTypeId(
            [In] ulong Module,
            [In, MarshalAs(UnmanagedType.LPStr)] string Name);

        uint GetTypeSize(
            [In] ulong Module,
            [In] uint TypeId);

        uint GetFieldOffset(
            [In] ulong Module,
            [In] uint TypeId,
            [In, MarshalAs(UnmanagedType.LPStr)] string Field);

        void GetSymbolTypeId(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol,
            [Out] out uint TypeId,
            [Out] out ulong Module);

        void GetOffsetTypeId(
            [In] ulong Offset,
            [Out] out uint TypeId,
            [Out] out ulong Module);

        void ReadTypedDataVirtual(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WriteTypedDataVirtual(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        void OutputTypedDataVirtual(
            [In] uint OutputControl,
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint Flags);

        void ReadTypedDataPhysical(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        void WriteTypedDataPhysical(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        void OutputTypedDataPhysical(
            [In] uint OutputControl,
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint Flags);

        void GetScope(
            [Out] out ulong InstructionOffset,
            [Out] out _DEBUG_STACK_FRAME ScopeFrame,
            [Out] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        void SetScope(
            [In] ulong InstructionOffset,
            [In] ref _DEBUG_STACK_FRAME ScopeFrame,
            [In] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        void ResetScope();

        void GetScopeSymbolGroup(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugSymbolGroup Update,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup Symbols);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugSymbolGroup CreateSymbolGroup();

        ulong StartSymbolMatch(
            [In, MarshalAs(UnmanagedType.LPStr)] string Pattern);

        void GetNextSymbolMatch(
            [In] ulong Handle,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize, [Out] out uint MatchSize,
            [Out] out ulong Offset);

        void EndSymbolMatch(
            [In] ulong Handle);

        void Reload(
            [In, MarshalAs(UnmanagedType.LPStr)] string Module);

        void GetSymbolPath(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        void SetSymbolPath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path);

        void AppendSymbolPath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Addition);

        void GetImagePath(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        void SetImagePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path);

        void AppendImagePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Addition);

        void GetSourcePath(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        void GetSourcePathElement(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ElementSize);

        void SetSourcePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path);

        void AppendSourcePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Addition);

        void FindSourceFile(
            [In] uint StartElement,
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] uint Flags,
            [Out] out uint FoundElement,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FoundSize);

        void GetSourceFileLineOffsets(
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [Out] out ulong Buffer,
            [In] uint BufferLines,
            [Out] out uint FileLines);

        // ---------------------------------------------------------------------------------------------
        // IDebugSymbols2
        // ---------------------------------------------------------------------------------------------

        void GetModuleVersionInformation(
            [In] uint Index,
            [In] ulong Base,
            [In, MarshalAs(UnmanagedType.LPStr)] string Item,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            out uint VerInfoSize);

        void GetModuleNameString(
            [In] uint Which,
            [In] uint Index,
            [In] ulong Base,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        void GetConstantName(
            [In] ulong Module,
            [In] uint TypeId,
            [In] ulong Value,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        void GetFieldName(
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint FieldIndex,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        uint GetTypeOptions();

        void AddTypeOptions(
            [In] uint Options);

        void RemoveTypeOptions(
            [In] uint Options);

        void SetTypeOptions(
            [In] uint Options);

        // ---------------------------------------------------------------------------------------------
        // IDebugSymbols3
        // ---------------------------------------------------------------------------------------------

        void GetNameByOffsetWide(
            [In] ulong Offset,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        ulong GetOffsetByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol);

        void GetNearNameByOffsetWide(
            [In] ulong Offset,
            [In] int Delta,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        void GetLineByOffsetWide(
            [In] ulong Offset,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        ulong GetOffsetByLineWide(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPWStr)] string File);

        void GetModuleByModuleNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In] uint StartIndex,
            [Out] out uint Index,
            [Out] out ulong Base);

        ulong GetSymbolModuleWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol);

        void GetTypeNameWide(
            [In] ulong Module,
            [In] uint TypeId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        uint GetTypeIdWide(
            [In] ulong Module,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name);

        uint GetFieldOffsetWide(
            [In] ulong Module,
            [In] uint TypeId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Field);

        void GetSymbolTypeIdWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol,
            [Out] out uint TypeId,
            [Out] out ulong Module);

        void GetScopeSymbolGroup2(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugSymbolGroup2 Update,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup2 Symbols);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugSymbolGroup2 CreateSymbolGroup2();

        ulong StartSymbolMatchWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Pattern);

        void GetNextSymbolMatchWide(
            [In] ulong Handle,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint MatchSize,
            [Out] out ulong Offset);

        void ReloadWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Module);

        void GetSymbolPathWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        void SetSymbolPathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path);

        void AppendSymbolPathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Addition);

        void GetImagePathWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        void SetImagePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path);

        void AppendImagePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Addition);

        void GetSourcePathWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        void GetSourcePathElementWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ElementSize);

        void SetSourcePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path);

        void AppendSourcePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Addition);

        void FindSourceFileWide(
            [In] uint StartElement,
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] uint Flags,
            [Out] out uint FoundElement,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FoundSize);

        void GetSourceFileLineOffsetsWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [Out] out ulong Buffer,
            [In] uint BufferLines,
            [Out] out uint FileLines);

        void GetModuleVersionInformationWide(
            [In] uint Index,
            [In] ulong Base,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Item,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint VerInfoSize);

        void GetModuleNameStringWide(
            [In] uint Which,
            [In] uint Index,
            [In] ulong Base,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        void GetConstantNameWide(
            [In] ulong Module,
            [In] uint TypeId,
            [In] ulong Value,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        void GetFieldNameWide(
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint FieldIndex,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        void IsManagedModule(
            [In] uint Index,
            [In] ulong Base);

        void GetModuleByModuleName2(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [In] uint StartIndex,
            [In] uint Flags,
            [Out] out uint Index,
            [Out] out ulong Base);

        void GetModuleByModuleName2Wide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In] uint StartIndex,
            [In] uint Flags,
            [Out] out uint Index,
            [Out] out ulong Base);

        void GetModuleByOffset2(
            [In] ulong Offset,
            [In] uint StartIndex,
            [In] uint Flags,
            [Out] out uint Index,
            [Out] out ulong Base);

        void AddSyntheticModule(
            [In] ulong Base,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPStr)] string ImagePath,
            [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName,
            [In] uint Flags);

        void AddSyntheticModuleWide(
            [In] ulong Base,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImagePath,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName,
            [In] uint Flags);

        void RemoveSyntheticModule(
            [In] ulong Base);

        uint GetCurrentScopeFrameIndex();

        void SetScopeFrameByIndex(
            [In] uint Index);

        void SetScopeFromJitDebugInfo(
            [In] uint OutputControl,
            [In] ulong InfoOffset);

        void SetScopeFromStoredEvent();

        void OutputSymbolByOffset(
            [In] uint OutputControl,
            [In] uint Flags,
            [In] ulong Offset);

        void GetFunctionEntryByOffset(
            [In] ulong Offset,
            [In] uint Flags,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BufferNeeded);

        void GetFieldTypeAndOffset(
            [In] ulong Module,
            [In] uint ContainerTypeId,
            [In, MarshalAs(UnmanagedType.LPStr)] string Field,
            [Out] out uint FieldTypeId,
            [Out] out uint Offset);

        void GetFieldTypeAndOffsetWide(
            [In] ulong Module,
            [In] uint ContainerTypeId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Field,
            [Out] out uint FieldTypeId,
            [Out] out uint Offset);

        void AddSyntheticSymbol(
            [In] ulong Offset,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [In] uint Flags,
            [Out] out _DEBUG_MODULE_AND_ID Id);

        void AddSyntheticSymbolWide(
            [In] ulong Offset,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In] uint Flags,
            [Out] out _DEBUG_MODULE_AND_ID Id);

        void RemoveSyntheticSymbol(
            [In] ref _DEBUG_MODULE_AND_ID Id);

        void GetSymbolEntriesByOffset(
            [In] ulong Offset,
            [In] uint Flags,
            [Out] IntPtr Ids,
            [Out] out ulong Displacements,
            [In] uint IdsCount,
            [Out] out uint Entries);

        void GetSymbolEntriesByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol,
            [In] uint Flags,
            [Out] IntPtr Ids,
            [In] uint IdsCount,
            [Out] out uint Entries);

        void GetSymbolEntriesByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol,
            [In] uint Flags,
            [Out] IntPtr Ids,
            [In] uint IdsCount,
            [Out] out uint Entries);

        _DEBUG_MODULE_AND_ID GetSymbolEntryByToken(
            [In] ulong ModuleBase,
            [In] uint Token);

        _DEBUG_SYMBOL_ENTRY GetSymbolEntryInformation(
            [In] ref _DEBUG_MODULE_AND_ID Id);

        void GetSymbolEntryString(
            [In] ref _DEBUG_MODULE_AND_ID Id,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void GetSymbolEntryStringWide(
            [In] ref _DEBUG_MODULE_AND_ID Id,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void GetSymbolEntryOffsetRegions(
            [In] ref _DEBUG_MODULE_AND_ID Id,
            [In] uint Flags,
            [Out] IntPtr Regions,
            [In] uint RegionsCount,
            [Out] out uint RegionsAvail);

        _DEBUG_MODULE_AND_ID GetSymbolEntryBySymbolEntry(
            [In] ref _DEBUG_MODULE_AND_ID FromId,
            [In] uint Flags);

        void GetSourceEntriesByOffset(
            [In] ulong Offset,
            [In] uint Flags,
            [Out] IntPtr Entries,
            [In] uint EntriesCount,
            [Out] out uint EntriesAvail);

        void GetSourceEntriesByLine(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] uint Flags,
            [Out] IntPtr Entries,
            [In] uint EntriesCount,
            [Out] out uint EntriesAvail);

        void GetSourceEntriesByLineWide(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] uint Flags,
            [Out] IntPtr Entries,
            [In] uint EntriesCount,
            [Out] out uint EntriesAvail);

        void GetSourceEntryString(
            [In] ref _DEBUG_SYMBOL_SOURCE_ENTRY Entry,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void GetSourceEntryStringWide(
            [In] ref _DEBUG_SYMBOL_SOURCE_ENTRY Entry,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void GetSourceEntryOffsetRegions(
            [In] ref _DEBUG_SYMBOL_SOURCE_ENTRY Entry,
            [In] uint Flags,
            [Out] IntPtr Regions,
            [In] uint RegionsCount,
            [Out] out uint RegionsAvail);

        _DEBUG_SYMBOL_SOURCE_ENTRY GetSourceEntryBySourceEntry(
            [In] ref _DEBUG_SYMBOL_SOURCE_ENTRY FromEntry,
            [In] uint Flags);

        // ---------------------------------------------------------------------------------------------
        // IDebugSymbols4
        // ---------------------------------------------------------------------------------------------

        void GetScopeEx(
            [Out] out ulong InstructionOffset,
            [Out] out _DEBUG_STACK_FRAME_EX ScopeFrame,
            [Out] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        void SetScopeEx(
            [In] ulong InstructionOffset,
            [In] ref _DEBUG_STACK_FRAME_EX ScopeFrame,
            [In] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        void GetNameByInlineContext(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        void GetNameByInlineContextWide(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        void GetLineByInlineContext(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        void GetLineByInlineContextWide(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        void OutputSymbolByInlineContext(
            [In] uint OutputControl,
            [In] uint Flags,
            [In] ulong Offset,
            [In] uint InlineContext);

        // ---------------------------------------------------------------------------------------------
        // IDebugSymbols5
        // ---------------------------------------------------------------------------------------------

        uint GetCurrentScopeFrameIndexEx(
            [In] uint Flags);

        void SetScopeFrameByIndexEx(
            [In] uint Flags,
            [In] uint Index);
    }
}
