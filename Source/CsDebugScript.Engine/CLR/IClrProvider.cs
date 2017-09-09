namespace CsDebugScript.CLR
{
    /// <summary>
    /// Entry interface that provides CLR debugability.
    /// </summary>
    public interface IClrProvider
    {
        /// <summary>
        /// Gets the CLR runtimes running in the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        IClrRuntime[] GetClrRuntimes(Process process);
    }
}
