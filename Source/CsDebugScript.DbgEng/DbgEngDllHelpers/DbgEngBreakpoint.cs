using System;
using System.Threading;
using DbgEng;

namespace CsDebugScript.Engine.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// IBreakpoint implemenation for dbgeng.
    /// </summary>
    public class DbgEngBreakpoint : IBreakpoint
    {
        /// <summary>
        /// Native breakpoint object.
        /// </summary>
        private IDebugBreakpoint2 breakpoint;

        /// <summary>
        /// Action to be executed when this breakpoint is hit.
        /// </summary>
        private Func<BreakpointHitResult> breakpointAction;

        /// <summary>
        /// Invalidate all the needed caches when break happens on this breakpoint.
        /// </summary>
        /// <remarks>
        /// Providing invalidate cache action maybe isn't needed. Caller shouldn't care about this.
        /// </remarks>
        private Action invalidateCache;

        /// <summary>
        /// Debug Control interface to dbgeng.
        /// </summary>
        private IDebugControl7 debugControlInterface;

        private AutoResetEvent breakpointHitEvent = new AutoResetEvent(false);

        /// <summary>
        /// Constructor for creating new breakpoints.
        /// </summary>
        /// <param name="breakpointSpec">Spec describing this breakpoint.</param>
        /// <param name="invalidateCache">Invalidate cache action.</param>
        /// <param name="debugControlInterface">Debug control interface.</param>
        /// <remarks>
        /// This about adding some sort of factory pattern here.
        /// </remarks>
        public DbgEngBreakpoint(BreakpointSpec breakpointSpec, Action invalidateCache, IDebugControl7 debugControlInterface)
        {
            this.debugControlInterface = debugControlInterface;
            this.breakpointAction = breakpointSpec.BreakpointAction;
            this.invalidateCache = invalidateCache;

            unchecked
            {
                if (breakpointSpec.BreakpointType == BreakpointType.Code)
                {
                    breakpoint = debugControlInterface.AddBreakpoint2((uint)Defines.DebugBreakpointCode, (uint)Defines.DebugAnyId);
                }
                else
                {
                    throw new NotImplementedException("Only supports Code breakpoints");
                }
            }

            breakpoint.SetOffset(breakpointSpec.BreakpointAddress);
            breakpoint.SetFlags((uint)Defines.DebugBreakpointEnabled);
            breakpointStatusEnabled = true;
        }

        /// <summary>
        /// Internal status of this point.
        /// </summary>
        private bool breakpointStatusEnabled;

        /// <summary>
        /// Gets or sets status of this breakpoint.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return breakpointStatusEnabled;
            }
            set
            {
                breakpointStatusEnabled = value;

                if (breakpointStatusEnabled)
                {
                    breakpoint.AddFlags((uint)Defines.DebugBreakpointEnabled);
                }
                else
                {
                    breakpoint.RemoveFlags((uint)Defines.DebugBreakpointEnabled);
                }
            }
        }

        /// <summary>
        /// Execute action when this breakpoint is hit.
        /// </summary>
        public BreakpointHitResult ExecuteAction()
        {
            invalidateCache();
            BreakpointHitResult bpHitState = breakpointAction();
            breakpointHitEvent.Set();

            return bpHitState;
        }

        /// <summary>
        ///  Get internal id of this breakpoint.
        /// </summary>
        /// <returns>Id of this breakpoint.</returns>
        public uint GetId()
        {
            return breakpoint.GetId();
        }

        /// <summary>
        /// Set new action.
        /// </summary>
        /// <param name="action">New action to be set.</param>
        public void SetAction(Func<BreakpointHitResult> action)
        {
            breakpointAction = action;
        }

        /// <summary>
        /// Wait until breakpoint gets hit.
        /// </summary>
        public void WaitForHit()
        {
            breakpointHitEvent.WaitOne();
        }
    }
}
