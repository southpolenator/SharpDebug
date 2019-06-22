using CsDebugScript.CodeGen.SymbolProviders;
using SharpPdb.Windows.SymbolRecords;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript.PdbSymbolProvider
{
    using ConstantSymbol = SharpPdb.Windows.SymbolRecords.ConstantSymbol;

    /// <summary>
    /// Represents global scope symbol for PDB reader.
    /// </summary>
    public class PdbGlobalScope : PdbSymbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbGlobalScope"/> class.
        /// </summary>
        /// <param name="module">Module that contains this symbol.</param>
        public PdbGlobalScope(PdbModule module)
            : base(module)
        {
            Tag = Engine.CodeTypeTag.ModuleGlobals;
            BasicType = DIA.BasicType.Void;
            Name = string.Empty;
            Id = uint.MaxValue;
        }

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public override bool HasVTable()
        {
            return false;
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public override void InitializeCache()
        {
            var fields = Fields;
        }

        /// <summary>
        /// Gets user type base classes.
        /// </summary>
        protected override IEnumerable<Symbol> GetBaseClasses()
        {
            yield break;
        }

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected override Symbol GetElementType()
        {
            // This should never happen
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        protected override IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            // This should never happen
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected override IEnumerable<SymbolField> GetFields()
        {
            // Add constants
            foreach (SymbolRecordKind kind in ConstantSymbol.Kinds)
                foreach (ConstantSymbol constant in PdbModule.PdbFile.PdbSymbolStream[kind].OfType<ConstantSymbol>())
                    yield return new PdbSymbolField(this, constant);

            // Add global variables
            foreach (SymbolRecordKind kind in DataSymbol.Kinds)
                foreach (DataSymbol data in PdbModule.PdbFile.PdbSymbolStream[kind].OfType<DataSymbol>())
                    yield return new PdbSymbolField(this, data);
        }

        /// <summary>
        /// Gets the pointer type to this symbol.
        /// </summary>
        protected override Symbol GetPointerType()
        {
            // This should never happen
            throw new NotImplementedException();
        }
    }
}
