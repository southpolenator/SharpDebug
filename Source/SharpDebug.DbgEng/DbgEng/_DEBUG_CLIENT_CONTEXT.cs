using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_CLIENT_CONTEXT
	{
		public uint cbSize;

		public uint eClient;
	}
}
