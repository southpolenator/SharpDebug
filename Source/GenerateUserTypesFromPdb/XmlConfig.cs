using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace GenerateUserTypesFromPdb
{
    [XmlRoot]
    public class XmlConfig
    {
        public bool DontGenerateFieldTypeInfoComment { get; set; }

        public bool MultiLineProperties { get; set; }

        public bool UseDiaSymbolProvider { get; set; }

        public bool ForceUserTypesToNewInsteadOfCasting { get; set; }

        public string GeneratedAssemblyName { get; set; }

        public bool GenerateAssemblyWithRoslyn { get; set; }

        public bool DisablePdbGeneration { get; set; }

        public bool DontSaveGeneratedCodeFiles { get; set; }

        public string GeneratedPropsFileName { get; set; }

        public string CommonTypesNamespace { get; set; }

        public bool CacheUserTypeFields { get; set; }

        public bool CacheStaticUserTypeFields { get; set; }

        public bool LazyCacheUserTypeFields { get; set; }

        public bool GeneratePhysicalMappingOfUserTypes { get; set; }

        public bool SingleFileExport { get; set; }

        public bool UseHungarianNotation { get; set; }

        [XmlArrayItem("Module")]
        public XmlModule[] Modules { get; set; }

        [XmlArrayItem("Type")]
        public XmlType[] Types { get; set; }

        [XmlArrayItem("IncludedFile")]
        public XmlIncludedFile[] IncludedFiles { get; set; }

        [XmlArrayItem("Transformation")]
        public XmlTypeTransformation[] Transformations { get; set; }

        internal static XmlConfig Read(string xmlConfigPath)
        {
            var serializer = CreateSerializer();

            using (var reader = new StreamReader(xmlConfigPath))
            {
                return (XmlConfig)serializer.Deserialize(reader);
            }
        }

        internal void Save(string xmlConfigPath)
        {
            var serializer = CreateSerializer();

            using (var writer = new StreamWriter(xmlConfigPath))
            {
                serializer.Serialize(writer, this);
            }
        }

        private static XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(XmlConfig), new Type[] { });
        }
    }

    public class XmlModule
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string PdbPath { get; set; }

        [XmlAttribute]
        public string Namespace { get; set; }
    }

    public class XmlIncludedFile
    {
        [XmlAttribute]
        public string Path { get; set; }
    }

    public class XmlType
    {
        [XmlAttribute]
        public string Name { get; set; }

        public HashSet<string> ExcludedFields { get; set; } = new HashSet<string>();

        public HashSet<string> IncludedFields { get; set; } = new HashSet<string>();

        public bool IsTemplate
        {
            get
            {
                return Name.EndsWith("<>");
            }
        }

        public string NameWildcard
        {
            get
            {
                if (!IsTemplate)
                    return Name;
                return Name.Replace("<>", "<*>");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class XmlTypeTransformation
    {
        public const string FieldRegexGroup = "${field}";
        public const string FieldOffsetRegexGroup = "${fieldOffset}";
        public const string NewTypeRegexGroup = "${newType}";
        public const string ClassNameRegexGroup = "${className}";
        public static readonly Dictionary<string, string> CastRegexGroups = new Dictionary<string, string>()
        {
            { "${new}", "new ${newType}(${field})" },
            { "${newOffset}", "new ${newType}(${field}, ${fieldOffset})" },
            { "${cast}", "(${newType})${field}" },
        };

        [XmlAttribute]
        public string NewType { get; set; }

        [XmlAttribute]
        public string Constructor { get; set; }

        [XmlAttribute]
        public string OriginalType { get; set; }

        [XmlAttribute]
        public bool HasPhysicalConstructor { get; set; }

        public bool Matches(string inputType)
        {
            return ParseType(OriginalType, inputType);
        }

        public string TransformType(string inputType, string className, Func<string, string> typeConverter)
        {
            Dictionary<string, string> groups = new Dictionary<string, string>(CastRegexGroups);

            ParseType(OriginalType, inputType, groups, typeConverter);
            groups.Add(ClassNameRegexGroup, className);
            return Transform(NewType, groups);
        }

        public string TransformConstructor(string inputType, string field, string fieldOffset, string className, Func<string, string> typeConverter)
        {
            Dictionary<string, string> groups = new Dictionary<string, string>(CastRegexGroups);

            ParseType(OriginalType, inputType, groups, typeConverter);
            groups.Add(FieldRegexGroup, field);
            groups.Add(FieldOffsetRegexGroup, fieldOffset);
            groups.Add(ClassNameRegexGroup, className);
            groups.Add(NewTypeRegexGroup, TransformType(inputType, className, typeConverter));
            return Transform(Constructor, groups);
        }

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

        internal static string ExtractType(string inputType, int j)
        {
            int k = j;
            int openedTypes = 0;

            while (k < inputType.Length && (openedTypes != 0 || (inputType[k] != ',' && inputType[k] != '>')))
            {
                switch (inputType[k])
                {
                    case '<':
                        openedTypes++;
                        break;
                    case '>':
                        openedTypes--;
                        break;
                }

                k++;
            }

            return inputType.Substring(j, k - j);
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

        public override string ToString()
        {
            return OriginalType + " => " + NewType;
        }
    }
}
