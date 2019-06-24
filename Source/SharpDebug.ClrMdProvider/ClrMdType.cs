using System;
using System.Collections.Generic;
using SharpDebug.CLR;

namespace SharpDebug.ClrMdProvider
{
    /// <summary>
    /// ClrMD implementation of the <see cref="IClrType"/>.
    /// </summary>
    internal class ClrMdType : IClrType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdType"/> class.
        /// </summary>
        /// <param name="provider">The CLR provider.</param>
        /// <param name="clrType">The CLR type.</param>
        public ClrMdType(CLR.ClrMdProvider provider, Microsoft.Diagnostics.Runtime.ClrType clrType)
        {
            Provider = provider;
            ClrType = clrType;
        }

        /// <summary>
        /// Gets the base size of the object.
        /// </summary>
        public int BaseSize => ClrType.BaseSize;

        /// <summary>
        /// Gets the base type that this type inherits. Can return null if it does not inherit (or is unknown)
        /// </summary>
        public IClrType BaseType => Provider.FromClrType(ClrType.BaseType);

        /// <summary>
        /// Gets the type of the element referenced by the pointer.
        /// </summary>
        public IClrType ComponentType => Provider.FromClrType(ClrType.ComponentType);

        /// <summary>
        /// Gets the size of individual elements of an array.
        /// </summary>
        public int ElementSize => ClrType.ElementSize;

        /// <summary>
        /// Gets the <see cref="T:SharpDebug.CLR.ClrElementType" />.
        /// </summary>
        public ClrElementType ElementType
        {
            get
            {
                switch (ClrType.ElementType)
                {
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Array:
                        return ClrElementType.Array;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Boolean:
                        return ClrElementType.Boolean;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Char:
                        return ClrElementType.Char;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Class:
                        return ClrElementType.Class;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Double:
                        return ClrElementType.Double;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Float:
                        return ClrElementType.Float;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.FunctionPointer:
                        return ClrElementType.FunctionPointer;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Int16:
                        return ClrElementType.Int16;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Int32:
                        return ClrElementType.Int32;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Int64:
                        return ClrElementType.Int64;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Int8:
                        return ClrElementType.Int8;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.NativeInt:
                        return ClrElementType.NativeInt;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.NativeUInt:
                        return ClrElementType.NativeUInt;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Object:
                        return ClrElementType.Object;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Pointer:
                        return ClrElementType.Pointer;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.String:
                        return ClrElementType.String;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.Struct:
                        return ClrElementType.Struct;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.UInt16:
                        return ClrElementType.UInt16;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.UInt32:
                        return ClrElementType.UInt32;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.UInt64:
                        return ClrElementType.UInt64;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.UInt8:
                        return ClrElementType.UInt8;
                    case Microsoft.Diagnostics.Runtime.ClrElementType.SZArray:
                        return ClrElementType.SZArray;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Gets all possible fields in this type. It does not return dynamically typed fields. Returns an empty list if there are no fields.
        /// </summary>
        public IEnumerable<IClrInstanceField> Fields
        {
            get
            {
                foreach (var field in ClrType.Fields)
                    yield return new ClrMdInstanceField(Provider, field);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance of this type has a simple value.
        /// </summary>
        /// <value>
        /// <c>true</c> if instance of this type has a simple value; otherwise, <c>false</c>.
        /// </value>
        public bool HasSimpleValue => ClrType.HasSimpleValue;

        /// <summary>
        /// Gets the heap. All types know the heap they belong to.
        /// </summary>
        public IClrHeap Heap => Provider.FromClrHeap(ClrType.Heap);

        /// <summary>
        /// Gets a value indicating whether this instance is array.
        /// A type is an array if you can use the array operators below,
        /// Abstractly arrays are objects that whose children are not statically known by just knowing the type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is array; otherwise, <c>false</c>.
        /// </value>
        public bool IsArray => ClrType.IsArray;

        /// <summary>
        /// Gets a value indicating whether this instance is enum.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enum; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnum => ClrType.IsEnum;

        /// <summary>
        /// Gets a value indicating whether this type is object reference.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is object reference; otherwise, <c>false</c>.
        /// </value>
        public bool IsObjectReference => ClrType.IsObjectReference;

        /// <summary>
        /// Gets a value indicating whether this instance is pointer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is pointer; otherwise, <c>false</c>.
        /// </value>
        public bool IsPointer => ClrType.IsPointer;

        /// <summary>
        /// Gets a value indicating whether this instance is primitive (int, float, etc).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is primitive; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimitive => ClrType.IsPrimitive;

        /// <summary>
        /// Gets a value indicating whether this type is System.String.
        /// </summary>
        /// <value>
        ///   <c>true</c> if type is System.String; otherwise, <c>false</c>.
        /// </value>
        public bool IsString => ClrType.IsString;

        /// <summary>
        /// Gets a value indicating whether this instance is value class (struct).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is value class; otherwise, <c>false</c>.
        /// </value>
        public bool IsValueClass => ClrType.IsValueClass;

        /// <summary>
        /// Gets the module this type is defined in.
        /// </summary>
        public IClrModule Module => Provider.FromClrModule(ClrType.Module);

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => ClrType.Name;

        /// <summary>
        /// Gets the CLR provider.
        /// </summary>
        internal CLR.ClrMdProvider Provider { get; private set; }

        /// <summary>
        /// Gets the CLR type.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrType ClrType { get; private set; }

        /// <summary>
        /// Gets the absolute address to the given array element.
        /// </summary>
        /// <param name="address">The object address.</param>
        /// <param name="index">The array index.</param>
        public ulong GetArrayElementAddress(ulong address, int index)
        {
            return ClrType.GetArrayElementAddress(address, index);
        }

        /// <summary>
        /// Gets the length of the array (number of elements in the array). Undefined behavior if this type is not an array.
        /// </summary>
        /// <param name="address">The address.</param>
        public int GetArrayLength(ulong address)
        {
            return ClrType.GetArrayLength(address);
        }

        /// <summary>
        /// Gets the field given by 'name', case sensitive. Returns null if no such field name exists (or on error).
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        public IClrStaticField GetStaticFieldByName(string variableName)
        {
            Microsoft.Diagnostics.Runtime.ClrStaticField staticField = ClrType.GetStaticFieldByName(variableName);

            if (staticField != null)
                return new ClrMdStaticField(Provider, staticField);
            return null;
        }
    }
}
