using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_OFFSET_REGION
	{
		public ulong Base;

		public ulong Size;
	}
}
