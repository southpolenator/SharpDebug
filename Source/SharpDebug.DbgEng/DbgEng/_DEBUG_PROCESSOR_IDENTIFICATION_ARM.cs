using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_PROCESSOR_IDENTIFICATION_ARM
	{
		public uint Model;

		public uint Revision;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public sbyte[] VendorString;
	}
}
