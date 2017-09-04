using System;

namespace DbgEng
{
	public enum DebugRequest
	{
		SourcePathHasSourceServer,
		TargetExceptionContext,
		TargetExceptionThread,
		TargetExceptionRecord,
		GetAdditionalCreateOptions,
		SetAdditionalCreateOptions,
		GetWin32MajorMinorVersions,
		ReadUserMinidumpStream,
		TargetCanDetach,
		SetLocalImplicitCommandLine,
		GetCapturedEventCodeOffset,
		ReadCapturedEventCodeStream,
		ExtTypedDataAnsi,
		GetExtensionSearchPathWide,
		GetTextCompletionsWide,
		GetCachedSymbolInfo,
		AddCachedSymbolInfo,
		RemoveCachedSymbolInfo,
		GetTextCompletionsAnsi,
		CurrentOutputCallbacksAreDmlAware,
		GetOffsetUnwindInformation,
		GetDumpHeader,
		SetDumpHeader,
		Midori,
		ProcessDescriptors,
		MiscInformation,
		OpenProcessToken,
		OpenThreadToken,
		DuplicateToken,
		QueryInfoToken,
		CloseToken,
		WowProcess,
		WowModule,
		DebugLiveUserNonInvasive,
		ResumeThread,
		InlineQuery,
		TlInstrumentationAware,
		GetInstrumentationVersion
	}
}
