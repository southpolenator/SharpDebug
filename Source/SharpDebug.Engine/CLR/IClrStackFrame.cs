using System;

namespace SharpDebug.CLR
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
        Tuple<string, uint, ulong> ReadSourceFileNameAndLine();

        /// <summary>
        /// Reads the function name and displacement.
        /// </summary>
        Tuple<string, ulong> ReadFunctionNameAndDisplacement();
    }
}
