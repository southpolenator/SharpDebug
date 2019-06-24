using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_SYMBOL_PARAMETERS
	{
		public ulong Module;

		public uint TypeId;

		public uint ParentSymbol;

		public uint SubElements;

		public uint Flags;

		public ulong Reserved;
	}
}
