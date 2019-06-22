using SharpDebug.CodeGen.SymbolProviders;
using SharpDebug.CodeGen.TypeInstances;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpDebug.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents Enum.
    /// </summary>
    /// <seealso cref="UserType" />
    internal class EnumUserType : UserType
    {
        /// <summary>
        /// Lazy cache for the <see cref="AreValuesFlags"/> property.
        /// </summary>
        private SimpleCacheStruct<bool> areValuesFlagsCache;

        /// <summary>
        /// Lazy cache for the <see cref="BasicType"/> property.
        /// </summary>
        private SimpleCacheStruct<Type> basicTypeCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumUserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        /// <param name="factory">User type factory that contains this element.</param>
        public EnumUserType(Symbol symbol, string nameSpace, UserTypeFactory factory)
            : base(symbol, null, nameSpace, factory)
        {
            areValuesFlagsCache = SimpleCache.CreateStruct(CheckIfValuesAreFlags);
            basicTypeCache = SimpleCache.CreateStruct(() => GetEnumBasicType(Symbol));
        }

        /// <summary>
        /// Gets the flag indicating that all values in enumeration are flags.
        /// </summary>
        public bool AreValuesFlags => areValuesFlagsCache.Value;

        /// <summary>
        /// Built-in type that should this enumeration inherit from.
        /// </summary>
        public Type BasicType => basicTypeCache.Value;

        /// <summary>
        /// Function that should evaluate <see cref="UserType.BaseClass"/> and <see cref="UserType.BaseClassOffset"/> properties.
        /// </summary>
        protected override Tuple<TypeInstance, int> GetBaseClass(Symbol symbol)
        {
            if (BasicType != null)
                return Tuple.Create<TypeInstance, int>(new BasicTypeInstance(CodeNaming, BasicType), 0);
            return Tuple.Create<TypeInstance, int>(new StaticClassTypeInstance(CodeNaming), 0);
        }

        /// <summary>
        /// Gets the .NET basic type string for the specified enumeration symbol.
        /// </summary>
        /// <param name="symbol">The enumeration symbol.</param>
        internal static Type GetEnumBasicType(Symbol symbol)
        {
            switch (symbol.BasicType)
            {
                case DIA.BasicType.Int:
                case DIA.BasicType.Long:
                    switch (symbol.Size)
                    {
                        case 8:
                            return typeof(long);
                        case 4:
                            return typeof(int);
                        case 2:
                            return typeof(short);
                        case 1:
                            return typeof(sbyte);
                        case 0:
                            return null;
                        default:
                            break;
                    }
                    break;

                case DIA.BasicType.UInt:
                case DIA.BasicType.ULong:
                    switch (symbol.Size)
                    {
                        case 8:
                            return typeof(ulong);
                        case 4:
                            return typeof(uint);
                        case 2:
                            return typeof(ushort);
                        case 1:
                            return typeof(byte);
                        case 0:
                            return null;
                        default:
                            break;
                    }
                    break;
                case DIA.BasicType.Char:
                    return typeof(sbyte);

                default:
                    break;
            }

            throw new InvalidDataException("Unknown enum type.");
        }

        /// <summary>
        /// Checks whether values inside the enumeration are flags.
        /// </summary>
        private bool CheckIfValuesAreFlags()
        {
            HashSet<long> values = new HashSet<long>();

            foreach (var enumValue in Symbol.EnumValues)
                values.Add(long.Parse(enumValue.Item2));
            foreach (var value in values)
                if (!IsPowerOfTwo(value))
                    return false;
            if (values.Count < 2 || (values.Contains(0) && values.Contains(1) && values.Count == 2)
                || (values.Contains(0) && values.Contains(1) && values.Contains(2) && values.Count == 3))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified number is power of two.
        /// </summary>
        /// <param name="x">The number.</param>
        private static bool IsPowerOfTwo(long x)
        {
            return (x & (x - 1)) == 0;
        }
    }
}
