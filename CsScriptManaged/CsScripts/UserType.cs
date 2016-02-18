using CsScriptManaged;
using CsScriptManaged.Utility;
using System;
using System.Linq;
using System.Text;

namespace CsScripts
{
    /// <summary>
    /// Extension class providing cast functionality.
    /// </summary>
    public static class VariableCastExtender
    {
        /// <summary>
        /// Dynamic Cast, cast with type check.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static T DynamicCastAs<T>(this Variable variable) where T : UserType
        {
            if (variable == null || !variable.GetRuntimeType().Inherits<T>())
                return null;

            return variable.CastAs<T>();
        }

        /// <summary>
        /// Downcast Interface, looks up the type based on virtual table.
        /// </summary>
        /// <param name="userType"></param>
        /// <returns></returns>
        public static Variable DowncastInterface(this Variable userType)
        {
            if (userType == null)
                return null;

            return userType.CastAs(userType.GetRuntimeType());
        }

        /// <summary>
        /// Check if inherits from given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userType"></param>
        /// <returns></returns>
        public static bool Inherits<T>(this Variable userType) where T : UserType
        {
            return userType.GetRuntimeType().Inherits<T>();
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
        protected byte[] memoryBuffer;

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
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        public UserType(Variable variable, byte[] buffer, int offset, ulong bufferAddress)
            : base(variable)
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
            UserTypeMetadata metadata = UserTypeMetadata.ReadFromType(baseClassType);

            return metadata.TypeName;
        }

        /// <summary>
        /// Returns array of 16-bit unsigned integers converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static ushort[] ReadUshortArray(byte[] buffer, int offset, int elements)
        {
            ushort[] array = new ushort[elements];

            for (int i = 0; i < elements; i++, offset += 2)
                array[i] = ReadUshort(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 32-bit unsigned integers converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static uint[] ReadUintArray(byte[] buffer, int offset, int elements)
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
        public static ulong[] ReadUlongArray(byte[] buffer, int offset, int elements)
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
        public static short[] ReadShortArray(byte[] buffer, int offset, int elements)
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
        public static int[] ReadIntArray(byte[] buffer, int offset, int elements)
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
        public static long[] ReadLongArray(byte[] buffer, int offset, int elements)
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
        public static byte[] ReadByteArray(byte[] buffer, int offset, int elements)
        {
            byte[] array = new byte[elements];

            Array.Copy(buffer, offset, array, 0, elements);
            return array;
        }

        /// <summary>
        /// Returns array of 8/16-bit unsigned integers converted from one/two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        /// <param name="charSize">Size of the character.</param>
        public static char[] ReadCharArray(byte[] buffer, int offset, int elements, int charSize)
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
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        /// <exception cref="System.Exception">Unsupported pointer size</exception>
        public static ulong[] ReadPointerArray(byte[] buffer, int offset, int elements, uint pointerSize)
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
        public static ushort ReadUshort(byte[] buffer, int offset, int bits = 16, int bitsOffset = 0)
        {
            var value = BitConverter.ToUInt16(buffer, offset);

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
        public static uint ReadUint(byte[] buffer, int offset, int bits = 32, int bitsOffset = 0)
        {
            var value = BitConverter.ToUInt32(buffer, offset);

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
        public static ulong ReadUlong(byte[] buffer, int offset, int bits = 64, int bitsOffset = 0)
        {
            var value = BitConverter.ToUInt64(buffer, offset);

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
        public static short ReadShort(byte[] buffer, int offset, int bits = 16, int bitsOffset = 0)
        {
            var value = BitConverter.ToInt16(buffer, offset);

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
        public static int ReadInt(byte[] buffer, int offset, int bits = 32, int bitsOffset = 0)
        {
            var value = BitConverter.ToInt32(buffer, offset);

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
        public static long ReadLong(byte[] buffer, int offset, int bits = 64, int bitsOffset = 0)
        {
            var value = BitConverter.ToInt64(buffer, offset);

            if (bits != 64 || bitsOffset != 0)
                value = (long)((value >> bitsOffset) & ((1L << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static double ReadDouble(byte[] buffer, int offset)
        {
            return BitConverter.ToDouble(buffer, offset);
        }

        /// <summary>
        /// Returns a Boolean value converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static bool ReadBool(byte[] buffer, int offset, int bits = 8, int bitsOffset = 0)
        {
            var value = buffer[offset];

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
        public static byte ReadByte(byte[] buffer, int offset, int bits = 8, int bitsOffset = 0)
        {
            var value = buffer[offset];

            if (bits != 8 || bitsOffset != 0)
                value = (byte)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from four/eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        /// <exception cref="System.Exception">Unsupported pointer size</exception>
        public static ulong ReadPointer(byte[] buffer, int offset, int pointerSize)
        {
            if (pointerSize == 4)
                return ReadUint(buffer, offset);
            else if (pointerSize == 8)
                return ReadUlong(buffer, offset);
            else
                throw new Exception("Unsupported pointer size");
        }

        private static DictionaryCache<Tuple<uint, ulong, int>, string> stringCache = new DictionaryCache<Tuple<uint, ulong, int>, string>(DoReadString);

        /// <summary>
        /// Reads the ANSI/Unicode string from the specified address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The memory address.</param>
        /// <param name="charSize">Size of the character.</param>
        public static string ReadString(Process process, ulong address, int charSize)
        {
            if (address == 0)
                return null;

           return stringCache[Tuple.Create(process.Id, address, charSize)];
        }

        private static string DoReadString(Tuple<uint, ulong, int> tuple)
        {
            Process process = Process.All.Where(p => p.Id == tuple.Item1).First();
            ulong address = tuple.Item2;
            int charSize = tuple.Item3;

            if (address == 0)
                return null;

            var dumpReader = process.DumpFileMemoryReader;

            if (dumpReader != null)
            {
                if (charSize == 1)
                {
                    return dumpReader.ReadAnsiString(address);
                }
                else if (charSize == 2)
                {
                    return dumpReader.ReadWideString(address);
                }
                else
                {
                    throw new Exception("Unsupported char size");
                }
            }
            else
            {
                using (ProcessSwitcher switcher = new ProcessSwitcher(process))
                {
                    uint stringLength;

                    if (charSize == 1)
                    {
                        StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                        Context.DataSpaces.ReadMultiByteStringVirtual(address, Constants.MaxStringReadLength, sb, (uint)sb.Capacity, out stringLength);
                        return sb.ToString();
                    }
                    else if (charSize == 2)
                    {
                        StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                        Context.DataSpaces.ReadUnicodeStringVirtualWide(address, Constants.MaxStringReadLength * 2, sb, (uint)sb.Capacity, out stringLength);
                        return sb.ToString();
                    }
                    else
                    {
                        throw new Exception("Unsupported char size");
                    }
                }
            }
        }
    }
}
