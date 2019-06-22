using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DbgEng
{
	[Guid("61A4905B-23F9-4247-B3C5-53D087529AB7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDebugEventContextCallbacks
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		uint GetInterestMask();

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Breakpoint([MarshalAs(UnmanagedType.Interface)] [In] IDebugBreakpoint2 Bp, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void Exception([In] ref _EXCEPTION_RECORD64 Exception, [In] uint FirstChance, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void CreateThread([In] ulong Handle, [In] ulong DataOffset, [In] ulong StartOffset, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void ExitThread([In] uint ExitCode, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void CreateProcess([In] ulong ImageFileHandle, [In] ulong Handle, [In] ulong BaseOffset, [In] uint ModuleSize, [MarshalAs(UnmanagedType.LPWStr)] [In] string ModuleName = null, [MarshalAs(UnmanagedType.LPWStr)] [In] string ImageName = null, [In] uint CheckSum = default(uint), [In] uint TimeDateStamp = default(uint), [In] ulong InitialThreadHandle = default(ulong), [In] ulong ThreadDataOffset = default(ulong), [In] ulong StartOffset = default(ulong), [In] IntPtr Context = default(IntPtr), [In] uint ContextSize = default(uint));

		[MethodImpl(MethodImplOptions.InternalCall)]
		void ExitProcess([In] uint ExitCode, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void LoadModule([In] ulong ImageFileHandle, [In] ulong BaseOffset, [In] uint ModuleSize, [MarshalAs(UnmanagedType.LPWStr)] [In] string ModuleName = null, [MarshalAs(UnmanagedType.LPWStr)] [In] string ImageName = null, [In] uint CheckSum = default(uint), [In] uint TimeDateStamp = default(uint), [In] IntPtr Context = default(IntPtr), [In] uint ContextSize = default(uint));

		[MethodImpl(MethodImplOptions.InternalCall)]
		void UnloadModule([MarshalAs(UnmanagedType.LPWStr)] [In] string ImageBaseName = null, [In] ulong BaseOffset = default(ulong), [In] IntPtr Context = default(IntPtr), [In] uint ContextSize = default(uint));

		[MethodImpl(MethodImplOptions.InternalCall)]
		void SystemError([In] uint Error, [In] uint Level, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void SessionStatus([In] uint Status);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void ChangeDebuggeeState([In] uint Flags, [In] ulong Argument, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void ChangeEngineState([In] uint Flags, [In] ulong Argument, [In] IntPtr Context, [In] uint ContextSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		void ChangeSymbolState([In] uint Flags, [In] ulong Argument);
	}
}
