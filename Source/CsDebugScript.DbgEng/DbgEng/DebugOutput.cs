using System;

namespace DbgEng
{
	public enum DebugOutput
	{
		Normal = 1,
		Error,
		Warning = 4,
		Verbose = 8,
		Prompt = 16,
		PromptRegisters = 32,
		ExtensionWarning = 64,
		Debuggee = 128,
		DebuggeePrompt = 256,
		Symbols = 512,
		Status = 1024
	}
}
