using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("5485216B-A54C-469F-9670-52B24D5229BB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaStackWalker
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void getEnumFrames([MarshalAs(UnmanagedType.Interface)] [In] IDiaStackWalkHelper pHelper, [MarshalAs(UnmanagedType.Interface)] out IDiaEnumStackFrames ppenum);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void getEnumFrames2([In] CV_CPU_TYPE_e cpuid, [MarshalAs(UnmanagedType.Interface)] [In] IDiaStackWalkHelper pHelper, [MarshalAs(UnmanagedType.Interface)] out IDiaEnumStackFrames ppenum);
	}
}
