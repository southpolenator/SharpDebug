using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("A2EF5353-F5A8-4EB3-90D2-CB526ACB3CDD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaSourceFile
	{
		[DispId(2)]
		uint uniqueId
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(3)]
		string fileName
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[return: MarshalAs(UnmanagedType.BStr)]
			get;
		}

		[DispId(4)]
		uint checksumType
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(5)]
		IDiaEnumSymbols compilands
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		void get_checksum([In] uint cbData, out uint pcbData, out byte pbData);
	}
}
