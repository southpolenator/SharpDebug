using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[ComConversionLoss]
	[StructLayout(LayoutKind.Explicit, Pack = 8, Size = 24)]
	public struct __MIDL___MIDL_itf_output_0001_0083_0001
	{
		[FieldOffset(0)]
		public byte I8;

		[FieldOffset(0)]
		public ushort I16;

		[FieldOffset(0)]
		public uint I32;

		[FieldOffset(0)]
		public __MIDL___MIDL_itf_output_0001_0083_0002 __MIDL____MIDL_itf_output_0001_00830000;

		[FieldOffset(0)]
		public float F32;

		[FieldOffset(0)]
		public double F64;

		[FieldOffset(0)]
		public __MIDL___MIDL_itf_output_0001_0083_0003 I64Parts32;

		[FieldOffset(0)]
		public __MIDL___MIDL_itf_output_0001_0083_0004 F128Parts64;
	}
}
