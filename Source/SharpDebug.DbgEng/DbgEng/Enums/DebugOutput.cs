using System;

namespace DbgEng
{
    /// <summary>
    /// The DEBUG_OUTPUT_XXX constants are output flags. The output flags form a bit field that indicates the type of the output that accompanies them.
    /// </summary>
    [Flags]
    public enum DebugOutput
    {
        /// <summary>
        /// Normal output.
        /// <note>Maps to DEBUG_OUTPUT_NORMAL.</note>
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Error output.
        /// <note>Maps to DEBUG_OUTPUT_ERROR.</note>
        /// </summary>
        Error = 2,

        /// <summary>
        /// Warnings.
        /// <note>Maps to DEBUG_OUTPUT_WARNING.</note>
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Additional output.
        /// <note>Maps to DEBUG_OUTPUT_VERBOSE.</note>
        /// </summary>
        Verbose = 8,

        /// <summary>
        /// Prompt output.
        /// <note>Maps to DEBUG_OUTPUT_PROMPT.</note>
        /// </summary>
        Prompt = 16,

        /// <summary>
        /// Register dump before prompt.
        /// <note>Maps to DEBUG_OUTPUT_PROMPT_REGISTERS.</note>
        /// </summary>
        PromptRegisters = 32,

        /// <summary>
        /// Warnings specific to extension operation.
        /// <note>Maps to DEBUG_OUTPUT_EXTENSION_WARNING.</note>
        /// </summary>
        ExtensionWarning = 64,

        /// <summary>
        /// Debug output from the target (for example, OutputDebugString or DbgPrint).
        /// <note>Maps to DEBUG_OUTPUT_DEBUGGEE.</note>
        /// </summary>
        Debuggee = 128,

        /// <summary>
        /// Debug input expected by the target (for example, DbgPrompt).
        /// <note>Maps to DEBUG_OUTPUT_DEBUGGEE_PROMPT.</note>
        /// </summary>
        DebuggeePrompt = 256,

        /// <summary>
        /// Symbol messages (for example, !sym noisy).
        /// <note>Maps to DEBUG_OUTPUT_SYMBOLS.</note>
        /// </summary>
        Symbols = 512,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_OUTPUT_STATUS.</note>
        /// </summary>
        Status = 1024
    }
}
