using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Reflection.Emit;

namespace CsScripts
{
    /// <summary>
    /// Wrapper class that represents a "static" array. For example "int a[4]";
    /// </summary>
    /// <typeparam name="T">The type of elements in the array</typeparam>
    public class CodeArray<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// The actual variable where we get all the values.
        /// </summary>
        private Variable variable;

        /// <summary>
        /// The pre-calculated array (if we were initialized with it, or we know how to read whole array)
        /// </summary>
        private IReadOnlyList<T> preCalculatedArray;

        /// <summary>
        /// The addresses array (if we don't know how to read the array, but we know that we have array of pointer and we could optimize a bit)
        /// </summary>
        private ulong[] addressesArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodeArray(Variable variable)
        {
            if (!variable.GetCodeType().IsArray)
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            Initialize(variable, variable.GetArrayLength());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        public CodeArray(Variable variable, int length)
        {
            if (!variable.GetCodeType().IsArray && !variable.GetCodeType().IsPointer)
            {
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }

            Initialize(variable, length);
        }

        /// <summary>
        /// Initializes this instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="length">The array length.</param>
        private void Initialize(Variable variable, int length)
        {
            this.variable = variable;
            Length = length;
            preCalculatedArray = ReadArray();
            if (preCalculatedArray == null && variable.GetCodeType().ElementType.IsPointer)
            {
                var process = variable.GetCodeType().Module.Process;
                var pointerSize = process.GetPointerSize();
                var buffer = Debugger.ReadMemory(process, variable.GetPointerAddress(), (uint)Length * pointerSize);

                addressesArray = UserType.ReadPointerArray(buffer, 0, Length, pointerSize);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeArray{T}"/> class.
        /// </summary>
        /// <param name="preCalculatedArray">The pre-calculated array.</param>
        public CodeArray(T[] preCalculatedArray)
        {
            this.preCalculatedArray = preCalculatedArray;
            Length = preCalculatedArray.Length;
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return Length;
            }
        }

        /// <summary>
        /// Gets the &lt;T&gt; at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public T this[int index]
        {
            get
            {
                if (preCalculatedArray != null)
                {
                    return preCalculatedArray[index];
                }

                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Index out of array length");
                }

                Variable item;

                if (addressesArray != null)
                {
                    item = addressesArray[index] == 0 ? null : Variable.CreatePointerNoCast(variable.GetCodeType().ElementType, addressesArray[index]);
                }
                else
                {
                    item = variable.GetArrayElement(index);
                }

                if (item == null)
                {
                    return default(T);
                }

                return item.CastAs<T>();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        /// <summary>
        /// Enumerates this array.
        /// </summary>
        private IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }

        private delegate T TypeConstructor(Variable variable, byte[] buffer, int offset, ulong bufferAddress);

        private IReadOnlyList<T> ReadArray()
        {
            var elementType = variable.GetCodeType().ElementType;
            var type = typeof(T);

            if (!elementType.IsPointer)
            {
                if (type.IsSubclassOf(typeof(UserType)))
                {
                    var process = variable.GetCodeType().Module.Process;

                    // Verify that CodeType for this user type is exactly elementType
                    var description = process.TypeToUserTypeDescription[type];
                    CodeType newType = description.UserType;

                    if (newType == elementType)
                    {
                        // Find constructor that has 4 arguments:
                        // Variable variable, byte[] buffer, int offset, ulong bufferAddress
                        var constructors = type.GetConstructors();
                        TypeConstructor activator = null;

                        foreach (var constructor in constructors)
                        {
                            if (!constructor.IsPublic)
                            {
                                continue;
                            }

                            var parameters = constructor.GetParameters();

                            if (parameters.Length < 4 || parameters.Count(p => !p.HasDefaultValue) > 4)
                            {
                                continue;
                            }

                            if (parameters[0].ParameterType == typeof(Variable)
                                && parameters[1].ParameterType == typeof(byte[])
                                && parameters[2].ParameterType == typeof(int)
                                && parameters[3].ParameterType == typeof(ulong))
                            {
                                DynamicMethod method = new DynamicMethod("CreateIntance", type, new Type[] { typeof(Variable), typeof(byte[]), typeof(int), typeof(ulong) });
                                ILGenerator gen = method.GetILGenerator();

                                gen.Emit(OpCodes.Ldarg_0);
                                gen.Emit(OpCodes.Ldarg_1);
                                gen.Emit(OpCodes.Ldarg_2);
                                gen.Emit(OpCodes.Ldarg_3);
                                gen.Emit(OpCodes.Newobj, constructor);
                                gen.Emit(OpCodes.Ret);
                                activator = (TypeConstructor)method.CreateDelegate(typeof(TypeConstructor));
                                break;
                            }
                        }

                        if (activator != null)
                        {
                            var address = variable.GetPointerAddress();

                            return new ElementCreatorReadOnlyList(activator, elementType, address);
                        }
                    }
                }
            }

            return null;
        }

        private class ElementCreatorReadOnlyList : IReadOnlyList<T>
        {
            private TypeConstructor activator;
            private CodeType elementType;
            private ulong arrayStartAddress;
            private uint elementTypeSize;

            public ElementCreatorReadOnlyList(TypeConstructor activator, CodeType elementType, ulong arrayStartAddress)
            {
                this.activator = activator;
                this.elementType = elementType;
                this.arrayStartAddress = arrayStartAddress;
                elementTypeSize = elementType.Size;
            }

            public T this[int index]
            {
                get
                {
                    ulong address = arrayStartAddress + (ulong)index * elementTypeSize;
                    var buffer = Debugger.ReadMemory(elementType.Module.Process, address, elementTypeSize);

                    return activator(Variable.CreateNoCast(elementType, address), buffer, 0, address);
                }
            }

            public int Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
