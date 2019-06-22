using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_STACK_FRAME
	{
		public ulong InstructionOffset;

		public ulong ReturnOffset;

		public ulong FrameOffset;

		public ulong StackOffset;

		public ulong FuncTableEntry;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public ulong[] Params;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public ulong[] Reserved;

		public int Virtual;

		public uint FrameNumber;
	}
}
