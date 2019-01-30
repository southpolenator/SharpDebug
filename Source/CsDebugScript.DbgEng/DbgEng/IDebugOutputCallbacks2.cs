using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [Guid("67721FE9-56D2-4A44-A325-2B65513CE6EB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDebugOutputCallbacks2 : IDebugOutputCallbacks
    {
#pragma warning disable CS0108 // XXX hides inherited member. This is COM default.

        #region IDebugOutputCallbacks
        void Output(
            [In] uint Mask,
            [In, MarshalAs(UnmanagedType.LPStr)] string Text);
        #endregion

#pragma warning restore CS0108 // XXX hides inherited member. This is COM default.

        uint GetInterestMask();

        void Output2(
            [In] uint Which,
            [In] uint Flags,
            [In] ulong Arg,
            [In, MarshalAs(UnmanagedType.LPWStr)] string Text);
    }
}
