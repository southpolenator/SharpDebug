using SharpDebug.CLR;

namespace SharpDebug.ClrMdProvider
{
    /// <summary>
    /// ClrMD implementation of the <see cref="IClrInstanceField"/>.
    /// </summary>
    internal class ClrMdInstanceField : IClrInstanceField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdInstanceField"/> class.
        /// </summary>
        /// <param name="provider">The CLR provider.</param>
        /// <param name="clrInstanceField">The CLR instance field.</param>
        public ClrMdInstanceField(CLR.ClrMdProvider provider, Microsoft.Diagnostics.Runtime.ClrInstanceField clrInstanceField)
        {
            Provider = provider;
            ClrInstanceField = clrInstanceField;
        }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string Name
        {
            get
            {
                return ClrInstanceField.Name;
            }
        }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public IClrType Type
        {
            get
            {
                return Provider.FromClrType(ClrInstanceField.Type);
            }
        }

        /// <summary>
        /// Gets the CLR provider.
        /// </summary>
        internal CLR.ClrMdProvider Provider { get; private set; }

        /// <summary>
        /// Gets the CLR instance field.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrInstanceField ClrInstanceField { get; private set; }

        /// <summary>
        /// Gets the field offset.
        /// </summary>
        /// <param name="isValueClass">if set to <c>true</c> it will return offset in a structure; otherwise it will return offset in a class.</param>
        public int GetOffset(bool isValueClass)
        {
            return (int)ClrInstanceField.GetAddress(0, isValueClass);
        }
    }
}
