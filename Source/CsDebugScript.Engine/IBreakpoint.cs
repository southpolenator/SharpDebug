using System;

namespace CsDebugScript
{
    /// <summary>
    /// Interace for breakpoints.
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
        void SetAction(Action action);
    }
}
