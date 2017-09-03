using System;
using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Helper interface that can be instantiated using coclass.
    /// </summary>
    [ComImport, CoClass(typeof(DiaStackWalkerClass)), Guid("5485216B-A54C-469F-9670-52B24D5229BB")]
    public interface DiaStackWalker : IDiaStackWalker
    {
    }
}
