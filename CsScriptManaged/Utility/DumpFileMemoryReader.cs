using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace CsScriptManaged.Utility
{
    internal unsafe class DumpFileMemoryReader : IDisposable
    {
        private FileStream fileStream;
        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewStream stream;
        private MemoryLocation[] ranges;
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

                var data = Marshal.PtrToStructure<MINIDUMP_MEMORY64_LIST>(streamPointer);
                ulong lastEnd = data.BaseRva;

                ranges = new MemoryLocation[data.NumberOfMemoryRanges];
                for (int i = 0; i < ranges.Length; i++)
                {
                    var descriptor = Marshal.PtrToStructure<MINIDUMP_MEMORY_DESCRIPTOR64>(streamPointer + sizeof(MINIDUMP_MEMORY64_LIST) + i * sizeof(MINIDUMP_MEMORY_DESCRIPTOR64));
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
                Array.Resize(ref ranges, newEnd);
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

        public void Dispose()
        {
            stream.SafeMemoryMappedViewHandle.ReleasePointer();
            stream.Dispose();
            memoryMappedFile.Dispose();
            fileStream.Dispose();
        }

        private int ReadMemory(ulong position, char[] buffer)
        {
            Marshal.Copy((IntPtr)(basePointer + position), buffer, 0, buffer.Length);
            return buffer.Length;
        }

        private int ReadMemory(ulong position, byte[] buffer)
        {
            Marshal.Copy((IntPtr)(basePointer + position), buffer, 0, buffer.Length);
            return buffer.Length;
        }

        public byte[] ReadMemory(ulong address, int size)
        {
            byte[] bytes = new byte[size];
            var positionAndSize = FindDumpPositionAndSize(address);
            var position = positionAndSize.Item1;
            var maxSize = positionAndSize.Item2;

            if ((ulong)size > maxSize)
            {
                throw new Exception("Reading more that it is found");
            }

            ReadMemory(position, bytes);
            return bytes;
        }

        public string ReadAnsiString(ulong address, int size = -1)
        {
            byte[] buffer = new byte[1000];
            int read;
            bool end = false;
            StringBuilder sb = new StringBuilder();
            var positionAndSize = FindDumpPositionAndSize(address);
            var position = positionAndSize.Item1;
            var maxSize = positionAndSize.Item2;

            if (size <= 0 || (ulong)size > maxSize)
            {
                size = (int)Math.Min(maxSize, int.MaxValue);
            }

            do
            {
                read = ReadMemory(position, buffer);
                position += (ulong)read;
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
            var positionAndSize = FindDumpPositionAndSize(address);
            var position = positionAndSize.Item1;
            var maxSize = positionAndSize.Item2;

            if (size <= 0 || (ulong)size > maxSize)
            {
                size = (int)Math.Min(maxSize, int.MaxValue);
            }

            do
            {
                read = ReadMemory(position, buffer);
                position += (ulong)read;
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

        private Tuple<ulong, ulong> FindDumpPositionAndSize(ulong address)
        {
            int low = 0, high = ranges.Length - 1;

            while (low < high)
            {
                int middle = (high + low) / 2;

                if (ranges[middle].MemoryStart > address)
                    high = middle;
                else if (ranges[middle].MemoryEnd < address)
                {
                    if (low == middle)
                        break;
                    low = middle;
                }
                else
                    return Tuple.Create(ranges[middle].FilePosition + address - ranges[middle].MemoryStart, ranges[middle].MemoryEnd - address);
            }

            return Tuple.Create(0UL, 0UL);
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
