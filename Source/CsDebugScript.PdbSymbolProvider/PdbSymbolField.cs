using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.PdbSymbolProvider.SymbolRecords;
using CsDebugScript.PdbSymbolProvider.TypeRecords;

namespace CsDebugScript.PdbSymbolProvider
{
    using ConstantSymbol = SymbolRecords.ConstantSymbol;

    /// <summary>
    /// Interface represents symbol field for PDB reader.
    /// </summary>
    public class PdbSymbolField : SymbolField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        /// <param name="record">Data member type record.</param>
        public PdbSymbolField(PdbSymbol parentType, DataMemberRecord record)
            : base(parentType)
        {
            Name = record.Name;
            DataKind = DIA.DataKind.Member;
            Offset = (int)record.FieldOffset;
            TypeRecord typeRecord = !record.Type.IsSimple ? parentType.PdbModule.PdbFile.TpiStream[record.Type] : null;
            if (typeRecord is BitFieldRecord bitFieldRecord)
            {
                LocationType = DIA.LocationType.BitField;
                BitPosition = bitFieldRecord.BitOffset;
                BitSize = bitFieldRecord.BitSize;
                Type = parentType.PdbModule.GetSymbol(bitFieldRecord.Type);
            }
            else
            {
                LocationType = DIA.LocationType.ThisRel;
                Type = parentType.PdbModule.GetSymbol(record.Type);
                Size = Type.Size;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        /// <param name="record">Static data member type record.</param>
        public PdbSymbolField(PdbSymbol parentType, StaticDataMemberRecord record)
            : base(parentType)
        {
            Name = record.Name;
            LocationType = DIA.LocationType.Static;
            DataKind = DIA.DataKind.StaticMember;
            Type = parentType.PdbModule.GetSymbol(record.Type);
            Size = Type.Size;

            // Try native constant
            ConstantSymbol constant;
            string nativeConstant = $"{parentType.Name}::{Name}";

            if (!parentType.PdbModule.Constants.TryGetValue(nativeConstant, out constant))
            {
                // Try managed constant
                string managedConstant = $"{parentType.Name}.{Name}";

                if (!parentType.PdbModule.Constants.TryGetValue(managedConstant, out constant))
                    constant = null;
            }

            if (constant != null && record.Type == constant.TypeIndex)
            {
                LocationType = DIA.LocationType.Constant;
                Value = constant.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        /// <param name="data">Data symbol type record.</param>
        public PdbSymbolField(PdbGlobalScope parentType, DataSymbol data)
            : base(parentType)
        {
            Name = data.Name;
            LocationType = DIA.LocationType.Static;
            DataKind = DIA.DataKind.StaticMember;
            Type = parentType.PdbModule.GetSymbol(data.Type);
            Size = Type.Size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        /// <param name="constant">Constant symbol record.</param>
        public PdbSymbolField(PdbGlobalScope parentType, ConstantSymbol constant)
            : base(parentType)
        {
            Name = constant.Name;
            LocationType = DIA.LocationType.Constant;
            DataKind = DIA.DataKind.StaticMember;
            Type = parentType.PdbModule.GetSymbol(constant.TypeIndex);
            Size = Type.Size;
            Value = constant.Value;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid static field.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid static field; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValidStatic
        {
            get
            {
                return IsStatic && ((ParentType is PdbGlobalScope) || Module.PublicSymbols.Contains(ParentType.Name + "::" + Name));
            }
        }
    }
}
