using Dia2Lib;
using System;
using System.Collections.Generic;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Interface represents symbol during debugging.
    /// </summary>
    internal interface ISymbol
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Gets the module.
        /// </summary>
        IModule Module { get; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// Gets the tag.
        /// </summary>
        SymTagEnum Tag { get; }

        /// <summary>
        /// Gets the basic type.
        /// </summary>
        BasicType BasicType { get; }

        /// <summary>
        /// Gets or sets the user type associated with this symbol.
        /// </summary>
        UserType UserType { get; set; }

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        List<string> Namespaces { get; }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        ISymbol ElementType { get; }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        ISymbolField[] Fields { get; }

        /// <summary>
        /// Gets the base classes.
        /// </summary>
        ISymbol[] BaseClasses { get; }

        /// <summary>
        /// Gets the pointer type.
        /// </summary>
        ISymbol PointerType { get; }

        /// <summary>
        /// Gets a value indicating whether this symbol is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this symbol is empty; otherwise, <c>false</c>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the enum values.
        /// </summary>
        IEnumerable<Tuple<string, string>> GetEnumValues();

        /// <summary>
        /// Casts as symbol field.
        /// </summary>
        ISymbolField CastAsSymbolField();

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        void InitializeCache();

        /// <summary>
        /// Extracts the dependent symbols into extractedSymbols if they are not recognized as transformations.
        /// </summary>
        /// <param name="extractedSymbols">The extracted symbols.</param>
        /// <param name="transformations">The transformations.</param>
        void ExtractDependantSymbols(HashSet<ISymbol> extractedSymbols, XmlTypeTransformation[] transformations);

        /// <summary>
        /// Links the symbols.
        /// </summary>
        void LinkSymbols(ISymbol s);

        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        bool HasVTable();

        /// <summary>
        /// Gets all base classes (including base classes of base classes).
        /// </summary>
        IEnumerable<ISymbol> GetAllBaseClasses();
    }
}
