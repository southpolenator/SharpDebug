using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.DwarfSymbolProvider
{
    internal class DwarfCompilationUnit
    {
        public DwarfCompilationUnit(DwarfMemoryReader debugData, DwarfMemoryReader debugDataDescription, Dictionary<int, string> debugStrings, ulong codeSegmentOffset)
        {
            InfoDataByOffset = new Dictionary<int, DwarfInfoData>();
            ReadData(debugData, debugDataDescription, debugStrings, codeSegmentOffset);
        }

        public DwarfInfoData[] InfoDataTree { get; private set; }

        public Dictionary<int, DwarfInfoData> InfoDataByOffset { get; private set; }

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

            // Read data
            List<DwarfInfoData> infoData = new List<DwarfInfoData>();
            Stack<DwarfInfoData> parents = new Stack<DwarfInfoData>();

            while (debugData.Position < endPosition)
            {
                int dataPosition = debugData.Position;
                uint code = debugData.LEB128();

                if (code == 0)
                {
                    parents.Pop();
                    continue;
                }

                FindDebugDataDescriptionPosition(debugDataDescription, debugDataDescriptionOffset, code);
                DwarfTag tag = (DwarfTag)debugDataDescription.LEB128();
                bool hasChildren = debugDataDescription.ReadByte() != 0;
                Dictionary<DwarfAttribute, DwarfAttributeValue> attributes = new Dictionary<DwarfAttribute, DwarfAttributeValue>();

                while (true)
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

                DwarfInfoData element = new DwarfInfoData()
                {
                    Tag = tag,
                    Attributes = attributes,
                    Offset = dataPosition,
                };

                InfoDataByOffset.Add(element.Offset, element);

                if (parents.Count > 0)
                {
                    parents.Peek().Children.Add(element);
                    element.Parent = parents.Peek();
                }
                else
                {
                    infoData.Add(element);
                }

                if (hasChildren)
                {
                    element.Children = new List<DwarfInfoData>();
                    parents.Push(element);
                }
            }

            InfoDataTree = infoData.ToArray();

            // Post process all elements
            foreach (DwarfInfoData element in InfoDataByOffset.Values)
            {
                Dictionary<DwarfAttribute, DwarfAttributeValue> attributes = element.Attributes as Dictionary<DwarfAttribute, DwarfAttributeValue>;

                foreach (DwarfAttributeValue value in attributes.Values)
                {
                    if (value.Type == DwarfAttributeValueType.Reference)
                    {
                        DwarfInfoData reference;

                        if (InfoDataByOffset.TryGetValue((int)value.Address, out reference))
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
            foreach (DwarfInfoData element in InfoDataByOffset.Values)
            {
                Dictionary<DwarfAttribute, DwarfAttributeValue> attributes = element.Attributes as Dictionary<DwarfAttribute, DwarfAttributeValue>;
                DwarfAttributeValue specificationValue;

                if (attributes.TryGetValue(DwarfAttribute.Specification, out specificationValue) && specificationValue.Type == DwarfAttributeValueType.ResolvedReference)
                {
                    DwarfInfoData reference = specificationValue.Reference;
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

        private static void FindDebugDataDescriptionPosition(DwarfMemoryReader debugDataDescription, int startingPosition, uint findCode)
        {
            // TODO: We can traverse only once through whole debugDataDescription stream and find positions for all codes and later just fetch from dictionary.
            debugDataDescription.Position = startingPosition;
            while (true)
            {
                uint code = debugDataDescription.LEB128();

                if (code == findCode)
                {
                    break;
                }

                uint tag = debugDataDescription.LEB128();
                bool hasChild = debugDataDescription.ReadByte() != 0;

                // Skip attributes
                DwarfAttribute attribute;
                DwarfFormat format;

                do
                {
                    attribute = (DwarfAttribute)debugDataDescription.LEB128();
                    format = (DwarfFormat)debugDataDescription.LEB128();
                }
                while ((attribute != DwarfAttribute.None) || (format != DwarfFormat.None));
            }
        }
    }
}
