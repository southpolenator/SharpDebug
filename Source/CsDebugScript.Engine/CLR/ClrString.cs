using CsDebugScript.Exceptions;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// CLR code String. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    /// <seealso cref="CsDebugScript.Variable" />
    public class ClrString : Variable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrString"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public ClrString(Variable variable)
            : base(variable)
        {
            // Check if code type is string type
            ClrCodeType codeType = variable.GetCodeType() as ClrCodeType;

            if (codeType == null || !codeType.ClrType.IsString)
            {
                throw new WrongCodeTypeException(variable.GetCodeType(), nameof(variable), "System.String");
            }
        }

        /// <summary>
        /// Gets the length of the string.
        /// </summary>
        public int Length
        {
            get
            {
                return (int)GetField("m_stringLength");
            }
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text
        {
            get
            {
                Variable firstChar = GetField("m_firstChar");

                return GetCodeType().Module.Process.ReadString(firstChar.GetPointerAddress(), 2, Length);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
