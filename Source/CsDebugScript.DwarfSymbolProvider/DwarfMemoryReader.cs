using System;
using System.Runtime.InteropServices;

namespace CsDebugScript.DwarfSymbolProvider
{
    internal class DwarfMemoryReader : IDisposable
    {
        private GCHandle pinnedData;
        private IntPtr pointer;

        public DwarfMemoryReader(byte[] data)
        {
            Data = data;
            Position = 0;
            pinnedData = GCHandle.Alloc(data, GCHandleType.Pinned);
            pointer = pinnedData.AddrOfPinnedObject();
        }

        public byte[] Data { get; private set; }

        public int Position { get; set; }

        public bool IsEnd
        {
            get
            {
                return Position >= Data.Length;
            }
        }

        public void Dispose()
        {
            pinnedData.Free();
            pointer = IntPtr.Zero;
        }

        public byte Peek()
        {
            return Data[Position];
        }

        public T ReadStructure<T>()
        {
            T result = Marshal.PtrToStructure<T>(pointer + Position);

            Position += Marshal.SizeOf<T>();
            return result;
        }

        public int ReadOffset(bool is64bit)
        {
            return is64bit ? (int)ReadUlong() : (int)ReadUint();
        }

        public ulong ReadLength(out bool is64bit)
        {
            ulong length = ReadUint();

            if (length == uint.MaxValue)
            {
                is64bit = true;
                length = ReadUlong();
            }
            else
            {
                is64bit = false;
            }

            return length;
        }

        public string ReadAnsiString()
        {
            string result = Marshal.PtrToStringAnsi(pointer + Position);

            Position += result.Length + 1;
            return result;
        }

        public byte ReadByte()
        {
            return Data[Position++];
        }

        public ushort ReadUshort()
        {
            ushort result = (ushort)Marshal.ReadInt16(pointer, Position);

            Position += 2;
            return result;
        }

        public uint ReadUint()
        {
            uint result = (uint)Marshal.ReadInt32(pointer, Position);

            Position += 4;
            return result;
        }

        public ulong ReadUlong()
        {
            ulong result = (ulong)Marshal.ReadInt64(pointer, Position);

            Position += 8;
            return result;
        }

        public ulong ReadUlong(uint size)
        {
            switch (size)
            {
                case 1:
                    return ReadByte();
                case 2:
                    return ReadUshort();
                case 4:
                    return ReadUint();
                case 8:
                    return ReadUlong();
                default:
                    throw new Exception("Unexpected read size");
            }
        }

        public uint LEB128()
        {
            uint x = 0;
            int shift = 0;

            while ((Data[Position] & 0x80) != 0)
            {
                x |= (uint)((Data[Position] & 0x7f) << shift);
                shift += 7;
                Position++;
            }
            x |= (uint)(Data[Position] << shift);
            Position++;
            return x;
        }

        public uint SLEB128()
        {
            int x = 0;
            int shift = 0;

            while ((Data[Position] & 0x80) != 0)
            {
                x |= (Data[Position] & 0x7f) << shift;
                shift += 7;
                Position++;
            }
            x |= Data[Position] << shift;
            if ((Data[Position] & 0x40) != 0)
            {
                x |= -(1 << (shift + 7)); // sign extend
            }
            Position++;
            return (uint)x;
        }

        public byte[] ReadBlock(uint size)
        {
            byte[] block = new byte[size];

            Array.Copy(Data, Position, block, 0, block.Length);
            Position += block.Length;
            return block;
        }

        public byte[] ReadBlock(uint size, int position)
        {
            int originalPosition = Position;
            Position = position;
            byte[] result = ReadBlock(size);
            Position = originalPosition;
            return result;
        }

        public string ReadAnsiString(int position)
        {
            int originalPosition = Position;
            Position = position;
            string result = ReadAnsiString();
            Position = originalPosition;
            return result;
        }

        public uint ReadUint(int position)
        {
            int originalPosition = Position;
            Position = position;
            uint result = ReadUint();
            Position = originalPosition;
            return result;
        }

        public T ReadStructure<T>(int position)
        {
            int originalPosition = Position;
            Position = position;
            T result = ReadStructure<T>();
            Position = originalPosition;
            return result;
        }
    }
}
