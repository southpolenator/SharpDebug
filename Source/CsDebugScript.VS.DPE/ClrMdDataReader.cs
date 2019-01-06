using Microsoft.Diagnostics.Runtime;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsDebugScript.VS.DPE
{
    /// <summary>
    /// Data reader necessary for Microsoft.Diagnostics.Runtime library initialization
    /// </summary>
    internal class ClrMdDataReader : IDataReader
    {
        /// <summary>
        /// Cache of modules list.
        /// </summary>
        private List<ModuleInfo> modulesCache = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdDataReader"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        public ClrMdDataReader(DkmProcess process)
        {
            Process = process;
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public DkmProcess Process { get; private set; }

        /// <summary>
        /// Returns true if this data reader can read data out of the target process asynchronously.
        /// </summary>
        public bool CanReadAsync => false;

        /// <summary>
        /// Returns true if the data target is a minidump (or otherwise may not contain full heap data).
        /// </summary>
        public bool IsMinidump => false;

        /// <summary>
        /// Called when the DataTarget is closing (Disposing).  Used to clean up resources.
        /// </summary>
        public void Close()
        {
            // Do nothing
        }

        /// <summary>
        /// Enumerates the OS thread ID of all threads in the process.
        /// </summary>
        /// <returns>
        /// An enumeration of all threads in the target process.
        /// </returns>
        public IEnumerable<uint> EnumerateAllThreads()
        {
            return Process.GetSystemThreads().Concat(Process.GetThreads()).Select(t => (uint)t.SystemPart.Id);
        }

        /// <summary>
        /// Enumerates modules in the target process.
        /// </summary>
        /// <returns>
        /// A list of the modules in the target process.
        /// </returns>
        public IList<ModuleInfo> EnumerateModules()
        {
            if (modulesCache == null)
                lock (this)
                {
                    DkmNativeRuntimeInstance runtimeInstance = Process.GetRuntimeInstances().OfType<DkmNativeRuntimeInstance>().FirstOrDefault();
                    List<ModuleInfo> modules;

                    if (runtimeInstance != null)
                        modules = runtimeInstance.GetModuleInstances().Select(m =>
                        {
                            return new ModuleInfo(this)
                            {
                                FileName = m.FullName,
                                ImageBase = m.BaseAddress,
                                FileSize = m.Size,
                                TimeStamp = (uint)Math.Round((DateTime.FromFileTime((long)m.TimeDateStamp) - new DateTime(1970, 1, 1)).TotalSeconds),
                                Version = ExtractVersion(m.Version?.FileVersionString),
                            };
                        }).ToList();
                    else
                    {
                        System.Diagnostics.Process diagProcess = System.Diagnostics.Process.GetProcessById(Process.LivePart.Id);

                        modules = new List<ModuleInfo>();
                        foreach (System.Diagnostics.ProcessModule module in diagProcess.Modules)
                            modules.Add(new ModuleInfo(this)
                            {
                                FileName = module.FileName,
                                ImageBase = (ulong)module.BaseAddress.ToInt64(),
                                FileSize = (uint)module.ModuleMemorySize,
                                TimeStamp = (uint)Math.Round((File.GetCreationTime(module.FileName) - new DateTime(1970, 1, 1)).TotalSeconds),
                                Version = new VersionInfo()
                                {
                                    Major = module.FileVersionInfo.FileMajorPart,
                                    Minor = module.FileVersionInfo.FileMinorPart,
                                    Revision = module.FileVersionInfo.FileBuildPart,
                                    Patch = module.FileVersionInfo.FilePrivatePart,
                                },
                            });
                    }
                    modulesCache = modules;
                }
            return modulesCache;
        }

        /// <summary>
        /// Informs the data reader that the user has requested all data be flushed.
        /// </summary>
        public void Flush()
        {
            // Do nothing?!?
        }

        /// <summary>
        /// Gets the architecture of the target.
        /// </summary>
        /// <returns>
        /// The architecture of the target.
        /// </returns>
        public Architecture GetArchitecture()
        {
            switch (Process.SystemInformation.ProcessorArchitecture)
            {
                case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL:
                case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64:
                    return (Process.SystemInformation.Flags & Microsoft.VisualStudio.Debugger.DefaultPort.DkmSystemInformationFlags.Is64Bit) != 0
                        ? Architecture.Amd64 : Architecture.X86;
                case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM:
                    return Architecture.Arm;
                default:
                    return Architecture.Unknown;
            }
        }

        /// <summary>
        /// Gets the size of a pointer in the target process.
        /// </summary>
        /// <returns>
        /// The pointer size of the target process.
        /// </returns>
        public uint GetPointerSize()
        {
            switch (GetArchitecture())
            {
                case Architecture.X86:
                    return 4U;
                case Architecture.Amd64:
                    return 8U;
                default:
                    throw new Exception($"Unsupported architecture type: {GetArchitecture()}");
            }
        }

        /// <summary>
        /// Gets the thread context for the given thread.
        /// </summary>
        /// <param name="threadID">The OS thread ID to read the context from.</param>
        /// <param name="contextFlags">The requested context flags, or 0 for default flags.</param>
        /// <param name="contextSize">The size (in bytes) of the context parameter.</param>
        /// <param name="context">A pointer to the buffer to write to.</param>
        public bool GetThreadContext(uint threadID, uint contextFlags, uint contextSize, byte[] context)
        {
            try
            {
                DkmThread thread = Process.GetThreads().Concat(Process.GetSystemThreads()).First(t => t.SystemPart.Id == threadID);

                thread.GetContext((int)contextFlags, context);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the thread context for the given thread.
        /// </summary>
        /// <param name="threadID">The OS thread ID to read the context from.</param>
        /// <param name="contextFlags">The requested context flags, or 0 for default flags.</param>
        /// <param name="contextSize">The size (in bytes) of the context parameter.</param>
        /// <param name="context">A pointer to the buffer to write to.</param>
        public unsafe bool GetThreadContext(uint threadID, uint contextFlags, uint contextSize, IntPtr context)
        {
            try
            {
                DkmThread thread = Process.GetThreads().Concat(Process.GetSystemThreads()).First(t => t.SystemPart.Id == threadID);

                thread.GetContext((int)contextFlags, context.ToPointer(), (int)contextSize);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the TEB of the specified thread.
        /// </summary>
        /// <param name="thread">The OS thread ID to get the TEB for.</param>
        /// <returns>
        /// The address of the thread's teb.
        /// </returns>
        public ulong GetThreadTeb(uint thread)
        {
            return Process.GetThreads().Concat(Process.GetSystemThreads()).First(t => t.SystemPart.Id == thread).TebAddress;
        }

        /// <summary>
        /// Gets the version information.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="version">The version.</param>
        public void GetVersionInfo(ulong address, out VersionInfo version)
        {
            DkmModuleInstance module = Process.GetRuntimeInstances()
                .SelectMany(ri => ri.GetModuleInstances())
                .FirstOrDefault(m => m.BaseAddress <= address && address < m.BaseAddress + m.Size);
            string versionString = module?.Version?.FileVersionString;

            version = ExtractVersion(versionString);
        }

        /// <summary>
        /// Read an int out of the target process.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns>
        /// The int at the give address, or 0 if that pointer doesn't exist in
        /// the data target.
        /// </returns>
        public uint ReadDwordUnsafe(ulong addr)
        {
            try
            {
                byte[] buffer = new byte[4];

                Process.ReadMemory(addr, DkmReadMemoryFlags.None, buffer);
                return BitConverter.ToUInt32(buffer, 0);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Read memory out of the target process.
        /// </summary>
        /// <param name="address">The address of memory to read.</param>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="bytesRequested">The number of bytes to read.</param>
        /// <param name="bytesRead">The number of bytes actually read out of the target process.</param>
        /// <returns>
        /// True if any bytes were read at all, false if the read failed (and no bytes were read).
        /// </returns>
        public unsafe bool ReadMemory(ulong address, IntPtr buffer, int bytesRequested, out int bytesRead)
        {
            if (bytesRequested > 0)
                try
                {
                    bytesRead = Process.ReadMemory(address, DkmReadMemoryFlags.AllowPartialRead, buffer.ToPointer(), bytesRequested);
                    return bytesRead > 0;
                }
                catch
                {
                }
            bytesRead = 0;
            return false;
        }

        /// <summary>
        /// Read memory out of the target process.
        /// </summary>
        /// <param name="address">The address of memory to read.</param>
        /// <param name="buffer">The buffer to write to.</param>
        /// <param name="bytesRequested">The number of bytes to read.</param>
        /// <param name="bytesRead">The number of bytes actually read out of the target process.</param>
        /// <returns>
        /// True if any bytes were read at all, false if the read failed (and no bytes were read).
        /// </returns>
        public unsafe bool ReadMemory(ulong address, byte[] buffer, int bytesRequested, out int bytesRead)
        {
            if (bytesRequested > 0)
                fixed (byte* pointer = buffer)
                    return ReadMemory(address, new IntPtr(pointer), bytesRequested, out bytesRead);
            bytesRead = 0;
            return false;
        }

        /// <summary>
        /// Read a pointer out of the target process.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns>
        /// The pointer at the give address, or 0 if that pointer doesn't exist in
        /// the data target.
        /// </returns>
        public ulong ReadPointerUnsafe(ulong addr)
        {
            try
            {
                uint pointerSize = GetPointerSize();
                byte[] buffer = new byte[pointerSize];

                Process.ReadMemory(addr, DkmReadMemoryFlags.None, buffer);
                if (pointerSize == 4)
                    return BitConverter.ToUInt32(buffer, 0);
                return BitConverter.ToUInt64(buffer, 0);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets information about the given memory range.
        /// </summary>
        /// <param name="addr">An arbitrary address in the target process.</param>
        /// <param name="vq">The base address and size of the allocation.</param>
        /// <returns>
        /// True if the address was found and vq was filled, false if the address is not valid memory.
        /// </returns>
        public bool VirtualQuery(ulong addr, out VirtualQueryData vq)
        {
            vq = new VirtualQueryData();
            return false;
        }

        /// <summary>
        /// Extracts version info from version formatted string.
        /// </summary>
        /// <param name="versionString">Version formatted string.</param>
        private static VersionInfo ExtractVersion(string versionString)
        {
            VersionInfo version = new VersionInfo();

            if (versionString != null)
            {
                try
                {
                    string[] entries = versionString.Split("v. ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    version.Major = int.Parse(entries[0]);
                    version.Minor = int.Parse(entries[1]);
                    version.Revision = int.Parse(entries[2]);
                    version.Patch = int.Parse(entries[3]);
                }
                catch
                {
                }
            }
            return version;
        }
    }
}
