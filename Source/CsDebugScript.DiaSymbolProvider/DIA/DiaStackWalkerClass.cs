using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Coclass for <see cref="DiaStackWalker"/> interface.
    /// </summary>
    [ComImport, ClassInterface(ClassInterfaceType.None), Guid("CE4A85DB-5768-475B-A4E1-C0BCA2112A6B")]
#if NET461
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
#endif
    public class DiaStackWalkerClass : IDiaStackWalker, DiaStackWalker
    {
        /// <summary>
        /// Retrieves a stack frame enumerator for x86 platforms.
        /// </summary>
        /// <param name="pHelper">The helper <see cref="IDiaStackWalkHelper"/> object.</param>
        /// <returns>Returns an <see cref="IDiaEnumStackFrames"/> object that contains a list of <see cref="IDiaStackFrame"/> objects.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern IDiaEnumStackFrames getEnumFrames(
            [In, MarshalAs(UnmanagedType.Interface)] IDiaStackWalkHelper pHelper);

        /// <summary>
        /// Retrieves a stack frame enumerator for a specific platform type.
        /// </summary>
        /// <param name="cpuid">A value from the <see cref="CV_CPU_TYPE_e"/> enumeration, specifying the platform type.</param>
        /// <param name="pHelper">The helper <see cref="IDiaStackWalkHelper"/> object.</param>
        /// <returns>Returns an <see cref="IDiaEnumStackFrames"/> object that contains a list of <see cref="IDiaStackFrame"/> objects.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public virtual extern IDiaEnumStackFrames getEnumFrames2(
            [In] CV_CPU_TYPE_e cpuid,
            [In, MarshalAs(UnmanagedType.Interface)] IDiaStackWalkHelper pHelper);
    }
}
