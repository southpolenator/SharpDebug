using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_BREAKPOINT_PARAMETERS
	{
		public ulong Offset;

		public uint Id;

		public uint BreakType;

		public uint ProcType;

		public uint Flags;

		public uint DataSize;

		public uint DataAccessType;

		public uint PassCount;

		public uint CurrentPassCount;

		public uint MatchThread;

		public uint CommandSize;

		public uint OffsetExpressionSize;
	}
}
