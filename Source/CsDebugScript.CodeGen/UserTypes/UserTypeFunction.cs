namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class that represents partial initialization function that is called in constructor.
    /// </summary>
    /// <seealso cref="UserTypeField" />
    internal class UserTypeFunction : UserTypeField
    {
        /// <summary>
        /// Writes the constructor code to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The current indentation.</param>
        public override void WriteConstructorCode(IndentedWriter output, int indentation)
        {
            output.WriteLine(indentation, "{0}();", FieldName);
        }

        /// <summary>
        /// Writes the field code.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The indentation.</param>
        /// <param name="options">The options.</param>
        public override void WriteFieldCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options)
        {
            output.WriteLine(indentation, "{0} {1}();", FieldType, FieldName);
        }

        /// <summary>
        /// Writes the property code.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The indentation.</param>
        /// <param name="options">The options.</param>
        /// <param name="firstField">if set to <c>true</c> [first field].</param>
        public override void WritePropertyCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options, ref bool firstField)
        {
            // Do nothing
        }
    }
}
