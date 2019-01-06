using CsDebugScript.CLR;

namespace CsDebugScript.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of the <see cref="IClrStaticField"/>.
    /// </summary>
    internal class VSClrStaticField : IClrStaticField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VSClrStaticField"/> class.
        /// </summary>
        /// <param name="parentType">The owning type.</param>
        /// <param name="name">The field name.</param>
        /// <param name="type">The field type.</param>
        public VSClrStaticField(VSClrType parentType, string name, VSClrType type)
        {
            ParentType = parentType;
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Gets the owning type.
        /// </summary>
        public VSClrType ParentType { get; private set; }

        /// <summary>
        /// Gets the field name.
        /// </summary>

        public string Name { get; private set; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public IClrType Type { get; private set; }

        /// <summary>
        /// Gets the address of the static field's value in memory.
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        public ulong GetAddress(IClrAppDomain appDomain)
        {
            return ParentType.Proxy.GetClrStaticFieldAddress(ParentType.Runtime.Process.Id, ParentType.Id, Name, appDomain.Id);
        }
    }
}
