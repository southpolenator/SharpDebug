using SharpDebug.CLR;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpDebug.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of the <see cref="IClrType"/>.
    /// </summary>
    internal class VSClrType : IClrType
    {
        /// <summary>
        /// Helper structure to hold simple cache data.
        /// </summary>
        private struct SimpleData
        {
            public int BaseSize;
            public int BaseTypeId;
            public int ComponentTypeId;
            public int ElementSize;
            public int ElementType;
            public bool HasSimpleValue;
            public bool IsArray;
            public bool IsEnum;
            public bool IsObjectReference;
            public bool IsPointer;
            public bool IsPrimitive;
            public bool IsString;
            public bool IsValueClass;
            public string Name;
        }

        /// <summary>
        /// The module cache.
        /// </summary>
        private SimpleCache<VSClrModule> moduleCache;

        /// <summary>
        /// The data cache.
        /// </summary>
        private SimpleCache<SimpleData> dataCache;

        /// <summary>
        /// The fields cache.
        /// </summary>
        private SimpleCache<VSClrInstanceField[]> fieldsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSClrType"/> class.
        /// </summary>
        /// <param name="runtime">The owning runtime.</param>
        /// <param name="id">The type identifier.</param>
        public VSClrType(VSClrRuntime runtime, int id)
        {
            Runtime = runtime;
            Id = id;
            moduleCache = SimpleCache.Create(() => Runtime.GetModule(Proxy.GetClrTypeModule(Runtime.Process.Id, id)));
            dataCache = SimpleCache.Create(() =>
            {
                Tuple<int, int, int, int, int, int, string> tuple = Proxy.GetClrTypeSimpleData(Runtime.Process.Id, id);

                return new SimpleData
                {
                    BaseSize = tuple.Item1,
                    BaseTypeId = tuple.Item2,
                    ComponentTypeId = tuple.Item3,
                    ElementSize = tuple.Item4,
                    ElementType = tuple.Item5,
                    HasSimpleValue = (tuple.Item6 & 1) != 0,
                    IsArray = (tuple.Item6 & 2) != 0,
                    IsEnum = (tuple.Item6 & 4) != 0,
                    IsObjectReference = (tuple.Item6 & 8) != 0,
                    IsPointer = (tuple.Item6 & 16) != 0,
                    IsPrimitive = (tuple.Item6 & 32) != 0,
                    IsString = (tuple.Item6 & 64) != 0,
                    IsValueClass = (tuple.Item6 & 128) != 0,
                    Name = tuple.Item7,
                };
            });
            fieldsCache = SimpleCache.Create(() =>
            {
                Tuple<string, int, int, int>[] fieldTuples = Proxy.GetClrTypeFields(Runtime.Process.Id, id);
                VSClrInstanceField[] fields = new VSClrInstanceField[fieldTuples.Length];

                for (int i = 0; i < fields.Length; i++)
                    fields[i] = new VSClrInstanceField(fieldTuples[i].Item1, Runtime.GetClrType(fieldTuples[i].Item2), fieldTuples[i].Item3, fieldTuples[i].Item4);
                return fields;
            });
        }

        /// <summary>
        /// Gets the Visual Studio implementation of the runtime.
        /// </summary>
        public VSClrRuntime Runtime { get; private set; }

        /// <summary>
        /// Gets the Visual Studio debugger proxy.
        /// </summary>
        public VSDebuggerProxy Proxy => Runtime.Proxy;

        /// <summary>
        /// Gets the type identifier.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the base size of the object.
        /// </summary>
        public int BaseSize => dataCache.Value.BaseSize;

        /// <summary>
        /// Gets the base type that this type inherits. Can return null if it does not inherit (or is unknown)
        /// </summary>
        public IClrType BaseType => Runtime.GetClrType(dataCache.Value.BaseTypeId);

        /// <summary>
        /// Gets the type of the element referenced by the pointer.
        /// </summary>
        public IClrType ComponentType => Runtime.GetClrType(dataCache.Value.ComponentTypeId);

        /// <summary>
        /// Gets the size of individual elements of an array.
        /// </summary>
        public int ElementSize => dataCache.Value.ElementSize;

        /// <summary>
        /// Gets the <see cref="T:SharpDebug.CLR.ClrElementType" />.
        /// </summary>
        public ClrElementType ElementType
        {
            get
            {
                switch ((Microsoft.Diagnostics.Runtime.ClrElementType)dataCache.Value.ElementType)
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
        public IEnumerable<IClrInstanceField> Fields => fieldsCache.Value;

        /// <summary>
        /// Gets a value indicating whether this instance of this type has a simple value.
        /// </summary>
        /// <value>
        /// <c>true</c> if instance of this type has a simple value; otherwise, <c>false</c>.
        /// </value>
        public bool HasSimpleValue => dataCache.Value.HasSimpleValue;

        /// <summary>
        /// Gets a value indicating whether this instance is array.
        /// A type is an array if you can use the array operators below,
        /// Abstractly arrays are objects that whose children are not statically known by just knowing the type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is array; otherwise, <c>false</c>.
        /// </value>
        public bool IsArray => dataCache.Value.IsArray;

        /// <summary>
        /// Gets a value indicating whether this instance is enum.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enum; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnum => dataCache.Value.IsEnum;

        /// <summary>
        /// Gets a value indicating whether this type is object reference.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this type is object reference; otherwise, <c>false</c>.
        /// </value>
        public bool IsObjectReference => dataCache.Value.IsObjectReference;

        /// <summary>
        /// Gets a value indicating whether this instance is pointer.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is pointer; otherwise, <c>false</c>.
        /// </value>
        public bool IsPointer => dataCache.Value.IsPointer;

        /// <summary>
        /// Gets a value indicating whether this instance is primitive (int, float, etc).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is primitive; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimitive => dataCache.Value.IsPrimitive;

        /// <summary>
        /// Gets a value indicating whether this type is System.String.
        /// </summary>
        /// <value>
        ///   <c>true</c> if type is System.String; otherwise, <c>false</c>.
        /// </value>
        public bool IsString => dataCache.Value.IsString;

        /// <summary>
        /// Gets a value indicating whether this instance is value class (struct).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is value class; otherwise, <c>false</c>.
        /// </value>
        public bool IsValueClass => dataCache.Value.IsValueClass;

        /// <summary>
        /// Gets the module this type is defined in.
        /// </summary>
        public IClrModule Module => moduleCache.Value;

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => dataCache.Value.Name;

        /// <summary>
        /// Gets the heap. All types know the heap they belong to.
        /// </summary>
        public IClrHeap Heap => Runtime.Heap;

        /// <summary>
        /// Gets the absolute address to the given array element.
        /// </summary>
        /// <param name="address">The object address.</param>
        /// <param name="index">The array index.</param>
        public ulong GetArrayElementAddress(ulong address, int index)
        {
            return Proxy.GetClrTypeArrayElementAddress(Runtime.Process.Id, Id, address, index);
        }

        /// <summary>
        /// Gets the length of the array (number of elements in the array). Undefined behavior if this type is not an array.
        /// </summary>
        /// <param name="address">The address.</param>
        public int GetArrayLength(ulong address)
        {
            try
            {
                return Runtime.Process.ReadInt(address + Runtime.Process.GetPointerSize());
            }
            catch
            {
                return Proxy.GetClrTypeArrayLength(Runtime.Process.Id, Id, address);
            }
        }

        /// <summary>
        /// Gets the field given by 'name', case sensitive. Returns null if no such field name exists (or on error).
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        public IClrStaticField GetStaticFieldByName(string variableName)
        {
            int fieldTypeId = Proxy.GetClrTypeStaticField(Runtime.Process.Id, Id, variableName);
            VSClrType fieldType = Runtime.GetClrType(fieldTypeId);

            if (fieldType == null)
                return null;
            return new VSClrStaticField(this, variableName, fieldType);
        }
    }
}
