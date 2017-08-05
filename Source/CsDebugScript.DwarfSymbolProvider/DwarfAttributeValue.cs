using System.Linq;

namespace CsDebugScript.DwarfSymbolProvider
{
    internal enum DwarfAttributeValueType
    {
        Invalid,
        Address,
        Block,
        Constant,
        String,
        Flag,
        Reference,
        ResolvedReference,
        ExpressionLocation,
        SecOffset,
    }

    internal class DwarfAttributeValue
    {
        public DwarfAttributeValueType Type { get; set; }

        public object Value { get; set; }

        public ulong Address
        {
            get
            {
                return (ulong)Value;
            }
        }

        public byte[] Block
        {
            get
            {
                return (byte[])Value;
            }
        }

        public ulong Constant
        {
            get
            {
                return (ulong)Value;
            }
        }

        public string String
        {
            get
            {
                return (string)Value;
            }
        }

        public bool Flag
        {
            get
            {
                return (bool)Value;
            }
        }

        public DwarfSymbol Reference
        {
            get
            {
                return (DwarfSymbol)Value;
            }
        }

        public byte[] ExpressionLocation
        {
            get
            {
                return (byte[])Value;
            }
        }

        public ulong SecOffset
        {
            get
            {
                return (ulong)Value;
            }
        }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            DwarfAttributeValue other = obj as DwarfAttributeValue;

            if (other != null)
            {
                return this == other;
            }

            return false;
        }

        public static bool operator ==(DwarfAttributeValue value1, DwarfAttributeValue value2)
        {
            if (value1.Type != value2.Type)
            {
                return false;
            }

            switch (value1.Type)
            {
                case DwarfAttributeValueType.Address:
                case DwarfAttributeValueType.Constant:
                case DwarfAttributeValueType.Reference:
                case DwarfAttributeValueType.SecOffset:
                    return (ulong)value1.Value == (ulong)value2.Value;
                case DwarfAttributeValueType.Block:
                case DwarfAttributeValueType.ExpressionLocation:
                    return Enumerable.SequenceEqual((byte[])value1.Value, (byte[])value2.Value);
                case DwarfAttributeValueType.Flag:
                    return (bool)value1.Value == (bool)value2.Value;
                case DwarfAttributeValueType.String:
                    return value1.Value.ToString() == value2.Value.ToString();
            }

            return true;
        }

        public static bool operator !=(DwarfAttributeValue value1, DwarfAttributeValue value2)
        {
            return !(value1 == value2);
        }
    }
}
