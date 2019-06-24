using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_THREAD_BASIC_INFORMATION
	{
		public uint Valid;

		public uint ExitStatus;

		public uint PriorityClass;

		public uint Priority;

		public ulong CreateTime;

		public ulong ExitTime;

		public ulong KernelTime;

		public ulong UserTime;

		public ulong StartOffset;

		public ulong Affinity;
	}
}
