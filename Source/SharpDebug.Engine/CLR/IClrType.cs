using System.Collections.Generic;

namespace SharpDebug.CLR
{
    /// <summary>
    /// CLR code type interface. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public interface IClrType
    {
        /// <summary>
        /// Gets the base size of the object.
        /// </summary>
        int BaseSize { get; }

        /// <summary>
        /// Gets the base type that this type inherits. Can return null if it does not inherit (or is unknown)
        /// </summary>
        IClrType BaseType { get; }

        /// <summary>
        /// Gets the type of the element referenced by the pointer.
        /// </summary>
        IClrType ComponentType { get; }

        /// <summary>
        /// Gets the size of individual elements of an array.
        /// </summary>
        int ElementSize { get; }

        /// <summary>
        /// Gets the <see cref="ClrElementType"/>.
        /// </summary>
        ClrElementType ElementType { get; }

        /// <summary>
        /// Gets all possible fields in this type. It does not return dynamically typed fields. Returns an empty list if there are no fields.
        /// </summary>
        IEnumerable<IClrInstanceField> Fields { get; }

        /// <summary>
        /// Gets a value indicating whether this instance of this type has a simple value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if instance of this type has a simple value; otherwise, <c>false</c>.
        /// </value>
        bool HasSimpleValue { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is array.
        /// A type is an array if you can use the array operators below,
        /// Abstractly arrays are objects that whose children are not statically known by just knowing the type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is array; otherwise, <c>false</c>.
        /// </value>
        bool IsArray { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is enum.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enum; otherwise, <c>false</c>.
        /// </value>
        bool IsEnum { get; }

        /// <summary>
        /// Gets a value indicating whether this type is object reference.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is object reference; otherwise, <c>false</c>.
        /// </value>
        bool IsObjectReference { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is pointer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is pointer; otherwise, <c>false</c>.
        /// </value>
        bool IsPointer { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is primitive (int, float, etc).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is primitive; otherwise, <c>false</c>.
        /// </value>
        bool IsPrimitive { get; }

        /// <summary>
        /// Gets a value indicating whether this type is System.String.
        /// </summary>
        /// <value>
        ///   <c>true</c> if type is System.String; otherwise, <c>false</c>.
        /// </value>
        bool IsString { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is value class (struct).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is value class; otherwise, <c>false</c>.
        /// </value>
        bool IsValueClass { get; }

        /// <summary>
        /// Gets the module this type is defined in.
        /// </summary>
        IClrModule Module { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the heap. All types know the heap they belong to.
        /// </summary>
        IClrHeap Heap { get; }

        /// <summary>
        /// Gets the field given by 'name', case sensitive. Returns null if no such field name exists (or on error).
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns></returns>
        IClrStaticField GetStaticFieldByName(string variableName);

        /// <summary>
        /// Gets the length of the array (number of elements in the array). Undefined behavior if this type is not an array.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        int GetArrayLength(ulong address);

        /// <summary>
        /// Gets the absolute address to the given array element.
        /// </summary>
        /// <param name="address">The object address.</param>
        /// <param name="index">The array index.</param>
        ulong GetArrayElementAddress(ulong address, int index);
    }
}
