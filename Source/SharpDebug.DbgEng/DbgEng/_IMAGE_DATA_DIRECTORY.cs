using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _IMAGE_DATA_DIRECTORY
	{
		public uint VirtualAddress;

		public uint Size;
	}
}
