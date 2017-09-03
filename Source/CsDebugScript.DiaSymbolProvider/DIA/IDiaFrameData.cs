using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("A39184B7-6A36-42DE-8EEC-7DF9F3F59F33"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaFrameData
	{
		[DispId(2)]
		uint addressSection
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(3)]
		uint addressOffset
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(4)]
		uint relativeVirtualAddress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(5)]
		ulong virtualAddress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(6)]
		uint lengthBlock
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(7)]
		uint lengthLocals
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(8)]
		uint lengthParams
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(9)]
		uint maxStack
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(10)]
		uint lengthProlog
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(11)]
		uint lengthSavedRegisters
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(12)]
		string program
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[return: MarshalAs(UnmanagedType.BStr)]
			get;
		}

		[DispId(13)]
		int systemExceptionHandling
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(14)]
		int cplusplusExceptionHandling
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(15)]
		int functionStart
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(16)]
		int allocatesBasePointer
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(17)]
		uint type
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(18)]
		IDiaFrameData functionParent
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		void execute([MarshalAs(UnmanagedType.Interface)] IDiaStackWalkFrame frame);
	}
}
