using System;

namespace CsDebugScript
{
    /// <summary>
    /// Status of action execution on breakpoint hit.
    /// </summary>
    public enum OnBreakpointHit
    {
        /// <summary>
        /// Break the execution. Debuggee stays in break mode.
        /// </summary>
        Break,

        /// <summary>
        /// Continue execution.
        /// </summary>
        Continue,
    }

    /// <summary>
    /// Interface for breakpoints.
    /// </summary>
    public interface IBreakpoint
    {
        /// <summary>
        /// Enable breakpoint.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable breakpoint.
        /// </summary>
        void Disable();

        /// <summary>
        /// Change action associated  to this breakpoint.
        /// </summary>
        /// <param name="action">Action to be executed when this breakpoint is hit.</param>
        void SetAction(Func<OnBreakpointHit> action);

        /// <summary>
        /// Wait for breakpoint to get hit.
        /// </summary>
        void WaitForHit();
    }
}
