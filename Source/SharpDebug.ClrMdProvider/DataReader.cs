using CsDebugScript.Engine;
using Microsoft.Diagnostics.Runtime;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Architecture = Microsoft.Diagnostics.Runtime.Architecture;

namespace CsDebugScript.ClrMdProvider
{
    /// <summary>
    /// Data reader necessary for Microsoft.Diagnostics.Runtime library initialization
    /// </summary>
    internal class DataReader : IDataReader
    {
        private SimpleCache<bool> isMinidump;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataReader"/> class.
        /// </summary>
        /// <param name="process">The process.</param>
        public DataReader(Process process)
        {
            Process = process;
            isMinidump = SimpleCache.Create(GetIsMinidump);
        }

        /// <summary>
        /// Gets the process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Returns true if this data reader can read data out of the target process asynchronously.
        /// </summary>
        public bool CanReadAsync
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the data target is a minidump (or otherwise may not contain full heap data).
        /// </summary>
        public bool IsMinidump
        {
            get
            {
                return isMinidump.Value;
            }
        }

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
            return Process.Threads.Select(t => t.SystemId);
        }

        /// <summary>
        /// Enumerates modules in the target process.
        /// </summary>
        /// <returns>
        /// A list of the modules in the target process.
        /// </returns>
        public IList<ModuleInfo> EnumerateModules()
        {
            return Process.OriginalModules.Select(m => new ModuleInfo(this)
            {
                TimeStamp = (uint)Math.Round((m.Timestamp - new DateTime(1970, 1, 1)).TotalSeconds),
                FileSize = (uint)m.Size,
                ImageBase = m.Address,
                FileName = m.ImageName,
            }).ToList();
        }

        /// <summary>
        /// Informs the data reader that the user has requested all data be flushed.
        /// </summary>
        public void Flush()
        {
            // TODO: Do nothing?!?
        }

        /// <summary>
        /// Gets the architecture of the target.
        /// </summary>
        /// <returns>
        /// The architecture of the target.
        /// </returns>
        public Architecture GetArchitecture()
        {
            switch (Process.ArchitectureType)
            {
                case ArchitectureType.Amd64:
                    return Architecture.Amd64;
                case ArchitectureType.X86:
                case ArchitectureType.X86OverAmd64:
                    return Architecture.X86;
                case ArchitectureType.Arm:
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
            return Process.GetPointerSize();
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
            var thread = Process.Threads.Where(t => t.SystemId == threadID).First();

            Array.Copy(thread.ThreadContext.Bytes, context, (int)contextSize);
            return true;
        }

        /// <summary>
        /// Gets the thread context for the given thread.
        /// </summary>
        /// <param name="threadID">The OS thread ID to read the context from.</param>
        /// <param name="contextFlags">The requested context flags, or 0 for default flags.</param>
        /// <param name="contextSize">The size (in bytes) of the context parameter.</param>
        /// <param name="context">A pointer to the buffer to write to.</param>
        public bool GetThreadContext(uint threadID, uint contextFlags, uint contextSize, IntPtr context)
        {
            var thread = Process.Threads.Where(t => t.SystemId == threadID).First();

            Marshal.Copy(thread.ThreadContext.Bytes, 0, context, (int)contextSize);
            return true;
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
            return Process.Threads.Where(t => t.SystemId == thread).First().TebAddress;
        }

        /// <summary>
        /// Gets the version information.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="version">The version.</param>
        public void GetVersionInfo(ulong address, out VersionInfo version)
        {
            Module module = Process.GetOriginalModuleByInnerAddress(address);
            var moduleVersion = module.ModuleVersion;

            version = new VersionInfo()
            {
                Major = moduleVersion.Major,
                Minor = moduleVersion.Minor,
                Patch = moduleVersion.Patch,
                Revision = moduleVersion.Revision,
            };
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
                return UserType.ReadUint(Debugger.ReadMemory(Process, addr, 4), 0);
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
        public bool ReadMemory(ulong address, IntPtr buffer, int bytesRequested, out int bytesRead)
        {
            try
            {
                // We might get pointer address that doesn't care about high bits, so we should trim it.
                if (address > uint.MaxValue && GetPointerSize() == 4)
                {
                    address = address & uint.MaxValue;
                }

                MemoryBuffer memoryBuffer = Debugger.ReadMemory(Process, address, (uint)bytesRequested);

                unsafe
                {
                    if (memoryBuffer.BytePointer != null)
                    {
                        byte* destination = (byte*)buffer.ToPointer();

                        MemoryBuffer.MemCpy(destination, memoryBuffer.BytePointer, (uint)bytesRequested);
                        bytesRead = memoryBuffer.BytePointerLength;
                    }
                    else
                    {
                        byte* destination = (byte*)buffer.ToPointer();

                        fixed (byte* bytePointer = memoryBuffer.Bytes)
                        {
                            MemoryBuffer.MemCpy(destination, bytePointer, (uint)bytesRequested);
                        }

                        bytesRead = memoryBuffer.Bytes.Length;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                bytesRead = 0;
                return false;
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
        public bool ReadMemory(ulong address, byte[] buffer, int bytesRequested, out int bytesRead)
        {
            try
            {
                MemoryBuffer memoryBuffer = Debugger.ReadMemory(Process, address, (uint)bytesRequested);

                unsafe
                {
                    if (memoryBuffer.BytePointer != null)
                    {
                        fixed (byte* destination = buffer)
                        {
                            MemoryBuffer.MemCpy(destination, memoryBuffer.BytePointer, (uint)bytesRequested);
                        }

                        bytesRead = memoryBuffer.BytePointerLength;
                    }
                    else
                    {
                        fixed (byte* destination = buffer)
                        fixed (byte* bytePointer = memoryBuffer.Bytes)
                        {
                            MemoryBuffer.MemCpy(destination, bytePointer, (uint)bytesRequested);
                        }

                        bytesRead = memoryBuffer.Bytes.Length;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                bytesRead = 0;
                return false;
            }
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

                return UserType.ReadPointer(Debugger.ReadMemory(Process, addr, pointerSize), 0, (int)pointerSize);
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
            try
            {
                var dumpReader = Process.DumpFileMemoryReader;
                ulong baseAddress, regionSize;

                if (dumpReader != null)
                    dumpReader.GetMemoryRange(addr, out baseAddress, out regionSize);
                else
                    Context.Debugger.QueryVirtual(Process, addr, out baseAddress, out regionSize);
                vq = new VirtualQueryData(baseAddress, regionSize);
                return true;
            }
            catch (Exception)
            {
                vq = new VirtualQueryData();
                return false;
            }
        }

        /// <summary>
        /// Returns true if the data target is a minidump (or otherwise may not contain full heap data).
        /// </summary>
        private bool GetIsMinidump()
        {
            return Context.Debugger.IsMinidump(Process);
        }
    }
}
