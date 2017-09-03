using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
	[Guid("0C733A30-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface ISequentialStream
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		void RemoteRead(out byte pv, [In] uint cb, out uint pcbRead);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void RemoteWrite([In] ref byte pv, [In] uint cb, out uint pcbWritten);
	}
}
