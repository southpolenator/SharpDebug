using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpDebug.CodeGen.CodeWriters
{
    using UserType = SharpDebug.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Base class for .NET code writers.
    /// </summary>
    internal abstract class DotNetCodeWriter : ICodeWriter, ICodeNaming
    {
        /// <summary>
        /// Name of the field used in code: baseClassString.
        /// Represents string member that holds this user type class name that can be used to
        /// get base class from variable specified in the constructor.
        /// </summary>
        protected const string BaseClassStringFieldName = "ѦbaseClassString";

        /// <summary>
        /// Name of the field used in code: thisClass.
        /// Represents member <see cref="Variable"/> that is casted to code type of this class.
        /// </summary>
        protected const string ThisClassFieldName = "ѦthisClass";

        /// <summary>
        /// Name of the static field used in code: ClassCodeType.
        /// Represents static <see cref="CodeType"/> member of this user type. It can be used in physical user types to
        /// generate <see cref="Variable"/> from memory buffer.
        /// </summary>
        protected const string ClassCodeTypeFieldName = "ClassCodeType";

        /// <summary>
        /// Name of the <see cref="SharpDebug.UserType"/> member field: memoryBuffer.
        /// It is being used in physical user type to get memory buffer that holds data for instance of this user type.
        /// </summary>
        protected const string MemoryBufferFieldName = "memoryBuffer";

        /// <summary>
        /// Name of the <see cref="SharpDebug.UserType"/> member field: memoryBufferOffset.
        /// It is being used in physical user type to get memory buffer offset where is the start of instance of this user type.
        /// </summary>
        protected const string MemoryBufferOffsetFieldName = "memoryBufferOffset";

        /// <summary>
        /// Name of the <see cref="SharpDebug.UserType"/> member field: memoryBufferAddress.
        /// It is being used in physical user type to get process address from which memory buffer starts. <see cref="MemoryBufferFieldName"/>
        /// </summary>
        protected const string MemoryBufferAddressFieldName = "memoryBufferAddress";

        /// <summary>
        /// Set of C# keywords that cannot be used as user type names.
        /// </summary>
        protected static readonly HashSet<string> Keywords = new HashSet<string>()
            {
                "lock", "base", "params", "enum", "in", "object", "event", "string", "private", "public", "internal", "namespace",
                "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "decimal", "fixed", "out", "class", "struct",
                "base", "this", "static", "readonly", "new", "using", "get", "set", "is", "null", "ref", "var", "typeof", "for",
                "foreach", "while", "do", "continue", "break", "return",
            };

        /// <summary>
        /// Longest keyword that we have in set of C# key words.
        /// </summary>
        protected static readonly int LongestKeyword = Keywords.Max(s => s.Length);

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetCodeWriter"/> class.
        /// </summary>
        /// <param name="generationFlags">The code generation options</param>
        /// <param name="nameLimit">Maximum number of characters that generated name can have.</param>
        /// <param name="fixKeywordsInUserNaming">Should we fix keywords by adding @ when fixing user naming?</param>
        public DotNetCodeWriter(UserTypeGenerationFlags generationFlags, int nameLimit, bool fixKeywordsInUserNaming)
        {
            GenerationFlags = generationFlags;
            NameLimit = nameLimit;
            FixKeywordsInUserNaming = fixKeywordsInUserNaming;
        }

        /// <summary>
        /// The code generation options
        /// </summary>
        public UserTypeGenerationFlags GenerationFlags { get; private set; }

        /// <summary>
        /// Maximum number of characters that generated name can have.
        /// </summary>
        public int NameLimit { get; private set; }

        /// <summary>
        /// Should we fix keywords by adding @ when fixing user naming?
        /// </summary>
        public bool FixKeywordsInUserNaming { get; private set; }

        /// <summary>
        /// Gets the code naming interface. <see cref="ICodeNaming"/>
        /// </summary>
        public ICodeNaming Naming => this;

        /// <summary>
        /// Returns <c>true</c> if code writer supports binary writer.
        /// </summary>
        public abstract bool HasBinaryWriter { get; }

        /// <summary>
        /// Returns <c>true</c> if code writer supports text writer.
        /// </summary>
        public abstract bool HasTextWriter { get; }

        /// <summary>
        /// Generates code for user type and writes it to the specified output.
        /// </summary>
        /// <param name="userType">User type for which code should be generated.</param>
        /// <param name="output">Output text writer.</param>
        public abstract void WriteUserType(UserType userType, StringBuilder output);

        /// <summary>
        /// Generated binary code for user types. This is used only if <see cref="HasBinaryWriter"/> is <c>true</c>.
        /// </summary>
        /// <param name="userTypes">User types for which code should be generated.</param>
        /// <param name="dllFileName">Output DLL file path.</param>
        /// <param name="generatePdb"><c>true</c> if PDB file should be generated.</param>
        /// <param name="additionalAssemblies">Enumeration of additional assemblies that we should load for type lookup - used with transformations.</param>
        public abstract void GenerateBinary(IEnumerable<UserType> userTypes, string dllFileName, bool generatePdb, IEnumerable<string> additionalAssemblies);

        /// <summary>
        /// Converts built-in type to string.
        /// </summary>
        /// <param name="type">Type to be converted.</param>
        /// <returns>String representation of the type.</returns>
        public string ToString(Type type)
        {
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(sbyte))
                return "sbyte";
            if (type == typeof(short))
                return "short";
            if (type == typeof(ushort))
                return "ushort";
            if (type == typeof(int))
                return "int";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(long))
                return "long";
            if (type == typeof(ulong))
                return "ulong";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(char))
                return "char";
            if (type == typeof(string))
                return "string";
            return type.ToString();
        }

        /// <summary>
        /// Corrects naming inside user type. Replaces unallowed characters and keywords.
        /// </summary>
        /// <param name="name">Name of the type, field, variable, enumeration, etc.</param>
        /// <returns>Name that can be used in generated code.</returns>
        public string FixUserNaming(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (ShouldFixUserNaming(name))
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < name.Length; i++)
                {
                    switch (name[i])
                    {
                        // Replace with _
                        case '.':
                        case ':':
                        case '&':
                        case '-':
                        case '<':
                        case '>':
                        case ' ':
                        case ',':
                        case '(':
                        case ')':
                        case '[':
                        case ']':
                        case '`':
                        case '\'':
                        case '$':
                        case '?':
                        case '@':
                        case '+':
                        case '=':
                        case '~':
                        case '%':
                        case '^':
                        case '|':
                            sb.Append('_');
                            break;
                        // Ignore
                        case '*':
                            break;
                        // Add this character
                        default:
                            sb.Append(name[i]);
                            break;
                    }
                }

                // Fixed name cannot be longer than some limit
                if (sb.Length > NameLimit)
                    sb.Length = NameLimit;
                name = sb.ToString();
            }
            else
            {
                // Fixed name cannot be longer than some limit
                if (name.Length > NameLimit)
                    name = name.Substring(0, NameLimit);
            }

            // Keywords should be prefixed with @...
            if (name.Length <= LongestKeyword && FixKeywordsInUserNaming && Keywords.Contains(name))
                return $"@{name}";
            return name;
        }

        /// <summary>
        /// Checks whether user naming should be fixed.
        /// </summary>
        /// <param name="name">Name of the type, field, variable, enumeration, etc.</param>
        /// <returns><c>true</c> if new string should be created; <c>false</c> if original name can be used.</returns>
        private static bool ShouldFixUserNaming(string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                switch (name[i])
                {
                    case '.':
                    case ':':
                    case '&':
                    case '-':
                    case '<':
                    case '>':
                    case ' ':
                    case ',':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '`':
                    case '\'':
                    case '$':
                    case '?':
                    case '@':
                    case '+':
                    case '=':
                    case '~':
                    case '%':
                    case '^':
                    case '|':
                    case '*':
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Converts built-in type to <see cref="SharpDebug.UserType"/> naming of it used in Read functions.
        /// </summary>
        /// <param name="type">Built-in type.</param>
        protected static string ToUserTypeName(Type type)
        {
            if (type == typeof(byte))
                return "Byte";
            if (type == typeof(sbyte))
                return "Sbyte";
            if (type == typeof(short))
                return "Short";
            if (type == typeof(ushort))
                return "Ushort";
            if (type == typeof(int))
                return "Int";
            if (type == typeof(uint))
                return "Uint";
            if (type == typeof(long))
                return "Long";
            if (type == typeof(ulong))
                return "Ulong";
            if (type == typeof(float))
                return "Float";
            if (type == typeof(double))
                return "Double";
            if (type == typeof(bool))
                return "Bool";
            if (type == typeof(bool))
                return "Char";
            return null;
        }

        /// <summary>
        /// Gets user type field name for the specified property name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected static string GetUserTypeFieldName(string propertyName)
        {
            if (propertyName.StartsWith("@"))
                return $"Ѧ{propertyName.Substring(1)}";
            return $"Ѧ{propertyName}";
        }
    }
}
