using SharpDebug.Exceptions;
using SharpUtilities;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;

namespace SharpDebug.Engine.Utility
{
    /// <summary>
    /// Wraps functionality for reading files that are region mapped.
    /// [Address start, Address end] maps to a region in the file.
    /// Dumps are usually like that, hence the class name.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract unsafe class DumpFileMemoryReader : IDisposable
    {
        /// <summary>
        /// Structure representing memory range map to file position.
        /// </summary>
        protected struct MemoryLocation
        {
            /// <summary>
            /// The memory start address.
            /// </summary>
            public ulong MemoryStart;

            /// <summary>
            /// The memory end address.
            /// </summary>
            public ulong MemoryEnd;

            /// <summary>
            /// The file position of memory start address.
            /// </summary>
            public ulong FilePosition;
        }

        /// <summary>
        /// The file stream of the opened dump file.
        /// </summary>
        protected FileStream fileStream;

        /// <summary>
        /// The memory mapped file.
        /// </summary>
        protected MemoryMappedFile memoryMappedFile;

        /// <summary>
        /// The memory mapped view stream.
        /// </summary>
        protected MemoryMappedViewStream stream;

        /// <summary>
        /// The base pointer.
        /// </summary>
        protected byte* basePointer = null;

        /// <summary>
        /// The memory ranges.
        /// </summary>
        private MemoryLocation[] ranges;

        /// <summary>
        /// The memory region finder.
        /// </summary>
        private MemoryRegionFinder finder;

        /// <summary>
        /// The previously used range
        /// </summary>
        private int previousRange;

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpFileMemoryReader"/> class.
        /// </summary>
        /// <param name="dumpFilePath">The dump file path.</param>
        protected DumpFileMemoryReader(string dumpFilePath)
        {
            fileStream = new FileStream(dumpFilePath, FileMode.Open, FileAccess.Read);
            memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, Guid.NewGuid().ToString(), fileStream.Length, MemoryMappedFileAccess.Read, HandleInheritability.Inheritable, false);
            stream = memoryMappedFile.CreateViewStream(0, fileStream.Length, MemoryMappedFileAccess.Read);
            stream.SafeMemoryMappedViewHandle.AcquirePointer(ref basePointer);
        }

        /// <summary>
        /// Initializes the this instance with the specified memory location ranges.
        /// </summary>
        /// <param name="ranges">The memory location ranges.</param>
        protected void Initialize(MemoryLocation[] ranges)
        {
            int newEnd = 0;

            for (int i = 1; i < ranges.Length; i++)
            {
                if (ranges[i].MemoryStart == ranges[newEnd].MemoryEnd)
                {
                    ranges[newEnd].MemoryEnd = ranges[i].MemoryEnd;
                }
                else
                {
                    ranges[++newEnd] = ranges[i];
                }
            }
            newEnd++;
            Array.Resize(ref ranges, newEnd);

            this.ranges = ranges;
            finder = new MemoryRegionFinder(ranges.Select(r => new MemoryRegion { BaseAddress = r.MemoryStart, MemoryEnd = r.MemoryEnd }).ToList());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            stream.SafeMemoryMappedViewHandle.ReleasePointer();
            stream.Dispose();
            memoryMappedFile.Dispose();
            fileStream.Dispose();
        }

        /// <summary>
        /// Reads the memory buffer from the file.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="size">The size.</param>
        public MemoryBuffer ReadMemory(ulong address, int size)
        {
            var position = FindDumpPositionAndSize(address);

            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "size cannot be less than 0");
            }

            if ((ulong)size > position.size)
            {
                throw new NotAllMemoryCanBeReadException(address, (uint)size, position.size);
            }

            return new MemoryBuffer(basePointer + position.position, size);
        }

        /// <summary>
        /// Reads the ANSI string from the file.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="size">The size. If -1, reads as 0 terminated.</param>
        public string ReadAnsiString(ulong address, int size = -1)
        {
            byte[] buffer = new byte[1000];
            int read;
            bool end = false;
            StringBuilder sb = new StringBuilder();
            var position = FindDumpPositionAndSize(address);

            if (size <= 0 || (ulong)size > position.size)
            {
                size = (int)Math.Min(position.size, int.MaxValue);
            }

            do
            {
                read = ReadMemory(position.position, buffer);
                position.position += (ulong)read;
                for (int i = 0; i < read && !end; i++)
                {
                    if (buffer[i] == 0 || sb.Length == size)
                    {
                        end = true;
                    }
                    else
                    {
                        sb.Append((char)buffer[i]);
                    }
                }
            }
            while (read == buffer.Length && !end);

            return sb.ToString();
        }

        /// <summary>
        /// Reads the wide (2-byte) string from the file.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="size">The size. If -1, reads as 0 terminated.</param>
        public string ReadWideString(ulong address, int size = -1)
        {
            char[] buffer = new char[1000];
            int read;
            bool end = false;
            StringBuilder sb = new StringBuilder();
            var position = FindDumpPositionAndSize(address);

            if (size <= 0 || (ulong)size > position.size)
            {
                size = (int)Math.Min(position.size / 2, int.MaxValue);
            }

            do
            {
                read = ReadMemory(position.position, buffer);
                position.position += (ulong)read;
                for (int i = 0; i < read && !end; i++)
                {
                    if (buffer[i] == 0 || sb.Length == size)
                    {
                        end = true;
                    }
                    else
                    {
                        sb.Append(buffer[i]);
                    }
                }
            }
            while (read == buffer.Length && !end);

            return sb.ToString();
        }

        /// <summary>
        /// Reads the wide unicode (4-byte) string from the file.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="size">The size. If -1, reads as 0 terminated.</param>
        public string ReadWideUnicodeString(ulong address, int size = -1)
        {
            int[] buffer = new int[1000];
            int read;
            bool end = false;
            var position = FindDumpPositionAndSize(address);
            var startingPosition = position;
            int length = 0;

            if (size <= 0 || (ulong)size > position.size)
            {
                size = (int)Math.Min(position.size / 4, int.MaxValue);
            }

            do
            {
                read = ReadMemory(position.position, buffer);
                position.position += (ulong)read;
                for (int i = 0; i < read && !end; i++)
                {
                    if (buffer[i] == 0 || length == size)
                    {
                        end = true;
                    }
                    else
                    {
                        length++;
                    }
                }
            }
            while (read == buffer.Length && !end);

            MemoryBuffer memoryBuffer = ReadMemory(address, length * sizeof(int));

            return Encoding.UTF32.GetString(memoryBuffer.Bytes);
        }

        /// <summary>
        /// Gets the memory range for the specified memory address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="baseAddress">The memory range start address.</param>
        /// <param name="rangeSize">Size of the memory range.</param>
        public void GetMemoryRange(ulong address, out ulong baseAddress, out ulong rangeSize)
        {
            var location = FindMemoryLocation(address);

            baseAddress = location.MemoryStart;
            rangeSize = location.MemoryEnd - location.MemoryStart;
        }

        /// <summary>
        /// Gets all memory ranges from the file regions.
        /// </summary>
        internal MemoryRegion[] GetMemoryRanges()
        {
            MemoryRegion[] regions = new MemoryRegion[ranges.Length];

            for (int i = 0; i < regions.Length; i++)
            {
                regions[i] = new MemoryRegion { BaseAddress = ranges[i].MemoryStart, MemoryEnd = ranges[i].MemoryEnd };
            }
            return regions;
        }

        private int ReadMemory(ulong position, int[] buffer)
        {
            fixed (int* destination = buffer)
            {
                byte* source = basePointer + position;

                MemoryBuffer.MemCpy(destination, source, (uint)buffer.Length * sizeof(int));
            }
            return buffer.Length;
        }

        private int ReadMemory(ulong position, char[] buffer)
        {
            fixed (char* destination = buffer)
            {
                byte* source = basePointer + position;

                MemoryBuffer.MemCpy(destination, source, (uint)buffer.Length * sizeof(char));
            }
            return buffer.Length;
        }

        private int ReadMemory(ulong position, byte[] buffer)
        {
            fixed (byte* destination = buffer)
            {
                byte* source = basePointer + position;

                MemoryBuffer.MemCpy(destination, source, (uint)buffer.Length);
            }
            return buffer.Length;
        }

        private MemoryLocation FindMemoryLocation(ulong address)
        {
            var location = ranges[previousRange];

            if (location.MemoryStart <= address && location.MemoryEnd > address)
            {
                return location;
            }

            int index = finder.Find(address);

            if (index >= 0)
            {
                location = ranges[index];

                if (location.MemoryStart <= address && location.MemoryEnd > address)
                {
                    previousRange = index;
                    return location;
                }
            }

            throw new InvalidMemoryAddressException(address);
        }

        private DumpPosition FindDumpPositionAndSize(ulong address)
        {
            var location = FindMemoryLocation(address);

            return new DumpPosition
            {
                position = location.FilePosition + address - location.MemoryStart,
                size = (uint)(location.MemoryEnd - address),
            };
        }

        private struct DumpPosition
        {
            public ulong position;
            public uint size;
        }
    }
}
