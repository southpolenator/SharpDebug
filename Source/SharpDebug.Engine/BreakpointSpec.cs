using System;

namespace SharpDebug
{
    /// <summary>
    /// Status of action execution on breakpoint hit.
    /// </summary>
    public enum BreakpointHitResult
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
    /// Breakpoint type enum.
    /// </summary>
    public enum BreakpointType
    {
        /// <summary>
        /// Code breakpoint breaks when given instruction is accessed.
        /// </summary>
        Code,

        /// <summary>
        /// Data breakpoint breaks when given address is accessed.
        /// </summary>
        Data,
    }

    /// <summary>
    /// Class used to provide details about breakpoint that is to be created.
    /// </summary>
    public class BreakpointSpec
    {
        /// <summary>
        /// Type of breakpoint.
        /// </summary>
        public BreakpointType BreakpointType { get; set; }

        /// <summary>
        /// Action to be execution when breakpoint is hit.
        /// </summary>
        public Func<BreakpointHitResult> BreakpointAction { get; set; }

        /// <summary>
        /// Address on which this breakpoint is to be set.
        /// </summary>
        public ulong BreakpointAddress { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="breakpointAddress">Address that will trigger this breakpoint.</param>
        /// <param name="breakpointAction">Action to be executed when breakpoint is hit. If not specified default is break on hit.</param>
        /// <param name="breakpointType">Breakpoint type.</param>
        public BreakpointSpec(
            ulong breakpointAddress,
            Func<BreakpointHitResult> breakpointAction = null,
            BreakpointType breakpointType = BreakpointType.Code)
        {
            if (breakpointAction == null)
            {
                BreakpointAction = () => BreakpointHitResult.Break;
            }
            else
            {
                BreakpointAction = breakpointAction;
            }

            BreakpointType = breakpointType;
            BreakpointAddress = breakpointAddress;
        }
    }
}