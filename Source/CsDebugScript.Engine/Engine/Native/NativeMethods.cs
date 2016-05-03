using System;
using System.Runtime.InteropServices;

namespace CsDebugScript.Engine.Native
{
    /// <summary>
    /// Exported native methods
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Copies a block of memory from one location to another.
        /// </summary>
        /// <param name="destination">The destination address.</param>
        /// <param name="source">The source address.</param>
        /// <param name="count">The number of bytes.</param>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint count);
    }
}
