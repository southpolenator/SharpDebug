using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[DefaultMember("Item"), Guid("08CBB41E-47A6-4F87-92F1-1C9C87CED044"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaEnumDebugStreams
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
		IDiaEnumDebugStreamData Item([MarshalAs(UnmanagedType.Struct)] [In] object index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Next([In] uint celt, [MarshalAs(UnmanagedType.Interface)] out IDiaEnumDebugStreamData rgelt, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Skip([In] uint celt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Clone([MarshalAs(UnmanagedType.Interface)] out IDiaEnumDebugStreams ppenum);
	}
}
