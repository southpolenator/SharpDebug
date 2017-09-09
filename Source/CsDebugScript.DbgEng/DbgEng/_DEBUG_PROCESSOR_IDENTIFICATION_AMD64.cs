using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_PROCESSOR_IDENTIFICATION_AMD64
	{
		public uint Family;

		public uint Model;

		public uint Stepping;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public sbyte[] VendorString;
	}
}
