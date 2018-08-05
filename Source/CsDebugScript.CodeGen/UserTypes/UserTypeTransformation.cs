using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.Engine.Utility;
using System;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class represents transformation done on the symbol that should convert it to user type.
    /// </summary>
    internal class UserTypeTransformation
    {
        /// <summary>
        /// The type converter
        /// </summary>
        private Func<string, string> typeConverter;

        /// <summary>
        /// The owner user type (for providing more info like class name).
        /// </summary>
        private UserType ownerUserType;

        /// <summary>
        /// The type that should be transformed to user type.
        /// </summary>
        private Symbol type;

        /// <summary>
        /// Lazy cache for the <see cref="TypeString"/> property.
        /// </summary>
        private SimpleCacheStruct<string> typeStringCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeTransformation"/> class.
        /// </summary>
        /// <param name="transformation">The XML transformation definition.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <param name="ownerUserType">The owner user type.</param>
        /// <param name="type">The type that should be transformed to user type.</param>
        public UserTypeTransformation(XmlTypeTransformation transformation, Func<string, string> typeConverter, UserType ownerUserType, Symbol type)
        {
            Transformation = transformation;
            this.typeConverter = typeConverter;
            this.ownerUserType = ownerUserType;
            this.type = type;
            typeStringCache = SimpleCache.CreateStruct(() => Transformation.TransformType(type.Name, ownerUserType.TypeName, typeConverter));
        }

        /// <summary>
        /// Gets the XML transformation definition.
        /// </summary>
        public XmlTypeTransformation Transformation { get; private set; }

        /// <summary>
        /// Gets the type string of this transformation.
        /// </summary>
        public string TypeString => typeStringCache.Value;

        /// <summary>
        /// Transforms the constructor based on field variable and field offset.
        /// </summary>
        /// <param name="simpleFieldValue">The simple field value.</param>
        /// <param name="fieldOffset">The field offset.</param>
        internal string TransformConstructor(string simpleFieldValue, string fieldOffset)
        {
            string originalFieldTypeString = type.Name;

            return Transformation.TransformConstructor(originalFieldTypeString, simpleFieldValue, fieldOffset, ownerUserType.TypeName, typeConverter);
        }
    }
}
