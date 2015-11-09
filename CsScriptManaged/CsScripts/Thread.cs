using CsScriptManaged;
using System;
using System.Linq;

namespace CsScripts
{
    public class Thread
    {
        /// <summary>
        /// Gets the current thread in current process.
        /// </summary>
        public static Thread Current
        {
            get
            {
                return Process.Current.CurrentThread;
            }
        }

        /// <summary>
        /// Gets all threads in current process.
        /// </summary>
        public static Thread[] All
        {
            get
            {
                return Process.Current.Threads;
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public uint Id { get; internal set; }

        /// <summary>
        /// Gets the system identifier.
        /// </summary>
        public uint SystemId { get; internal set; }
    }
}
