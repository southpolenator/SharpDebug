using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_CREATE_PROCESS_OPTIONS
	{
		public uint CreateFlags;

		public uint EngCreateFlags;

		public uint VerifierFlags;

		public uint Reserved;
	}
}
