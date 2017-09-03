using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("5EDBC96D-CDD6-4792-AFBE-CC89007D9610"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDiaStackFrame
	{
		[DispId(1)]
		uint type
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(2)]
		ulong @base
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(3)]
		uint size
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(4)]
		ulong returnAddress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(5)]
		ulong localsBase
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(6)]
		uint lengthLocals
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(7)]
		uint lengthParams
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(8)]
		uint lengthProlog
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(9)]
		uint lengthSavedRegisters
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(10)]
		int systemExceptionHandling
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(11)]
		int cplusplusExceptionHandling
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(12)]
		int functionStart
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(13)]
		int allocatesBasePointer
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(14)]
		uint maxStack
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[DispId(15)]
		ulong registerValue
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
	}
}
