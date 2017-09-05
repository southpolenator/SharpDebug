using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Undocumented on MSDN
    /// </summary>
    [ComImport, Guid("3BFE56B0-390C-4863-9430-1F3D083B7684"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaInputAssemblyFile
    {
        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(1)]
        uint uniqueId { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(2)]
        uint index { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(3)]
        uint timeStamp { get; }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(4)]
        bool pdbAvailableAtILMerge
        {
            [return: MarshalAs(UnmanagedType.Bool)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        [DispId(5)]
        string fileName
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        /// <param name="cbData"></param>
        /// <param name="pcbData"></param>
        /// <param name="pbData"></param>
        void get_version(
            [In] uint cbData,
            [Out] out uint pcbData,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] pbData);
    }
}
