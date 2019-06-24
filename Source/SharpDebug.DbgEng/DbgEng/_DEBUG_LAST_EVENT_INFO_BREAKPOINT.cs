using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_LAST_EVENT_INFO_BREAKPOINT
	{
		public uint Id;
	}
}
