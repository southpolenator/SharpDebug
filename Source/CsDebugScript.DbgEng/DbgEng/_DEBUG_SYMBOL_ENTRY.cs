using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct _DEBUG_SYMBOL_ENTRY
	{
		public ulong ModuleBase;

		public ulong Offset;

		public ulong Id;

		public ulong Arg64;

		public uint Size;

		public uint Flags;

		public uint TypeId;

		public uint NameSize;

		public uint Token;

		public uint Tag;

		public uint Arg32;

		public uint Reserved;
	}
}
