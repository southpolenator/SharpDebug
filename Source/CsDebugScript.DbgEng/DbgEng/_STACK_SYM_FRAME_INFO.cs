using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _STACK_SYM_FRAME_INFO
	{
		public _DEBUG_STACK_FRAME_EX StackFrameEx;

		public _STACK_SRC_INFO SrcInfo;
	}
}
