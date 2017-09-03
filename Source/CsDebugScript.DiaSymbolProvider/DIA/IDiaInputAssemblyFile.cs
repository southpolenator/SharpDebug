using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("3BFE56B0-390C-4863-9430-1F3D083B7684"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaInputAssemblyFile
	{
		[DispId(1)]
		uint uniqueId
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(2)]
		uint index
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(3)]
		uint timeStamp
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(4)]
		int pdbAvailableAtILMerge
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(5)]
		string fileName
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[return: MarshalAs(UnmanagedType.BStr)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		void get_version([In] uint cbData, out uint pcbData, out byte pbData);
	}
}
