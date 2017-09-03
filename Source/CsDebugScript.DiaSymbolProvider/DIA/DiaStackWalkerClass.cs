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
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void getEnumFrames([MarshalAs(UnmanagedType.Interface)] [In] IDiaStackWalkHelper pHelper, [MarshalAs(UnmanagedType.Interface)] out IDiaEnumStackFrames ppenum);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void getEnumFrames2([In] CV_CPU_TYPE_e cpuid, [MarshalAs(UnmanagedType.Interface)] [In] IDiaStackWalkHelper pHelper, [MarshalAs(UnmanagedType.Interface)] out IDiaEnumStackFrames ppenum);
    }
}
