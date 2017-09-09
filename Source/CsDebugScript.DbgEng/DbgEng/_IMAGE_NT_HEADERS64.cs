using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _IMAGE_NT_HEADERS64
	{
		public uint Signature;

		public _IMAGE_FILE_HEADER FileHeader;

		public _IMAGE_OPTIONAL_HEADER64 OptionalHeader;
	}
}
