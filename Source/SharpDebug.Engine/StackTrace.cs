using SharpDebug.Engine;

namespace SharpDebug
{
    /// <summary>
    /// Stack trace of the process being debugged.
    /// </summary>
    public class StackTrace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackTrace"/> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
        internal StackTrace(Thread thread)
        {
            Thread = thread;
        }

        /// <summary>
        /// Gets the current stack trace in current thread of current process.
        /// </summary>
        public static StackTrace Current
        {
            get
            {
                return Thread.Current.StackTrace;
            }
        }

        /// <summary>
        /// Gets the owning thread.
        /// </summary>
        public Thread Thread { get; internal set; }

        /// <summary>
        /// Gets the array of all frames.
        /// </summary>
        public StackFrame[] Frames { get; internal set; }

        /// <summary>
        /// Gets the current stack frame.
        /// </summary>
        public StackFrame CurrentFrame
        {
            get
            {
                return Context.Debugger.GetThreadCurrentStackFrame(Thread);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Frames = {Frames.Length}";
        }
    }
}
