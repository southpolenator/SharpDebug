using System;

namespace CsDebugScript.Engine.Debuggers.DbgEngDllHelpers
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
    /// <remarks>For performance reasons, after using scope, previous stack frame won't be set until it is needed. Always use this class to insure correctness.</remarks>
    /// </summary>
    internal class StackFrameSwitcher : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrameSwitcher" /> class.
        /// </summary>
        /// <param name="stateCache">The state cache.</param>
        /// <param name="stackFrame">The stack frame.</param>
        public StackFrameSwitcher(StateCache stateCache, StackFrame stackFrame)
        {
            stateCache.SwitchStackFrame(stackFrame);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
