using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("EC9D461D-CE74-4711-A020-7D8F9A1DD255"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaEnumStackFrames
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void Next([In] uint celt, [MarshalAs(UnmanagedType.Interface)] out IDiaStackFrame rgelt, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Reset();
	}
}
