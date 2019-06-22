﻿using System;

namespace SharpDebug.CodeGen.TypeInstances
{
    /// <summary>
    /// Type instance that represents array type.
    /// </summary>
    /// <seealso cref="TypeInstance" />
    internal class ArrayTypeInstance : TypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeInstance"/> class.
        /// </summary>
        /// <param name="elementType">The element type instance.</param>
        public ArrayTypeInstance(TypeInstance elementType)
            : base(elementType.CodeNaming)
        {
            ElementType = elementType;
        }

        /// <summary>
        /// Gets the array element type instance.
        /// </summary>
        public TypeInstance ElementType { get; private set; }

        /// <summary>
        /// Flag that represents if physical user type changed this array into .NET built-in array instead of CodeArray wrapper.
        /// </summary>
        public bool IsPhysical { get; set; }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            string elementTypeString = ElementType.GetTypeString(truncateNamespace);

            if (IsPhysical)
                return $"{elementTypeString}[]";
            return $"{CodeNaming.ToString(typeof(CodeArray))}<{elementTypeString}>";
        }

        /// <summary>
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public override Type GetType(ITypeConverter typeConverter)
        {
            Type elementType = ElementType.GetType(typeConverter);

            if (IsPhysical)
                return elementType.MakeArrayType();
            return typeof(CodeArray<>).MakeGenericType(elementType);
        }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return ElementType.ContainsUndefinedType();
        }
    }
}
