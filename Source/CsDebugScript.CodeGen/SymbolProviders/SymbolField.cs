using Dia2Lib;

namespace CsDebugScript.CodeGen.SymbolProviders
{
    /// <summary>
    /// Interface represents symbol field during debugging.
    /// </summary>
    public abstract class SymbolField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolField"/> class.
        /// </summary>
        /// <param name="parentType">The parent type.</param>
        public SymbolField(Symbol parentType)
        {
            ParentType = parentType;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid static field.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid static field; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsValidStatic { get; }

        /// <summary>
        /// Gets a value indicating whether this field is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field is static; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsStatic
        {
            get
            {
                return LocationType == LocationType.Static;
            }
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        public Module Module
        {
            get
            {
                return ParentType.Module;
            }
        }

        /// <summary>
        /// Gets the parent type.
        /// </summary>
        public Symbol ParentType { get; private set; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public Symbol Type { get; protected set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public int Offset { get; protected set; }

        /// <summary>
        /// Gets the bit position.
        /// </summary>
        public int BitPosition { get; protected set; }

        /// <summary>
        /// Gets the type of the location.
        /// </summary>
        public LocationType LocationType { get; protected set; }

        /// <summary>
        /// Gets the data kind.
        /// </summary>
        public DataKind DataKind { get; protected set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public dynamic Value { get; protected set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        internal string PropertyName { get; set; }
    }
}
