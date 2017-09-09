using System;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// CLR code StackFrame interface. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public interface IClrStackFrame
    {
        /// <summary>
        /// Gets the module this frame is associated with.
        /// </summary>
        IClrModule Module { get; }

        /// <summary>
        /// Gets the arguments for this stack frame.
        /// </summary>
        VariableCollection Arguments { get; }

        /// <summary>
        /// Gets the locals for this stack frame.
        /// </summary>
        VariableCollection Locals { get; }

        /// <summary>
        /// Reads the name of the source file, line and displacement.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="instructionOffset">The instruction offset.</param>
        Tuple<string, uint, ulong> ReadSourceFileNameAndLine(Module module, ulong instructionOffset);

        /// <summary>
        /// Reads the function name and displacement.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="instructionOffset">The instruction offset.</param>
        Tuple<string, ulong> ReadFunctionNameAndDisplacement(Module module, ulong instructionOffset);
    }
}
