using Dia2Lib;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Interface represents symbol field during debugging.
    /// </summary>
    internal interface ISymbolField
    {
        /// <summary>
        /// Gets a value indicating whether this instance is valid static field.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid static field; otherwise, <c>false</c>.
        /// </value>
        bool IsValidStatic { get; }

        /// <summary>
        /// Gets a value indicating whether this field is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field is static; otherwise, <c>false</c>.
        /// </value>
        bool IsStatic { get; }

        /// <summary>
        /// Gets the module.
        /// </summary>
        IModule Module { get; }

        /// <summary>
        /// Gets the parent type.
        /// </summary>
        ISymbol ParentType { get; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        ISymbol Type { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        string PropertyName { get; set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// Gets the bit position.
        /// </summary>
        int BitPosition { get; }

        /// <summary>
        /// Gets the type of the location.
        /// </summary>
        LocationType LocationType { get; }

        /// <summary>
        /// Gets the data kind.
        /// </summary>
        DataKind DataKind { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        dynamic Value { get; }
    }
}
