using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _WINDBG_EXTENSION_APIS32
	{
		public uint NotSupported;
	}
}
