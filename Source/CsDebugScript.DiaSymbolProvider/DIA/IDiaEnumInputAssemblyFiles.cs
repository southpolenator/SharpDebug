using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[DefaultMember("Item"), Guid("1C7FF653-51F7-457E-8419-B20F57EF7E4D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaEnumInputAssemblyFiles
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
		IDiaInputAssemblyFile Item([In] uint index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Next([In] uint celt, [MarshalAs(UnmanagedType.Interface)] out IDiaInputAssemblyFile rgelt, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Skip([In] uint celt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Clone([MarshalAs(UnmanagedType.Interface)] out IDiaEnumInputAssemblyFiles ppenum);
	}
}
