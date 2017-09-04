using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DbgEng
{
    [ComImport, ComConversionLoss, Guid("7DF74A86-B03F-407F-90AB-A20DADCEAD08"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugControl3 : IDebugControl2
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
    }
}
