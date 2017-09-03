using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("07C590C1-438D-4F47-BDCD-4397BC81AD75"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaStackWalkFrame
	{
		[DispId(1)]
		ulong registerValue
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		void readMemory([In] MemoryTypeEnum type, [In] ulong va, [In] uint cbData, out uint pcbData, out byte pbData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void searchForReturnAddress([MarshalAs(UnmanagedType.Interface)] [In] IDiaFrameData frame, out ulong returnAddress);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void searchForReturnAddressStart([MarshalAs(UnmanagedType.Interface)] [In] IDiaFrameData frame, [In] ulong startAddress, out ulong returnAddress);
	}
}
