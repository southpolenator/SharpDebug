using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("8C31E98C-983A-48A5-9016-6FE5D667A950"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbols
    {
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
    }
}
