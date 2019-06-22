using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng.NoExceptions
{
    [ComImport, ComConversionLoss, Guid("7DF74A86-B03F-407F-90AB-A20DADCEAD08"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugControl3 : IDebugControl2
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

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

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
    }
}
