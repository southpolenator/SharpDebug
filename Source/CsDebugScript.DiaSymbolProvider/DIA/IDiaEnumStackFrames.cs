using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Enumerate the various stack frames contained in the data source.
    /// </summary>
    [ComImport, Guid("EC9D461D-CE74-4711-A020-7D8F9A1DD255"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDiaEnumStackFrames
    {
        /// <summary>
        /// Retrieves a specified number of stack frames in the enumeration sequence.
        /// </summary>
        /// <param name="celt">The number of stack frames in the enumerator to be retrieved.</param>
        /// <param name="rgelt">Returns an array of <see cref="IDiaStackFrame"/> objects that represents the desired stack frames.</param>
        /// <param name="pceltFetched">Returns the number of stack frames in the fetched enumerator.</param>
        void Next(
            [In] uint celt,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] IDiaStackFrame[] rgelt,
            [Out] out uint pceltFetched);

        /// <summary>
        /// Resets an enumeration sequence to the beginning.
        /// </summary>
        void Reset();
    }
}
