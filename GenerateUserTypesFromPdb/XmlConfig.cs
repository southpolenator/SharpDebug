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

        public XmlType[] Types { get; set; }

        internal static XmlConfig Read(string xmlConfigPath)
        {
            var serializer = new XmlSerializer(typeof(XmlConfig));

            using (var reader = new StreamReader(xmlConfigPath))
            {
                return (XmlConfig)serializer.Deserialize(reader);
            }
        }
    }

    public class XmlType
    {
        [XmlAttribute]
        public string Name { get; set; }

        public HashSet<string> ExcludedFields { get; set; }

        public HashSet<string> IncludedFields { get; set; }
    }
}
