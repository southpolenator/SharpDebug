using System.Runtime.InteropServices;

namespace DIA
{
    /// <summary>
    /// Helper interface that can be instantiated using coclass.
    /// </summary>
    [ComImport, CoClass(typeof(DiaSourceClass)), Guid("79F1BB5F-B66E-48E5-B6A9-1545C323CA3D")]
    public interface DiaSource : IDiaDataSource
    {
    }
}
