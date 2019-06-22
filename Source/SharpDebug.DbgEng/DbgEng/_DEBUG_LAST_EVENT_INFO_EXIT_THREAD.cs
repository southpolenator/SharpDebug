using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_LAST_EVENT_INFO_EXIT_THREAD
	{
		public uint ExitCode;
	}
}
