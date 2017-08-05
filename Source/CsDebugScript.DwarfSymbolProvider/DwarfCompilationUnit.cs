using System;
using System.Collections.Generic;

namespace CsDebugScript.DwarfSymbolProvider
{
    internal class DwarfCompilationUnit
    {
        private Dictionary<int, DwarfSymbol> symbolsByOffset = new Dictionary<int, DwarfSymbol>();

        public DwarfCompilationUnit(DwarfMemoryReader debugData, DwarfMemoryReader debugDataDescription, Dictionary<int, string> debugStrings, ulong codeSegmentOffset)
        {
            ReadData(debugData, debugDataDescription, debugStrings, codeSegmentOffset);
        }

        public DwarfSymbol[] SymbolsTree { get; private set; }

        public IEnumerable<DwarfSymbol> Symbols
        {
            get
            {
                return symbolsByOffset.Values;
            }
        }

        private void ReadData(DwarfMemoryReader debugData, DwarfMemoryReader debugDataDescription, Dictionary<int, string> debugStrings, ulong codeSegmentOffset)
        {
            // Read header
            bool is64bit;
            int beginPosition = debugData.Position;
            ulong length = debugData.ReadLength(out is64bit);
            int endPosition = debugData.Position + (int)length;
            ushort version = debugData.ReadUshort();
            int debugDataDescriptionOffset = debugData.ReadOffset(is64bit);
            byte addressSize = debugData.ReadByte();
            DataDescriptionReader dataDescriptionReader = new DataDescriptionReader(debugDataDescription, debugDataDescriptionOffset);

            // Read data
            List<DwarfSymbol> symbols = new List<DwarfSymbol>();
            Stack<DwarfSymbol> parents = new Stack<DwarfSymbol>();

            while (debugData.Position < endPosition)
            {
                int dataPosition = debugData.Position;
                uint code = debugData.LEB128();

                if (code == 0)
                {
                    parents.Pop();
                    continue;
                }

                DataDescription description = dataDescriptionReader.FindDebugDataDescription(code);
                Dictionary<DwarfAttribute, DwarfAttributeValue> attributes = new Dictionary<DwarfAttribute, DwarfAttributeValue>();

                foreach (DataDescriptionAttribute descriptionAttribute in description.Attributes)
                {
                    DwarfAttribute attribute = descriptionAttribute.Attribute;
                    DwarfFormat format = descriptionAttribute.Format;
                    DwarfAttributeValue attributeValue = new DwarfAttributeValue();

                    switch (format)
                    {
                        case DwarfFormat.Address:
                            attributeValue.Type = DwarfAttributeValueType.Address;
                            attributeValue.Value = debugData.ReadUlong(addressSize);
                            break;
                        case DwarfFormat.Block:
                            attributeValue.Type = DwarfAttributeValueType.Block;
                            attributeValue.Value = debugData.ReadBlock(debugData.LEB128());
                            break;
                        case DwarfFormat.Block1:
                            attributeValue.Type = DwarfAttributeValueType.Block;
                            attributeValue.Value = debugData.ReadBlock(debugData.ReadByte());
                            break;
                        case DwarfFormat.Block2:
                            attributeValue.Type = DwarfAttributeValueType.Block;
                            attributeValue.Value = debugData.ReadBlock(debugData.ReadUshort());
                            break;
                        case DwarfFormat.Block4:
                            attributeValue.Type = DwarfAttributeValueType.Block;
                            attributeValue.Value = debugData.ReadBlock(debugData.ReadUint());
                            break;
                        case DwarfFormat.Data1:
                            attributeValue.Type = DwarfAttributeValueType.Constant;
                            attributeValue.Value = (ulong)debugData.ReadByte();
                            break;
                        case DwarfFormat.Data2:
                            attributeValue.Type = DwarfAttributeValueType.Constant;
                            attributeValue.Value = (ulong)debugData.ReadUshort();
                            break;
                        case DwarfFormat.Data4:
                            attributeValue.Type = DwarfAttributeValueType.Constant;
                            attributeValue.Value = (ulong)debugData.ReadUint();
                            break;
                        case DwarfFormat.Data8:
                            attributeValue.Type = DwarfAttributeValueType.Constant;
                            attributeValue.Value = (ulong)debugData.ReadUlong();
                            break;
                        case DwarfFormat.SData:
                            attributeValue.Type = DwarfAttributeValueType.Constant;
                            attributeValue.Value = (ulong)debugData.SLEB128();
                            break;
                        case DwarfFormat.UData:
                            attributeValue.Type = DwarfAttributeValueType.Constant;
                            attributeValue.Value = (ulong)debugData.LEB128();
                            break;
                        case DwarfFormat.String:
                            attributeValue.Type = DwarfAttributeValueType.String;
                            attributeValue.Value = debugData.ReadAnsiString();
                            break;
                        case DwarfFormat.Strp:
                            attributeValue.Type = DwarfAttributeValueType.String;
                            attributeValue.Value = debugStrings[debugData.ReadOffset(is64bit)];
                            break;
                        case DwarfFormat.Flag:
                            attributeValue.Type = DwarfAttributeValueType.Flag;
                            attributeValue.Value = debugData.ReadByte() != 0;
                            break;
                        case DwarfFormat.FlagPresent:
                            attributeValue.Type = DwarfAttributeValueType.Flag;
                            attributeValue.Value = true;
                            break;
                        case DwarfFormat.Ref1:
                            attributeValue.Type = DwarfAttributeValueType.Reference;
                            attributeValue.Value = (ulong)debugData.ReadByte() + (ulong)beginPosition;
                            break;
                        case DwarfFormat.Ref2:
                            attributeValue.Type = DwarfAttributeValueType.Reference;
                            attributeValue.Value = (ulong)debugData.ReadUshort() + (ulong)beginPosition;
                            break;
                        case DwarfFormat.Ref4:
                            attributeValue.Type = DwarfAttributeValueType.Reference;
                            attributeValue.Value = (ulong)debugData.ReadUint() + (ulong)beginPosition;
                            break;
                        case DwarfFormat.Ref8:
                            attributeValue.Type = DwarfAttributeValueType.Reference;
                            attributeValue.Value = (ulong)debugData.ReadUlong() + (ulong)beginPosition;
                            break;
                        case DwarfFormat.RefUData:
                            attributeValue.Type = DwarfAttributeValueType.Reference;
                            attributeValue.Value = (ulong)debugData.LEB128() + (ulong)beginPosition;
                            break;
                        case DwarfFormat.RefAddr:
                            attributeValue.Type = DwarfAttributeValueType.Reference;
                            attributeValue.Value = (ulong)debugData.ReadOffset(is64bit);
                            break;
                        case DwarfFormat.RefSig8:
                            attributeValue.Type = DwarfAttributeValueType.Invalid;
                            debugData.Position += 8;
                            break;
                        case DwarfFormat.ExpressionLocation:
                            attributeValue.Type = DwarfAttributeValueType.ExpressionLocation;
                            attributeValue.Value = debugData.ReadBlock(debugData.LEB128());
                            break;
                        case DwarfFormat.SecOffset:
                            attributeValue.Type = DwarfAttributeValueType.SecOffset;
                            attributeValue.Value = (ulong)debugData.ReadOffset(is64bit);
                            break;
                        default:
                            throw new Exception($"Unsupported DwarfFormat: {format}");
                    }

                    if (attributes.ContainsKey(attribute))
                    {
                        if (attributes[attribute] != attributeValue)
                        {
                            attributes[attribute] = attributeValue;
                        }
                    }
                    else
                    {
                        attributes.Add(attribute, attributeValue);
                    }
                }

                DwarfSymbol symbol = new DwarfSymbol()
                {
                    Tag = description.Tag,
                    Attributes = attributes,
                    Offset = dataPosition,
                };

                symbolsByOffset.Add(symbol.Offset, symbol);

                if (parents.Count > 0)
                {
                    parents.Peek().Children.Add(symbol);
                    symbol.Parent = parents.Peek();
                }
                else
                {
                    symbols.Add(symbol);
                }

                if (description.HasChildren)
                {
                    symbol.Children = new List<DwarfSymbol>();
                    parents.Push(symbol);
                }
            }

            SymbolsTree = symbols.ToArray();

            // Post process all symbols
            foreach (DwarfSymbol symbol in Symbols)
            {
                Dictionary<DwarfAttribute, DwarfAttributeValue> attributes = symbol.Attributes as Dictionary<DwarfAttribute, DwarfAttributeValue>;

                foreach (DwarfAttributeValue value in attributes.Values)
                {
                    if (value.Type == DwarfAttributeValueType.Reference)
                    {
                        DwarfSymbol reference;

                        if (symbolsByOffset.TryGetValue((int)value.Address, out reference))
                        {
                            value.Type = DwarfAttributeValueType.ResolvedReference;
                            value.Value = reference;
                        }
                    }
                    else if (value.Type == DwarfAttributeValueType.Address)
                    {
                        value.Value = value.Address - codeSegmentOffset;
                    }
                }
            }

            // Merge specifications
            foreach (DwarfSymbol symbol in Symbols)
            {
                Dictionary<DwarfAttribute, DwarfAttributeValue> attributes = symbol.Attributes as Dictionary<DwarfAttribute, DwarfAttributeValue>;
                DwarfAttributeValue specificationValue;

                if (attributes.TryGetValue(DwarfAttribute.Specification, out specificationValue) && specificationValue.Type == DwarfAttributeValueType.ResolvedReference)
                {
                    DwarfSymbol reference = specificationValue.Reference;
                    Dictionary<DwarfAttribute, DwarfAttributeValue> referenceAttributes = reference.Attributes as Dictionary<DwarfAttribute, DwarfAttributeValue>;

                    foreach (KeyValuePair<DwarfAttribute, DwarfAttributeValue> kvp in attributes)
                    {
                        if (kvp.Key != DwarfAttribute.Specification)
                        {
                            referenceAttributes[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
        }

        private struct DataDescription
        {
            public DwarfTag Tag { get; set; }

            public bool HasChildren { get; set; }

            public List<DataDescriptionAttribute> Attributes { get; set; }
        }

        private struct DataDescriptionAttribute
        {
            public DwarfAttribute Attribute { get; set; }

            public DwarfFormat Format { get; set; }
        }

        private class DataDescriptionReader
        {
            DwarfMemoryReader debugDataDescription;
            Dictionary<uint, DataDescription> readDescriptions;
            int lastReadPosition;

            public DataDescriptionReader(DwarfMemoryReader debugDataDescription, int startingPosition)
            {
                readDescriptions = new Dictionary<uint, DataDescription>();
                lastReadPosition = startingPosition;
                this.debugDataDescription = debugDataDescription;
            }

            public DataDescription FindDebugDataDescription(uint findCode)
            {
                DataDescription result;

                if (readDescriptions.TryGetValue(findCode, out result))
                {
                    return result;
                }

                debugDataDescription.Position = lastReadPosition;
                while (!debugDataDescription.IsEnd)
                {
                    uint code = debugDataDescription.LEB128();
                    DwarfTag tag = (DwarfTag)debugDataDescription.LEB128();
                    bool hasChildren = debugDataDescription.ReadByte() != 0;
                    List<DataDescriptionAttribute> attributes = new List<DataDescriptionAttribute>();

                    while (!debugDataDescription.IsEnd)
                    {
                        DwarfAttribute attribute = (DwarfAttribute)debugDataDescription.LEB128();
                        DwarfFormat format = (DwarfFormat)debugDataDescription.LEB128();

                        while (format == DwarfFormat.Indirect)
                        {
                            format = (DwarfFormat)debugDataDescription.LEB128();
                        }

                        if (attribute == DwarfAttribute.None && format == DwarfFormat.None)
                        {
                            break;
                        }

                        attributes.Add(new DataDescriptionAttribute()
                        {
                            Attribute = attribute,
                            Format = format,
                        });
                    }

                    result = new DataDescription()
                    {
                        Tag = tag,
                        HasChildren = hasChildren,
                        Attributes = attributes,
                    };
                    readDescriptions.Add(code, result);
                    if (code == findCode)
                    {
                        lastReadPosition = debugDataDescription.Position;
                        return result;
                    }
                }

                throw new NotImplementedException();
            }
        }
    }
}
