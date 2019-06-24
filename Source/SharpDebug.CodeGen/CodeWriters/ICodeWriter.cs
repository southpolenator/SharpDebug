using System;
using System.Collections.Generic;
using System.Text;

namespace SharpDebug.CodeGen.CodeWriters
{
    using UserType = SharpDebug.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Declares how come naming should behave. Code naming interfaces are used to output code namings for generated user types.
    /// </summary>
    internal interface ICodeNaming
    {
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

    /// <summary>
    /// Declares how code writers should behave. Code writers are used to output code for generated user types.
    /// </summary>
    internal interface ICodeWriter
    {
        /// <summary>
        /// Gets the code naming interface. <see cref="ICodeNaming"/>
        /// </summary>
        ICodeNaming Naming { get; }

        /// <summary>
        /// Returns <c>true</c> if code writer supports binary writer.
        /// </summary>
        bool HasBinaryWriter { get; }

        /// <summary>
        /// Returns <c>true</c> if code writer supports text writer.
        /// </summary>
        bool HasTextWriter { get; }

        /// <summary>
        /// Generates code for user type and writes it to the specified output. This is used only if <see cref="HasTextWriter"/> is <c>true</c>.
        /// </summary>
        /// <param name="userType">User type for which code should be generated.</param>
        /// <param name="output">Output text writer.</param>
        void WriteUserType(UserType userType, StringBuilder output);

        /// <summary>
        /// Generated binary code for user types. This is used only if <see cref="HasBinaryWriter"/> is <c>true</c>.
        /// </summary>
        /// <param name="userTypes">User types for which code should be generated.</param>
        /// <param name="dllFileName">Output DLL file path.</param>
        /// <param name="generatePdb"><c>true</c> if PDB file should be generated.</param>
        /// <param name="additionalAssemblies">Enumeration of additional assemblies that we should load for type lookup - used with transformations.</param>
        void GenerateBinary(IEnumerable<UserType> userTypes, string dllFileName, bool generatePdb, IEnumerable<string> additionalAssemblies);
    }
}
