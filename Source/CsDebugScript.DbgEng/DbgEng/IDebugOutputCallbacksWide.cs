using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[Guid("4C7FD663-C394-4E26-8EF1-34AD5ED3764C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDebugOutputCallbacksWide
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void Output([In] uint Mask, [MarshalAs(UnmanagedType.LPWStr)] [In] string Text);
	}
}
