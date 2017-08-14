using ELFSharp.ELF;
using ELFSharp.ELF.Segments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using CsDebugScript.Engine.Utility;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// Simple ELF core dump reader.
    /// </summary>
    public class ElfCoreDump
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
        /// The ELF note files loaded in the dump
        /// </summary>
        private elf_note_file[] files = new elf_note_file[0];

        /// <summary>
        /// Memory reader for ELF core dump files.
        /// </summary>
        /// <seealso cref="CsDebugScript.Engine.Utility.DumpFileMemoryReader" />
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
            Path = coreDumpPath;

            foreach (var segment in elf.Segments)
            {
                if (segment.Type == SegmentType.Note)
                {
                    DwarfMemoryReader reader = new DwarfMemoryReader(ReadSegment(segment));
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

                        Console.WriteLine($"{name}: {note.n_type} [{note.n_descsz}]");
                        if (note.n_type == elf_note_type.Prstatus)
                        {
                            DwarfMemoryReader data = new DwarfMemoryReader(content);
                            elf_prstatus prstatus = data.ReadStructure<elf_prstatus>();

                            threads.Add(prstatus);
                        }
                        else if (note.n_type == elf_note_type.Prpsinfo)
                        {
                            // TODO: Use when needed
                            //DwarfMemoryReader data = new DwarfMemoryReader(content);
                            //elf_prpsinfo prpsinfo = data.ReadStructure<elf_prpsinfo>();
                            //Console.WriteLine($"  Filename: {prpsinfo.Filename}");
                            //Console.WriteLine($"  ArgList: {prpsinfo.ArgList}");
                        }
                        else if (note.n_type == elf_note_type.File)
                        {
                            DwarfMemoryReader data = new DwarfMemoryReader(content);

                            files = elf_note_file.Parse(data);
                        }
                    }
                }
            }

            DumpFileMemoryReader = new CoreDumpReader(coreDumpPath, elf.Segments.Where(s => s.Type == SegmentType.Load));

            if (threads.Count > 0)
            {
                ProcessId = (uint)threads[0].pr_pid;
            }
        }

        /// <summary>
        /// Gets the ELF core dump path.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the dumped process identifier.
        /// </summary>
        public uint ProcessId { get; private set; }

        /// <summary>
        /// Gets the dump file memory reader.
        /// </summary>
        internal DumpFileMemoryReader DumpFileMemoryReader { get; private set; }

        /// <summary>
        /// Gets the array of thread ids available in the dump.
        /// </summary>
        public int[] GetThreadIds()
        {
            int[] ids = new int[threads.Count];

            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = threads[i].pr_pid;
            }
            return ids;
        }

        /// <summary>
        /// Gets the thread stack trace.
        /// </summary>
        /// <param name="threadIndex">Index of the thread.</param>
        /// <returns>Array of tuples of instruction offset, stack offset and frame offset.</returns>
        public Tuple<ulong, ulong, ulong>[] GetThreadStackTrace(int threadIndex)
        {
            elf_prstatus prstatus = threads[threadIndex];
            ulong bp = prstatus.pr_reg[X64RegisterIndex.RBP];
            ulong ip = prstatus.pr_reg[X64RegisterIndex.RIP];

            return GetThreadStackTrace(bp, ip);
        }

        /// <summary>
        /// Gets the thread stack trace for Intel x86 and AMD64 platforms.
        /// </summary>
        /// <param name="bp">The base pointer.</param>
        /// <param name="ip">The instruction pointer.</param>
        /// <returns>Array of tuples of instruction offset, stack offset and frame offset.</returns>
        private Tuple<ulong, ulong, ulong>[] GetThreadStackTrace(ulong bp, ulong ip)
        {
            List<Tuple<ulong, ulong, ulong>> result = new List<Tuple<ulong, ulong, ulong>>();
            bool is64bit = elf.Class == Class.Bit64;
            int pointerSize = is64bit ? 8 : 4;
            ulong segmentStartAddress, segmentSize;

            DumpFileMemoryReader.GetMemoryRange(bp, out segmentStartAddress, out segmentSize);
            while (bp >= segmentStartAddress && bp < segmentStartAddress + segmentSize)
            {
                result.Add(Tuple.Create(ip, bp, bp));

                MemoryBuffer buffer = DumpFileMemoryReader.ReadMemory(bp, pointerSize * 2);
                bp = UserType.ReadPointer(buffer, 0, pointerSize);
                ip = UserType.ReadPointer(buffer, pointerSize, pointerSize);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Gets base address of all loaded modules.
        /// </summary>
        /// <returns>Array of base addresses.</returns>
        public ulong[] GetModulesBaseAddresses()
        {
            Dictionary<string, ulong> baseAddresses = new Dictionary<string, ulong>();

            foreach (elf_note_file file in files)
            {
                if (!baseAddresses.ContainsKey(file.name))
                {
                    baseAddresses.Add(file.name, file.start);
                }
                else
                {
                    baseAddresses[file.name] = Math.Min(file.start, baseAddresses[file.name]);
                }
            }

            return baseAddresses.Values.ToArray();
        }

        /// <summary>
        /// Gets the size of the module.
        /// </summary>
        /// <param name="baseAddress">The module base address.</param>
        public ulong GetModuleSize(ulong baseAddress)
        {
            string name = files.First(f => f.start == baseAddress).name;
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
        /// Reads the segment from the dump.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        private byte[] ReadSegment(Segment<ulong> segment)
        {
            try
            {
                return segment.GetContents();
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

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
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

            public static elf_note_file[] Parse(DwarfMemoryReader reader)
            {
                ulong count = reader.ReadUlong();
                ulong page_size = reader.ReadUlong();

                elf_note_file[] files = new elf_note_file[count];

                for (int i = 0; i < files.Length; i++)
                {
                    files[i].start = reader.ReadUlong();
                    files[i].end = reader.ReadUlong();
                    files[i].file_ofs = reader.ReadUlong() * page_size;
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
