using CsScripts;
using System;

namespace CsScriptManaged.Utility
{
    /// <summary>
    /// Used for scoped stack frame switching.
    /// <example><code>
    ///     using (var switcher = new StackFrameSwitcher(stackFrame))
    ///     {
    ///         // Invoke DbgEng.dll interface function
    ///     }
    /// </code></example>
    /// <remarks>Use this class for accessing stack frame information from DbgEng.dll interfaces to insure correct stack frame information access.</remarks>
    /// <remarks>For performance reasons, after using scope, previous stack frame won't be set until is needed. Use this class to insure correctness.</remarks>
    /// </summary>
    public class StackFrameSwitcher : IDisposable
    {
        /// <summary>
        /// The thread switcher
        /// </summary>
        private ThreadSwitcher threadSwitcher;

        /// <summary>
        /// The old stack frame identifier
        /// </summary>
        private uint oldStackFrameId;

        /// <summary>
        /// The new stack frame identifier
        /// </summary>
        private uint newStackFrameId;

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrameSwitcher"/> class.
        /// </summary>
        /// <param name="stackFrame">The stack frame.</param>
        public StackFrameSwitcher(StackFrame stackFrame)
        {
            threadSwitcher = new ThreadSwitcher(stackFrame.Thread);
            oldStackFrameId = Context.Symbols.GetCurrentScopeFrameIndex();
            newStackFrameId = stackFrame.FrameNumber;

            SetStackFrameIndex(newStackFrameId);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            SetStackFrameIndex(oldStackFrameId);
            threadSwitcher.Dispose();
        }

        /// <summary>
        /// Sets the current stack frame index.
        /// </summary>
        /// <param name="newStackFrameId">The new stack frame identifier.</param>
        private void SetStackFrameIndex(uint newStackFrameId)
        {
            Context.Symbols.SetScopeFrameByIndex(newStackFrameId);
        }
    }
}
