using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct _DEBUG_EXCEPTION_FILTER_PARAMETERS
	{
		public uint ExecutionOption;

		public uint ContinueOption;

		public uint TextSize;

		public uint CommandSize;

		public uint SecondCommandSize;

		public uint ExceptionCode;
	}
}
