using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_VALUE
	{
		public __MIDL___MIDL_itf_output_0001_0083_0001 __MIDL____MIDL_itf_output_0001_00830001;

		public uint TailOfRawBytes;

		public uint Type;
	}
}
