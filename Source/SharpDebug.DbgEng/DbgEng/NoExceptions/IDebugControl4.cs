using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("94E60CE9-9B41-4B19-9FC0-6D9EB35272B3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugControl4 : IDebugControl3
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugControl
        [PreserveSig]
        int GetInterrupt();

        [PreserveSig]
        int SetInterrupt(
            [In] uint Flags);

        [PreserveSig]
        int GetInterruptTimeout(
            [Out] out uint Seconds);

        [PreserveSig]
        int SetInterruptTimeout(
            [In] uint Seconds);

        [PreserveSig]
        int GetLogFile(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out int Append);

        [PreserveSig]
        int OpenLogFile(
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] int Append);

        [PreserveSig]
        int CloseLogFile();

        [PreserveSig]
        int GetLogMask(
            [Out] out uint Mask);

        [PreserveSig]
        int SetLogMask(
            [In] uint Mask);

        [PreserveSig]
        int Input(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint InputSize);

        [PreserveSig]
        int ReturnInput(
            [In, MarshalAs(UnmanagedType.LPStr)] string Buffer);

        [PreserveSig]
        int Output(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        [PreserveSig]
        int OutputVaList(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In] ref sbyte Args);

        [PreserveSig]
        int ControlledOutput(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        [PreserveSig]
        int ControlledOutputVaList(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In] ref sbyte Args);

        [PreserveSig]
        int OutputPrompt(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        [PreserveSig]
        int OutputPromptVaList(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string Format,
            [In] ref sbyte Args);

        [PreserveSig]
        int GetPromptText(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        [PreserveSig]
        int OutputCurrentState(
            [In] uint OutputControl,
            [In] uint Flags);

        [PreserveSig]
        int OutputVersionInformation(
            [In] uint OutputControl);

        [PreserveSig]
        int GetNotifyEventHandle(
            [Out] out ulong Handle);

        [PreserveSig]
        int SetNotifyEventHandle(
            [In] ulong Handle);

        [PreserveSig]
        int Assemble(
            [In] ulong Offset,
            [In, MarshalAs(UnmanagedType.LPStr)] string Instr,
            [Out] out ulong EndOffset);

        [PreserveSig]
        int Disassemble(
            [In] ulong Offset,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint DisassemblySize,
            [Out] out ulong EndOffset);

        [PreserveSig]
        int GetDisassembleEffectiveOffset(
            [Out] out ulong Offset);

        [PreserveSig]
        int OutputDisassembly(
            [In] uint OutputControl,
            [In] ulong Offset,
            [In] uint Flags,
            [Out] out ulong EndOffset);

        [PreserveSig]
        int OutputDisassemblyLines(
            [In] uint OutputControl,
            [In] uint PreviousLines,
            [In] uint TotalLines,
            [In] ulong Offset,
            [In] uint Flags,
            [Out] out uint OffsetLine,
            [Out] out ulong StartOffset,
            [Out] out ulong EndOffset,
            [Out] out ulong LineOffsets);

        [PreserveSig]
        int GetNearInstruction(
            [In] ulong Offset,
            [In] int Delta,
            [Out] out ulong NearOffset);

        [PreserveSig]
        int GetStackTrace(
            [In] ulong FrameOffset,
            [In] ulong StackOffset,
            [In] ulong InstructionOffset,
            [Out] IntPtr Frames,
            [In] uint FramesSize,
            [Out] out uint FramesFilled);

        [PreserveSig]
        int GetReturnOffset(
            [Out] out ulong Offset);

        [PreserveSig]
        int OutputStackTrace(
            [In] uint OutputControl,
            [In] IntPtr Frames = default(IntPtr),
            [In] uint FramesSize = default(uint),
            [In] uint Flags = default(uint));

        [PreserveSig]
        int GetDebuggeeType(
            [Out] out uint Class,
            [Out] out uint Qualifier);

        [PreserveSig]
        int GetActualProcessorType(
            [Out] out uint Type);

        [PreserveSig]
        int GetExecutingProcessorType(
            [Out] out uint Type);

        [PreserveSig]
        int GetNumberPossibleExecutingProcessorTypes(
            [Out] out uint Number);

        [PreserveSig]
        int GetPossibleExecutingProcessorTypes(
            [In] uint Start,
            [In] uint Count,
            [Out, MarshalAs(UnmanagedType.LPArray)] uint[] Types);

        [PreserveSig]
        int GetNumberProcessors(
            [Out] out uint Number);

        [PreserveSig]
        int GetSystemVersion(
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

        [PreserveSig]
        int GetPageSize(
            [Out] out uint Size);

        [PreserveSig]
        int IsPointer64Bit();

        [PreserveSig]
        int ReadBugCheckData(
            [Out] out uint Code,
            [Out] out ulong Arg1,
            [Out] out ulong Arg2,
            [Out] out ulong Arg3,
            [Out] out ulong Arg4);

        [PreserveSig]
        int GetNumberSupportedProcessorTypes(
            [Out] out uint Number);

        [PreserveSig]
        int GetSupportedProcessorTypes(
            [In] uint Start,
            [In] uint Count,
            [Out, MarshalAs(UnmanagedType.LPArray)] uint[] Types);

        [PreserveSig]
        int GetProcessorTypeNames(
            [In] uint Type,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        [PreserveSig]
        int GetEffectiveProcessorType(
            [Out] out uint Type);

        [PreserveSig]
        int SetEffectiveProcessorType(
            [In] uint Type);

        [PreserveSig]
        int GetExecutionStatus(
            [Out] out uint Status);

        [PreserveSig]
        int SetExecutionStatus(
            [In] uint Status);

        [PreserveSig]
        int GetCodeLevel(
            [Out] out uint Level);

        [PreserveSig]
        int SetCodeLevel(
            [In] uint Level);

        [PreserveSig]
        int GetEngineOptions(
            [Out] out uint Options);

        [PreserveSig]
        int AddEngineOptions(
            [In] uint Options);

        [PreserveSig]
        int RemoveEngineOptions(
            [In] uint Options);

        [PreserveSig]
        int SetEngineOptions(
            [In] uint Options);

        [PreserveSig]
        int GetSystemErrorControl(
            [Out] out uint OutputLevel,
            [Out] out uint BreakLevel);

        [PreserveSig]
        int SetSystemErrorControl(
            [In] uint OutputLevel,
            [In] uint BreakLevel);

        [PreserveSig]
        int GetTextMacro(
            [In] uint Slot,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint MacroSize);

        [PreserveSig]
        int SetTextMacro(
            [In] uint Slot,
            [In, MarshalAs(UnmanagedType.LPStr)] string Macro);

        [PreserveSig]
        int GetRadix(
            [Out] out uint Radix);

        [PreserveSig]
        int SetRadix(
            [In] uint Radix);

        [PreserveSig]
        int Evaluate(
            [In, MarshalAs(UnmanagedType.LPStr)] string Expression,
            [In] uint DesiredType,
            [Out] out _DEBUG_VALUE Value,
            [Out] out uint RemainderIndex);

        [PreserveSig]
        int CoerceValue(
            [In] _DEBUG_VALUE In,
            [In] uint OutType,
            [Out] out _DEBUG_VALUE Out);

        [PreserveSig]
        int CoerceValues(
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] In,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] OutTypes,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_VALUE[] Out);

        [PreserveSig]
        int Execute(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string Command,
            [In] uint Flags);

        [PreserveSig]
        int ExecuteCommandFile(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPStr)] string CommandFile,
            [In] uint Flags);

        [PreserveSig]
        int GetNumberBreakpoints(
            [Out] out uint Number);

        [PreserveSig]
        int GetBreakpointByIndex(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint Bp);

        [PreserveSig]
        int GetBreakpointById(
            [In] uint Id,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint Bp);

        [PreserveSig]
        int GetBreakpointParameters(
            [In] uint Count,
            [In] ref uint Ids,
            [In] uint Start = default(uint),
            [Out] IntPtr Params = default(IntPtr));

        [PreserveSig]
        int AddBreakpoint(
            [In] uint Type,
            [In] uint DesiredId,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint Bp);

        [PreserveSig]
        int RemoveBreakpoint(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp);

        [PreserveSig]
        int AddExtension(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path,
            [In] uint Flags,
            [Out] out ulong Handle);

        [PreserveSig]
        int RemoveExtension(
            [In] ulong Handle);

        [PreserveSig]
        int GetExtensionByPath(
            [In, MarshalAs(UnmanagedType.LPStr)] string Path,
            [Out] out ulong Handle);

        [PreserveSig]
        int CallExtension(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPStr)] string Function,
            [In, MarshalAs(UnmanagedType.LPStr)] string Arguments = null);

        [PreserveSig]
        int GetExtensionFunction(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPStr)] string FuncName,
            [Out] out IntPtr Function);

        [PreserveSig]
        int GetWindbgExtensionApis32(
            [In, Out] ref _WINDBG_EXTENSION_APIS32 Api);

        [PreserveSig]
        int GetWindbgExtensionApis64(
            [In, Out] ref _WINDBG_EXTENSION_APIS64 Api);

        [PreserveSig]
        int GetNumberEventFilters(
            [Out] out uint SpecificEvents,
            [Out] out uint SpecificExceptions,
            [Out] out uint ArbitraryExceptions);

        [PreserveSig]
        int GetEventFilterText(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        [PreserveSig]
        int GetEventFilterCommand(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        [PreserveSig]
        int SetEventFilterCommand(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Command);

        [PreserveSig]
        int GetSpecificFilterParameters(
            [In] uint Start,
            [In] uint Count,
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_SPECIFIC_FILTER_PARAMETERS[] Params);

        [PreserveSig]
        int SetSpecificFilterParameters(
            [In] uint Start,
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] _DEBUG_SPECIFIC_FILTER_PARAMETERS[] Params);

        [PreserveSig]
        int GetSpecificFilterArgument(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ArgumentSize);

        [PreserveSig]
        int SetSpecificFilterArgument(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Argument);

        [PreserveSig]
        int GetExceptionFilterParameters(
            [In] uint Count,
            [In] ref uint Codes,
            [In] uint Start = default(uint),
            [Out, MarshalAs(UnmanagedType.LPArray)] _DEBUG_EXCEPTION_FILTER_PARAMETERS[] Params = null);

        [PreserveSig]
        int SetExceptionFilterParameters(
            [In] uint Count,
            [In, MarshalAs(UnmanagedType.LPArray)] _DEBUG_EXCEPTION_FILTER_PARAMETERS[] Params);

        [PreserveSig]
        int GetExceptionFilterSecondCommand(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        [PreserveSig]
        int SetExceptionFilterSecondCommand(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPStr)] string Command);

        [PreserveSig]
        int WaitForEvent(
            [In] uint Flags,
            [In] uint Timeout);

        [PreserveSig]
        int GetLastEventInformation(
            [Out] out uint Type,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId,
            [Out] IntPtr ExtraInformation,
            [In] uint ExtraInformationSize,
            [Out] out uint ExtraInformationUsed,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Description,
            [In] uint DescriptionSize,
            [Out] out uint DescriptionUsed);
        #endregion

        #region IDebugControl2
        [PreserveSig]
        int GetCurrentTimeDate(
            [Out] out uint TimeDate);

        [PreserveSig]
        int GetCurrentSystemUpTime(
            [Out] out uint UpTime);

        [PreserveSig]
        int GetDumpFormatFlags(
            [Out] out uint FormatFlags);

        [PreserveSig]
        int GetNumberTextReplacements(
            [Out] out uint NumRepl);

        [PreserveSig]
        int GetTextReplacement(
            [In, MarshalAs(UnmanagedType.LPStr)] string SrcText,
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder SrcBuffer,
            [In] uint SrcBufferSize,
            [Out] out uint SrcSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder DstBuffer,
            [In] uint DstBufferSize,
            [Out] out uint DstSize);

        [PreserveSig]
        int SetTextReplacement(
            [In, MarshalAs(UnmanagedType.LPStr)] string SrcText,
            [In, MarshalAs(UnmanagedType.LPStr)] string DstText);

        [PreserveSig]
        int RemoveTextReplacements();

        [PreserveSig]
        int OutputTextReplacements(
            [In] uint OutputControl,
            [In] uint Flags);
        #endregion

        #region IDebugControl3
        [PreserveSig]
        int GetAssemblyOptions(
            [Out] out uint Options);

        [PreserveSig]
        int AddAssemblyOptions(
            [In] uint Options);

        [PreserveSig]
        int RemoveAssemblyOptions(
            [In] uint Options);

        [PreserveSig]
        int SetAssemblyOptions(
            [In] uint Options);

        [PreserveSig]
        int GetExpressionSyntax(
            [Out] out uint Flags);

        [PreserveSig]
        int SetExpressionSyntax(
            [In] uint Flags);

        [PreserveSig]
        int SetExpressionSyntaxByName(
            [In, MarshalAs(UnmanagedType.LPStr)] string AbbrevName);

        [PreserveSig]
        int GetNumberExpressionSyntaxes(
            [Out] out uint Number);

        [PreserveSig]
        int GetExpressionSyntaxNames(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        [PreserveSig]
        int GetNumberEvents(
            [Out] out uint Number);

        [PreserveSig]
        int GetEventIndexDescription(
            [In] uint Index,
            [In] uint Which,
            [In, MarshalAs(UnmanagedType.LPStr)] string Buffer,
            [In] uint BufferSize,
            [Out] out uint DescSize);

        [PreserveSig]
        int GetCurrentEventIndex(
            [Out] out uint Index);

        [PreserveSig]
        int SetNextEventIndex(
            [In] uint Relation,
            [In] uint Value,
            [Out] out uint NextIndex);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        [PreserveSig]
        int GetLogFileWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out int Append);

        [PreserveSig]
        int OpenLogFileWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] int Append);

        [PreserveSig]
        int InputWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint InputSize);

        [PreserveSig]
        int ReturnInputWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Buffer);

        [PreserveSig]
        int OutputWide(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        [PreserveSig]
        int OutputVaListWide(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In] ref sbyte Args);

        [PreserveSig]
        int ControlledOutputWide(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        [PreserveSig]
        int ControlledOutputVaListWide(
            [In] uint OutputControl,
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In] ref sbyte Args);

        [PreserveSig]
        int OutputPromptWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] params object[] parameters);

        [PreserveSig]
        int OutputPromptVaListWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Format,
            [In] ref sbyte Args);

        [PreserveSig]
        int GetPromptTextWide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        [PreserveSig]
        int AssembleWide(
            [In] ulong Offset,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Instr,
            [Out] out ulong EndOffset);

        [PreserveSig]
        int DisassembleWide(
            [In] ulong Offset,
            [In] uint Flags,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint DisassemblySize,
            [Out] out ulong EndOffset);

        [PreserveSig]
        int GetProcessorTypeNamesWide(
            [In] uint Type,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        [PreserveSig]
        int GetTextMacroWide(
            [In] uint Slot,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint MacroSize);

        [PreserveSig]
        int SetTextMacroWide(
            [In] uint Slot,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Macro);

        [PreserveSig]
        int EvaluateWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Expression,
            [In] uint DesiredType,
            [Out] out _DEBUG_VALUE Value,
            [Out] out uint RemainderIndex);

        [PreserveSig]
        int ExecuteWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command,
            [In] uint Flags);

        [PreserveSig]
        int ExecuteCommandFileWide(
            [In] uint OutputControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string CommandFile,
            [In] uint Flags);

        [PreserveSig]
        int GetBreakpointByIndex2(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint2 Bp);

        [PreserveSig]
        int GetBreakpointById2(
            [In] uint Id,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint2 Bp);

        [PreserveSig]
        int AddBreakpoint2(
            [In] uint Type,
            [In] uint DesiredId,
            [Out, MarshalAs(UnmanagedType.Interface)] out IDebugBreakpoint2 Bp);

        [PreserveSig]
        int RemoveBreakpoint2(
            [In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 Bp);

        [PreserveSig]
        int AddExtensionWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path,
            [In] uint Flags,
            [Out] out ulong Handle);

        [PreserveSig]
        int GetExtensionByPathWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Path,
            [Out] out ulong Handle);

        [PreserveSig]
        int CallExtensionWide(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Function,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Arguments = null);

        [PreserveSig]
        int GetExtensionFunctionWide(
            [In] ulong Handle,
            [In, MarshalAs(UnmanagedType.LPWStr)] string FuncName,
            [Out] out IntPtr Function);

        [PreserveSig]
        int GetEventFilterTextWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint TextSize);

        [PreserveSig]
        int GetEventFilterCommandWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        [PreserveSig]
        int SetEventFilterCommandWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command);

        [PreserveSig]
        int GetSpecificFilterArgumentWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint ArgumentSize);

        [PreserveSig]
        int SetSpecificFilterArgumentWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Argument);

        [PreserveSig]
        int GetExceptionFilterSecondCommandWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint CommandSize);

        [PreserveSig]
        int SetExceptionFilterSecondCommandWide(
            [In] uint Index,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Command);

        [PreserveSig]
        int GetLastEventInformationWide(
            [Out] out uint Type,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId,
            [Out] IntPtr ExtraInformation,
            [In] uint ExtraInformationSize,
            [Out] out uint ExtraInformationUsed,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Description,
            [In] uint DescriptionSize,
            [Out] out uint DescriptionUsed);

        [PreserveSig]
        int GetTextReplacementWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string SrcText,
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder SrcBuffer,
            [In] uint SrcBufferSize,
            [Out] out uint SrcSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder DstBuffer,
            [In] uint DstBufferSize,
            [Out] out uint DstSize);

        [PreserveSig]
        int SetTextReplacementWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string SrcText,
            [In, MarshalAs(UnmanagedType.LPWStr)] string DstText = null);

        [PreserveSig]
        int SetExpressionSyntaxByNameWide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string AbbrevName);

        [PreserveSig]
        int GetExpressionSyntaxNamesWide(
            [In] uint Index,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder FullNameBuffer,
            [In] uint FullNameBufferSize,
            [Out] out uint FullNameSize,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder AbbrevNameBuffer,
            [In] uint AbbrevNameBufferSize,
            [Out] out uint AbbrevNameSize);

        [PreserveSig]
        int GetEventIndexDescriptionWide(
            [In] uint Index,
            [In] uint Which,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Buffer,
            [In] uint BufferSize,
            [Out] out uint DescSize);

        [PreserveSig]
        int GetLogFile2(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out uint Flags);

        [PreserveSig]
        int OpenLogFile2(
            [In, MarshalAs(UnmanagedType.LPStr)] string File,
            [In] uint Flags);

        [PreserveSig]
        int GetLogFile2Wide(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint FileSize,
            [Out] out uint Flags);

        [PreserveSig]
        int OpenLogFile2Wide(
            [In, MarshalAs(UnmanagedType.LPWStr)] string File,
            [In] uint Flags);

        [PreserveSig]
        int GetSystemVersionValues(
            [Out] out uint PlatformId,
            [Out] out uint Win32Major,
            [Out] out uint Win32Minor,
            [Out] out uint KdMajor,
            [Out] out uint KdMinor);

        [PreserveSig]
        int GetSystemVersionString(
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        [PreserveSig]
        int GetSystemVersionStringWide(
            [In] uint Which,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
            [In] uint BufferSize,
            [Out] out uint StringSize);

        [PreserveSig]
        int GetContextStackTrace(
            [In] IntPtr StartContext,
            [In] uint StartContextSize,
            [Out] IntPtr Frames,
            [In] uint FramesSize,
            [Out] IntPtr FrameContexts,
            [In] uint FrameContextsSize,
            [In] uint FrameContextsEntrySize,
            [Out] out uint FramesFilled);

        [PreserveSig]
        int OutputContextStackTrace(
            [In] uint OutputControl,
            [In] IntPtr Frames,
            [In] uint FramesSize,
            [In] IntPtr FrameContexts,
            [In] uint FrameContextsSize,
            [In] uint FrameContextsEntrySize,
            [In] uint Flags);

        [PreserveSig]
        int GetStoredEventInformation(
            [Out] out uint Type,
            [Out] out uint ProcessId,
            [Out] out uint ThreadId,
            [Out] IntPtr Context,
            [In] uint ContextSize,
            [Out] out uint ContextUsed,
            [Out] IntPtr ExtraInformation,
            [In] uint ExtraInformationSize,
            [Out] out uint ExtraInformationUsed);

        [PreserveSig]
        int GetManagedStatus(
            [Out] out uint Flags,
            [In] uint WhichString,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder String,
            [In] uint StringSize,
            [Out] out uint StringNeeded);

        [PreserveSig]
        int GetManagedStatusWide(
            [Out] out uint Flags,
            [In] uint WhichString,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder String,
            [In] uint StringSize,
            [Out] out uint StringNeeded);

        [PreserveSig]
        int ResetManagedStatus(
            [In] uint Flags);
    }
}
