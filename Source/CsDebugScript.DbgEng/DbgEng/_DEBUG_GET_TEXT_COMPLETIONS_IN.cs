using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_GET_TEXT_COMPLETIONS_IN
	{
		public uint Flags;

		public uint MatchCountLimit;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public ulong[] Reserved;
	}
}
