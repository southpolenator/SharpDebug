using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("94E60CE9-9B41-4B19-9FC0-6D9EB35272B3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugControl4 : IDebugControl3
    {
        // ---------------------------------------------------------------------------------------------
        // IDebugControl
        // ---------------------------------------------------------------------------------------------

        void GetInterrupt();

        void SetInterrupt(
            [In] uint Flags);

        uint GetInterruptTimeout();

        void SetInterruptTimeout(
            [In] uint Seconds);

        void GetLogFile(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out int Append);

        void OpenLogFile(
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] int Append);

        void CloseLogFile();

        uint GetLogMask();

        void SetLogMask(
            [In] uint Mask);

        void Input(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint InputSize);

        void ReturnInput(
            [In, MarshalAs(UnmanagedType.LPStr)] string Buffer);

        void Output(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        void OutputVaList(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In] ref sbyte Args);

        void ControlledOutput(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        void ControlledOutputVaList(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In] ref sbyte Args);

        void OutputPrompt(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        void OutputPromptVaList(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In] ref sbyte Args);

        void GetPromptText(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        void OutputCurrentState(
            [In] uint OutputControl,
            [In] uint Flags);

        void OutputVersionInformation(
            [In] uint OutputControl);

        ulong GetNotifyEventHandle();

        void SetNotifyEventHandle(
            [In] ulong Handle);

        ulong Assemble(
            [In] ulong Offset,
            [In, MarshalAs(UnmanagedType.LPStr)] string Instr);

        void Disassemble(
            [In] ulong Offset,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint DisassemblySize,
            [Out] out ulong EndOffset);

        ulong GetDisassembleEffectiveOffset();

        ulong OutputDisassembly(
            [In] uint OutputControl,
            [In] ulong Offset,
            [In] uint Flags);

        void OutputDisassemblyLines(
            [In] uint OutputControl,
            [In] uint PreviousLines,
            [In] uint TotalLines,
            [In] ulong Offset,
            [In] uint Flags,
            [Out] out uint OffsetLine,
            [Out] out ulong StartOffset,
            [Out] out ulong EndOffset,
            [Out] out ulong LineOffsets);

        ulong GetNearInstruction(
            [In] ulong Offset,
            [In] int Delta);

        void GetStackTrace(
            [In] ulong FrameOffset,
            [In] ulong StackOffset,
            [In] ulong InstructionOffset,
            [Out] IntPtr Frames,
            [In] uint FramesSize,
            [Out] out uint FramesFilled);

        ulong GetReturnOffset();

        void OutputStackTrace(
            [In] uint OutputControl,
            [In] IntPtr Frames = default(IntPtr),
            [In] uint FramesSize = default(uint),
            [In] uint Flags = default(uint));

        void GetDebuggeeType(
            [Out] out uint Class,
            [Out] out uint Qualifier);

        uint GetActualProcessorType();

        uint GetExecutingProcessorType();

        uint GetNumberPossibleExecutingProcessorTypes();

        uint GetPossibleExecutingProcessorTypes(
            [In] uint Start,
            [In] uint Count);

        uint GetNumberProcessors();

        void GetSystemVersion(
            [Out] out uint PlatformId,
            [Out] out uint Major,
            [Out] out uint Minor,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder ServicePackString,
            [In] uint ServicePackStringSize,
            [Out] out uint ServicePackStringUsed,
            [Out] out uint ServicePackNumber,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder BuildString,
            [In] uint BuildStringSize,
            [Out] out uint BuildStringUsed);

        uint GetPageSize();

        void IsPointer64Bit();

        void ReadBugCheckData(
            [Out] out uint Code,
            [Out] out ulong Arg1,
            [Out] out ulong Arg2,
            [Out] out ulong Arg3,
            [Out] out ulong Arg4);

        uint GetNumberSupportedProcessorTypes();

        uint GetSupportedProcessorTypes(
            [In] uint Start,
            [In] uint Count);

        void GetProcessorTypeNames(
            [In] uint Type,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        uint GetEffectiveProcessorType();

        void SetEffectiveProcessorType(
            [In] uint Type);

        uint GetExecutionStatus();

        void SetExecutionStatus(
            [In] uint Status);

        uint GetCodeLevel();

        void SetCodeLevel(
            [In] uint Level);

        uint GetEngineOptions();

        void AddEngineOptions(
            [In] uint Options);

        void RemoveEngineOptions(
            [In] uint Options);

        void SetEngineOptions(
            [In] uint Options);

        void GetSystemErrorControl(
            [Out] out uint OutputLevel,
            [Out] out uint BreakLevel);

        void SetSystemErrorControl(
            [In] uint OutputLevel,
            [In] uint BreakLevel);

        void GetTextMacro(
            [In] uint Slot,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint MacroSize);

        void SetTextMacro(
            [In] uint Slot,
            [In, MarshalAs(UnmanagedType.LPStr)] string Macro);

        uint GetRadix();

        void SetRadix(
            [In] uint Radix);

        void Evaluate(
            [In, MarshalAs(UnmanagedType.LPStr)] string Expression,
            [In] uint DesiredType,
            [Out] out _DEBUG_VALUE Value,
            [Out] out uint RemainderIndex);

        _DEBUG_VALUE CoerceValue(
            [In] ref _DEBUG_VALUE In,
            [In] uint OutType);

        IntPtr CoerceValues(
            [In] uint Count,
            [In] IntPtr In,
            [In] ref uint OutTypes);

        void Execute(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string Command,
            [In] uint Flags);

        void ExecuteCommandFile(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string CommandFile,
            [In] uint Flags);

        uint GetNumberBreakpoints();

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugBreakpoint GetBreakpointByIndex(
            [In] uint Index);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugBreakpoint GetBreakpointById(
            [In] uint Id);

        void GetBreakpointParameters(
            [In] uint Count,
            [In] ref uint Ids,
            [In] uint Start = default(uint),
            [Out] IntPtr Params = default(IntPtr));

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugBreakpoint AddBreakpoint(
            [In] uint Type,
            [In] uint DesiredId);

        void RemoveBreakpoint(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp);

        ulong AddExtension(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path,
            [In] uint Flags);

        void RemoveExtension(
            [In] ulong Handle);

        ulong GetExtensionByPath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path);

        void CallExtension(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPStr)] string Function,
            [In, MarshalAs(UnmanagedType.LPStr)] string Arguments = null);

        IntPtr GetExtensionFunction(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPStr)] string FuncName);

        _WINDBG_EXTENSION_APIS32 GetWindbgExtensionApis32();

        _WINDBG_EXTENSION_APIS64 GetWindbgExtensionApis64();

        void GetNumberEventFilters(
            [Out] out uint SpecificEvents,
            [Out] out uint SpecificExceptions,
            [Out] out uint ArbitraryExceptions);

        void GetEventFilterText(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        void GetEventFilterCommand(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        void SetEventFilterCommand(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Command);

        IntPtr GetSpecificFilterParameters(
            [In] uint Start,
            [In] uint Count);

        void SetSpecificFilterParameters(
            [In] uint Start,
            [In] uint Count,
            [In] IntPtr Params);

        void GetSpecificFilterArgument(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ArgumentSize);

        void SetSpecificFilterArgument(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Argument);

        void GetExceptionFilterParameters(
            [In] uint Count,
            [In] ref uint Codes,
            [In] uint Start = default(uint),
            [Out] IntPtr Params = default(IntPtr));

        void SetExceptionFilterParameters(
            [In] uint Count,
            [In] IntPtr Params);

        void GetExceptionFilterSecondCommand(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        void SetExceptionFilterSecondCommand(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Command);

        void WaitForEvent(
            [In] uint Flags,
            [In] uint Timeout);

        void GetLastEventInformation(
            [Out] out uint Type,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId,
            [Out] IntPtr ExtraInformation,
            [In] uint ExtraInformationSize,
            [Out] out uint ExtraInformationUsed,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Description,
            [In] uint DescriptionSize,
            [Out] out uint DescriptionUsed);

        // ---------------------------------------------------------------------------------------------
        // IDebugControl2
        // ---------------------------------------------------------------------------------------------

        uint GetCurrentTimeDate();

        uint GetCurrentSystemUpTime();

        uint GetDumpFormatFlags();

        uint GetNumberTextReplacements();

        void GetTextReplacement(
            [In, MarshalAs(UnmanagedType.LPStr)] string SrcText,
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder SrcBuffer,
            [In] uint SrcBufferSize,
            [Out] out uint SrcSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder DstBuffer,
            [In] uint DstBufferSize,
            [Out] out uint DstSize);

        void SetTextReplacement(
            [In, MarshalAs(UnmanagedType.LPStr)] string SrcText,
            [In, MarshalAs(UnmanagedType.LPStr)] string DstText);

        void RemoveTextReplacements();

        void OutputTextReplacements(
            [In] uint OutputControl,
            [In] uint Flags);

        // ---------------------------------------------------------------------------------------------
        // IDebugControl3
        // ---------------------------------------------------------------------------------------------

        uint GetAssemblyOptions();

        void AddAssemblyOptions(
            [In] uint Options);

        void RemoveAssemblyOptions(
            [In] uint Options);

        void SetAssemblyOptions(
            [In] uint Options);

        uint GetExpressionSyntax();

        void SetExpressionSyntax(
            [In] uint Flags);

        void SetExpressionSyntaxByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string AbbrevName);

        uint GetNumberExpressionSyntaxes();

        void GetExpressionSyntaxNames(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        uint GetNumberEvents();

        void GetEventIndexDescription(
            [In] uint Index,
            [In] uint Which,
            [In, MarshalAs(UnmanagedType.LPStr)] string Buffer,
            [In] uint BufferSize,
            [Out] out uint DescSize);

        uint GetCurrentEventIndex();

        uint SetNextEventIndex(
            [In] uint Relation,
            [In] uint Value);

        // ---------------------------------------------------------------------------------------------
        // IDebugControl4
        // ---------------------------------------------------------------------------------------------

        void GetLogFileWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out int Append);

        void OpenLogFileWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] int Append);

        void InputWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint InputSize);

        void ReturnInputWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Buffer);

        void OutputWide(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        void OutputVaListWide(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In] ref sbyte Args);

        void ControlledOutputWide(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        void ControlledOutputVaListWide(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In] ref sbyte Args);

        void OutputPromptWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        void OutputPromptVaListWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In] ref sbyte Args);

        void GetPromptTextWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        ulong AssembleWide(
            [In] ulong Offset,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Instr);

        void DisassembleWide(
            [In] ulong Offset,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint DisassemblySize,
            [Out] out ulong EndOffset);

        void GetProcessorTypeNamesWide(
            [In] uint Type,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        void GetTextMacroWide(
            [In] uint Slot,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint MacroSize);

        void SetTextMacroWide(
            [In] uint Slot,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Macro);

        void EvaluateWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Expression,
            [In] uint DesiredType,
            [Out] out _DEBUG_VALUE Value,
            [Out] out uint RemainderIndex);

        void ExecuteWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command,
            [In] uint Flags);

        void ExecuteCommandFileWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string CommandFile,
            [In] uint Flags);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugBreakpoint2 GetBreakpointByIndex2(
            [In] uint Index);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugBreakpoint2 GetBreakpointById2(
            [In] uint Id);

        [return: MarshalAs(UnmanagedType.Interface)]
        IDebugBreakpoint2 AddBreakpoint2(
            [In] uint Type,
            [In] uint DesiredId);

        void RemoveBreakpoint2(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 Bp);

        ulong AddExtensionWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path,
            [In] uint Flags);

        ulong GetExtensionByPathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path);

        void CallExtensionWide(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Function,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments = null);

        IntPtr GetExtensionFunctionWide(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPWStr)] string FuncName);

        void GetEventFilterTextWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        void GetEventFilterCommandWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        void SetEventFilterCommandWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command);

        void GetSpecificFilterArgumentWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ArgumentSize);

        void SetSpecificFilterArgumentWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Argument);

        void GetExceptionFilterSecondCommandWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        void SetExceptionFilterSecondCommandWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command);

        void GetLastEventInformationWide(
            [Out] out uint Type,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId,
            [Out] IntPtr ExtraInformation,
            [In] uint ExtraInformationSize,
            [Out] out uint ExtraInformationUsed,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Description,
            [In] uint DescriptionSize,
            [Out] out uint DescriptionUsed);

        void GetTextReplacementWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string SrcText,
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder SrcBuffer,
            [In] uint SrcBufferSize,
            [Out] out uint SrcSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder DstBuffer,
            [In] uint DstBufferSize,
            [Out] out uint DstSize);

        void SetTextReplacementWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string SrcText,
            [In, MarshalAs(UnmanagedType.LPWStr)] string DstText = null);

        void SetExpressionSyntaxByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string AbbrevName);

        void GetExpressionSyntaxNamesWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        void GetEventIndexDescriptionWide(
            [In] uint Index,
            [In] uint Which,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Buffer,
            [In] uint BufferSize,
            [Out] out uint DescSize);

        void GetLogFile2(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out uint Flags);

        void OpenLogFile2(
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] uint Flags);

        void GetLogFile2Wide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out uint Flags);

        void OpenLogFile2Wide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] uint Flags);

        void GetSystemVersionValues(
            [Out] out uint PlatformId,
            [Out] out uint Win32Major,
            [Out] out uint Win32Minor,
            [Out] out uint KdMajor,
            [Out] out uint KdMinor);

        void GetSystemVersionString(
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void GetSystemVersionStringWide(
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        void GetContextStackTrace(
            [In] IntPtr StartContext,
            [In] uint StartContextSize,
            [Out] IntPtr Frames,
            [In] uint FramesSize,
            [Out] IntPtr FrameContexts,
            [In] uint FrameContextsSize,
            [In] uint FrameContextsEntrySize,
            [Out] out uint FramesFilled);

        void OutputContextStackTrace(
            [In] uint OutputControl,
            [In] IntPtr Frames,
            [In] uint FramesSize,
            [In] IntPtr FrameContexts,
            [In] uint FrameContextsSize,
            [In] uint FrameContextsEntrySize,
            [In] uint Flags);

        void GetStoredEventInformation(
            [Out] out uint Type,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId,
            [Out] IntPtr Context,
            [In] uint ContextSize,
            [Out] out uint ContextUsed,
            [Out] IntPtr ExtraInformation,
            [In] uint ExtraInformationSize,
            [Out] out uint ExtraInformationUsed);

        void GetManagedStatus(
            [Out] out uint Flags,
            [In] uint WhichString,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder String,
            [In] uint StringSize,
            [Out] out uint StringNeeded);

        void GetManagedStatusWide(
            [Out] out uint Flags,
            [In] uint WhichString,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder String,
            [In] uint StringSize,
            [Out] out uint StringNeeded);

        void ResetManagedStatus(
            [In] uint Flags);
    }
}
