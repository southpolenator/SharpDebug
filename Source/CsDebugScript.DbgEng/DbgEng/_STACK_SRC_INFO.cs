using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _STACK_SRC_INFO
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		public string ImagePath;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string ModuleName;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Function;

		public uint Displacement;

		public uint Row;

		public uint Column;
	}
}
