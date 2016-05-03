using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using System;
using System.Linq;

namespace CsDebugScript
{
    /// <summary>
    /// Extension class providing cast functionality.
    /// </summary>
    public static class VariableCastExtender
    {
        /// <summary>
        /// Does the dynamic cast, cast with type check.
        /// </summary>
        /// <typeparam name="T">New type to cast variable to.</typeparam>
        /// <param name="variable">The variable.</param>
        public static T DynamicCastAs<T>(this Variable variable) where T : UserType
        {
            if (variable == null)
            {
                return null;
            }

            CodeType runtimeType = variable.DereferencePointer().GetRuntimeType();

            if (runtimeType.IsPointer)
            {
                runtimeType = runtimeType.ElementType;
            }

            if (runtimeType.Inherits<T>())
            {
                return variable.CastAs<T>();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Does the safe cast, cast with null check.
        /// </summary>
        /// <typeparam name="T">New type to cast variable to.</typeparam>
        /// <param name="variable">The variable.</param>
        public static T SafeCastAs<T>(this Variable variable) where T : UserType
        {
            if (variable == null)
            {
                return null;
            }

            return variable.CastAs<T>();
        }

        /// <summary>
        /// Does the full downcast, looks up the type based on virtual table and shifts variable address if multi-inheritance was involved.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public static Variable DowncastInterface(this Variable variable)
        {
            if (variable == null)
            {
                return null;
            }

            var runtimeTypeAndOffset = variable.runtimeCodeTypeAndOffset.Value;

            if (runtimeTypeAndOffset.Item2 != 0 || variable.GetCodeType() != runtimeTypeAndOffset.Item1)
            {
                return Variable.CreatePointer(runtimeTypeAndOffset.Item1.PointerToType, variable.GetPointerAddress() - (uint)runtimeTypeAndOffset.Item2);
            }

            return variable;
        }

        /// <summary>
        /// Check if variable runtime type inherits from the specified type.
        /// </summary>
        /// <typeparam name="T">Type to verify inheritance of.</typeparam>
        /// <param name="variable">The variable.</param>
        public static bool Inherits<T>(this Variable variable)
            where T : UserType
        {
            return variable.GetRuntimeType().Inherits<T>();
        }

       /// <summary>
        /// Reinterpret Cast, chanches underlaying code type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userType"></param>
        /// <returns></returns>
        public static CodePointer<T> ReinterpretPointerCast<T>(this Variable userType) where T : struct
        {
            // Get CodeType from the generic argument.
            //
            string codeTypeName;

            if (typeof (T) == typeof (int))
            {
                codeTypeName = "int";
            }
            else if (typeof (T) == typeof (short))
            {
                codeTypeName = "short";
            }
            else if (typeof (T) == typeof (uint))
            {
                codeTypeName = "unsigned int";
            }
            else if (typeof (T) == typeof (ushort))
            {
                codeTypeName = "unsigned short";
            }
            else
            {
                throw new NotSupportedException("Requested type is not supported.");
            }

            // Return CodePointer<T>
            //
            return new CodePointer<T>(
                Variable.CreatePointer(
                    CodeType.Create(codeTypeName, userType.GetCodeType().Module).PointerToType,
                    userType.GetPointerAddress()));
        }


        /// <summary>
        /// Adjust Pointer and Cast To Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userType"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static T AdjustPointer<T>(this Variable userType, int offset) where T : UserType
        {
            return userType.AdjustPointer(offset).CastAs<T>();
        }
    }

    /// <summary>
    /// Helper class to be used in Generics User Type classes as partial class extension.
    /// It is needed when Generics User Type casts variable to new user type (that is template and possible generics). When user type is generics, engine cannot
    /// deduce CodeType that needs to be used, while generated user type code expects correct one. This class provides the bridge between two worlds.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericsElementCaster<T>
    {
        /// <summary>
        /// The element code type
        /// </summary>
        private CodeType elementCodeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsElementCaster{T}"/> class.
        /// </summary>
        /// <param name="thisClass">The thisClass variable in generated UserType.</param>
        /// <param name="argumentNumber">The argument number.</param>
        public GenericsElementCaster(UserMember<Variable> thisClass, int argumentNumber)
            : this(thisClass.Value, argumentNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsElementCaster{T}"/> class.
        /// </summary>
        /// <param name="thisClass">The thisClass variable in generated UserType.</param>
        /// <param name="argumentNumber">The argument number.</param>
        public GenericsElementCaster(Variable thisClass, int argumentNumber)
            : this(thisClass.GetCodeType(), argumentNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsElementCaster{T}"/> class.
        /// </summary>
        /// <param name="parentCodeType">CodeType of the generics class that will be using this helper class.</param>
        /// <param name="argumentNumber">The argument number.</param>
        public GenericsElementCaster(CodeType parentCodeType, int argumentNumber)
        {
            try
            {
                elementCodeType = CodeType.Create(parentCodeType.TemplateArgumentsStrings[argumentNumber], parentCodeType.Module);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Casts variable to the new type.
        /// </summary>
        /// <param name="variable">The variable to be casted.</param>
        /// <returns>Computed variable that is of new type.</returns>
        public T CastAs(Variable variable)
        {
            if (elementCodeType != null)
                variable = variable.CastAs(elementCodeType);
            return variable.CastAs<T>();
        }
    }

    /// <summary>
    /// Base class for user defined types in C# scripts
    /// </summary>
    public class UserType : Variable
    {
        /// <summary>
        /// The memory buffer using which user type was initialized
        /// </summary>
        protected MemoryBuffer memoryBuffer;

        /// <summary>
        /// The offset inside the memory buffer using which user type was initialized
        /// </summary>
        protected int memoryBufferOffset;

        /// <summary>
        /// The address of the memory buffer using which user type was initialized
        /// </summary>
        protected ulong memoryBufferAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public UserType(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        public UserType(Variable variable, MemoryBuffer buffer, int offset, ulong bufferAddress)
            : base(variable)
        {
            memoryBuffer = buffer;
            memoryBufferOffset = offset;
            memoryBufferAddress = bufferAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType" /> class.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        /// <param name="codeType">The variable code type.</param>
        /// <param name="address">The variable address.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="path">The variable path.</param>
        public UserType(MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name = Variable.ComputedName, string path = Variable.UnknownPath)
            : base(codeType, address, name, path)
        {
            memoryBuffer = buffer;
            memoryBufferOffset = offset;
            memoryBufferAddress = bufferAddress;
        }

        /// <summary>
        /// Gets the base class string.
        /// </summary>
        /// <param name="baseClassType">Type of the base class.</param>
        public static string GetBaseClassString(Type baseClassType)
        {
            if (!baseClassType.IsSubclassOf(typeof(Variable)))
                throw new Exception("Specified type doesn't inherit Variable class");

            // TODO: Make it work with exported template classes
            UserTypeMetadata metadata = UserTypeMetadata.ReadFromType(baseClassType).First();

            return metadata.TypeName;
        }

        /// <summary>
        /// Returns array of 16-bit unsigned integers converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static ushort[] ReadUshortArray(MemoryBuffer buffer, int offset, int elements)
        {
            ushort[] array = new ushort[elements];

            for (int i = 0; i < elements; i++, offset += 2)
                array[i] = ReadUshort(buffer, offset);
            return array;
        }


        /// <summary>
        /// Returns array of bool.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static bool[] ReadBoolArray(MemoryBuffer buffer, int offset, int elements)
        {
            bool[] array = new bool[elements];

            for (int i = 0; i < elements; i++, offset += 2)
                array[i] = ReadBool(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 32-bit unsigned integers converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static uint[] ReadUintArray(MemoryBuffer buffer, int offset, int elements)
        {
            uint[] array = new uint[elements];

            for (int i = 0; i < elements; i++, offset += 4)
                array[i] = ReadUint(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 64-bit unsigned integers converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static ulong[] ReadUlongArray(MemoryBuffer buffer, int offset, int elements)
        {
            ulong[] array = new ulong[elements];

            for (int i = 0; i < elements; i++, offset += 8)
                array[i] = ReadUlong(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 16-bit signed integers converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static short[] ReadShortArray(MemoryBuffer buffer, int offset, int elements)
        {
            short[] array = new short[elements];

            for (int i = 0; i < elements; i++, offset += 2)
                array[i] = ReadShort(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 32-bit signed integers converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static int[] ReadIntArray(MemoryBuffer buffer, int offset, int elements)
        {
            int[] array = new int[elements];

            for (int i = 0; i < elements; i++, offset += 4)
                array[i] = ReadInt(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 64-bit signed integers converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static long[] ReadLongArray(MemoryBuffer buffer, int offset, int elements)
        {
            long[] array = new long[elements];

            for (int i = 0; i < elements; i++, offset += 8)
                array[i] = ReadLong(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 8-bit unsigned integers converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static unsafe byte[] ReadByteArray(MemoryBuffer buffer, int offset, int elements)
        {
            byte[] array = new byte[elements];

            if (buffer.BytePointer != null)
            {
                fixed (byte* destination = array)
                {
                    byte* source = buffer.BytePointer + offset;
                    MemoryBuffer.MemCpy(destination, source, (uint)array.Length);
                }
            }
            else
                Array.Copy(buffer.Bytes, offset, array, 0, elements);
            return array;
        }

        /// <summary>
        /// Returns array of floats.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static float[] ReadFloatArray(MemoryBuffer buffer, int offset, int elements)
        {
            float[] array = new float[elements];

            for (int i = 0; i < elements; i++)
            {
                array[i] = ReadFloat(buffer, offset);
                offset += sizeof(float);
            }

            return array;
        }

        /// <summary>
        /// Returns array of floats.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static double[] ReadDoubleArray(MemoryBuffer buffer, int offset, int elements)
        {
            double[] array = new double[elements];

            for (int i = 0; i < elements; i++)
            {
                array[i] = ReadDouble(buffer, offset);
                offset += sizeof(float);
            }

            return array;
        }

        /// <summary>
        /// Returns array of 8/16-bit unsigned integers converted from one/two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        /// <param name="charSize">Size of the character.</param>
        public static char[] ReadCharArray(MemoryBuffer buffer, int offset, int elements, int charSize)
        {
            char[] array = new char[elements];

            if (charSize == 1)
                for (int i = 0; i < elements; i++)
                    array[i] = (char)ReadByte(buffer, offset + i);
            else if (charSize == 2)
                for (int i = 0; i < elements; i++)
                    array[i] = (char)ReadUshort(buffer, offset + 2 * i);
            else
            {
                throw new Exception("Unsupported char size: " + charSize);
            }
            return array;
        }

        /// <summary>
        /// Returns array of 64-bit unsigned integers converted from four/eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        /// <exception cref="System.Exception">Unsupported pointer size</exception>
        public static ulong[] ReadPointerArray(MemoryBuffer buffer, int offset, int elements, uint pointerSize)
        {
            ulong[] array = new ulong[elements];

            if (pointerSize == 4)
                for (int i = 0; i < elements; i++, offset += 4)
                    array[i] = ReadUint(buffer, offset);
            else if (pointerSize == 8)
                for (int i = 0; i < elements; i++, offset += 8)
                    array[i] = ReadUlong(buffer, offset);
            else
                throw new Exception("Unsupported pointer size");
            return array;
        }

        /// <summary>
        /// Returns a 16-bit unsigned integer converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe ushort ReadUshort(MemoryBuffer buffer, int offset, int bits = 16, int bitsOffset = 0)
        {
            ushort value;

            if (buffer.BytePointer != null)
                value = *((ushort*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToUInt16(buffer.Bytes, offset);

            if (bits != 16 || bitsOffset != 0)
                value = (ushort)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 32-bit unsigned integer converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe uint ReadUint(MemoryBuffer buffer, int offset, int bits = 32, int bitsOffset = 0)
        {
            uint value;

            if (buffer.BytePointer != null)
                value = *((uint*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToUInt32(buffer.Bytes, offset);

            if (bits != 32 || bitsOffset != 0)
                value = (uint)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe ulong ReadUlong(MemoryBuffer buffer, int offset, int bits = 64, int bitsOffset = 0)
        {
            ulong value;

            if (buffer.BytePointer != null)
                value = *((ulong*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToUInt64(buffer.Bytes, offset);

            if (bits != 64 || bitsOffset != 0)
                value = (ulong)((value >> bitsOffset) & ((1UL << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 16-bit signed integer converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe short ReadShort(MemoryBuffer buffer, int offset, int bits = 16, int bitsOffset = 0)
        {
            short value;

            if (buffer.BytePointer != null)
                value = *((short*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToInt16(buffer.Bytes, offset);

            if (bits != 16 || bitsOffset != 0)
                value = (short)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 32-bit signed integer converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe int ReadInt(MemoryBuffer buffer, int offset, int bits = 32, int bitsOffset = 0)
        {
            int value;

            if (buffer.BytePointer != null)
                value = *((int*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToInt32(buffer.Bytes, offset);

            if (bits != 32 || bitsOffset != 0)
                value = (int)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe long ReadLong(MemoryBuffer buffer, int offset, int bits = 64, int bitsOffset = 0)
        {
            long value;

            if (buffer.BytePointer != null)
                value = *((long*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToInt64(buffer.Bytes, offset);

            if (bits != 64 || bitsOffset != 0)
                value = (long)((value >> bitsOffset) & ((1L << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a single-precision floating point number converted from hour bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static unsafe float ReadFloat(MemoryBuffer buffer, int offset)
        {
            if (buffer.BytePointer != null)
                return *((float*)(buffer.BytePointer + offset));
            else
                return BitConverter.ToSingle(buffer.Bytes, offset);
        }

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static unsafe double ReadDouble(MemoryBuffer buffer, int offset)
        {
            if (buffer.BytePointer != null)
                return *((double*)(buffer.BytePointer + offset));
            else
                return BitConverter.ToDouble(buffer.Bytes, offset);
        }

        /// <summary>
        /// Returns a Boolean value converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe bool ReadBool(MemoryBuffer buffer, int offset, int bits = 8, int bitsOffset = 0)
        {
            byte value;

            if (buffer.BytePointer != null)
                value = *(buffer.BytePointer + offset);
            else
                value = buffer.Bytes[offset];

            if (bits != 8 || bitsOffset != 0)
                value = (byte)((value >> bitsOffset) & ((1 << bits) - 1));
            return value != 0;
        }

        /// <summary>
        /// Returns a 8-bit signed integer converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe byte ReadByte(MemoryBuffer buffer, int offset, int bits = 8, int bitsOffset = 0)
        {
            byte value;

            if (buffer.BytePointer != null)
                value = *(buffer.BytePointer + offset);
            else
                value = buffer.Bytes[offset];

            if (bits != 8 || bitsOffset != 0)
                value = (byte)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a signed byte.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static unsafe sbyte ReadSbyte(MemoryBuffer buffer, int offset)
        {
            if (buffer.BytePointer != null)
                return *((sbyte*)(buffer.BytePointer + offset));
            else
                return (sbyte)buffer.Bytes[offset];
        }

        /// <summary>
        /// Returns a single character.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static char ReadChar(MemoryBuffer buffer, int offset)
        {
            return (char)ReadByte(buffer, offset);
        }


        /// <summary>
        /// Returns a 64-bit unsigned integer converted from four/eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        /// <exception cref="System.Exception">Unsupported pointer size</exception>
        public static ulong ReadPointer(MemoryBuffer buffer, int offset, int pointerSize)
        {
            if (pointerSize == 4)
                return ReadUint(buffer, offset);
            else if (pointerSize == 8)
                return ReadUlong(buffer, offset);
            else
                throw new Exception("Unsupported pointer size");
        }

        /// <summary>
        /// Reads the ANSI/Unicode string from the specified address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The memory address.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated.</param>
        public static string ReadString(Process process, ulong address, int charSize, int length = -1)
        {
            if (address == 0)
                return null;

            return process.ReadString(address, charSize, length);
        }

        /// <summary>
        /// Reads the pointer and casts it to the type.
        /// </summary>
        /// <typeparam name="T">Type to be casted to</typeparam>
        /// <param name="thisClass">Variable that contains UserMember of the this class.</param>
        /// <param name="classFieldName">Name of the class field.</param>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        public static T ReadPointer<T>(UserMember<Variable> thisClass, string classFieldName, MemoryBuffer buffer, int offset, int pointerSize)
        {
            ulong pointer = ReadPointer(buffer, offset, pointerSize);

            if (pointer == 0)
            {
                return default(T);
            }

            return Variable.CreatePointerNoCast(thisClass.Value.GetCodeType().GetClassFieldType(classFieldName), pointer, classFieldName).CastAs<T>();
        }

        /// <summary>
        /// Reads the pointer and casts it to the type.
        /// </summary>
        /// <typeparam name="T">Type to be casted to</typeparam>
        /// <param name="classCodeType">The class code type.</param>
        /// <param name="classFieldName">Name of the class field.</param>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        public static T ReadPointer<T>(CodeType classCodeType, string classFieldName, MemoryBuffer buffer, int offset, int pointerSize)
        {
            ulong pointer = ReadPointer(buffer, offset, pointerSize);

            if (pointer == 0)
            {
                return default(T);
            }

            return Variable.CreatePointerNoCast(classCodeType.GetClassFieldType(classFieldName), pointer, classFieldName).CastAs<T>();
        }
    }
}
