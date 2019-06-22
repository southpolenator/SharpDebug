using System;
using System.Runtime.InteropServices;

namespace DbgEng
{
    [Guid("9F50E42C-F136-499E-9A97-73036C94ED2D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDebugInputCallbacks
    {
        void StartInput(
            [In] uint BufferSize);

        void EndInput();
    }
}
