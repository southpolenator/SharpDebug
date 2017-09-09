using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_EVENT_CONTEXT
	{
		public uint Size;

		public uint ProcessEngineId;

		public uint ThreadEngineId;

		public uint FrameEngineId;
	}
}
