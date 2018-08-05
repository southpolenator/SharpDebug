using System;
using System.IO;

namespace CsDebugScript.CodeGen.CodeWriters
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Declares how code writers should behave. Code writers are used to output code for generated user types.
    /// </summary>
    internal interface ICodeWriter
    {
        /// <summary>
        /// Generates code for user type and writes it to the specified output.
        /// </summary>
        /// <param name="userType">User type for which code should be generated.</param>
        /// <param name="output">Output text writer.</param>
        void WriteUserType(UserType userType, TextWriter output);

        /// <summary>
        /// Corrects naming inside user type. Replaces unallowed characters and keywords.
        /// </summary>
        /// <param name="name">Name of the type, field, variable, enumeration, etc.</param>
        /// <returns>Name that can be used in generated code.</returns>
        string FixUserNaming(string name);

        /// <summary>
        /// Converts built-in type to string.
        /// </summary>
        /// <param name="type">Type to be converted.</param>
        /// <returns>String representation of the type.</returns>
        string ToString(Type type);
    }
}
