using System;
using System.Runtime.InteropServices;

namespace CsDebugScript.Engine.Utility
{
    internal unsafe class WindowsDumpFileMemoryReader : DumpFileMemoryReader
    {
        public WindowsDumpFileMemoryReader(string dumpFilePath)
            : base(dumpFilePath)
        {
            bool dispose = true;

            try
            {
                IntPtr streamPointer = IntPtr.Zero;
                uint streamSize = 0;
                MINIDUMP_DIRECTORY directory = new MINIDUMP_DIRECTORY();

                if (!MiniDumpReadDumpStream((IntPtr)basePointer, MINIDUMP_STREAM_TYPE.Memory64ListStream, ref directory, ref streamPointer, ref streamSize))
                {
                    throw new Exception("Unable to read mini dump stream");
                }

                var data = (MINIDUMP_MEMORY64_LIST)Marshal.PtrToStructure(streamPointer, typeof(MINIDUMP_MEMORY64_LIST));
                ulong lastEnd = data.BaseRva;
                MemoryLocation[] ranges = new MemoryLocation[data.NumberOfMemoryRanges];

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
                Initialize(ranges);
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
