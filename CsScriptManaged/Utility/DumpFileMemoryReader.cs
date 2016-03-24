using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CsScriptManaged.Utility
{
    internal unsafe class DumpFileMemoryReader : IDisposable
    {
        private const int BucketSizeBits = 8;

        private FileStream fileStream;
        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewStream stream;
        private MemoryLocation[] ranges;
        private ulong triesStartMask;
        private int triesStartBits;
        private TriesElement[] buckets;
        private int previousRange;
        byte* basePointer = null;

        private delegate void MemCpyFunction(void* des, void* src, uint bytes);

        private static readonly MemCpyFunction MemCpy;

        static DumpFileMemoryReader()
        {
            var dynamicMethod = new DynamicMethod
            (
                "MemCpy",
                typeof(void),
                new[] { typeof(void*), typeof(void*), typeof(uint) },
                typeof(DumpFileMemoryReader)
            );

            var ilGenerator = dynamicMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);

            ilGenerator.Emit(OpCodes.Cpblk);
            ilGenerator.Emit(OpCodes.Ret);

            MemCpy = (MemCpyFunction)dynamicMethod.CreateDelegate(typeof(MemCpyFunction));
        }

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
                var minValue = ranges[0].MemoryStart;
                var maxValue = ranges[ranges.Length - 1].MemoryEnd;

                triesStartBits = 64 - BucketSizeBits;
                triesStartMask = ((1UL << BucketSizeBits) - 1) << triesStartBits;
                while ((triesStartMask & (maxValue - 1)) == 0)
                {
                    triesStartMask >>= BucketSizeBits;
                    triesStartBits -= BucketSizeBits;
                }

                var rangesTuple = new Tuple<int, MemoryLocation>[ranges.Length];
                for (int i = 0; i < rangesTuple.Length; i++)
                    rangesTuple[i] = Tuple.Create(i, ranges[i]);
                var element = new TriesElement(rangesTuple, triesStartMask, triesStartBits);

                buckets = element.buckets;
                dispose = false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw;
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

                MemCpy(destination, source, (uint)buffer.Length * sizeof(char));
            }
            return buffer.Length;
        }

        private int ReadMemory(ulong position, byte[] buffer)
        {
            fixed (byte* destination = buffer)
            {
                byte* source = basePointer + position;

                MemCpy(destination, source, (uint)buffer.Length);
            }
            return buffer.Length;
        }

        public byte[] ReadMemory(ulong address, int size)
        {
            byte[] bytes = new byte[size];
            var position = FindDumpPositionAndSize(address);

            if ((ulong)size > position.size)
            {
                throw new Exception("Reading more that it is found");
            }

            ReadMemory(position.position, bytes);
            return bytes;
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

        private DumpPosition FindDumpPositionAndSize(ulong address)
        {
            var location = ranges[previousRange];

            if (location.MemoryStart <= address && location.MemoryEnd > address)
            {
                return new DumpPosition
                {
                    position = location.FilePosition + address - location.MemoryStart,
                    size = (uint)(location.MemoryEnd - address),
                };
            }

            var mask = triesStartMask;
            var offset = triesStartBits;
            var bucketIndex = (address & mask) >> offset;
            var bucket = buckets[bucketIndex];

            while (bucket != null && bucket.buckets != null)
            {
                mask >>= BucketSizeBits;
                offset -= BucketSizeBits;
                bucketIndex = (address & mask) >> offset;
                bucket = bucket.buckets[bucketIndex];
            }

            if (bucket != null)
            {
                location = ranges[bucket.location];

                if (location.MemoryStart <= address && location.MemoryEnd > address)
                {
                    previousRange = bucket.location;
                    return new DumpPosition
                    {
                        position = location.FilePosition + address - location.MemoryStart,
                        size = (uint)(location.MemoryEnd - address),
                    };
                }
            }

            return new DumpPosition();
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

        private class TriesElement
        {
            public TriesElement[] buckets;
            public int location;

            public TriesElement(IEnumerable<Tuple<int, MemoryLocation>> ranges, ulong triesStartMask, int triesStartBits, ulong minValue = 0, ulong maxValue = ulong.MaxValue)
            {
                if (ranges.Count() > 1)
                {
                    var division = new List<Tuple<int, MemoryLocation>>[1 << BucketSizeBits];

                    foreach (var range in ranges)
                    {
                        var bucketStart = (Math.Max(minValue, range.Item2.MemoryStart) & triesStartMask) >> triesStartBits;
                        var bucketEnd = (Math.Min(maxValue, range.Item2.MemoryEnd - 1) & triesStartMask) >> triesStartBits;

                        for (var j = bucketStart; j <= bucketEnd; j++)
                        {
                            if (division[j] == null)
                                division[j] = new List<Tuple<int, MemoryLocation>>();
                            division[j].Add(range);
                        }
                    }

                    buckets = new TriesElement[1 << BucketSizeBits];
                    for (int i = 0; i < 1 << BucketSizeBits; i++)
                        if (division[i] != null)
                            buckets[i] = new TriesElement(division[i], triesStartMask >> BucketSizeBits, triesStartBits - BucketSizeBits, minValue | ((ulong)i << triesStartBits), minValue | (((ulong)i + 1) << triesStartBits) - 1);
                }
                else
                {
                    location = ranges.First().Item1;
                }
            }
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
