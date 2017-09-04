using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_GET_TEXT_COMPLETIONS_OUT
	{
		public uint Flags;

		public uint ReplaceIndex;

		public uint MatchCount;

		public uint Reserved1;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public ulong[] Reserved2;
	}
}
