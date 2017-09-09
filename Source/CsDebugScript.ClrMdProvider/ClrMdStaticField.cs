using CsDebugScript.CLR;

namespace CsDebugScript.ClrMdProvider
{
    /// <summary>
    /// ClrMD implementation of the <see cref="IClrStaticField"/>.
    /// </summary>
    internal class ClrMdStaticField : IClrStaticField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdStaticField"/> class.
        /// </summary>
        /// <param name="clrStaticField">The CLR static field.</param>
        public ClrMdStaticField(Microsoft.Diagnostics.Runtime.ClrStaticField clrStaticField)
        {
            ClrStaticField = clrStaticField;
        }

        /// <summary>
        /// Gets the CLR static field.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrStaticField ClrStaticField { get; private set; }

        /// <summary>
        /// Gets the address of the static field's value in memory.
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        public ulong GetAddress(IClrAppDomain appDomain)
        {
            return ClrStaticField.GetAddress(((ClrMdAppDomain)appDomain).ClrAppDomain);
        }
    }
}
