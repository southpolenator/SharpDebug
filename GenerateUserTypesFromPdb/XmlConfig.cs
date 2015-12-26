using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace GenerateUserTypesFromPdb
{
    [XmlRoot]
    public class XmlConfig
    {
        public bool DontGenerateFieldTypeInfoComment { get; set; }

        public bool MultiLineProperties { get; set; }

        public bool UseDiaSymbolProvider { get; set; }

        [XmlArrayItem("Type")]
        public XmlType[] Types { get; set; }

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
            return new XmlSerializer(typeof(XmlConfig), new Type[] { typeof(RegexTransformation), typeof(PlainStringTransformation) });
        }
    }

    public class XmlType
    {
        [XmlAttribute]
        public string Name { get; set; }

        public HashSet<string> ExcludedFields { get; set; }

        public HashSet<string> IncludedFields { get; set; }
    }

    [XmlInclude(typeof(PlainStringTransformation))]
    [XmlInclude(typeof(RegexTransformation))]
    public abstract class XmlTypeTransformation
    {
        public const string FieldRegexGroup = "${field}";
        public const string FieldOffsetRegexGroup = "${fieldOffset}";
        public const string NewTypeRegexGroup = "${newType}";
        public const string ClassNameRegexGroup = "${className}";

        [XmlAttribute]
        public string TypeText { get; set; }

        [XmlAttribute]
        public string ConstructorText { get; set; }

        public abstract bool Matches(string originalType);

        public string TransformType(string originalType, string className)
        {
            return Transform(originalType, TypeText).Replace(ClassNameRegexGroup, className);
        }

        public string TransformConstructor(string originalType, string field, string fieldOffset, string className)
        {
            string transform = ConstructorText.Replace(FieldRegexGroup, field).Replace(FieldOffsetRegexGroup, fieldOffset).Replace(ClassNameRegexGroup, className);

            if (transform.Contains(NewTypeRegexGroup))
                transform = transform.Replace(NewTypeRegexGroup, TransformType(originalType, className));
            return Transform(originalType, transform);
        }

        protected abstract string Transform(string input, string transform);
    }

    public class RegexTransformation : XmlTypeTransformation
    {
        [XmlIgnore]
        private string regexText;

        [XmlIgnore]
        public Regex Regex { get; private set; }

        [XmlAttribute]
        public string RegexText
        {
            get
            {
                return regexText;
            }

            set
            {
                regexText = value;
                Regex = new Regex(regexText);
            }
        }

        public override bool Matches(string input)
        {
            Match match = Regex.Match(input);

            return match != null && match.Success && match.Length == input.Length;
        }

        protected override string Transform(string input, string transform)
        {
            return Regex.Replace(input, transform);
        }
    }

    public class PlainStringTransformation : XmlTypeTransformation
    {
        [XmlAttribute]
        public string InputText { get; set; }

        public override bool Matches(string input)
        {
            return input == InputText;
        }

        protected override string Transform(string input, string transform)
        {
            return transform;
        }
    }
}
