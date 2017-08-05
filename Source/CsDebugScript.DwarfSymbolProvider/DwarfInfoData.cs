using System;
using System.Collections.Generic;

namespace CsDebugScript.DwarfSymbolProvider
{
    internal class DwarfInfoData
    {
        public DwarfTag Tag { get; internal set; }

        public IReadOnlyDictionary<DwarfAttribute, DwarfAttributeValue> Attributes { get; internal set; }

        public List<DwarfInfoData> Children { get; internal set; }

        public DwarfInfoData Parent { get; internal set; }

        public int Offset { get; set; }

        public string Name
        {
            get
            {
                DwarfAttributeValue nameValue;

                if (Attributes.TryGetValue(DwarfAttribute.Name, out nameValue))
                {
                    return nameValue.String;
                }

                return null;
            }
        }

        public string FullName
        {
            get
            {
                if (Parent != null && Parent.Tag != DwarfTag.CompileUnit)
                {
                    return Parent.FullName + "::" + Name;
                }

                return Name;
            }
        }

        public override string ToString()
        {
            return $"{Tag} (Offset = {Offset}, Attributes = {Attributes.Count}, Children = {Children?.Count}";
        }

        public ulong GetConstantAttribute(DwarfAttribute attribute, ulong defaultValue = 0)
        {
            DwarfAttributeValue value;

            if (Attributes.TryGetValue(attribute, out value))
            {
                return value.Constant;
            }

            return defaultValue;
        }
    }
}
