using System;

namespace DbgEng
{
    /// <summary>
    /// Specifies the flags that control how the debugger attaches to the kernel target.
    /// </summary>
    [Flags]
    public enum DebugAttachKernel
    {
        /// <summary>
        /// Attach to the kernel on the target computer.
        /// <note>Maps to DEBUG_ATTACH_KERNEL_CONNECTION.</note>
        /// </summary>
        KernelConnection = 0,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_ATTACH_LOCAL_KERNEL.</note>
        /// </summary>
        LocalKernel = 1,

        /// <summary>
        /// Attach to a kernel by using an eXDI driver.
        /// <note>Maps to DEBUG_ATTACH_EXDI_DRIVER.</note>
        /// </summary>
        ExdiDriver = 2,

        /// <summary>
        /// Undocumented on MSDN.
        /// <note>Maps to DEBUG_ATTACH_INSTALL_DRIVER.</note>
        /// </summary>
        InstallDriver = 4,
    }
}
