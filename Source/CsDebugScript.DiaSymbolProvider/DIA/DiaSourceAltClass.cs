using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Coclass for <see cref="DiaSourceAlt"/> interface.
    /// </summary>
    [ComImport, ClassInterface(ClassInterfaceType.None), Guid("91904831-49CA-4766-B95C-25397E2DD6DC")]
#if NET461
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
#endif
    public class DiaSourceAltClass : IDiaDataSource, DiaSourceAlt
    {
        /// <summary>
        /// Retrieves the file name for the last load error.
        /// </summary>
        [DispId(1)]
        public virtual extern string lastError
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            [return: MarshalAs(UnmanagedType.BStr)]
            get;
        }

        /// <summary>
        /// Opens and prepares a program database (.pdb) file as a debug data source.
        /// </summary>
        /// <param name="pdbPath">The path to the .pdb file.</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void loadDataFromPdb(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pdbPath);

        /// <summary>
        /// Opens and verifies that the program database (.pdb) file matches the signature information provided, and prepares the .pdb file as a debug data source.
        /// </summary>
        /// <param name="pdbPath">The path to the .pdb file.</param>
        /// <param name="pcsig70">The GUID signature to verify against the .pdb file signature. Only .pdb files in Visual C++ and later have GUID signatures.</param>
        /// <param name="sig">The 32-bit signature to verify against the .pdb file signature.</param>
        /// <param name="age">Age value to verify. The age does not necessarily correspond to any known time value, it is used to determine if a .pdb file is out of sync with a corresponding .exe file.</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void loadAndValidateDataFromPdb(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pdbPath,
            [In] ref Guid pcsig70,
            [In] uint sig,
            [In] uint age);

        /// <summary>
        /// Opens and prepares the debug data associated with the .exe/.dll file.
        /// </summary>
        /// <param name="executable">Path to the .exe or .dll file.</param>
        /// <param name="searchPath">Alternate path to search for debug data.</param>
        /// <param name="pCallback">An IUnknown interface for an object that supports a debug callback interface, such as the IDiaLoadCallback, IDiaLoadCallback2, the IDiaReadExeAtOffsetCallback, and/or the IDiaReadExeAtRVACallback interfaces.</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void loadDataForExe(
            [In, MarshalAs(UnmanagedType.LPWStr)] string executable,
            [In, MarshalAs(UnmanagedType.LPWStr)] string searchPath,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pCallback);

        /// <summary>
        /// Prepares the debug data stored in a program database (.pdb) file accessed through an in-memory data stream.
        /// </summary>
        /// <param name="pIStream">An IStream object representing the data stream to use.</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void loadDataFromIStream(
            [In, MarshalAs(UnmanagedType.Interface)] object pIStream);

        /// <summary>
        /// Opens a session for querying symbols.
        /// </summary>
        /// <param name="ppSession">Returns an <see cref="IDiaSession"/> object representing the open session.</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void openSession(
            [Out, MarshalAs(UnmanagedType.Interface)] out IDiaSession ppSession);

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="searchPath"></param>
        /// <param name="cbCvInfo"></param>
        /// <param name="pbCvInfo"></param>
        /// <param name="pCallback"></param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void loadDataFromCodeViewInfo(
            [In, MarshalAs(UnmanagedType.LPWStr)] string executable,
            [In, MarshalAs(UnmanagedType.LPWStr)] string searchPath,
            [In] uint cbCvInfo,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbCvInfo,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pCallback);

        /// <summary>
        /// Undocumented on MSDN
        /// </summary>
        /// <param name="executable"></param>
        /// <param name="searchPath"></param>
        /// <param name="timeStampExe"></param>
        /// <param name="timeStampDbg"></param>
        /// <param name="sizeOfExe"></param>
        /// <param name="cbMiscInfo"></param>
        /// <param name="pbMiscInfo"></param>
        /// <param name="pCallback"></param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public virtual extern void loadDataFromMiscInfo(
            [In, MarshalAs(UnmanagedType.LPWStr)] string executable,
            [In, MarshalAs(UnmanagedType.LPWStr)] string searchPath,
            [In] uint timeStampExe,
            [In] uint timeStampDbg,
            [In] uint sizeOfExe,
            [In] uint cbMiscInfo,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] pbMiscInfo,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pCallback);
    }
}
