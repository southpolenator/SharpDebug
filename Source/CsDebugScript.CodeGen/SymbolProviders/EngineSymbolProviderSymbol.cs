using CsDebugScript.Engine;
using DIA;
using System;
using System.Collections.Generic;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Class represents symbol during debugging.
    /// </summary>
    public class EngineSymbolProviderSymbol : Symbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineSymbolProviderSymbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public EngineSymbolProviderSymbol(Module module, uint typeId)
            : base(module)
        {
            Id = typeId;
            Tag = EngineModuleProvider.GetTypeTag(Id);
            BasicType = ConvertToBasicType(EngineModuleProvider.GetTypeBuiltinType(Id));
            if (Tag != CodeTypeTag.ModuleGlobals)
            {
                Name = EngineModuleProvider.GetTypeName(Id);
            }
            else
            {
                Name = "";
            }
            ulong size = EngineModuleProvider.GetTypeSize(Id);

            if (size > int.MaxValue)
            {
                throw new ArgumentException("Symbol size is unexpected");
            }
            Size = (int)size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineSymbolProviderSymbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="tag">The code type tag.</param>
        public EngineSymbolProviderSymbol(Module module, uint typeId, int offset, CodeTypeTag tag)
            : this(module, typeId)
        {
            Offset = offset;
            Tag = tag;
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public new EngineSymbolProviderModule Module
        {
            get
            {
                return (EngineSymbolProviderModule)base.Module;
            }
        }

        /// <summary>
        /// Gets the engine module.
        /// </summary>
        public CsDebugScript.Module EngineModule
        {
            get
            {
                return Module.EngineModule;
            }
        }

        /// <summary>
        /// Gets the engine module provider.
        /// </summary>
        public ISymbolProviderModule EngineModuleProvider
        {
            get
            {
                return Module.EngineModuleProvider;
            }
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public override void InitializeCache()
        {
            // TODO: Do nothing?
        }

        /// <summary>
        /// Gets user type base classes.
        /// </summary>
        protected override IEnumerable<Symbol> GetBaseClasses()
        {
            foreach (Tuple<uint, int> baseClass in EngineModuleProvider.GetTypeDirectBaseClasses(Id).Values)
            {
                Symbol baseClassTypeSymbol = Module.GetSymbol(baseClass.Item1);
                Symbol baseClassSymbol = new EngineSymbolProviderSymbol(Module, baseClass.Item1, baseClass.Item2, CodeTypeTag.BaseClass);

                baseClassTypeSymbol.LinkSymbols(baseClassSymbol);
                yield return baseClassSymbol;
            }
        }

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected override Symbol GetElementType()
        {
            uint elementTypeId = EngineModuleProvider.GetTypeElementTypeId(Id);

            return Module.GetSymbol(elementTypeId);
        }

        /// <summary>
        /// Gets the pointer type to this symbol.
        /// </summary>
        protected override Symbol GetPointerType()
        {
            uint pointerTypeId = EngineModuleProvider.GetTypePointerToTypeId(Id);

            return Module.GetSymbol(pointerTypeId);
        }

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected override IEnumerable<SymbolField> GetFields()
        {
            // TODO: Static fields are missing
            foreach (string fieldName in EngineModuleProvider.GetTypeFieldNames(Id))
            {
                Tuple<uint, int> fieldTypeAndOffset = EngineModuleProvider.GetTypeFieldTypeAndOffset(Id, fieldName);
                Symbol fieldType = Module.GetSymbol(fieldTypeAndOffset.Item1);

                yield return new EngineSymbolProviderSymbolField(this, fieldName, fieldType, fieldTypeAndOffset.Item2);
            }
        }

        /// <summary>
        /// Casts as symbol field.
        /// </summary>
        public override SymbolField CastAsSymbolField()
        {
            return new EngineSymbolProviderSymbolField(this, Name, this, Offset);
        }

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public override bool HasVTable()
        {
            return EngineModuleProvider.HasTypeVTable(Id);
        }

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        protected override IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            return EngineModuleProvider.GetEnumValues(Id);
        }

        /// <summary>
        /// Converts <see cref="BuiltinType"/> to <see cref="BasicType"/>.
        /// </summary>
        /// <param name="builtinType">The built-in type.</param>
        private static BasicType ConvertToBasicType(BuiltinType builtinType)
        {
            switch (builtinType)
            {
                default:
                case BuiltinType.NoType:
                    return BasicType.NoType;
                case BuiltinType.Char8:
                    return BasicType.Char;
                case BuiltinType.Char16:
                    return BasicType.WChar;
                case BuiltinType.Char32:
                    return BasicType.Char32;
                case BuiltinType.Bool:
                    return BasicType.Bool;
                case BuiltinType.Void:
                    return BasicType.Void;
                case BuiltinType.Int8:
                case BuiltinType.Int16:
                case BuiltinType.Int32:
                    return BasicType.Int;
                case BuiltinType.Int64:
                case BuiltinType.Int128:
                    return BasicType.Long;
                case BuiltinType.UInt8:
                case BuiltinType.UInt16:
                case BuiltinType.UInt32:
                    return BasicType.UInt;
                case BuiltinType.UInt64:
                case BuiltinType.UInt128:
                    return BasicType.ULong;
                case BuiltinType.Float32:
                case BuiltinType.Float64:
                case BuiltinType.Float80:
                    return BasicType.Float;
            }
        }
    }
}
