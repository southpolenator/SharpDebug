using System;

namespace SharpDebug
{
    /// <summary>
    /// Interface for breakpoints.
    /// </summary>
    public interface IBreakpoint
    {
        /// <summary>
        /// Get or set whether this breakpoint is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Change action associated  to this breakpoint.
        /// </summary>
        /// <param name="action">Action to be executed when this breakpoint is hit.</param>
        void SetAction(Func<BreakpointHitResult> action);

        /// <summary>
        /// Wait for breakpoint to get hit.
        /// </summary>
        void WaitForHit();
    }
}