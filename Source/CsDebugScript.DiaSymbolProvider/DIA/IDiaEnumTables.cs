using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[DefaultMember("Item"), Guid("C65C2B0A-1150-4D7A-AFCC-E05BF3DEE81E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaEnumTables
	{
		[DispId(1)]
		int count
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler")]
		IEnumerator GetEnumerator();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDiaTable Item([MarshalAs(UnmanagedType.Struct)] [In] object index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Next(uint celt, [MarshalAs(UnmanagedType.Interface)] ref IDiaTable rgelt, ref uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Skip([In] uint celt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Clone([MarshalAs(UnmanagedType.Interface)] out IDiaEnumTables ppenum);
	}
}
