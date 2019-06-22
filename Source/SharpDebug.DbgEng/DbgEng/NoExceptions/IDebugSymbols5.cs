using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("C65FA83E-1E69-475E-8E0E-B5D79E9CC17E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbols5 : IDebugSymbols4
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugSymbols
        [PreserveSig]
        int GetSymbolOptions(
            [Out] out uint Options);

        [PreserveSig]
        int AddSymbolOptions(
            [In] uint Options);

        [PreserveSig]
        int RemoveSymbolOptions(
            [In] uint Options);

        [PreserveSig]
        int SetSymbolOptions(
            [In] uint Options);

        [PreserveSig]
        int GetNameByOffset(
            [In] ulong Offset,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetOffsetByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol,
            [Out] out ulong Offset);

        [PreserveSig]
        int GetNearNameByOffset(
            [In] ulong Offset,
            [In] int Delta,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetLineByOffset(
            [In] ulong Offset,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetOffsetByLine(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [Out] out ulong Offset);

        [PreserveSig]
        int GetNumberModules(
            [Out] out uint Loaded,
            [Out] out uint Unloaded);

        [PreserveSig]
        int GetModuleByIndex(
            [In] uint Index,
            [Out] out ulong Base);

        [PreserveSig]
        int GetModuleByModuleName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [In] uint StartIndex,
            [Out] out uint Index,
            [Out] out ulong Base);

        [PreserveSig]
        int GetModuleByOffset(
            [In] ulong Offset,
            [In] uint StartIndex,
            [Out] out uint Index,
            [Out] out ulong Base);

        [PreserveSig]
        int GetModuleNames(
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

        [PreserveSig]
        int GetModuleParameters(
            [In] uint Count,
            [In] ref ulong Bases,
            [In] uint Start = default(uint),
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_MODULE_PARAMETERS[] Params = null);

        [PreserveSig]
        int GetSymbolModule(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol,
            [Out] out ulong Base);

        [PreserveSig]
        int GetTypeName(
            [In] ulong Module,
            [In] uint TypeId,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetTypeId(
            [In] ulong Module,
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [Out] out uint Id);

        [PreserveSig]
        int GetTypeSize(
            [In] ulong Module,
            [In] uint TypeId,
            [Out] out uint Size);

        [PreserveSig]
        int GetFieldOffset(
            [In] ulong Module,
            [In] uint TypeId,
            [In, MarshalAs(UnmanagedType.LPStr)] string Field,
            [Out] out uint Offset);

        [PreserveSig]
        int GetSymbolTypeId(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol,
            [Out] out uint TypeId,
            [Out] out ulong Module);

        [PreserveSig]
        int GetOffsetTypeId(
            [In] ulong Offset,
            [Out] out uint TypeId,
            [Out] out ulong Module);

        [PreserveSig]
        int ReadTypedDataVirtual(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WriteTypedDataVirtual(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int OutputTypedDataVirtual(
            [In] uint OutputControl,
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint Flags);

        [PreserveSig]
        int ReadTypedDataPhysical(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesRead);

        [PreserveSig]
        int WriteTypedDataPhysical(
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BytesWritten);

        [PreserveSig]
        int OutputTypedDataPhysical(
            [In] uint OutputControl,
            [In] ulong Offset,
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint Flags);

        [PreserveSig]
        int GetScope(
            [Out] out ulong InstructionOffset,
            [Out] out _DEBUG_STACK_FRAME ScopeFrame,
            [Out] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        [PreserveSig]
        int SetScope(
            [In] ulong InstructionOffset,
            [In] ref _DEBUG_STACK_FRAME ScopeFrame,
            [In] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        [PreserveSig]
        int ResetScope();

        [PreserveSig]
        int GetScopeSymbolGroup(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugSymbolGroup Update,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup Symbols);

        [PreserveSig]
        int CreateSymbolGroup(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup Symbols);

        [PreserveSig]
        int StartSymbolMatch(
            [In, MarshalAs(UnmanagedType.LPStr)] string Pattern,
            [Out] out ulong Handle);

        [PreserveSig]
        int GetNextSymbolMatch(
            [In] ulong Handle,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize, [Out] out uint MatchSize,
            [Out] out ulong Offset);

        [PreserveSig]
        int EndSymbolMatch(
            [In] ulong Handle);

        [PreserveSig]
        int Reload(
            [In, MarshalAs(UnmanagedType.LPStr)] string Module);

        [PreserveSig]
        int GetSymbolPath(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        [PreserveSig]
        int SetSymbolPath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path);

        [PreserveSig]
        int AppendSymbolPath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Addition);

        [PreserveSig]
        int GetImagePath(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        [PreserveSig]
        int SetImagePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path);

        [PreserveSig]
        int AppendImagePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Addition);

        [PreserveSig]
        int GetSourcePath(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        [PreserveSig]
        int GetSourcePathElement(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ElementSize);

        [PreserveSig]
        int SetSourcePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path);

        [PreserveSig]
        int AppendSourcePath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Addition);

        [PreserveSig]
        int FindSourceFile(
            [In] uint StartElement,
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] uint Flags,
            [Out] out uint FoundElement,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FoundSize);

        [PreserveSig]
        int GetSourceFileLineOffsets(
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [Out] out ulong Buffer,
            [In] uint BufferLines,
            [Out] out uint FileLines);
        #endregion

        #region IDebugSymbols2
        [PreserveSig]
        int GetModuleVersionInformation(
            [In] uint Index,
            [In] ulong Base,
            [In, MarshalAs(UnmanagedType.LPStr)] string Item,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint VerInfoSize);

        [PreserveSig]
        int GetModuleNameString(
            [In] uint Which,
            [In] uint Index,
            [In] ulong Base,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetConstantName(
            [In] ulong Module,
            [In] uint TypeId,
            [In] ulong Value,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetFieldName(
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint FieldIndex,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetTypeOptions(
            [Out] out uint Options);

        [PreserveSig]
        int AddTypeOptions(
            [In] uint Options);

        [PreserveSig]
        int RemoveTypeOptions(
            [In] uint Options);

        [PreserveSig]
        int SetTypeOptions(
            [In] uint Options);
        #endregion

        #region IDebugSymbols3
        [PreserveSig]
        int GetNameByOffsetWide(
            [In] ulong Offset,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetOffsetByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol,
            [Out] out ulong Offset);

        [PreserveSig]
        int GetNearNameByOffsetWide(
            [In] ulong Offset,
            [In] int Delta,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetLineByOffsetWide(
            [In] ulong Offset,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetOffsetByLineWide(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [Out] out ulong Offset);

        [PreserveSig]
        int GetModuleByModuleNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In] uint StartIndex,
            [Out] out uint Index,
            [Out] out ulong Base);

        [PreserveSig]
        int GetSymbolModuleWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol,
            [Out] out ulong Base);

        [PreserveSig]
        int GetTypeNameWide(
            [In] ulong Module,
            [In] uint TypeId,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetTypeIdWide(
            [In] ulong Module,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [Out] out uint Id);

        [PreserveSig]
        int GetFieldOffsetWide(
            [In] ulong Module,
            [In] uint TypeId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Field,
            [Out] out uint Offset);

        [PreserveSig]
        int GetSymbolTypeIdWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol,
            [Out] out uint TypeId,
            [Out] out ulong Module);

        [PreserveSig]
        int GetScopeSymbolGroup2(
            [In] uint Flags,
            [In, MarshalAs(UnmanagedType.Interface)] IDebugSymbolGroup2 Update,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup2 Symbols);

        [PreserveSig]
        int CreateSymbolGroup2(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugSymbolGroup2 Symbols);

        [PreserveSig]
        int StartSymbolMatchWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Pattern,
            [Out] out ulong Handle);

        [PreserveSig]
        int GetNextSymbolMatchWide(
            [In] ulong Handle,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint MatchSize,
            [Out] out ulong Offset);

        [PreserveSig]
        int ReloadWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Module);

        [PreserveSig]
        int GetSymbolPathWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        [PreserveSig]
        int SetSymbolPathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path);

        [PreserveSig]
        int AppendSymbolPathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Addition);

        [PreserveSig]
        int GetImagePathWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        [PreserveSig]
        int SetImagePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path);

        [PreserveSig]
        int AppendImagePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Addition);

        [PreserveSig]
        int GetSourcePathWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint PathSize);

        [PreserveSig]
        int GetSourcePathElementWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ElementSize);

        [PreserveSig]
        int SetSourcePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path);

        [PreserveSig]
        int AppendSourcePathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Addition);

        [PreserveSig]
        int FindSourceFileWide(
            [In] uint StartElement,
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] uint Flags,
            [Out] out uint FoundElement,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FoundSize);

        [PreserveSig]
        int GetSourceFileLineOffsetsWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [Out] out ulong Buffer,
            [In] uint BufferLines,
            [Out] out uint FileLines);

        [PreserveSig]
        int GetModuleVersionInformationWide(
            [In] uint Index,
            [In] ulong Base,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Item,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint VerInfoSize);

        [PreserveSig]
        int GetModuleNameStringWide(
            [In] uint Which,
            [In] uint Index,
            [In] ulong Base,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetConstantNameWide(
            [In] ulong Module,
            [In] uint TypeId,
            [In] ulong Value,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int GetFieldNameWide(
            [In] ulong Module,
            [In] uint TypeId,
            [In] uint FieldIndex,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize);

        [PreserveSig]
        int IsManagedModule(
            [In] uint Index,
            [In] ulong Base);

        [PreserveSig]
        int GetModuleByModuleName2(
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [In] uint StartIndex,
            [In] uint Flags,
            [Out] out uint Index,
            [Out] out ulong Base);

        [PreserveSig]
        int GetModuleByModuleName2Wide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In] uint StartIndex,
            [In] uint Flags,
            [Out] out uint Index,
            [Out] out ulong Base);

        [PreserveSig]
        int GetModuleByOffset2(
            [In] ulong Offset,
            [In] uint StartIndex,
            [In] uint Flags,
            [Out] out uint Index,
            [Out] out ulong Base);

        [PreserveSig]
        int AddSyntheticModule(
            [In] ulong Base,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPStr)] string ImagePath,
            [In, MarshalAs(UnmanagedType.LPStr)] string ModuleName,
            [In] uint Flags);

        [PreserveSig]
        int AddSyntheticModuleWide(
            [In] ulong Base,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ImagePath,
            [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName,
            [In] uint Flags);

        [PreserveSig]
        int RemoveSyntheticModule(
            [In] ulong Base);

        [PreserveSig]
        int GetCurrentScopeFrameIndex(
            [Out] out uint Index);

        [PreserveSig]
        int SetScopeFrameByIndex(
            [In] uint Index);

        [PreserveSig]
        int SetScopeFromJitDebugInfo(
            [In] uint OutputControl,
            [In] ulong InfoOffset);

        [PreserveSig]
        int SetScopeFromStoredEvent();

        [PreserveSig]
        int OutputSymbolByOffset(
            [In] uint OutputControl,
            [In] uint Flags,
            [In] ulong Offset);

        [PreserveSig]
        int GetFunctionEntryByOffset(
            [In] ulong Offset,
            [In] uint Flags,
            [Out] IntPtr Buffer,
            [In] uint BufferSize,
            [Out] out uint BufferNeeded);

        [PreserveSig]
        int GetFieldTypeAndOffset(
            [In] ulong Module,
            [In] uint ContainerTypeId,
            [In, MarshalAs(UnmanagedType.LPStr)] string Field,
            [Out] out uint FieldTypeId,
            [Out] out uint Offset);

        [PreserveSig]
        int GetFieldTypeAndOffsetWide(
            [In] ulong Module,
            [In] uint ContainerTypeId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Field,
            [Out] out uint FieldTypeId,
            [Out] out uint Offset);

        [PreserveSig]
        int AddSyntheticSymbol(
            [In] ulong Offset,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPStr)] string Name,
            [In] uint Flags,
            [Out] out _DEBUG_MODULE_AND_ID Id);

        [PreserveSig]
        int AddSyntheticSymbolWide(
            [In] ulong Offset,
            [In] uint Size,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Name,
            [In] uint Flags,
            [Out] out _DEBUG_MODULE_AND_ID Id);

        [PreserveSig]
        int RemoveSyntheticSymbol(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_MODULE_AND_ID Id);

        [PreserveSig]
        int GetSymbolEntriesByOffset(
            [In] ulong Offset,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_MODULE_AND_ID[] Ids,
            [Out, MarshalAs(UnmanagedType.LPArray)] ulong[] Displacements,
            [In] uint IdsCount,
            [Out] out uint Entries);

        [PreserveSig]
        int GetSymbolEntriesByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string Symbol,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_MODULE_AND_ID[] Ids,
            [In] uint IdsCount,
            [Out] out uint Entries);

        [PreserveSig]
        int GetSymbolEntriesByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Symbol,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_MODULE_AND_ID[] Ids,
            [In] uint IdsCount,
            [Out] out uint Entries);

        [PreserveSig]
        int GetSymbolEntryByToken(
            [In] ulong ModuleBase,
            [In] uint Token,
            [Out] out _DEBUG_MODULE_AND_ID Id);

        [PreserveSig]
        int GetSymbolEntryInformation(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_MODULE_AND_ID Id,
            [Out] out _DEBUG_SYMBOL_ENTRY Info);

        [PreserveSig]
        int GetSymbolEntryString(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_MODULE_AND_ID Id,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        [PreserveSig]
        int GetSymbolEntryStringWide(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_MODULE_AND_ID Id,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        [PreserveSig]
        int GetSymbolEntryOffsetRegions(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_MODULE_AND_ID Id,
            [In] uint Flags,
            [Out] IntPtr Regions,
            [In] uint RegionsCount,
            [Out] out uint RegionsAvail);

        [PreserveSig]
        int GetSymbolEntryBySymbolEntry(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_MODULE_AND_ID FromId,
            [In] uint Flags,
            [Out] out _DEBUG_MODULE_AND_ID Id);

        [PreserveSig]
        int GetSourceEntriesByOffset(
            [In] ulong Offset,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_SYMBOL_SOURCE_ENTRY[] Entries,
            [In] uint EntriesCount,
            [Out] out uint EntriesAvail);

        [PreserveSig]
        int GetSourceEntriesByLine(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_SYMBOL_SOURCE_ENTRY[] Entries,
            [In] uint EntriesCount,
            [Out] out uint EntriesAvail);

        [PreserveSig]
        int GetSourceEntriesByLineWide(
            [In] uint Line,
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_SYMBOL_SOURCE_ENTRY[] Entries,
            [In] uint EntriesCount,
            [Out] out uint EntriesAvail);

        [PreserveSig]
        int GetSourceEntryString(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_SYMBOL_SOURCE_ENTRY Entry,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        [PreserveSig]
        int GetSourceEntryStringWide(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_SYMBOL_SOURCE_ENTRY Entry,
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        [PreserveSig]
        int GetSourceEntryOffsetRegions(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_SYMBOL_SOURCE_ENTRY Entry,
            [In] uint Flags,
            [Out] IntPtr Regions,
            [In] uint RegionsCount,
            [Out] out uint RegionsAvail);

        [PreserveSig]
        int GetSourceEntryBySourceEntry(
            [In, MarshalAs(UnmanagedType.LPStruct)] _DEBUG_SYMBOL_SOURCE_ENTRY FromEntry,
            [In] uint Flags,
            [Out] out _DEBUG_SYMBOL_SOURCE_ENTRY ToEntry);
        #endregion

        #region IDebugSymbols4
        [PreserveSig]
        int GetScopeEx(
            [Out] out ulong InstructionOffset,
            [Out] out _DEBUG_STACK_FRAME_EX ScopeFrame,
            [Out] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        [PreserveSig]
        int SetScopeEx(
            [In] ulong InstructionOffset,
            [In] ref _DEBUG_STACK_FRAME_EX ScopeFrame,
            [In] IntPtr ScopeContext = default(IntPtr),
            [In] uint ScopeContextSize = default(uint));

        [PreserveSig]
        int GetNameByInlineContext(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetNameByInlineContextWide(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder NameBuffer,
            [In] uint NameBufferSize,
            [Out] out uint NameSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetLineByInlineContext(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int GetLineByInlineContextWide(
            [In] ulong Offset,
            [In] uint InlineContext,
            [Out] out uint Line,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FileBuffer,
            [In] uint FileBufferSize,
            [Out] out uint FileSize,
            [Out] out ulong Displacement);

        [PreserveSig]
        int OutputSymbolByInlineContext(
            [In] uint OutputControl,
            [In] uint Flags,
            [In] ulong Offset,
            [In] uint InlineContext);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int GetCurrentScopeFrameIndexEx(
            [In] uint Flags,
            [Out] out uint Index);

        [PreserveSig]
        int SetScopeFrameByIndexEx(
            [In] uint Flags,
            [In] uint Index);
    }
}
