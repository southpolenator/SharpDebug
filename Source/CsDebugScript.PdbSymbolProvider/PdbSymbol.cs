using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.PdbSymbolProvider.TypeRecords;
using System;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider
{
    /// <summary>
    /// Class represents CodeGen symbol for PDB reader.
    /// </summary>
    public class PdbSymbol : Symbol
    {
        /// <summary>
        /// Cache for <see cref="HasVTable()"/>.
        /// </summary>
        private bool hasVTable;

        /// <summary>
        /// Type record that represents this symbol or <c>null</c> if it is simple built-in type.
        /// </summary>
        private TypeRecord typeRecord;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSymbol"/> class.
        /// </summary>
        /// <param name="module">Module that contains this symbol.</param>
        protected PdbSymbol(PdbModule module)
            : base(module)
        {
            PdbModule = module;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSymbol"/> class.
        /// </summary>
        /// <param name="module">Module that contains this symbol.</param>
        /// <param name="id">Type identifier.</param>
        /// <param name="typeRecord">Type record that represents this symbol.</param>
        public PdbSymbol(PdbModule module, uint id, TypeRecord typeRecord)
            : base(module)
        {
            PdbModule = module;
            Id = id;
            this.typeRecord = typeRecord;
            if (typeRecord is ClassRecord classRecord)
                Initialize(classRecord);
            else if (typeRecord is UnionRecord unionRecord)
                Initialize(unionRecord);
            else if (typeRecord is EnumRecord enumRecord)
                Initialize(enumRecord);
            else if (typeRecord is BaseClassRecord baseClassRecord)
                Initialize(baseClassRecord);
            else if (typeRecord is VirtualBaseClassRecord virtualBaseClassRecord)
                Initialize(virtualBaseClassRecord);
            else if (typeRecord is PointerRecord pointerRecord)
                Initialize(pointerRecord);
            else if (typeRecord is ProcedureRecord procedureRecord)
                Initialize(procedureRecord);
            else if (typeRecord is ArrayRecord arrayRecord)
                Initialize(arrayRecord);
            else if (typeRecord is ModifierRecord modifierRecord)
                Initialize(modifierRecord);
            else if (typeRecord is MemberFunctionRecord memberFunctionRecord)
                Initialize(memberFunctionRecord);
            else
                throw new NotImplementedException($"Unexpected type record: {typeRecord.Kind}");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSymbol"/> class.
        /// </summary>
        /// <param name="module">Module that contains this symbol.</param>
        /// <param name="typeIndex">Type index of the simple build-in type.</param>
        public PdbSymbol(PdbModule module, TypeIndex typeIndex)
            : base(module)
        {
            PdbModule = module;
            Id = typeIndex.Index;
            Initialize(typeIndex);
        }

        /// <summary>
        /// Initializes data based on the <see cref="ModifierRecord"/>.
        /// </summary>
        private void Initialize(ModifierRecord record)
        {
            PdbSymbol symbol = PdbModule.GetSymbol(record.ModifiedType);
            LinkSymbols(symbol);
            Tag = symbol.Tag;
            BasicType = symbol.BasicType;
            Offset = symbol.Offset;
            Size = symbol.Size;
            IsVirtualInheritance = symbol.IsVirtualInheritance;
            IsForwardReference = symbol.IsForwardReference;
            UniqueName = symbol.UniqueName;
            Name = symbol.Name;
            if ((record.Modifiers & ModifierOptions.Unaligned) != ModifierOptions.None)
                Name = "unaligned " + Name;
            if ((record.Modifiers & ModifierOptions.Volatile) != ModifierOptions.None)
                Name = "volatile " + Name;
            if ((record.Modifiers & ModifierOptions.Const) != ModifierOptions.None)
                Name = "const " + Name;
        }

        /// <summary>
        /// Initializes data based on the <see cref="MemberFunctionRecord"/>.
        /// </summary>
        private void Initialize(MemberFunctionRecord record)
        {
            Tag = Engine.CodeTypeTag.Function;
            BasicType = DIA.BasicType.NoType;
        }

        /// <summary>
        /// Initializes data based on the <see cref="ProcedureRecord"/>.
        /// </summary>
        private void Initialize(ProcedureRecord record)
        {
            Tag = Engine.CodeTypeTag.Function;
            BasicType = DIA.BasicType.NoType;
        }

        /// <summary>
        /// Initializes data based on the <see cref="ArrayRecord"/>.
        /// </summary>
        private void Initialize(ArrayRecord record)
        {
            Tag = Engine.CodeTypeTag.Pointer;
            BasicType = DIA.BasicType.NoType;
            Size = (int)record.Size;
            Name = ElementType.Name + "[]";
        }

        /// <summary>
        /// Initializes data based on the <see cref="PointerRecord"/>.
        /// </summary>
        private void Initialize(PointerRecord record)
        {
            Tag = Engine.CodeTypeTag.Pointer;
            BasicType = DIA.BasicType.NoType;
            Size = record.Size;
            Name = ElementType.Name + "*";
            ElementType.PointerType = this;
        }

        /// <summary>
        /// Initializes data based on the <see cref="ClassRecord"/>.
        /// </summary>
        private void Initialize(ClassRecord record)
        {
            if (record.Kind == TypeLeafKind.LF_CLASS || record.Kind == TypeLeafKind.LF_INTERFACE)
                Tag = Engine.CodeTypeTag.Class;
            else if (record.Kind == TypeLeafKind.LF_STRUCTURE)
                Tag = Engine.CodeTypeTag.Structure;
            else
                throw new NotImplementedException($"Unexpected record kind: {record.Kind}");
            BasicType = DIA.BasicType.NoType;
            Name = record.Name;

            ulong size = record.Size;

            if (size > int.MaxValue)
            {
                throw new ArgumentException("Symbol size is unexpected");
            }
            Size = (int)size;
            hasVTable = record.VirtualTableShape != TypeIndex.None;
            IsForwardReference = record.IsForwardReference;
            UniqueName = record.UniqueName;
        }

        /// <summary>
        /// Initializes data based on the <see cref="UnionRecord"/>.
        /// </summary>
        private void Initialize(UnionRecord record)
        {
            Tag = Engine.CodeTypeTag.Union;
            BasicType = DIA.BasicType.NoType;
            Name = record.Name;

            ulong size = record.Size;

            if (size > int.MaxValue)
            {
                throw new ArgumentException("Symbol size is unexpected");
            }
            Size = (int)size;
            IsForwardReference = record.IsForwardReference;
            UniqueName = record.UniqueName;
        }

        /// <summary>
        /// Initializes data based on the <see cref="EnumRecord"/>.
        /// </summary>
        private void Initialize(EnumRecord record)
        {
            Initialize(record.UnderlyingType);
            Tag = Engine.CodeTypeTag.Enum;
            Name = record.Name;
            IsForwardReference = record.IsForwardReference;
            UniqueName = record.UniqueName;
        }

        /// <summary>
        /// Initializes data based on the <see cref="BaseClassRecord"/>.
        /// </summary>
        private void Initialize(BaseClassRecord record)
        {
            Symbol symbol = PdbModule.GetSymbol(record.Type);
            Offset = (int)record.Offset;
            Tag = Engine.CodeTypeTag.BaseClass;
            BasicType = symbol.BasicType;
            Name = symbol.Name;
            Size = symbol.Size;
            IsVirtualInheritance = false;
        }

        /// <summary>
        /// Initializes data based on the <see cref="VirtualBaseClassRecord"/>.
        /// </summary>
        private void Initialize(VirtualBaseClassRecord record)
        {
            Symbol symbol = PdbModule.GetSymbol(record.BaseType);
            Tag = Engine.CodeTypeTag.BaseClass;
            BasicType = symbol.BasicType;
            Name = symbol.Name;
            Size = symbol.Size;
            IsVirtualInheritance = true;
        }

        /// <summary>
        /// Initializes data based on the simple built-in type.
        /// </summary>
        private void Initialize(TypeIndex typeIndex)
        {
            Name = typeIndex.SimpleTypeName;
            Tag = typeIndex.SimpleMode != SimpleTypeMode.Direct ? Engine.CodeTypeTag.Pointer : Engine.CodeTypeTag.BuiltinType;
            switch (typeIndex.SimpleKind)
            {
                case SimpleTypeKind.None:
                    BasicType = DIA.BasicType.NoType;
                    Size = 0;
                    break;
                case SimpleTypeKind.Void:
                    BasicType = DIA.BasicType.Void;
                    Size = 0;
                    break;
                case SimpleTypeKind.HResult:
                    BasicType = DIA.BasicType.Hresult;
                    Size = 4;
                    break;
                case SimpleTypeKind.NarrowCharacter:
                case SimpleTypeKind.UnsignedCharacter:
                case SimpleTypeKind.SignedCharacter:
                    BasicType = DIA.BasicType.Char;
                    Size = 1;
                    break;
                case SimpleTypeKind.WideCharacter:
                    BasicType = DIA.BasicType.WChar;
                    Size = 2;
                    break;
                case SimpleTypeKind.Character16:
                    BasicType = DIA.BasicType.Char16;
                    Size = 2;
                    break;
                case SimpleTypeKind.Character32:
                    BasicType = DIA.BasicType.Char32;
                    Size = 4;
                    break;
                case SimpleTypeKind.SByte:
                    BasicType = DIA.BasicType.Int;
                    Size = 1;
                    break;
                case SimpleTypeKind.Byte:
                    BasicType = DIA.BasicType.UInt;
                    Size = 1;
                    break;
                case SimpleTypeKind.Int16Short:
                case SimpleTypeKind.Int16:
                    BasicType = DIA.BasicType.Int;
                    Size = 2;
                    break;
                case SimpleTypeKind.UInt16:
                case SimpleTypeKind.UInt16Short:
                    BasicType = DIA.BasicType.UInt;
                    Size = 2;
                    break;
                case SimpleTypeKind.Int32Long:
                case SimpleTypeKind.Int32:
                    BasicType = DIA.BasicType.Int;
                    Size = 4;
                    break;
                case SimpleTypeKind.UInt32Long:
                case SimpleTypeKind.UInt32:
                    BasicType = DIA.BasicType.UInt;
                    Size = 4;
                    break;
                case SimpleTypeKind.Int64Quad:
                case SimpleTypeKind.Int64:
                    BasicType = DIA.BasicType.Long;
                    Size = 8;
                    break;
                case SimpleTypeKind.UInt64Quad:
                case SimpleTypeKind.UInt64:
                    BasicType = DIA.BasicType.ULong;
                    Size = 8;
                    break;
                case SimpleTypeKind.Int128Oct:
                case SimpleTypeKind.Int128:
                    BasicType = DIA.BasicType.Long;
                    Size = 16;
                    break;
                case SimpleTypeKind.UInt128Oct:
                case SimpleTypeKind.UInt128:
                    BasicType = DIA.BasicType.ULong;
                    Size = 16;
                    break;
                case SimpleTypeKind.Float16:
                    BasicType = DIA.BasicType.Float;
                    Size = 2;
                    break;
                case SimpleTypeKind.Float32:
                case SimpleTypeKind.Float32PartialPrecision:
                    BasicType = DIA.BasicType.Float;
                    Size = 4;
                    break;
                case SimpleTypeKind.Float48:
                    BasicType = DIA.BasicType.Float;
                    Size = 6;
                    break;
                case SimpleTypeKind.Float64:
                    BasicType = DIA.BasicType.Float;
                    Size = 8;
                    break;
                case SimpleTypeKind.Float80:
                    BasicType = DIA.BasicType.Float;
                    Size = 10;
                    break;
                case SimpleTypeKind.Float128:
                    BasicType = DIA.BasicType.Float;
                    Size = 12;
                    break;
                case SimpleTypeKind.Complex16:
                    BasicType = DIA.BasicType.Complex;
                    Size = 2;
                    break;
                case SimpleTypeKind.Complex32:
                case SimpleTypeKind.Complex32PartialPrecision:
                    BasicType = DIA.BasicType.Complex;
                    Size = 4;
                    break;
                case SimpleTypeKind.Complex48:
                    BasicType = DIA.BasicType.Complex;
                    Size = 6;
                    break;
                case SimpleTypeKind.Complex64:
                    BasicType = DIA.BasicType.Complex;
                    Size = 8;
                    break;
                case SimpleTypeKind.Complex80:
                    BasicType = DIA.BasicType.Complex;
                    Size = 10;
                    break;
                case SimpleTypeKind.Complex128:
                    BasicType = DIA.BasicType.Complex;
                    Size = 16;
                    break;
                case SimpleTypeKind.Boolean8:
                    BasicType = DIA.BasicType.Bool;
                    Size = 1;
                    break;
                case SimpleTypeKind.Boolean16:
                    BasicType = DIA.BasicType.Bool;
                    Size = 2;
                    break;
                case SimpleTypeKind.Boolean32:
                    BasicType = DIA.BasicType.Bool;
                    Size = 4;
                    break;
                case SimpleTypeKind.Boolean64:
                    BasicType = DIA.BasicType.Bool;
                    Size = 8;
                    break;
                case SimpleTypeKind.Boolean128:
                    BasicType = DIA.BasicType.Bool;
                    Size = 16;
                    break;
                case SimpleTypeKind.NotTranslated:
                    BasicType = DIA.BasicType.NoType;
                    Size = 0;
                    break;
                default:
                    throw new NotImplementedException($"Unexpected simple type: {typeIndex.SimpleKind}, from type index: {typeIndex}");
            }
            if (typeIndex.SimpleMode != SimpleTypeMode.Direct)
            {
                ElementType = PdbModule.GetSymbol(new TypeIndex(typeIndex.SimpleKind));
                ElementType.PointerType = this;
            }
        }

        /// <summary>
        /// Gets the module that contains this symbol.
        /// </summary>
        public PdbModule PdbModule { get; private set; }

        /// <summary>
        /// <c>true</c> if this symbol represents forward reference.
        /// </summary>
        public bool IsForwardReference { get; private set; }

        /// <summary>
        /// Gets the unique name from PDB if available.
        /// </summary>
        public string UniqueName { get; private set; }

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public override bool HasVTable()
        {
            return hasVTable;
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public override void InitializeCache()
        {
            if (Tag == Engine.CodeTypeTag.Class || Tag == Engine.CodeTypeTag.ModuleGlobals || Tag == Engine.CodeTypeTag.Structure || Tag == Engine.CodeTypeTag.Union)
            {
                var fields = Fields;
            }
            if (Tag == Engine.CodeTypeTag.Class || Tag == Engine.CodeTypeTag.Structure)
            {
                var baseClasses = BaseClasses;
            }
            if (Tag == Engine.CodeTypeTag.Pointer || Tag == Engine.CodeTypeTag.Array)
            {
                var elementType = ElementType;
            }
            //var pointerType = PointerType;
            if (Tag == Engine.CodeTypeTag.Enum)
            {
                var enumValues = EnumValues;
                var enumValuesByValue = EnumValuesByValue;
            }
            var namespaces = Namespaces;
        }

        /// <summary>
        /// Gets user type base classes.
        /// </summary>
        protected override IEnumerable<Symbol> GetBaseClasses()
        {
            if (typeRecord is ClassRecord classRecord)
            {
                if (classRecord.DerivationList != TypeIndex.None)
                {
                    TypeRecord baseClasses = PdbModule.PdbFile.TpiStream[classRecord.DerivationList];

                    throw new NotImplementedException();
                }
                else if (classRecord.FieldList != TypeIndex.None)
                {
                    foreach (TypeRecord field in EnumerateFieldList(classRecord.FieldList))
                        if (field is BaseClassRecord || field is VirtualBaseClassRecord)
                            yield return PdbModule.GetSymbol(field);
                }
            }
        }

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected override Symbol GetElementType()
        {
            if (typeRecord is ArrayRecord arrayRecord)
                return PdbModule.GetSymbol(arrayRecord.ElementType);
            if (typeRecord is PointerRecord pointerRecord)
                return PdbModule.GetSymbol(pointerRecord.ReferentType);
            return null;
        }

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        protected override IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            if (typeRecord is EnumRecord enumRecord)
                foreach (TypeRecord field in EnumerateFieldList(enumRecord.FieldList))
                    if (field is EnumeratorRecord enumField)
                        yield return Tuple.Create(enumField.Name, enumField.Value.ToString());
        }

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected override IEnumerable<SymbolField> GetFields()
        {
            TypeIndex fieldListIndex = TypeIndex.None;

            if (typeRecord is ClassRecord classRecord)
                fieldListIndex = classRecord.FieldList;
            else if (typeRecord is UnionRecord unionRecord)
                fieldListIndex = unionRecord.FieldList;

            foreach (TypeRecord field in EnumerateFieldList(fieldListIndex))
                if (field is DataMemberRecord dataMember)
                    yield return new PdbSymbolField(this, dataMember);
                else if (field is StaticDataMemberRecord staticDataMember)
                    yield return new PdbSymbolField(this, staticDataMember);
        }

        /// <summary>
        /// Enumerates type records from field list type index.
        /// </summary>
        /// <param name="fieldListIndex">Type index of the field list type record.</param>
        private IEnumerable<TypeRecord> EnumerateFieldList(TypeIndex fieldListIndex)
        {
            while (fieldListIndex != TypeIndex.None)
            {
                TypeIndex nextFieldListIndex = TypeIndex.None;
                TypeRecord fieldList = PdbModule.PdbFile.TpiStream[fieldListIndex];
                if (fieldList is FieldListRecord fieldListRecord)
                    foreach (TypeRecord field in fieldListRecord.Fields)
                        if (field is ListContinuationRecord listContinuation)
                            nextFieldListIndex = listContinuation.ContinuationIndex;
                        else
                            yield return field;
                fieldListIndex = nextFieldListIndex;
            }
        }
    }
}
