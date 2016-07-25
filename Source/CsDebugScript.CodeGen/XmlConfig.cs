using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace CsDebugScript.CodeGen
{
    /// <summary>
    /// XML configuration for the code generator.
    /// </summary>
    [XmlRoot]
    public class XmlConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether generator shouldn't generate comment with field type information.
        /// </summary>
        /// <value>
        /// <c>true</c> if generator shouldn't generate comment with field type information; otherwise, <c>false</c>.
        /// </value>
        public bool DontGenerateFieldTypeInfoComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated properties should be formatted in multiple lines.
        /// </summary>
        /// <value>
        ///   <c>true</c> if generated properties should be formatted in multiple lines; otherwise, <c>false</c>.
        /// </value>
        public bool MultiLineProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated code will use DIA symbol provider.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated code will use DIA symbol provider; otherwise, <c>false</c>.
        /// </value>
        public bool UseDiaSymbolProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated code should be compressed.
        /// Compressed code has one space indent, removes comments and uses single like for generating properties.
        /// </summary>
        /// <value>
        ///   <c>true</c> if generated code should be compressed; otherwise, <c>false</c>.
        /// </value>
        public bool CompressedOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated code should use operator new for creating user types instead of casting.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated code should use operator new for creating user types instead of casting; otherwise, <c>false</c>.
        /// </value>
        public bool ForceUserTypesToNewInsteadOfCasting { get; set; }

        /// <summary>
        /// Gets or sets the name of the generated assembly.
        /// If not empty, code generator will compile generated code into generated assembly.
        /// </summary>
        /// <value>
        /// The name of the generated assembly.
        /// </value>
        public string GeneratedAssemblyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated assembly should be compiled with Roslyn.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated assembly should be compiled with Roslyn; otherwise, <c>false</c>.
        /// </value>
        public bool GenerateAssemblyWithRoslyn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated assembly won't have PDB generated.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated assembly won't have PDB generated; otherwise, <c>false</c>.
        /// </value>
        public bool DisablePdbGeneration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated code files shouldn't be saved on disk.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated code files shouldn't be saved on disk; otherwise, <c>false</c>.
        /// </value>
        public bool DontSaveGeneratedCodeFiles { get; set; }

        /// <summary>
        /// Gets or sets the name of the generated props file.
        /// If not empty, code generator will generate props file that contains compile statement for each generated code file.
        /// </summary>
        /// <value>
        /// The name of the generated props file.
        /// </value>
        public string GeneratedPropsFileName { get; set; }

        /// <summary>
        /// Gets or sets the namespace name for types found in multiple modules.
        /// </summary>
        /// <value>
        /// The namespace name for types found in multiple modules.
        /// </value>
        public string CommonTypesNamespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated user type should cache value of fields.
        /// While using this will result in better performance when calling properties multiple times,
        /// it will also drastically impact user type creation (specially when user type has a lot of fields).
        /// </summary>
        /// <value>
        /// <c>true</c> if generated user type should cache value of fields; otherwise, <c>false</c>.
        /// </value>
        public bool CacheUserTypeFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated user type should cache value of static fields.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated user type should cache value of static fields; otherwise, <c>false</c>.
        /// </value>
        public bool CacheStaticUserTypeFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated user type should lazily cache value of fields.
        /// While using this will result in better performance when calling properties multiple times,
        /// it will also drastically impact user type memory usage and somewhat impact its creation.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated user type should lazily cache value of fields; otherwise, <c>false</c>.
        /// </value>
        public bool LazyCacheUserTypeFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated code will use physical mapping instead of symbolic to access fields.
        /// Using this greatly improve performance, but lower re-usability of generated code after adding new fields.
        /// When using this, consider making code generator part of the build.
        /// </summary>
        /// <value>
        /// <c>true</c> if generated code will use physical mapping instead of symbolic to access fields; otherwise, <c>false</c>.
        /// </value>
        public bool GeneratePhysicalMappingOfUserTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generated code should be saved in single file.
        /// If set to <c>false</c>, every generated class/enum will be saved in separate file and every namespace will be separate folder.
        /// </summary>
        /// <value>
        ///   <c>true</c> if generated code should be saved in single file; otherwise, <c>false</c>.
        /// </value>
        public bool SingleFileExport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether generator should try to match use of Hungarian notation.
        /// </summary>
        /// <value>
        /// <c>true</c> if generator should try to match use of Hungarian notation; otherwise, <c>false</c>.
        /// </value>
        public bool UseHungarianNotation { get; set; }

        /// <summary>
        /// Gets or sets the list of modules for which user types should be exported.
        /// </summary>
        [XmlArrayItem("Module")]
        public XmlModule[] Modules { get; set; }

        /// <summary>
        /// Gets or sets the list of user types to be exported.
        /// If the list is empty, all user type will be exported.
        /// </summary>
        [XmlArrayItem("Type")]
        public XmlType[] Types { get; set; }

        /// <summary>
        /// Gets or sets the list of files to be compiled when generating assembly.
        /// Since all exported user types are partial classes, you can easily add code for it.
        /// For example, if you have container class, you can add include file to implement IEnumerable interface for that type.
        /// </summary>
        [XmlArrayItem("IncludedFile")]
        public XmlIncludedFile[] IncludedFiles { get; set; }

        /// <summary>
        /// Gets or sets the list of referenced assemblies when generating assembly.
        /// </summary>
        [XmlArrayItem("ReferencedAssembly")]
        public XmlReferencedAssembly[] ReferencedAssemblies { get; set; }

        /// <summary>
        /// Gets or sets the list of transformations that will be applied on generated user types.
        /// </summary>
        [XmlArrayItem("Transformation")]
        public XmlTypeTransformation[] Transformations { get; set; }

        /// <summary>
        /// Reads the XML configuration from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>Read XML configuration.</returns>
        public static XmlConfig Read(Stream stream)
        {
            var serializer = CreateSerializer();

            using (var reader = new StreamReader(stream))
            {
                return (XmlConfig)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Reads the XML configuration from the specified path.
        /// </summary>
        /// <param name="xmlConfigPath">The XML configuration path.</param>
        /// <returns>Read XML configuration.</returns>
        internal static XmlConfig Read(string xmlConfigPath)
        {
            var serializer = CreateSerializer();

            using (var reader = new StreamReader(xmlConfigPath))
            {
                return (XmlConfig)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Gets the user type generation flags.
        /// </summary>
        internal UserTypeGenerationFlags GetGenerationFlags()
        {
            UserTypeGenerationFlags generationFlags = UserTypeGenerationFlags.None;

            if (CompressedOutput)
            {
                generationFlags |= UserTypeGenerationFlags.CompressedOutput;
                DontGenerateFieldTypeInfoComment = true;
                MultiLineProperties = false;
            }

            if (!DontGenerateFieldTypeInfoComment)
                generationFlags |= UserTypeGenerationFlags.GenerateFieldTypeInfoComment;
            if (!MultiLineProperties)
                generationFlags |= UserTypeGenerationFlags.SingleLineProperty;
            if (UseDiaSymbolProvider)
                generationFlags |= UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider;
            if (ForceUserTypesToNewInsteadOfCasting)
                generationFlags |= UserTypeGenerationFlags.ForceUserTypesToNewInsteadOfCasting;
            if (CacheUserTypeFields)
                generationFlags |= UserTypeGenerationFlags.CacheUserTypeFields;
            if (CacheStaticUserTypeFields)
                generationFlags |= UserTypeGenerationFlags.CacheStaticUserTypeFields;
            if (LazyCacheUserTypeFields)
                generationFlags |= UserTypeGenerationFlags.LazyCacheUserTypeFields;
            if (GeneratePhysicalMappingOfUserTypes)
                generationFlags |= UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes;
            if (SingleFileExport)
                generationFlags |= UserTypeGenerationFlags.SingleFileExport;
            if (UseHungarianNotation)
                generationFlags |= UserTypeGenerationFlags.UseHungarianNotation;
            return generationFlags;
        }

        /// <summary>
        /// Creates the XML serializer for this XML configuration.
        /// </summary>
        private static XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(XmlConfig), new Type[] { });
        }
    }

    /// <summary>
    /// The module definition from which we will export user types.
    /// </summary>
    public class XmlModule
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path to PDB file.
        /// </summary>
        [XmlAttribute]
        public string PdbPath { get; set; }

        /// <summary>
        /// Gets or sets the namespace where all exported user types should be placed.
        /// </summary>
        [XmlAttribute]
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the common module namespace where all exported user types should be placed.
        /// </summary>
        [XmlAttribute]
        public string CommonNamespace { get; set; }
    }

    /// <summary>
    /// The C# code file that contains partial class definition to be used when generating assembly.
    /// </summary>
    public class XmlIncludedFile
    {
        /// <summary>
        /// Gets or sets the path to C# file.
        /// </summary>
        [XmlAttribute]
        public string Path { get; set; }
    }

    /// <summary>
    /// The assembly that will be referenced when generating assembly.
    /// </summary>
    public class XmlReferencedAssembly
    {
        /// <summary>
        /// Gets or sets the path to assembly file.
        /// </summary>
        [XmlAttribute]
        public string Path { get; set; }
    }

    /// <summary>
    /// User type that will be exported from modules.
    /// </summary>
    public class XmlType
    {
        /// <summary>
        /// Gets or sets the name of the user type.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of excluded fields.
        /// </summary>
        public HashSet<string> ExcludedFields { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the list of included fields.
        /// </summary>
        public HashSet<string> IncludedFields { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets a value indicating whether this user type is template.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user type is template; otherwise, <c>false</c>.
        /// </value>
        internal bool IsTemplate
        {
            get
            {
                return SymbolNameHelper.ContainsTemplateType(Name);
            }
        }

        /// <summary>
        /// Gets the name wildcard pattern when searching for the type.
        /// </summary>
        /// <value>
        /// The name wildcard.
        /// </value>
        internal string NameWildcard
        {
            get
            {
                if (!IsTemplate)
                    return Name;
                return Name.Replace("<>", "<*>");
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
            return Name;
        }
    }

    /// <summary>
    /// The transformation to be applied for types found in modules.
    /// </summary>
    public class XmlTypeTransformation
    {
        internal const string FieldRegexGroup = "${field}";
        internal const string FieldOffsetRegexGroup = "${fieldOffset}";
        internal const string NewTypeRegexGroup = "${newType}";
        internal const string ClassNameRegexGroup = "${className}";
        internal static readonly Dictionary<string, string> CastRegexGroups = new Dictionary<string, string>()
        {
            { "${new}", "new ${newType}(${field})" },
            { "${newOffset}", "new ${newType}(${field}, ${fieldOffset})" },
            { "${cast}", "(${newType})${field}" },
        };

        /// <summary>
        /// Gets or sets the transformation for generating new type.
        /// </summary>
        [XmlAttribute]
        public string NewType { get; set; }

        /// <summary>
        /// Gets or sets the transformation for generating constructor.
        /// </summary>
        [XmlAttribute]
        public string Constructor { get; set; }

        /// <summary>
        /// Gets or sets the transformation for matching original type.
        /// </summary>
        [XmlAttribute]
        public string OriginalType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether transformed type has physical constructor.
        /// </summary>
        /// <value>
        /// <c>true</c> if transformed type has physical constructor; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool HasPhysicalConstructor { get; set; }

        /// <summary>
        /// Checks whether this transformation matches the specified input type.
        /// </summary>
        /// <param name="inputType">Type of the input.</param>
        internal bool Matches(string inputType)
        {
            return ParseType(OriginalType, inputType);
        }

        /// <summary>
        /// Transforms the specified input type based on the class name and type converter.
        /// </summary>
        /// <param name="inputType">Type of the input.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <returns>Transformed type</returns>
        internal string TransformType(string inputType, string className, Func<string, string> typeConverter)
        {
            Dictionary<string, string> groups = new Dictionary<string, string>(CastRegexGroups);

            ParseType(OriginalType, inputType, groups, typeConverter);
            groups.Add(ClassNameRegexGroup, className);
            return Transform(NewType, groups);
        }

        /// <summary>
        /// Transforms the constructor for the specified input type.
        /// </summary>
        /// <param name="inputType">Type of the input.</param>
        /// <param name="field">The field.</param>
        /// <param name="fieldOffset">The field offset.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="typeConverter">The type converter.</param>
        internal string TransformConstructor(string inputType, string field, string fieldOffset, string className, Func<string, string> typeConverter)
        {
            Dictionary<string, string> groups = new Dictionary<string, string>(CastRegexGroups);

            ParseType(OriginalType, inputType, groups, typeConverter);
            groups.Add(FieldRegexGroup, field);
            groups.Add(FieldOffsetRegexGroup, fieldOffset);
            groups.Add(ClassNameRegexGroup, className);
            groups.Add(NewTypeRegexGroup, TransformType(inputType, className, typeConverter));
            return Transform(Constructor, groups);
        }

        /// <summary>
        /// Transforms the specified input by replacing occurrences of the groups inside it.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="groups">The groups.</param>
        private string Transform(string input, Dictionary<string, string> groups)
        {
            for (bool changed = true; changed; )
            {
                changed = false;
                foreach (var g in groups)
                {
                    string transformed = input.Replace(g.Key, g.Value);

                    if (transformed != input)
                    {
                        changed = true;
                        input = transformed;
                        break;
                    }
                }
            }

            return input;
        }

        /// <summary>
        /// Tries to match original type with the input type and parses found groups.
        /// </summary>
        /// <param name="originalType">Type of the original.</param>
        /// <param name="inputType">Type of the input.</param>
        /// <param name="groups">The groups.</param>
        /// <param name="typeConverter">The type converter.</param>
        private static bool ParseType(string originalType, string inputType, Dictionary<string, string> groups = null, Func<string, string> typeConverter = null)
        {
            int i = 0, j = 0;

            while (i < originalType.Length && j < inputType.Length)
            {
                if (originalType[i] != '$')
                {
                    if (originalType[i] != inputType[j])
                    {
                        return false;
                    }

                    i++;
                    j++;
                    continue;
                }

                string newType = ExtractNewCastName(originalType, i);
                string extractedType = ExtractType(inputType, j);

                i += newType.Length;
                j += extractedType.Length;
                if (groups != null)
                {
                    extractedType = extractedType.Trim();
                    groups.Add(newType, typeConverter != null ? typeConverter(extractedType) : extractedType);
                }
            }

            return i == originalType.Length && j == inputType.Length;
        }

        /// <summary>
        /// Extracts the type from the specified input and offset.
        /// </summary>
        /// <param name="inputType">Type of the input.</param>
        /// <param name="offset">The offset.</param>
        internal static string ExtractType(string inputType, int offset)
        {
            int k = offset;
            int openedTypes = 0;
            int function = 0;

            while (k < inputType.Length && (function != 0 || (openedTypes != 0 || (inputType[k] != ',' && inputType[k] != '>'))))
            {
                switch (inputType[k])
                {
                    case '(':
                        function++;
                        break;
                    case ')':
                        function--;
                        break;
                    case '<':
                        openedTypes++;
                        break;
                    case '>':
                        openedTypes--;
                        break;
                }

                k++;
            }

            return inputType.Substring(offset, k - offset);
        }

        /// <summary>
        /// Get number of template arguments..
        /// </summary>
        /// <param name="inputType">Type of the input.</param>
        /// <param name="offset">The offset.</param>
        internal static int GetTemplateArgCount(string inputType, int offset)
        {
            int k = offset;
            int openedTypes = 0;
            int function = 0;
            int argCount = 0;

            while (k < inputType.Length && (function != 0 || (openedTypes != 0 || (inputType[k] != ',' && inputType[k] != '>'))))
            {
                switch (inputType[k])
                {
                    case '(':
                        function++;
                        break;
                    case ')':
                        function--;
                        break;
                    case '<':
                        openedTypes++;
                        break;
                    case '>':
                        openedTypes--;
                        break;
                    case ',':
                        if (openedTypes == 1 && function == 0)
                        {
                            argCount++;
                        }
                        break;
                }

                k++;
            }

            return argCount;
        }

        private static string ExtractNewCastName(string originalType, int i)
        {
            int k = i;
            bool correct = k < originalType.Length && originalType[k++] == '$';

            correct = correct && k < originalType.Length && originalType[k++] == '{';
            while (correct && k < originalType.Length && originalType[k] != '}')
            {
                correct = char.IsLetterOrDigit(originalType[k]) || originalType[k] == '_';
                k++;
            }
            correct = correct && k < originalType.Length && originalType[k++] == '}';
            correct = correct && k < originalType.Length && (originalType[k] == ',' || originalType[k] == '>');
            if (!correct)
                throw new Exception("Incorrect format of OriginalType '" + originalType + "' at char " + i);

            return originalType.Substring(i, k - i);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return OriginalType + " => " + NewType;
        }
    }
}
