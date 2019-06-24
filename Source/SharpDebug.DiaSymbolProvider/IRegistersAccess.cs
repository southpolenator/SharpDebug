using DIA;

namespace SharpDebug.Engine.SymbolProviders
{
    /// <summary>
    /// Interface for accessing register values.
    /// </summary>
    internal interface IRegistersAccess
    {
        /// <summary>
        /// Gets register value.
        /// </summary>
        /// <param name="registerId">Register index.</param>
        /// <returns>Register value.</returns>
        ulong GetRegisterValue(CV_HREG_e registerId);
    }
}
