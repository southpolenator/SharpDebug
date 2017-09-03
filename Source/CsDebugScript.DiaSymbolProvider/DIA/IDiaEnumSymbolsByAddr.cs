using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("624B7D9C-24EA-4421-9D06-3B577471C1FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaEnumSymbolsByAddr
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDiaSymbol symbolByAddr([In] uint isect, [In] uint offset);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDiaSymbol symbolByRVA([In] uint relativeVirtualAddress);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDiaSymbol symbolByVA([In] ulong virtualAddress);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Next([In] uint celt, [MarshalAs(UnmanagedType.Interface)] out IDiaSymbol rgelt, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Prev([In] uint celt, [MarshalAs(UnmanagedType.Interface)] out IDiaSymbol rgelt, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Clone([MarshalAs(UnmanagedType.Interface)] out IDiaEnumSymbolsByAddr ppenum);
	}
}
