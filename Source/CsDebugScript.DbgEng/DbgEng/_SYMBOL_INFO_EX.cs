using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _SYMBOL_INFO_EX
	{
		public uint SizeOfStruct;

		public uint TypeOfInfo;

		public ulong Offset;

		public uint Line;

		public uint Displacement;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] Reserved;
	}
}
