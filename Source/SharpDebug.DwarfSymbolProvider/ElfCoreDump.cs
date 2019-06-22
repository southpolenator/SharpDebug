using SharpDebug.Engine;
using SharpDebug.Engine.Utility;
using ELFSharp.ELF;
using ELFSharp.ELF.Segments;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpDebug.DwarfSymbolProvider
{
    /// <summary>
    /// Simple ELF core dump reader.
    /// </summary>
    public class ElfCoreDump : IDisposable
    {
        /// <summary>
        /// The elf reader.
        /// </summary>
        private ELF<ulong> elf;

        /// <summary>
        /// The instance
        /// </summary>
        private IInstance instance;

        /// <summary>
        /// The ELF note files loaded in the dump
        /// </summary>
        private elf_note_file[] files = new elf_note_file[0];

        /// <summary>
        /// AUX vector.
        /// </summary>
        private List<AuxvEntry> auxVector = new List<AuxvEntry>();

        /// <summary>
        /// Memory reader for ELF core dump files.
        /// </summary>
        /// <seealso cref="SharpDebug.Engine.Utility.DumpFileMemoryReader" />
        private class CoreDumpReader : DumpFileMemoryReader
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CoreDumpReader" /> class.
            /// </summary>
            /// <param name="dumpFilePath">The dump file path.</param>
            /// <param name="segments">The segments.</param>
            public CoreDumpReader(string dumpFilePath, IEnumerable<Segment<ulong>> segments)
                : base(dumpFilePath)
            {
                Segment<ulong>[] segmentsArray = segments.OrderBy(s => s.Address).ToArray();
                MemoryLocation[] ranges = new MemoryLocation[segmentsArray.Length];

                for (int i = 0; i < ranges.Length; i++)
                {
                    ranges[i] = new MemoryLocation()
                    {
                        MemoryStart = segmentsArray[i].Address,
                        MemoryEnd = segmentsArray[i].Address + (ulong)segmentsArray[i].FileSize,
                        FilePosition = (ulong)segmentsArray[i].Offset,
                    };
                }
                Initialize(ranges);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElfCoreDump"/> class.
        /// </summary>
        /// <param name="coreDumpPath">The core dump path.</param>
        public ElfCoreDump(string coreDumpPath)
        {
            elf = ELFReader.Load<ulong>(coreDumpPath);
            if (elf.Type != FileType.Core)
            {
                throw new Exception($"Expected core dump, but got: {elf.Type}");
            }
            switch (elf.Machine)
            {
                case Machine.Intel386:
                    instance = new Intel386Instance(elf);
                    break;
                case Machine.AMD64:
                    instance = new AMD64Instance(elf);
                    break;
                default:
                    throw new Exception($"Unsupported machine type: {elf.Machine}");
            }
            Path = coreDumpPath;

            foreach (var segment in elf.Segments)
            {
                if (segment.Type == SegmentType.Note)
                {
                    using (DwarfMemoryReader reader = new DwarfMemoryReader(ReadSegment(segment)))
                    {
                        int noteStructSize = Marshal.SizeOf<elf_32note>();

                        while (reader.Position + noteStructSize < reader.Data.Length)
                        {
                            // Read note
                            elf_32note note = reader.ReadStructure<elf_32note>();
                            int nameEnd = reader.Position + (int)note.NameSize;

                            // Check if note is available to be read
                            if (nameEnd + note.n_descsz > reader.Data.Length)
                            {
                                break;
                            }

                            // Read name and content
                            string name = reader.ReadString();
                            reader.Position = nameEnd;
                            byte[] content = reader.ReadBlock(note.n_descsz);

                            instance.ProcessNote(name, content, note.n_type);
                            if (note.n_type == elf_note_type.File)
                            {
                                using (DwarfMemoryReader data = new DwarfMemoryReader(content))
                                {
                                    files = elf_note_file.Parse(data, Is64bit);
                                }
                            }
                            else if (note.n_type == elf_note_type.Auxv)
                            {
                                using (DwarfMemoryReader data = new DwarfMemoryReader(content))
                                {
                                    uint addressSize = elf.Class == Class.Bit32 ? 4U : 8U;

                                    while (!data.IsEnd)
                                    {
                                        AuxvEntry entry = new AuxvEntry
                                        {
                                            Type = (AuxvEntryType)data.ReadUlong(addressSize),
                                            Value = data.ReadUlong(addressSize)
                                        };

                                        if (entry.Type == AuxvEntryType.Null)
                                        {
                                            break;
                                        }

                                        if (entry.Type == AuxvEntryType.Ignore)
                                        {
                                            continue;
                                        }

                                        auxVector.Add(entry);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            DumpFileMemoryReader = new CoreDumpReader(coreDumpPath, elf.Segments.Where(s => s.Type == SegmentType.Load));
        }

        /// <summary>
        /// Gets the ELF core dump path.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the dumped process identifier.
        /// </summary>
        public uint ProcessId
        {
            get
            {
                return instance.ProcessId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ElfCoreDump"/> is is 64bit dump.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is 64bit; otherwise, <c>false</c>.
        /// </value>
        public bool Is64bit
        {
            get
            {
                return elf.Class == Class.Bit64;
            }
        }

        /// <summary>
        /// Gets the dump file memory reader.
        /// </summary>
        internal DumpFileMemoryReader DumpFileMemoryReader { get; private set; }

        /// <summary>
        /// Gets the array of thread ids available in the dump.
        /// </summary>
        public int[] GetThreadIds()
        {
            return instance.GetThreadIds();
        }

        /// <summary>
        /// Gets the thread stack trace.
        /// </summary>
        /// <param name="threadIndex">Index of the thread.</param>
        /// <param name="process">Process being debugged.</param>
        /// <param name="symbolProvider">The symbol provider.</param>
        /// <returns>Array of tuples of instruction offset, stack offset and frame offset.</returns>
        public Tuple<ulong, ulong, ulong>[] GetThreadStackTrace(int threadIndex, Process process, DwarfSymbolProvider symbolProvider)
        {
            return instance.GetThreadStackTrace(threadIndex, DumpFileMemoryReader, process, symbolProvider);
        }

        /// <summary>
        /// Gets base address of all loaded modules.
        /// </summary>
        /// <returns>Array of base addresses.</returns>
        public ulong[] GetModulesBaseAddresses()
        {
            // Return list of files where offset is start of the file
            return files.Where(f => f.file_ofs == 0).Select(f => f.start).ToArray();
        }

        /// <summary>
        /// Gets the module load offset.
        /// </summary>
        /// <param name="baseAddress">The module base address.</param>
        public ulong GetModuleLoadOffset(ulong baseAddress)
        {
            // Check if we are looking for load offset of main module
            if (baseAddress == files[0].start)
            {
                // Find file with symbols for main module
                string mainModulePath = files[0].name;

                if (!string.IsNullOrEmpty(mainModulePath))
                {
                    mainModulePath = ElfCoreDumpDebuggingEngine.GetModuleMappedImage(this, mainModulePath);
                }

                // Find offset for main module
                ulong offset = 0;

                if (!string.IsNullOrEmpty(mainModulePath) && File.Exists(mainModulePath))
                {
                    var elf = ELFReader.Load<ulong>(mainModulePath);
                    foreach (AuxvEntry entry in auxVector)
                    {
                        if (entry.Type == AuxvEntryType.Entry)
                        {
                            offset = entry.Value - elf.EntryPoint;
                            break;
                        }
                    }
                }

                return offset;
            }

            return 0;
        }

        /// <summary>
        /// Gets the size of the module.
        /// </summary>
        /// <param name="baseAddress">The module base address.</param>
        public ulong GetModuleSize(ulong baseAddress)
        {
            string name = files.First(f => f.start == baseAddress).name;
            string imagePath = ElfCoreDumpDebuggingEngine.GetModuleMappedImage(this, name);

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                // Return file size
                return (ulong)(new FileInfo(imagePath).Length);
            }

            ulong endAddress = 0;

            foreach (elf_note_file file in files)
            {
                if (file.name == name)
                {
                    endAddress = Math.Max(endAddress, file.end);
                }
            }

            return endAddress - baseAddress;
        }

        /// <summary>
        /// Gets the module original path.
        /// </summary>
        /// <param name="baseAddress">The module base address.</param>
        public string GetModuleOriginalPath(ulong baseAddress)
        {
            return files.First(f => f.start == baseAddress).name;
        }

        /// <summary>
        /// Gets the architecture type.
        /// </summary>
        public ArchitectureType GetProcessArchitectureType()
        {
            return instance.GetProcessArchitectureType();
        }

        /// <summary>
        /// Reads the segment from the dump.
        /// </summary>
        /// <param name="segment">The segment.</param>
        private byte[] ReadSegment(Segment<ulong> segment)
        {
            try
            {
                return segment.GetFileContents();
            }
            catch
            {
                using (FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] bytes = new byte[segment.FileSize];

                    stream.Seek(segment.Offset, SeekOrigin.Begin);
                    stream.Read(bytes, 0, bytes.Length);
                    return bytes;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            elf.Dispose();
            DumpFileMemoryReader.Dispose();
        }

        /// <summary>
        /// Interface that explains what different platforms will provide
        /// </summary>
        private interface IInstance
        {
            /// <summary>
            /// Gets the dumped process identifier.
            /// </summary>
            uint ProcessId { get; }

            /// <summary>
            /// Processes the parsed note.
            /// </summary>
            /// <param name="name">The note name.</param>
            /// <param name="content">The note content.</param>
            /// <param name="type">The note type.</param>
            void ProcessNote(string name, byte[] content, elf_note_type type);

            /// <summary>
            /// Gets the architecture type.
            /// </summary>
            ArchitectureType GetProcessArchitectureType();

            /// <summary>
            /// Gets the array of thread ids available in the dump.
            /// </summary>
            int[] GetThreadIds();

            /// <summary>
            /// Gets the thread stack trace.
            /// </summary>
            /// <param name="threadIndex">Index of the thread.</param>
            /// <param name="dumpFileMemoryReader">The dump file memory reader.</param>
            /// <param name="process">Process being debugged.</param>
            /// <param name="symbolProvider">The symbol provider.</param>
            /// <returns>Array of tuples of instruction offset, stack offset and frame offset.</returns>
            Tuple<ulong, ulong, ulong>[] GetThreadStackTrace(int threadIndex, DumpFileMemoryReader dumpFileMemoryReader, Process process, DwarfSymbolProvider symbolProvider);
        }

        /// <summary>
        /// Implementation for Intel386 platform
        /// </summary>
        /// <seealso cref="SharpDebug.DwarfSymbolProvider.ElfCoreDump.IInstance" />
        private class Intel386Instance : IInstance
        {
            /// <summary>
            /// The elf reader.
            /// </summary>
            private ELF<ulong> elf;

            /// <summary>
            /// The list of available threads in the dump
            /// </summary>
            private List<elf_prstatus> threads = new List<elf_prstatus>();

            /// <summary>
            /// Initializes a new instance of the <see cref="Intel386Instance"/> class.
            /// </summary>
            /// <param name="elf">The elf reader.</param>
            public Intel386Instance(ELF<ulong> elf)
            {
                this.elf = elf;
            }

            /// <summary>
            /// Gets the dumped process identifier.
            /// </summary>
            public uint ProcessId
            {
                get
                {
                    return (uint)threads[0].ProcessId;
                }
            }

            /// <summary>
            /// Processes the parsed note.
            /// </summary>
            /// <param name="name">The note name.</param>
            /// <param name="content">The note content.</param>
            /// <param name="type">The note type.</param>
            public void ProcessNote(string name, byte[] content, elf_note_type type)
            {
                if (type == elf_note_type.Prstatus)
                {
                    using (DwarfMemoryReader data = new DwarfMemoryReader(content))
                    {
                        elf_prstatus prstatus = data.ReadStructure<elf_prstatus>();

                        threads.Add(prstatus);
                    }
                }
                else if (type == elf_note_type.Prpsinfo)
                {
                    // TODO: Use when needed
                    //using (DwarfMemoryReader data = new DwarfMemoryReader(content))
                    //{
                    //    elf_prpsinfo prpsinfo = data.ReadStructure<elf_prpsinfo>();
                    //    Console.WriteLine($"  Filename: {prpsinfo.Filename}");
                    //    Console.WriteLine($"  ArgList: {prpsinfo.ArgList}");
                    //}
                }
            }

            /// <summary>
            /// Gets the array of thread ids available in the dump.
            /// </summary>
            public int[] GetThreadIds()
            {
                int[] ids = new int[threads.Count];

                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = threads[i].ProcessId;
                }
                return ids;
            }

            /// <summary>
            /// Gets the thread stack trace.
            /// </summary>
            /// <param name="threadIndex">Index of the thread.</param>
            /// <param name="dumpFileMemoryReader">The dump file memory reader.</param>
            /// <param name="process">Process being debugged.</param>
            /// <param name="symbolProvider">The symbol provider.</param>
            /// <returns>Array of tuples of instruction offset, stack offset and frame offset.</returns>
            public Tuple<ulong, ulong, ulong>[] GetThreadStackTrace(int threadIndex, DumpFileMemoryReader dumpFileMemoryReader, Process process, DwarfSymbolProvider symbolProvider)
            {
                const int pointerSize = 4;
                elf_prstatus prstatus = threads[threadIndex];
                ulong bp = prstatus.pr_reg[X86RegisterIndex.EBP];
                ulong ip = prstatus.pr_reg[X86RegisterIndex.EIP];
                List<Tuple<ulong, ulong, ulong>> result = new List<Tuple<ulong, ulong, ulong>>();
                ulong segmentStartAddress, segmentSize;

                dumpFileMemoryReader.GetMemoryRange(bp, out segmentStartAddress, out segmentSize);
                while (bp >= segmentStartAddress && bp < segmentStartAddress + segmentSize)
                {
                    result.Add(Tuple.Create(ip, bp, bp));

                    ulong savedLocationForRegisters = bp;
                    Module module = process.GetModuleByInnerAddress(ip);

                    if (module != null)
                    {
                        DwarfSymbolProviderModule symbolProviderModule = symbolProvider.GetSymbolProviderModule(module) as DwarfSymbolProviderModule;

                        if (symbolProviderModule != null)
                        {
                            ThreadContext frameContext = new ThreadContext(ip, bp, bp, null);
                            ulong canonicalFrameAddress = symbolProviderModule.GetFunctionCanonicalFrameAddress(process, ip, frameContext);

                            if (canonicalFrameAddress != 0)
                            {
                                savedLocationForRegisters = canonicalFrameAddress - pointerSize * 2;
                            }
                        }
                    }

                    MemoryBuffer buffer = dumpFileMemoryReader.ReadMemory(savedLocationForRegisters, pointerSize * 2);
                    bp = UserType.ReadPointer(buffer, 0, pointerSize);
                    ip = UserType.ReadPointer(buffer, pointerSize, pointerSize);
                }
                return result.ToArray();
            }

            /// <summary>
            /// Gets the architecture type.
            /// </summary>
            public ArchitectureType GetProcessArchitectureType()
            {
                return ArchitectureType.X86;
            }

            #region Native structures
            private static class X86RegisterIndex
            {
                public const int EBP = 5;
                public const int EIP = 12;
                public const int ESP = 15;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            private struct timeval
            {
                /// <summary>
                /// Seconds
                /// </summary>
                public int tv_sec;

                /// <summary>
                /// Microseconds
                /// </summary>
                public int tv_usec;
            }

            /// <summary>
            /// Definitions to generate Intel SVR4-like core files.
            /// These mostly have the same names as the SVR4 types with "elf_"
            /// tacked on the front to prevent clashes with linux definitions,
            /// and the typedef forms have been avoided.This is mostly like
            /// the SVR4 structure, but more Linuxy, with things that Linux does
            /// not support and which gdb doesn't really use excluded.
            /// Fields present but not used are marked with "XXX".
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            private struct elf_prstatus
            {

                /// <summary>
                /// Info associated with signal
                /// </summary>
                public elf_siginfo pr_info;

                /// <summary>
                /// Current signal
                /// </summary>
                public short pr_cursig;

                /// <summary>
                /// Set of pending signals
                /// </summary>
                public uint pr_sigpend;

                /// <summary>
                /// Set of held signals
                /// </summary>
                public uint pr_sighold;

                public int pr_pid;

                public int pr_ppid;

                public int pr_pgrp;

                public int pr_sid;

                /// <summary>
                /// User time
                /// </summary>
                public timeval pr_utime;

                /// <summary>
                /// System time
                /// </summary>
                public timeval pr_stime;

                /// <summary>
                /// Cumulative user time
                /// </summary>
                public timeval pr_cutime;

                /// <summary>
                /// Cumulative system time
                /// </summary>
                public timeval pr_cstime;

                /// <summary>
                /// GP registers
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
                public uint[] pr_reg;

                /// <summary>
                /// True if math co-processor being used.
                /// </summary>
                public int pr_fpvalid;

                public int ProcessId
                {
                    get
                    {
                        return pr_pid;
                    }
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            private struct elf_prpsinfo
            {
                /// <summary>
                /// Numeric process state
                /// </summary>
                public sbyte pr_state;

                /// <summary>
                /// Char for pr_state
                /// </summary>
                public sbyte pr_sname;

                /// <summary>
                /// zombie
                /// </summary>
                public sbyte pr_zomb;

                /// <summary>
                /// nice val
                /// </summary>
                public sbyte pr_nice;

                /// <summary>
                /// Flags
                /// </summary>
                public uint pr_flag;

                public ushort pr_uid;

                public ushort pr_gid;

                public int pr_pid;

                public int pr_ppid;

                public int pr_pgrp;

                public int pr_sid;

                /// <summary>
                /// Filename of executable
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                byte[] pr_fname;

                /// <summary>
                /// Initial part of arg list
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
                byte[] pr_psargs;

                public string Filename
                {
                    get
                    {
                        return Encoding.UTF8.GetString(pr_fname, 0, ZeroIndex(pr_fname));
                    }
                }

                public string ArgList
                {
                    get
                    {
                        return Encoding.UTF8.GetString(pr_psargs, 0, ZeroIndex(pr_psargs));
                    }
                }

                private int ZeroIndex(byte[] bytes)
                {
                    for (int index = 0; index < bytes.Length; index++)
                    {
                        if (bytes[index] == 0)
                        {
                            return index;
                        }
                    }

                    return bytes.Length;
                }
            }
            #endregion
        }

        /// <summary>
        /// Implementation for AMD64 platform
        /// </summary>
        /// <seealso cref="SharpDebug.DwarfSymbolProvider.ElfCoreDump.IInstance" />
        private class AMD64Instance : IInstance
        {
            /// <summary>
            /// The elf reader.
            /// </summary>
            private ELF<ulong> elf;

            /// <summary>
            /// The list of available threads in the dump
            /// </summary>
            private List<elf_prstatus> threads = new List<elf_prstatus>();

            /// <summary>
            /// Initializes a new instance of the <see cref="AMD64Instance"/> class.
            /// </summary>
            /// <param name="elf">The elf reader.</param>
            public AMD64Instance(ELF<ulong> elf)
            {
                this.elf = elf;
            }

            /// <summary>
            /// Gets the dumped process identifier.
            /// </summary>
            public uint ProcessId
            {
                get
                {
                    return (uint)threads[0].ProcessId;
                }
            }

            /// <summary>
            /// Processes the parsed note.
            /// </summary>
            /// <param name="name">The note name.</param>
            /// <param name="content">The note content.</param>
            /// <param name="type">The note type.</param>
            public void ProcessNote(string name, byte[] content, elf_note_type type)
            {
                if (type == elf_note_type.Prstatus)
                {
                    using (DwarfMemoryReader data = new DwarfMemoryReader(content))
                    {
                        elf_prstatus prstatus = data.ReadStructure<elf_prstatus>();

                        threads.Add(prstatus);
                    }
                }
                else if (type == elf_note_type.Prpsinfo)
                {
                    // TODO: Use when needed
                    //using (DwarfMemoryReader data = new DwarfMemoryReader(content))
                    //{
                    //    elf_prpsinfo prpsinfo = data.ReadStructure<elf_prpsinfo>();
                    //    Console.WriteLine($"  Filename: {prpsinfo.Filename}");
                    //    Console.WriteLine($"  ArgList: {prpsinfo.ArgList}");
                    //}
                }
            }

            /// <summary>
            /// Gets the array of thread ids available in the dump.
            /// </summary>
            public int[] GetThreadIds()
            {
                int[] ids = new int[threads.Count];

                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = threads[i].ProcessId;
                }
                return ids;
            }

            /// <summary>
            /// Gets the thread stack trace.
            /// </summary>
            /// <param name="threadIndex">Index of the thread.</param>
            /// <param name="dumpFileMemoryReader">The dump file memory reader.</param>
            /// <param name="process">Process being debugged.</param>
            /// <param name="symbolProvider">The symbol provider.</param>
            /// <returns>Array of tuples of instruction offset, stack offset and frame offset.</returns>
            public Tuple<ulong, ulong, ulong>[] GetThreadStackTrace(int threadIndex, DumpFileMemoryReader dumpFileMemoryReader, Process process, DwarfSymbolProvider symbolProvider)
            {
                const int pointerSize = 8;
                elf_prstatus prstatus = threads[threadIndex];
                ulong bp = prstatus.pr_reg[X64RegisterIndex.RBP];
                ulong ip = prstatus.pr_reg[X64RegisterIndex.RIP];
                List<Tuple<ulong, ulong, ulong>> result = new List<Tuple<ulong, ulong, ulong>>();
                ulong segmentStartAddress, segmentSize;

                dumpFileMemoryReader.GetMemoryRange(bp, out segmentStartAddress, out segmentSize);
                while (bp >= segmentStartAddress && bp < segmentStartAddress + segmentSize)
                {
                    result.Add(Tuple.Create(ip, bp, bp));

                    ulong savedLocationForRegisters = bp;
                    Module module = process.GetModuleByInnerAddress(ip);

                    if (module != null)
                    {
                        DwarfSymbolProviderModule symbolProviderModule = symbolProvider.GetSymbolProviderModule(module) as DwarfSymbolProviderModule;

                        if (symbolProviderModule != null)
                        {
                            ThreadContext frameContext = new ThreadContext(ip, bp, bp, null);
                            ulong canonicalFrameAddress = symbolProviderModule.GetFunctionCanonicalFrameAddress(process, ip, frameContext);

                            if (canonicalFrameAddress != 0)
                            {
                                savedLocationForRegisters = canonicalFrameAddress - pointerSize * 2;
                            }
                        }
                    }

                    MemoryBuffer buffer = dumpFileMemoryReader.ReadMemory(savedLocationForRegisters, pointerSize * 2);
                    bp = UserType.ReadPointer(buffer, 0, pointerSize);
                    ip = UserType.ReadPointer(buffer, pointerSize, pointerSize);
                }
                return result.ToArray();
            }

            /// <summary>
            /// Gets the architecture type.
            /// </summary>
            public ArchitectureType GetProcessArchitectureType()
            {
                return ArchitectureType.Amd64;
            }

            #region Native structures
            private static class X64RegisterIndex
            {
                public const int R15 = 0;
                public const int R14 = 1;
                public const int R13 = 2;
                public const int R12 = 3;
                public const int RBP = 4;
                public const int RBX = 5;
                public const int R11 = 6;
                public const int R10 = 7;
                public const int R9 = 8;
                public const int R8 = 9;
                public const int RAX = 10;
                public const int RCX = 11;
                public const int RDX = 12;
                public const int RSI = 13;
                public const int RDI = 14;
                public const int OrigRAX = 15;
                public const int RIP = 16;
                public const int CS = 17;
                public const int EFlags = 18;
                public const int RSP = 19;
                public const int SS = 20;
                public const int FSBase = 21;
                public const int GSBase = 22;
                public const int DS = 23;
                public const int ES = 24;
                public const int FS = 25;
                public const int GS = 26;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 8)]
            private struct timeval
            {
                /// <summary>
                /// Seconds
                /// </summary>
                public long tv_sec;

                /// <summary>
                /// Microseconds
                /// </summary>
                public long tv_usec;
            }

            /// <summary>
            /// Definitions to generate Intel SVR4-like core files.
            /// These mostly have the same names as the SVR4 types with "elf_"
            /// tacked on the front to prevent clashes with linux definitions,
            /// and the typedef forms have been avoided.This is mostly like
            /// the SVR4 structure, but more Linuxy, with things that Linux does
            /// not support and which gdb doesn't really use excluded.
            /// Fields present but not used are marked with "XXX".
            /// </summary>
            [StructLayout(LayoutKind.Sequential, Pack = 8)]
            private struct elf_prstatus
            {

                /// <summary>
                /// Info associated with signal
                /// </summary>
                public elf_siginfo pr_info;

                /// <summary>
                /// Current signal
                /// </summary>
                public short pr_cursig;

                /// <summary>
                /// Set of pending signals
                /// </summary>
                public ulong pr_sigpend;

                /// <summary>
                /// Set of held signals
                /// </summary>
                public ulong pr_sighold;

                public int pr_pid;

                public int pr_ppid;

                public int pr_pgrp;

                public int pr_sid;

                /// <summary>
                /// User time
                /// </summary>
                public timeval pr_utime;

                /// <summary>
                /// System time
                /// </summary>
                public timeval pr_stime;

                /// <summary>
                /// Cumulative user time
                /// </summary>
                public timeval pr_cutime;

                /// <summary>
                /// Cumulative system time
                /// </summary>
                public timeval pr_cstime;

                /// <summary>
                /// GP registers
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
                public ulong[] pr_reg;

                /// <summary>
                /// True if math co-processor being used.
                /// </summary>
                public int pr_fpvalid;

                public int ProcessId
                {
                    get
                    {
                        return pr_pid;
                    }
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 8)]
            private struct elf_prpsinfo
            {
                /// <summary>
                /// Numeric process state
                /// </summary>
                public sbyte pr_state;

                /// <summary>
                /// Char for pr_state
                /// </summary>
                public sbyte pr_sname;

                /// <summary>
                /// zombie
                /// </summary>
                public sbyte pr_zomb;

                /// <summary>
                /// nice val
                /// </summary>
                public sbyte pr_nice;

                /// <summary>
                /// Flags
                /// </summary>
                public ulong pr_flag;

                public uint pr_uid;

                public uint pr_gid;

                public int pr_pid;

                public int pr_ppid;

                public int pr_pgrp;

                public int pr_sid;

                /// <summary>
                /// Filename of executable
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                byte[] pr_fname;

                /// <summary>
                /// Initial part of arg list
                /// </summary>
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
                byte[] pr_psargs;

                public string Filename
                {
                    get
                    {
                        return Encoding.UTF8.GetString(pr_fname, 0, ZeroIndex(pr_fname));
                    }
                }

                public string ArgList
                {
                    get
                    {
                        return Encoding.UTF8.GetString(pr_psargs, 0, ZeroIndex(pr_psargs));
                    }
                }

                private int ZeroIndex(byte[] bytes)
                {
                    for (int index = 0; index < bytes.Length; index++)
                    {
                        if (bytes[index] == 0)
                        {
                            return index;
                        }
                    }

                    return bytes.Length;
                }
            }
            #endregion
        }

        private enum AuxvEntryType
        {
            /// <summary>
            /// [AUXV_AT_NULL] End of auxv.
            /// </summary>
            Null = 0,

            /// <summary>
            /// [AUXV_AT_IGNORE] Ignore entry.
            /// </summary>
            Ignore = 1,

            /// <summary>
            /// [AUXV_AT_EXECFD] File descriptor of program.
            /// </summary>
            ExecFileDescriptor = 2,

            /// <summary>
            /// [AUXV_AT_PHDR] Program headers.
            /// </summary>
            ProgramHeaders = 3,

            /// <summary>
            /// [AUXV_AT_PHENT] Size of program header.
            /// </summary>
            ProgramHeadersSize = 4,

            /// <summary>
            /// [AUXV_AT_PHNUM] Number of program headers.
            /// </summary>
            ProgramHeadersCount = 5,

            /// <summary>
            /// [AUXV_AT_PAGESZ] Page size.
            /// </summary>
            PageSize = 6,

            /// <summary>
            /// [AUXV_AT_BASE] Interpreter base address.
            /// </summary>
            Base = 7,

            /// <summary>
            /// [AUXV_AT_FLAGS] Flags.
            /// </summary>
            Flags = 8,

            /// <summary>
            /// [AUXV_AT_ENTRY] Program entry point.
            /// </summary>
            Entry = 9,

            /// <summary>
            /// [AUXV_AT_NOTELF] Set if program is not an ELF.
            /// </summary>
            NotElf = 10,

            /// <summary>
            /// [AUXV_AT_UID] UID.
            /// </summary>
            UID = 11,

            /// <summary>
            /// [AUXV_AT_EUID] Effective UID.
            /// </summary>
            EffectiveUID = 12,

            /// <summary>
            /// [AUXV_AT_GID] GID.
            /// </summary>
            GID = 13,

            /// <summary>
            /// [AUXV_AT_EGID] Effective GID.
            /// </summary>
            EffectiveGID = 14,

            /// <summary>
            /// [AUXV_AT_PLATFORM] String identifying platform.
            /// </summary>
            Platform = 15,

            /// <summary>
            /// [AUXV_AT_HWCAP] Machine dependent hints about processor capabilities.
            /// </summary>
            HardwareCapabilities = 16,

            /// <summary>
            /// [AUXV_AT_CLKTCK] Clock frequency (e.g. times(2)).
            /// </summary>
            ClockTicks = 17,

            /// <summary>
            /// [AUXV_AT_FPUCW] Used FPU control word.
            /// </summary>
            FPUControlWord = 18,

            /// <summary>
            /// [AUXV_AT_DCACHEBSIZE] Data cache block size.
            /// </summary>
            DataCacheBlockSize = 19,

            /// <summary>
            /// [AUXV_AT_ICACHEBSIZE] Instruction cache block size.
            /// </summary>
            InstructionCacheBlokSize = 20,

            /// <summary>
            /// [AUXV_AT_UCACHEBSIZE] Unified cache block size.
            /// </summary>
            UnifiedCacheBlockSize = 21,

            /// <summary>
            /// [AUXV_AT_IGNOREPPC] Entry should be ignored.
            /// </summary>
            IgnorePPC = 22,

            /// <summary>
            /// [AUXV_AT_SECURE] Boolean, was exec setuid-like?
            /// </summary>
            Secure = 23,

            /// <summary>
            /// [AUXV_AT_BASE_PLATFORM] String identifying real platforms.
            /// </summary>
            BasePlatform = 24,

            /// <summary>
            /// [AUXV_AT_RANDOM] Address of 16 random bytes.
            /// </summary>
            Random = 25,

            /// <summary>
            /// [AUXV_AT_EXECFN] Filename of executable.
            /// </summary>
            ExecutableFileName = 31,

            /// <summary>
            /// [AUXV_AT_SYSINFO] Pointer to the global system page used for system calls and other nice things.
            /// </summary>
            SystemInfo = 32,

            AUXV_AT_SYSINFO_EHDR = 33,
            AUXV_AT_L1I_CACHESHAPE = 34, // Shapes of the caches.
            AUXV_AT_L1D_CACHESHAPE = 35,
            AUXV_AT_L2_CACHESHAPE = 36,
            AUXV_AT_L3_CACHESHAPE = 37,
        };

        private struct AuxvEntry
        {
            public AuxvEntryType Type;
            public ulong Value;

            public override string ToString()
            {
                return $"{Type}: {Value}";
            }
        }

        #region Native structures
        private enum elf_note_type : uint
        {
            Prstatus = 1,
            Prfpreg = 2,
            Prpsinfo = 3,
            Taskstruct = 4,
            Auxv = 6,
            Siginfo = 0x53494749,
            File = 0x46494c45,
            Prxpfreg = 0x46e62b7f,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct elf_32note
        {
            /// <summary>
            /// Name size
            /// </summary>
            public uint n_namesz;

            /// <summary>
            /// Content size
            /// </summary>
            public uint n_descsz;

            /// <summary>
            /// Content type
            /// </summary>
            public elf_note_type n_type;

            public uint NameSize
            {
                get
                {
                    return (n_namesz + 3) / 4 * 4;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct elf_siginfo
        {
            /// <summary>
            /// Signal number
            /// </summary>
            public int si_signo;

            /// <summary>
            /// Extra code
            /// </summary>
            public int si_code;

            /// <summary>
            /// errno
            /// </summary>
            public int si_errno;
        }

        private struct elf_note_file
        {
            public ulong start;
            public ulong end;
            public ulong file_ofs;
            public string name;

            public ulong Length
            {
                get
                {
                    return end - start;
                }
            }

            public static elf_note_file[] Parse(DwarfMemoryReader reader, bool is64bit)
            {
                ulong count = is64bit ? reader.ReadUlong() : reader.ReadUint();
                ulong page_size = is64bit ? reader.ReadUlong() : reader.ReadUint();

                elf_note_file[] files = new elf_note_file[count];

                for (int i = 0; i < files.Length; i++)
                {
                    files[i].start = is64bit ? reader.ReadUlong() : reader.ReadUint();
                    files[i].end = is64bit ? reader.ReadUlong() : reader.ReadUint();
                    files[i].file_ofs = (is64bit ? reader.ReadUlong() : reader.ReadUint()) * page_size;
                }
                for (int i = 0; i < files.Length; i++)
                {
                    files[i].name = reader.ReadString();
                }
                return files;
            }

            public override string ToString()
            {
                return $"0x{start:X} - 0x{end:X} [{Length}] {name} + {file_ofs}";
            }
        }
        #endregion
    }
}
