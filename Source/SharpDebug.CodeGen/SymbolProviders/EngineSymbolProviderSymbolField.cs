﻿using SharpDebug.Engine;

namespace SharpDebug.CodeGen.SymbolProviders
{
    /// <summary>
    /// Class represents symbol field during debugging.
    /// </summary>
    public class EngineSymbolProviderSymbolField : SymbolField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineSymbolProviderSymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        /// <param name="name">The field name.</param>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="offset">The offset.</param>
        public EngineSymbolProviderSymbolField(Symbol parentType, string name, Symbol fieldType, int offset)
            : base(parentType)
        {
            Name = name;
            Offset = offset;
            Size = fieldType != null ? fieldType.Size : 0;
            Type = fieldType;

            // TODO: BitPosition, LocationType, DataKind, Value
            DataKind = DIA.DataKind.Member;
            LocationType = DIA.LocationType.RegRel;
            if (offset < 0)
                LocationType = DIA.LocationType.Static;

            if (LocationType == DIA.LocationType.BitField)
            {
                BitSize = Size;
            }
            else
            {
                BitSize = Size * 8;
            }
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
        public SharpDebug.Module EngineModule
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
        /// Gets a value indicating whether this instance is valid static field.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid static field; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValidStatic
        {
            get
            {
                return IsStatic;
            }
        }
    }
}
