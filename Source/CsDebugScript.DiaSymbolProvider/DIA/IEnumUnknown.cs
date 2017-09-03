using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("00000100-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IEnumUnknown
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void RemoteNext([In] uint celt, [MarshalAs(UnmanagedType.IUnknown)] out object rgelt, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Skip([In] uint celt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumUnknown ppenum);
	}
}
