using System.Collections.Generic;

namespace SharpDebug.CLR
{
    /// <summary>
    /// CLR code thread interface. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public interface IClrThread
    {
        /// <summary>
        /// Gets the runtime associated with this thread.
        /// </summary>
        IClrRuntime Runtime { get; }

        /// <summary>
        /// Gets the AppDomain the thread is running in.
        /// </summary>
        IClrAppDomain AppDomain { get; }

        /// <summary>
        /// Gets the managed stack trace of the thread.
        /// </summary>
        /// <remarks>
        /// <note type="note">This property may return incomplete data in the case of a bad stack unwind or if there is a very large
        /// number of methods on the stack. (This is usually caused by a stack overflow on
        /// the target thread, stack corruption which leads to a bad stack unwind, or other
        /// inconsistent state in the target debuggee.)</note>
        /// <note type="note">
        /// This property uses a heuristic to attempt to detect bad unwinds to stop enumerating frames by inspecting the
        /// stack pointer and instruction pointer of each frame to ensure the stack walk
        /// is "making progress". Additionally we cap the number of frames returned by this
        /// method as another safegaurd. This means we may not have all frames even if the
        /// stack walk was making progress. If you want to ensure that you receive an un-clipped
        /// stack trace, you should use EnumerateStackTrace instead of this property, and
        /// be sure to handle the case of repeating stack frames.
        /// </note>
        /// </remarks>
        StackTrace ClrStackTrace { get; }

        /// <summary>
        /// Gets the last thrown exception on the thread. Note that this field may be null.
        /// It may be stale...meaning the thread could be done processing the exception but
        /// a crash dump was taken before the exception was cleared off the field.
        /// </summary>
        Variable LastThrownException { get; }

        /// <summary>
        /// Gets a value indicating whether this is finalizer thread.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is finalizer thread; otherwise, <c>false</c>.
        /// </value>
        bool IsFinalizerThread { get; }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        uint SystemId { get; }

        /// <summary>
        /// Enumerates the GC references (objects) on the stack.
        /// </summary>
        IEnumerable<Variable> EnumerateStackObjects();

        /// <summary>
        /// Gets the CLR stack frame with the specified instruction offset.
        /// </summary>
        /// <param name="instructionOffset">The instruction offset.</param>
        IClrStackFrame GetClrStackFrame(ulong instructionOffset);
    }
}
