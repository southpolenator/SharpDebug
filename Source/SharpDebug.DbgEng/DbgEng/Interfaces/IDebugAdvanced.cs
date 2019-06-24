using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    /// <summary>
    /// The <see cref="IDebugAdvanced"/> interface has these methods.
    /// </summary>
    [ComImport, Guid("F2DF5F53-071F-47BD-9DE6-5734C3FED689"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDebugAdvanced
    {
        /// <summary>
        /// The <see cref="GetThreadContext"/> method returns the current thread context.
        /// </summary>
        /// <param name="Context">Receives the current thread context. The type of the thread context is the CONTEXT structure for the target's effective processor. The buffer Context must be large enough to hold this structure.</param>
        /// <param name="ContextSize">Specifies the size of the buffer <paramref name="Context"/>.</param>
        void GetThreadContext(
            [Out] IntPtr Context,
            [In] uint ContextSize);

        /// <summary>
        /// The <see cref="SetThreadContext"/> method sets the current thread context.
        /// </summary>
        /// <param name="Context">Specifies the thread context. The type of the thread context is the CONTEXT structure for the target's effective processor. The buffer Context must be large enough to hold this structure.</param>
        /// <param name="ContextSize">Specifies the size of the buffer <paramref name="Context"/>.</param>
        void SetThreadContext(
            [In] IntPtr Context,
            [In] uint ContextSize);
    }
}
