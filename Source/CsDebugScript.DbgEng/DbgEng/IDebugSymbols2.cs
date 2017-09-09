using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("3A707211-AFDD-4495-AD4F-56FECDF8163F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugSymbols2 : IDebugSymbols
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugSymbols
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
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

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
    }
}
