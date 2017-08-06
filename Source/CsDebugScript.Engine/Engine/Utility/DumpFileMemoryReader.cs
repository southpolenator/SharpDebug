using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using CsDebugScript.Exceptions;

namespace CsDebugScript.Engine.Utility
{
    internal unsafe class DumpFileMemoryReader : IDisposable
    {
        private FileStream fileStream;
        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewStream stream;
        private MemoryLocation[] ranges;
        private MemoryRegionFinder finder;
        private int previousRange;
        byte* basePointer = null;

        public DumpFileMemoryReader(string dumpFilePath)
        {
            bool dispose = true;
            FileStream fileStream = null;
            MemoryMappedFile memoryMappedFile = null;
            MemoryMappedViewStream stream = null;

            try
            {
                fileStream = new FileStream(dumpFilePath, FileMode.Open, FileAccess.Read);
                memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, Guid.NewGuid().ToString(), fileStream.Length, MemoryMappedFileAccess.Read, new MemoryMappedFileSecurity(), HandleInheritability.Inheritable, false);
                stream = memoryMappedFile.CreateViewStream(0, fileStream.Length, MemoryMappedFileAccess.Read);

                stream.SafeMemoryMappedViewHandle.AcquirePointer(ref basePointer);
                IntPtr streamPointer = IntPtr.Zero;
                uint streamSize = 0;
                MINIDUMP_DIRECTORY directory = new MINIDUMP_DIRECTORY();

                if (!MiniDumpReadDumpStream((IntPtr)basePointer, MINIDUMP_STREAM_TYPE.Memory64ListStream, ref directory, ref streamPointer, ref streamSize))
                    throw new Exception("Unable to read mini dump stream");

                var data = (MINIDUMP_MEMORY64_LIST)Marshal.PtrToStructure(streamPointer, typeof(MINIDUMP_MEMORY64_LIST));
                ulong lastEnd = data.BaseRva;

                ranges = new MemoryLocation[data.NumberOfMemoryRanges];
                for (int i = 0; i < ranges.Length; i++)
                {
                    var descriptor = (MINIDUMP_MEMORY_DESCRIPTOR64)Marshal.PtrToStructure(streamPointer + sizeof(MINIDUMP_MEMORY64_LIST) + i * sizeof(MINIDUMP_MEMORY_DESCRIPTOR64), typeof(MINIDUMP_MEMORY_DESCRIPTOR64));
                    ranges[i] = new MemoryLocation()
                    {
                        MemoryStart = descriptor.StartOfMemoryRange,
                        MemoryEnd = descriptor.StartOfMemoryRange + descriptor.DataSize,
                        FilePosition = lastEnd,
                    };
                    lastEnd += descriptor.DataSize;
                }

                int newEnd = 0;
                for (int i = 1; i < ranges.Length; i++)
                    if (ranges[i].MemoryStart == ranges[newEnd].MemoryEnd)
                        ranges[newEnd].MemoryEnd = ranges[i].MemoryEnd;
                    else
                        ranges[++newEnd] = ranges[i];
                newEnd++;
                Array.Resize(ref ranges, newEnd);
                finder = new MemoryRegionFinder(ranges.Select(r => new MemoryRegion { BaseAddress = r.MemoryStart, MemoryEnd = r.MemoryEnd }).ToList());
                dispose = false;
            }
            finally
            {
                if (dispose)
                {
                    stream.SafeMemoryMappedViewHandle.ReleasePointer();
                    if (stream != null)
                    {
                        stream.Dispose();
                    }

                    if (memoryMappedFile != null)
                    {
                        memoryMappedFile.Dispose();
                    }

                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                    }
                }
                else
                {
                    this.fileStream = fileStream;
                    this.stream = stream;
                    this.memoryMappedFile = memoryMappedFile;
                }
            }
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

        public string ReadWideString(ulong address, int size = -1)
        {
            char[] buffer = new char[1000];
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
                        sb.Append(buffer[i]);
                    }
                }
            }
            while (read == buffer.Length && !end);

            return sb.ToString();
        }

        public void GetMemoryRange(ulong address, out ulong baseAddress, out ulong rangeSize)
        {
            var location = FindMemoryLocation(address);

            baseAddress = location.MemoryStart;
            rangeSize = location.MemoryEnd - location.MemoryStart;
        }

        internal MemoryRegion[] GetMemoryRanges()
        {
            MemoryRegion[] regions = new MemoryRegion[ranges.Length];

            for (int i = 0; i < regions.Length; i++)
                regions[i] = new MemoryRegion { BaseAddress = ranges[i].MemoryStart, MemoryEnd = ranges[i].MemoryEnd };
            return regions;
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

        private struct MemoryLocation
        {
            public ulong MemoryStart;
            public ulong MemoryEnd;
            public ulong FilePosition;
        }

        #region Native structures and methods
        private enum MINIDUMP_STREAM_TYPE : uint
        {
            UnusedStream = 0,
            ReservedStream0 = 1,
            ReservedStream1 = 2,
            ThreadListStream = 3,
            ModuleListStream = 4,
            MemoryListStream = 5,
            ExceptionStream = 6,
            SystemInfoStream = 7,
            ThreadExListStream = 8,
            Memory64ListStream = 9,
            CommentStreamA = 10,
            CommentStreamW = 11,
            HandleDataStream = 12,
            FunctionTableStream = 13,
            UnloadedModuleListStream = 14,
            MiscInfoStream = 15,
            MemoryInfoListStream = 16,
            ThreadInfoListStream = 17,
            HandleOperationListStream = 18,
            LastReservedStream = 0xffff
        }

        private struct MINIDUMP_LOCATION_DESCRIPTOR
        {
            public UInt32 DataSize;
            public uint Rva;
        }

        private struct MINIDUMP_DIRECTORY
        {
            public UInt32 StreamType;
            public MINIDUMP_LOCATION_DESCRIPTOR Location;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MINIDUMP_MEMORY_DESCRIPTOR64
        {
            public UInt64 StartOfMemoryRange;
            public UInt64 DataSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MINIDUMP_MEMORY64_LIST
        {
            public UInt64 NumberOfMemoryRanges;
            public UInt64 BaseRva;
        }

        [DllImport("dbghelp.dll", SetLastError = true)]
        private static extern bool MiniDumpReadDumpStream(
            IntPtr BaseOfDump,
            MINIDUMP_STREAM_TYPE StreamNumber,
            ref MINIDUMP_DIRECTORY Dir,
            ref IntPtr StreamPointer,
            ref uint StreamSize);
        #endregion
    }
}
