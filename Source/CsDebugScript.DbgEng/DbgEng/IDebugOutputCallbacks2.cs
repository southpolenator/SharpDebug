using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[Guid("67721FE9-56D2-4A44-A325-2B65513CE6EB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDebugOutputCallbacks2
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void Output([In] uint Mask, [MarshalAs(UnmanagedType.LPStr)] [In] string Text);

		[MethodImpl(MethodImplOptions.InternalCall)]
		uint GetInterestMask();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Output2([In] uint Which, [In] uint Flags, [In] ulong Arg, [MarshalAs(UnmanagedType.LPWStr)] [In] string Text = null);
	}
}
