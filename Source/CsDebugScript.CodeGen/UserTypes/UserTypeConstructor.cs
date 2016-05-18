namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class represents constructor for UserType
    /// </summary>
    internal class UserTypeConstructor
    {
        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public string Arguments { get; set; } = "";

        /// <summary>
        /// Gets or sets the base class initialization code.
        /// </summary>
        public string BaseClassInitialization { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether this constructor contains field definitions in the body.
        /// </summary>
        /// <value>
        /// <c>true</c> if this constructor contains field definitions in the body; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsFieldDefinitions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this constructor is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if static constructor; otherwise, <c>false</c>.
        /// </value>
        public bool Static { get; set; }

        /// <summary>
        /// Writes the constructor code to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The current indentation.</param>
        /// <param name="fields">The list of fields in the user type.</param>
        /// <param name="constructorName">The constructor name.</param>
        public void WriteCode(IndentedWriter output, int indentation, UserTypeField[] fields, string constructorName)
        {
            if (Static)
            {
                // Do nothing. We are initializing static variables in declaration statement because of the performance problems with generics.
            }
            else
            {
                output.WriteLine();
                output.WriteLine(indentation, "public {0}({1})", constructorName, Arguments);
                if (!string.IsNullOrEmpty(BaseClassInitialization))
                    output.WriteLine(indentation + 1, ": {0}", BaseClassInitialization);
                output.WriteLine(indentation++, "{{");
                if (ContainsFieldDefinitions)
                    foreach (var field in fields)
                    {
                        if (!field.CacheResult && !field.UseUserMember)
                            continue;
                        if (!field.Static)
                            field.WriteConstructorCode(output, indentation);
                    }
                output.WriteLine(--indentation, "}}");
            }
        }
    }
}
