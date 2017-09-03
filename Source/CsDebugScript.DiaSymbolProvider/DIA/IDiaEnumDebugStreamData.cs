using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[DefaultMember("Item"), Guid("486943E8-D187-4A6B-A3C4-291259FFF60D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaEnumDebugStreamData
	{
		[DispId(1)]
		int count
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(2)]
		string name
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[return: MarshalAs(UnmanagedType.BStr)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
		IEnumerator GetEnumerator();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Item([In] uint index, [In] uint cbData, out uint pcbData, out byte pbData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Next([In] uint celt, [In] uint cbData, out uint pcbData, out byte pbData, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Skip([In] uint celt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Clone([MarshalAs(UnmanagedType.Interface)] out IDiaEnumDebugStreamData ppenum);
	}
}
