using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[Guid("4BF58045-D654-4C40-B0AF-683090F356DC"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDebugOutputCallbacks
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void Output([In] uint Mask, [MarshalAs(UnmanagedType.LPStr)] [In] string Text);
	}
}
