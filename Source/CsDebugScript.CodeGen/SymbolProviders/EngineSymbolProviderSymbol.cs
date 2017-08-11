﻿using CsDebugScript.Engine;
using Dia2Lib;
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
            Tag = (SymTagEnum)EngineModuleProvider.GetTypeTag(EngineModule, Id);
            BasicType = EngineModuleProvider.GetTypeBasicType(EngineModule, Id);
            if (Tag != SymTagEnum.SymTagExe)
            {
                Name = EngineModuleProvider.GetTypeName(EngineModule, Id);
                if (string.IsNullOrEmpty(Name))
                {
                    Name = Name;
                }
            }
            else
            {
                Name = "";
            }
            ulong size = EngineModuleProvider.GetTypeSize(EngineModule, Id);

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
        /// <param name="tag">The symbol tag.</param>
        public EngineSymbolProviderSymbol(Module module, uint typeId, int offset, SymTagEnum tag)
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
            foreach (Tuple<uint, int> baseClass in EngineModuleProvider.GetTypeDirectBaseClasses(EngineModule, Id).Values)
            {
                Symbol baseClassTypeSymbol = Module.GetSymbol(baseClass.Item1);
                Symbol baseClassSymbol = new EngineSymbolProviderSymbol(Module, baseClass.Item1, baseClass.Item2, SymTagEnum.SymTagBaseClass);

                baseClassTypeSymbol.LinkSymbols(baseClassSymbol);
                yield return baseClassSymbol;
            }
        }

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected override Symbol GetElementType()
        {
            uint elementTypeId = EngineModuleProvider.GetTypeElementTypeId(EngineModule, Id);

            return Module.GetSymbol(elementTypeId);
        }

        /// <summary>
        /// Gets the pointer type to this symbol.
        /// </summary>
        protected override Symbol GetPointerType()
        {
            uint pointerTypeId = EngineModuleProvider.GetTypePointerToTypeId(EngineModule, Id);

            return Module.GetSymbol(pointerTypeId);
        }

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected override IEnumerable<SymbolField> GetFields()
        {
            // TODO: Static fields are missing
            foreach (string fieldName in EngineModuleProvider.GetTypeFieldNames(EngineModule, Id))
            {
                Tuple<uint, int> fieldTypeAndOffset = EngineModuleProvider.GetTypeFieldTypeAndOffset(EngineModule, Id, fieldName);
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
    }
}