using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using CsDebugScript.VS.DPE;
using DIA;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.CallStack;
using Microsoft.VisualStudio.Debugger.Clr;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Symbols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Visual Studio Debugger Proxy object to default AppDomain
    /// </summary>
    /// <seealso cref="System.MarshalByRefObject" />
    internal class VSDebuggerProxy : MarshalByRefObject
    {
        public const string AppDomainDataName = "VSDebuggerProxy.Value";
        private static readonly Guid VS_SourceId = new Guid("CF7BFB1A-F3DD-4A81-A3E9-D1A3CAEED7E9");

        private struct ThreadStruct
        {
            public DkmThread Thread;
            public SimpleCache<DkmStackFrame[]> Frames;
        }

        /// <summary>
        /// The cached DKM processes
        /// </summary>
        private List<DkmProcess> processes = new List<DkmProcess>();

        /// <summary>
        /// The cached DKM threads
        /// </summary>
        private List<ThreadStruct> threads = new List<ThreadStruct>();

        /// <summary>
        /// The cached DKM modules
        /// </summary>
        private List<DkmModuleInstance> modules = new List<DkmModuleInstance>();

        /// <summary>
        /// The DKM component manager initialization for the thread
        /// </summary>
        private static System.Threading.ThreadLocal<bool> initializationForThread;

        /// <summary>
        /// Initializes the <see cref="VSDebuggerProxy"/> class.
        /// </summary>
        static VSDebuggerProxy()
        {
            initializationForThread = new System.Threading.ThreadLocal<bool>(() =>
            {
                try
                {
                    DkmComponentManager.InitializeThread(DkmComponentManager.IdeComponentId);
                    return true;
                }
                catch (DkmException ex)
                {
                    if (ex.Code == DkmExceptionCode.E_XAPI_ALREADY_INITIALIZED)
                        return true;
                }
                catch
                {
                }
                return false;
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSDebuggerProxy"/> class.
        /// </summary>
        public VSDebuggerProxy()
        {
            processes = new List<DkmProcess>();
        }

        /// <summary>
        /// Gets the cached DKM processes.
        /// </summary>
        internal List<DkmProcess> Processes
        {
            get
            {
                if (processes.Count == 0)
                {
                    ExecuteOnDkmInitializedThread(() =>
                    {
                        processes.AddRange(DkmProcess.GetProcesses());
                    });
                }

                return processes;
            }
        }

        #region Debugger functionality
        public bool IsProcessLiveDebugging(uint processId)
        {
            DkmProcess process = GetProcess(processId);

            return (process.SystemInformation.Flags & Microsoft.VisualStudio.Debugger.DefaultPort.DkmSystemInformationFlags.DumpFile) == 0;
        }

        public int[] GetAllProcesses()
        {
            return Processes.Select(p => p?.LivePart?.Id ?? 0).ToArray();
        }

        public int GetCurrentProcessSystemId()
        {
            return VSContext.DTE.Debugger.CurrentProcess.ProcessID;
        }

        public int GetCurrentThreadSystemId()
        {
            return VSContext.DTE.Debugger.CurrentThread.ID;
        }

        public int GetCurrentStackFrameNumber(int threadId)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;

            DkmStackWalkFrame frame = dispatcher.Invoke(() =>
            {
                var stackFrame = VSContext.DTE.Debugger.CurrentStackFrame;

                return DkmStackFrame.ExtractFromDTEObject(stackFrame);
            });
            DkmStackWalkFrame[] frames = GetThreadFrames((uint)threadId);

            for (int i = 0; i < frames.Length; i++)
                if (frames[i].FrameBase == frame.FrameBase)
                    return i;
            return -1;
        }

        public ulong GetModuleAddress(uint processId, string moduleName)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                moduleName = moduleName.ToLower();
                return process.GetRuntimeInstances().SelectMany(r => r.GetModuleInstances()).Where(m => GetModuleName(m).ToLower() == moduleName).Single().BaseAddress;
            });
        }

        public string GetModuleImageName(uint moduleId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmModuleInstance module = GetModule(moduleId);

                return module.FullName;
            });
        }

        public string GetModuleName(uint moduleId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmModuleInstance module = GetModule(moduleId);

                return GetModuleName(module);
            });
        }

        public string GetModuleSymbolName(uint moduleId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmModuleInstance module = GetModule(moduleId);
                DkmSymbolFileId symbolFileId = module.SymbolFileId;

                if (symbolFileId.TagValue == DkmSymbolFileId.Tag.PdbFileId)
                {
                    DkmPdbFileId pdbFileId = (DkmPdbFileId)symbolFileId;

                    if (File.Exists(pdbFileId.PdbName))
                        return pdbFileId.PdbName;

                    string symbolFilePath = module.Module?.GetSymbolFilePath();

                    if (File.Exists(symbolFilePath))
                        return symbolFilePath;
                }

                return module.FullName;
            });
        }

        public object GetModuleDiaSession(uint moduleId)
        {
            Func<object> executor = () =>
            {
                try
                {
                    DkmModuleInstance module = GetModule(moduleId);

                    return module.Module?.GetSymbolInterface(Marshal.GenerateGuidForType(typeof(IDiaSession)));
                }
                catch
                {
                    return null;
                }
            };

            object result = executor();

            if (result == null)
            {
                result = ExecuteOnDkmInitializedThread(executor);
            }
            return result;
        }

        public Tuple<DateTime, ulong> GetModuleTimestampAndSize(uint moduleId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmModuleInstance module = GetModule(moduleId);

                return Tuple.Create(DateTime.FromFileTimeUtc((long)module.TimeDateStamp), (ulong)module.Size);
            });
        }

        public void GetModuleVersion(uint moduleId, out int major, out int minor, out int revision, out int patch)
        {
            int tempMajor = 0, tempMinor = 0, tempRevision = 0, tempPatch = 0;

            ExecuteOnDkmInitializedThread(() =>
            {
                DkmModuleInstance module = GetModule(moduleId);

                if (module.Version != null)
                {
                    tempMajor = (int)(module.Version.ProductVersionMS / 65536);
                    tempMinor = (int)(module.Version.ProductVersionMS % 65536);
                    tempRevision = (int)(module.Version.ProductVersionLS / 65536);
                    tempPatch = (int)(module.Version.ProductVersionLS % 65536);
                }
                else
                {
                    tempMajor = tempMinor = tempRevision = tempPatch = 0;
                }
            });

            major = tempMajor;
            minor = tempMinor;
            revision = tempRevision;
            patch = tempPatch;
        }

        public ArchitectureType GetProcessArchitectureType(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                switch (process.SystemInformation.ProcessorArchitecture)
                {
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL:
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64:
                        return (process.SystemInformation.Flags & Microsoft.VisualStudio.Debugger.DefaultPort.DkmSystemInformationFlags.Is64Bit) != 0
                            ? ArchitectureType.Amd64 : ArchitectureType.X86OverAmd64;
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM:
                        return ArchitectureType.Arm;
                    default:
                        throw new NotImplementedException("Unexpected DkmProcessorArchitecture");
                }
            });
        }

        public string GetProcessDumpFileName(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                if (process.LivePart == null)
                {
                    return process.Path;
                }

                return string.Empty;
            });
        }

        public string GetProcessExecutableName(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return process.Path;
            });
        }

        public Tuple<uint, ulong>[] GetProcessModules(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);
                var modules = process.GetRuntimeInstances().SelectMany(r => r.GetModuleInstances());
                List<Tuple<uint, ulong>> result = new List<Tuple<uint, ulong>>();

                lock (this.modules)
                {
                    foreach (var module in modules)
                    {
                        result.Add(Tuple.Create((uint)this.modules.Count, module.BaseAddress));
                        this.modules.Add(module);
                    }
                }
                return result.ToArray();
            });
        }

        public uint GetModuleId(uint processId, ulong address)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);
                var module = process.GetRuntimeInstances().SelectMany(r => r.GetModuleInstances()).First(m => m.BaseAddress == address);

                lock (modules)
                {
                    uint id = (uint)modules.Count;
                    modules.Add(module);
                    return id;
                }
            });
        }

        public uint GetProcessSystemId(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                if (process.LivePart != null)
                {
                    return (uint)process.LivePart.Id;
                }

                return (uint)0;
            });
        }

        public Tuple<uint, uint>[] GetProcessThreads(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);
                DkmThread[] threads = process.GetThreads();
                Tuple<uint, uint>[] result = new Tuple<uint, uint>[threads.Length];

                lock (this.threads)
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = Tuple.Create((uint)this.threads.Count, (uint)threads[i].SystemPart.Id);
                        this.threads.Add(new ThreadStruct()
                        {
                            Thread = threads[i],
                            Frames = new SimpleCache<DkmStackFrame[]>(() =>
                            {
                                throw new NotImplementedException("You should first enumerate process threads!");
                            }),
                        });
                    }
                }

                return result;
            });
        }

        public unsafe void GetThreadContext(uint threadId, IntPtr contextBufferPointer, int contextBufferSize)
        {
            ExecuteOnDkmInitializedThread(() =>
            {
                DkmThread thread = GetThread(threadId);
                int flags = 0x1f;

                thread.GetContext(flags, contextBufferPointer.ToPointer(), contextBufferSize);
            });
        }

        public ulong GetThreadEnvironmentBlockAddress(uint threadId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmThread thread = GetThread(threadId);

                return thread.TebAddress;
            });
        }

        public ulong GetRegisterValue(uint threadId, uint frameId, uint registerId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmFrameRegisters frameRegisters = GetThreadFrames(threadId)[frameId].Registers;
                CV_HREG_e register = (CV_HREG_e)registerId;
                byte[] data = new byte[129];
                int result = frameRegisters.GetRegisterValue(registerId, data);

                if (result > 0)
                {
                    if (result == data.Length)
                    {
                        result = (int)Process.Current.GetPointerSize();
                    }

                    switch (result)
                    {
                        case 8:
                            return BitConverter.ToUInt64(data, 0);
                        case 4:
                            return BitConverter.ToUInt32(data, 0);
                        case 2:
                            return BitConverter.ToUInt16(data, 0);
                        case 1:
                            return data[0];
                        default:
                            throw new NotImplementedException($"Unexpected number of bytes for register value: {result}");
                    }
                }

                switch (frameRegisters.TagValue)
                {
                    case DkmFrameRegisters.Tag.X64Registers:
                        {
                            DkmX64FrameRegisters registers = (DkmX64FrameRegisters)frameRegisters;

                            switch (register)
                            {
                                case CV_HREG_e.CV_AMD64_RIP:
                                    return registers.Rip;
                                case CV_HREG_e.CV_AMD64_RSP:
                                    return registers.Rsp;
                            }
                        }
                        break;
                    case DkmFrameRegisters.Tag.X86Registers:
                        {
                            DkmX86FrameRegisters registers = (DkmX86FrameRegisters)frameRegisters;

                            switch (register)
                            {
                                case CV_HREG_e.CV_REG_EIP:
                                    return registers.Eip;
                                case CV_HREG_e.CV_REG_ESP:
                                    return registers.Esp;
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Unexpected DkmFrameRegisters.Tag: {frameRegisters.TagValue}");
                }

                for (int i = 0; i < frameRegisters.UnwoundRegisters.Count; i++)
                {
                    if (register == (CV_HREG_e)frameRegisters.UnwoundRegisters[i].Identifier)
                    {
                        byte[] bytes = frameRegisters.UnwoundRegisters[i].Value.ToArray();

                        switch (bytes.Length)
                        {
                            case 8:
                                return BitConverter.ToUInt64(bytes, 0);
                            case 4:
                                return BitConverter.ToUInt32(bytes, 0);
                            case 2:
                                return BitConverter.ToUInt16(bytes, 0);
                            case 1:
                                return bytes[0];
                            default:
                                throw new NotImplementedException($"Unexpected number of bytes for register value: {bytes.Length}");
                        }
                    }
                }

                throw new KeyNotFoundException($"Register not found: {register}");
            });
        }

        public Tuple<ulong, ulong, ulong>[] GetThreadStackTrace(uint threadId, byte[] threadContextBytes)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmThread thread = GetThread(threadId);
                List<DkmStackFrame> frames = new List<DkmStackFrame>();
                DkmProcess process = thread.Process;

                using (DkmInspectionSession dkmInspectionSession = DkmInspectionSession.Create(process, null))
                {
                    using (DkmStackContext dkmStackContext = DkmStackContext.Create(dkmInspectionSession, thread, DkmCallStackFilterOptions.None, new DkmFrameFormatOptions(), new System.Collections.ObjectModel.ReadOnlyCollection<byte>(threadContextBytes), null))
                    {
                        bool done = false;

                        while (!done)
                        {
                            DkmWorkList dkmWorkList = DkmWorkList.Create(null);

                            dkmStackContext.GetNextFrames(dkmWorkList, int.MaxValue, (ar) =>
                            {
                                frames.AddRange(ar.Frames);
                                done = ar.Frames.Length == 0;
                            });

                            dkmWorkList.Execute();
                        }
                    }
                }

                lock (threads)
                {
                    threads[(int)threadId].Frames.Value = frames.ToArray();
                }

                Tuple<ulong, ulong, ulong>[] result = new Tuple<ulong, ulong, ulong>[frames.Count];

                for (int i = 0; i < result.Length; i++)
                {
                    ulong stackOffset, instructionOffset;

                    switch (frames[i].Registers.TagValue)
                    {
                        case DkmFrameRegisters.Tag.X64Registers:
                            {
                                DkmX64FrameRegisters registers = (DkmX64FrameRegisters)frames[i].Registers;

                                instructionOffset = registers.Rip;
                                stackOffset = registers.Rsp;
                            }
                            break;
                        case DkmFrameRegisters.Tag.X86Registers:
                            {
                                DkmX86FrameRegisters registers = (DkmX86FrameRegisters)frames[i].Registers;

                                instructionOffset = registers.Eip;
                                stackOffset = registers.Esp;
                            }
                            break;
                        default:
                            throw new NotImplementedException("Unexpected DkmFrameRegisters.Tag");
                    }

                    bool found = false;
                    ulong frameOffset = 0;

                    for (int j = 0; !found && j < frames[i].Registers.UnwoundRegisters.Count; j++)
                    {
                        switch ((CV_HREG_e)frames[i].Registers.UnwoundRegisters[j].Identifier)
                        {
                            case CV_HREG_e.CV_AMD64_EBP:
                            case CV_HREG_e.CV_AMD64_RBP:
                                {
                                    byte[] bytes = frames[i].Registers.UnwoundRegisters[j].Value.ToArray();

                                    found = true;
                                    frameOffset = bytes.Length == 8 ? BitConverter.ToUInt64(bytes, 0) : BitConverter.ToUInt32(bytes, 0);
                                    break;
                                }
                        }
                    }

                    if (frames[i].InstructionAddress != null
                        && frames[i].InstructionAddress.CPUInstructionPart != null
                        && instructionOffset != frames[i].InstructionAddress.CPUInstructionPart.InstructionPointer)
                    {
                        throw new Exception("Instruction offset is not the same?");
                    }

                    result[i] = Tuple.Create(instructionOffset, stackOffset, frameOffset);
                }

                return result;
            });
        }

        public byte[] ReadMemory(uint processId, ulong address, uint size)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);
                byte[] bytes = new byte[size];

                process.ReadMemory(address, DkmReadMemoryFlags.None, bytes);
                return bytes;
            });
        }

        private static string ReadMemoryString(DkmProcess process, ulong address, int length, ushort charSize, System.Text.Encoding encoding)
        {
            bool trimNullTermination = false;

            if (length < 0)
            {
                length = ushort.MaxValue;
                trimNullTermination = true;
            }

            byte[] bytes = process.ReadMemoryString(address, DkmReadMemoryFlags.AllowPartialRead, charSize, length);

            if (trimNullTermination && bytes[bytes.Length - 1] == 0)
            {
                return encoding.GetString(bytes, 0, bytes.Length - 1);
            }

            return encoding.GetString(bytes);
        }

        public string ReadAnsiString(uint processId, ulong address, int length)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return ReadMemoryString(process, address, length, 1, System.Text.ASCIIEncoding.Default);
            });
        }

        public string ReadUnicodeString(uint processId, ulong address, int length)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return ReadMemoryString(process, address, length, 2, System.Text.UnicodeEncoding.Unicode);
            });
        }

        public string ReadWideUnicodeString(uint processId, ulong address, int length)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return ReadMemoryString(process, address, length, 4, System.Text.Encoding.UTF32);
            });
        }

        public void SetCurrentProcess(uint processId)
        {
            ExecuteOnDkmInitializedThread(() =>
            {
                uint processSystemId = GetProcessSystemId(processId);

                if (VSContext.DTE.Debugger.CurrentProcess.ProcessID != processSystemId)
                {
                    foreach (EnvDTE.Process vsProcess in VSContext.DTE.Debugger.DebuggedProcesses)
                    {
                        if (processSystemId == vsProcess.ProcessID)
                        {
                            VSContext.DTE.Debugger.CurrentProcess = vsProcess;
                            return;
                        }
                    }

                    throw new ArgumentException("Process wasn't found", nameof(processId));
                }
            });
        }

        public void SetCurrentThread(uint threadId)
        {
            ExecuteOnDkmInitializedThread(() =>
            {
                DkmThread thread = GetThread(threadId);
                int threadSystemId = thread.SystemPart.Id;

                if (VSContext.DTE.Debugger.CurrentThread.ID != threadSystemId)
                {
                    foreach (EnvDTE.Program vsProgram in VSContext.DTE.Debugger.CurrentProcess.Programs)
                    {
                        foreach (EnvDTE.Thread vsThread in vsProgram.Threads)
                        {
                            if (threadSystemId == vsThread.ID)
                            {
                                VSContext.DTE.Debugger.CurrentThread = vsThread;
                                return;
                            }
                        }
                    }

                    throw new ArgumentException("Thread wasn't found", nameof(threadId));
                }
            });
        }

        public void ClearCache()
        {
            ExecuteOnDkmInitializedThread(() =>
            {
                lock (processes)
                    foreach (DkmProcess process in processes)
                        try
                        {
                            QueryDPE<bool>(process, MessageCodes.ClearCache, true);
                        }
                        catch
                        {
                        }
            });
            processes.Clear();
            threads.Clear();
            modules.Clear();
        }
        #endregion

        #region IClrRuntime
        public Tuple<int, int, int, int, int>[] GetClrRuntimes(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                if (!process.GetRuntimeInstances().OfType<DkmClrRuntimeInstance>().Any())
                    return null;

                return QueryDPE<Tuple<int, int, int, int, int>[]>(process, MessageCodes.GetClrRuntimes, processId);
            });
        }

        public Tuple<bool, uint, bool, ulong>[] GetClrRuntimeThreads(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple< bool, uint, bool, ulong>[]>(process, MessageCodes.ClrRuntime_GetThreads, runtimeId);
            });
        }

        public ulong[] GetClrRuntimeModules(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<ulong[]>(process, MessageCodes.ClrRuntime_GetModules, runtimeId);
            });
        }

        public Tuple<int, string, ulong, string, string>[] GetClrRuntimeAppDomains(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<int, string, ulong, string, string>[]>(process, MessageCodes.ClrRuntime_GetAppDomains, runtimeId);
            });
        }

        public Tuple<int, string, ulong, string, string> GetClrRuntimeSharedAppDomain(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<int, string, ulong, string, string>>(process, MessageCodes.ClrRuntime_GetSharedAppDomain, runtimeId);
            });
        }

        public Tuple<int, string, ulong, string, string> GetClrRuntimeSystemAppDomain(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<int, string, ulong, string, string>>(process, MessageCodes.ClrRuntime_GetSystemAppDomain, runtimeId);
            });
        }

        public int GetClrRuntimeHeapCount(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<int>(process, MessageCodes.ClrRuntime_GetHeapCount, runtimeId);
            });
        }

        public bool GetClrRuntimeServerGC(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<bool>(process, MessageCodes.ClrRuntime_GetServerGC, runtimeId);
            });
        }

        public Tuple<string, ulong> ReadClrRuntimeFunctionNameAndDisplacement(uint processId, int runtimeId, ulong address)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<string, ulong>>(process, MessageCodes.ClrRuntime_ReadFunctionNameAndDisplacement, Tuple.Create(runtimeId, address));
            });
        }

        public Tuple<string, uint, ulong> ReadClrRuntimeSourceFileNameAndLine(uint processId, int runtimeId, ulong address)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);
                Tuple<ulong, uint, uint> tuple = QueryDPE<Tuple<ulong, uint, uint>>(process, MessageCodes.ClrRuntime_GetInstructionPointerInfo, Tuple.Create(runtimeId, address));
                DkmModuleInstance moduleInstance = process.GetRuntimeInstances().SelectMany(ri => ri.GetModuleInstances()).FirstOrDefault(m => m.BaseAddress == tuple.Item1);
                string pdbPath = moduleInstance?.Module?.GetSymbolFilePath();

                if (File.Exists(pdbPath))
                {
                    // TODO: This needs to be cached
                    using (var pdbReader = new Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader(pdbPath))
                    {
                        var function = pdbReader.GetFunctionFromToken(tuple.Item2);
                        uint ilOffset = tuple.Item3;

                        ulong distance = ulong.MaxValue;
                        string sourceFileName = "";
                        uint sourceFileLine = uint.MaxValue;

                        foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePointCollection sequenceCollection in function.SequencePoints)
                            foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePoint point in sequenceCollection.Lines)
                                if (point.Offset <= ilOffset)
                                {
                                    ulong dist = ilOffset - point.Offset;

                                    if (dist < distance)
                                    {
                                        sourceFileName = sequenceCollection.File.Name;
                                        sourceFileLine = point.LineBegin;
                                        distance = dist;
                                    }
                                }
                        return Tuple.Create(sourceFileName, sourceFileLine, distance);
                    }
                }

                throw new NotImplementedException();
            });
        }
        #endregion

        #region IClrAppDomain
        public ulong[] GetClrAppDomainModules(uint processId, int runtimeId, int appDomainId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<ulong[]>(process, MessageCodes.ClrAppDomain_GetModules, Tuple.Create(runtimeId, appDomainId));
            });
        }
        #endregion

        #region IClrHeap
        public bool GetClrHeapCanWalkHeap(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<bool>(process, MessageCodes.ClrHeap_GetCanWalkHeap, runtimeId);
            });
        }

        public ulong GetClrHeapTotalHeapSize(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<ulong>(process, MessageCodes.ClrHeap_GetTotalHeapSize, runtimeId);
            });
        }

        public int GetClrHeapObjectType(uint processId, int runtimeId, ulong objectAddress)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<int>(process, MessageCodes.ClrHeap_GetObjectType, Tuple.Create(runtimeId, objectAddress));
            });
        }

        public Tuple<int, Tuple<ulong, int>[]> EnumerateClrHeapObjects(uint processId, int runtimeId, int batchCount)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<int, Tuple<ulong, int>[]>>(process, MessageCodes.ClrHeap_EnumerateObjects, Tuple.Create(runtimeId, batchCount));
            });
        }

        public Tuple<ulong, int>[] GetVariableEnumeratorNextBatch(uint processId, int runtimeId, int batchCount)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<ulong, int>[]>(process, MessageCodes.VariableEnumerator_GetNextBatch, Tuple.Create(runtimeId, batchCount));
            });
        }

        public bool DisposeVariableEnumerator(uint processId, int runtimeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<bool>(process, MessageCodes.VariableEnumerator_Dispose, runtimeId);
            });
        }

        public ulong GetClrHeapSizeByGeneration(uint processId, int runtimeId, int generation)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<ulong>(process, MessageCodes.ClrHeap_GetSizeByGeneration, Tuple.Create(runtimeId, generation));
            });
        }
        #endregion

        #region IClrThread
        public Tuple<int, ulong, ulong, ulong>[] GetClrThreadFrames(uint processId, int runtimeId, uint threadSystemId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<int, ulong, ulong, ulong>[]>(process, MessageCodes.ClrThread_GetFrames, Tuple.Create(runtimeId, threadSystemId));
            });
        }

        public Tuple<ulong, int> GetClrThreadLastException(uint processId, int runtimeId, uint threadSystemId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<ulong, int>>(process, MessageCodes.ClrThread_GetLastException, Tuple.Create(runtimeId, threadSystemId));
            });
        }

        public Tuple<int, Tuple<ulong, int>[]> EnumerateClrThreadStackObjects(uint processId, int runtimeId, uint threadSystemId, int batchCount)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<int, Tuple<ulong, int>[]>>(process, MessageCodes.ClrThread_EnumerateStackObjects, Tuple.Create(runtimeId, threadSystemId, batchCount));
            });
        }
        #endregion

        #region ClrStackFrame
        public Tuple<ulong, int, string>[] GetClrStackFrameArguments(uint processId, int runtimeId, uint threadSystemId, int stackFrameId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<ulong, int, string>[]>(process, MessageCodes.ClrStackFrame_GetArguments, Tuple.Create(runtimeId, threadSystemId, stackFrameId));
            });
        }

        public Tuple<ulong, int, string>[] GetClrStackFrameLocals(uint processId, int runtimeId, uint threadSystemId, int stackFrameId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);
                var variables = QueryDPE<Tuple<ulong, int, ulong, uint, uint>[]>(process, MessageCodes.ClrStackFrame_GetLocals, Tuple.Create(runtimeId, threadSystemId, stackFrameId));
                string[] names = null;
                int length = variables.Length;

                if (length > 0)
                {
                    DkmModuleInstance moduleInstance = process.GetRuntimeInstances().SelectMany(ri => ri.GetModuleInstances()).FirstOrDefault(m => m.BaseAddress == variables[0].Item3);
                    string pdbPath = moduleInstance?.Module?.GetSymbolFilePath();

                    if (File.Exists(pdbPath))
                    {
                        // TODO: This needs to be cached
                        using (var pdbReader = new Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader(pdbPath))
                        {
                            var function = pdbReader.GetFunctionFromToken(variables[0].Item4);
                            var scope = function.FindScopeByILOffset(variables[0].Item5);

                            names = GetRecursiveSlots(scope).Select(s => s.Name).ToArray();
                        }
                    }

                    if (names == null)
                        names = Enumerable.Range(0, variables.Length).Select(id => string.Format("local_{0}", id)).ToArray();
                    if (names.Length < length)
                        length = names.Length;
                }

                Tuple<ulong, int, string>[] result = new Tuple<ulong, int, string>[length];

                for (int i = 0; i < length; i++)
                    result[i] = Tuple.Create(variables[i].Item1, variables[i].Item2, names[i]);
                return result;
            });
        }

        /// <summary>
        /// Gets the recursive slots.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="results">The results.</param>
        private static IEnumerable<Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSlot> GetRecursiveSlots(Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbScope scope, List<Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSlot> results = null)
        {
            if (results == null)
                results = new List<Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSlot>();
            foreach (var slot in scope.Slots)
            {
                while (results.Count <= slot.Slot)
                    results.Add(null);
                results[(int)slot.Slot] = slot;
            }

            foreach (var innerScope in scope.Scopes)
                GetRecursiveSlots(innerScope, results);
            return results;
        }
        #endregion

        #region IClrModule
        public int GetClrModuleTypeByName(uint processId, int runtimeId, ulong imageBase, string typeName)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<int>(process, MessageCodes.ClrModule_GetTypeByName, Tuple.Create(runtimeId, imageBase, typeName));
            });
        }
        #endregion

        #region IClrType
        public ulong GetClrTypeModule(uint processId, int clrTypeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<ulong>(process, MessageCodes.ClrType_GetModule, clrTypeId);
            });
        }

        public Tuple<int, int, int, int, int, int, string> GetClrTypeSimpleData(uint processId, int clrTypeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<int, int, int, int, int, int, string>>(process, MessageCodes.ClrType_GetSimpleData, clrTypeId);
            });
        }

        public Tuple<string, int, int, int>[] GetClrTypeFields(uint processId, int clrTypeId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<Tuple<string, int, int, int>[]>(process, MessageCodes.ClrType_GetFields, clrTypeId);
            });
        }

        public ulong GetClrTypeArrayElementAddress(uint processId, int clrTypeId, ulong address, int index)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<ulong>(process, MessageCodes.ClrType_GetArrayElementAddress, Tuple.Create(clrTypeId, address, index));
            });
        }

        public int GetClrTypeArrayLength(uint processId, int clrTypeId, ulong address)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<int>(process, MessageCodes.ClrType_GetArrayLength, Tuple.Create(clrTypeId, address));
            });
        }

        public int GetClrTypeStaticField(uint processId, int clrTypeId, string name)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<int>(process, MessageCodes.ClrType_GetStaticField, Tuple.Create(clrTypeId, name));
            });
        }
        #endregion

        #region IClrStaticField
        public ulong GetClrStaticFieldAddress(uint processId, int clrTypeId, string name, int appDomainId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return QueryDPE<ulong>(process, MessageCodes.ClrStaticField_GetAddress, Tuple.Create(clrTypeId, name));
            });
        }
        #endregion

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <param name="module">The module.</param>
        private static string GetModuleName(DkmModuleInstance module)
        {
            return System.IO.Path.GetFileNameWithoutExtension(module.Name);
        }

        private DkmProcess GetProcess(uint id)
        {
            lock (processes)
            {
                return Processes[(int)id];
            }
        }

        private DkmThread GetThread(uint id)
        {
            lock (threads)
            {
                return threads[(int)id].Thread;
            }
        }

        private DkmStackWalkFrame[] GetThreadFrames(uint threadId)
        {
            lock (threads)
            {
                return threads[(int)threadId].Frames.Value;
            }
        }


        private DkmModuleInstance GetModule(uint id)
        {
            lock (modules)
            {
                return modules[(int)id];
            }
        }

        #region Executing evaluators on correct thread
        /// <summary>
        /// Helper class that enables background execution on specially initialized thread.
        /// </summary>
        private class ThreadActionPool : IDisposable
        {
            /// <summary>
            /// Signal event when there is something new in thread queue.
            /// </summary>
            private System.Threading.AutoResetEvent actionAvailable = new System.Threading.AutoResetEvent(false);

            /// <summary>
            /// Signal event when thread should stop.
            /// </summary>
            private System.Threading.AutoResetEvent shouldStop = new System.Threading.AutoResetEvent(false);

            /// <summary>
            /// Queue of thread actions.
            /// </summary>
            private Queue<Action> actions = new Queue<Action>();

            /// <summary>
            /// Local thread storage for signal event when thread process' action.
            /// </summary>
            private System.Threading.ThreadLocal<System.Threading.AutoResetEvent> threadSyncEvent = new System.Threading.ThreadLocal<System.Threading.AutoResetEvent>(() => new System.Threading.AutoResetEvent(false));

            /// <summary>
            /// Thread that will be executing action pool
            /// </summary>
            private volatile System.Threading.Thread thread;

            /// <summary>
            /// Background thread apartment state that will be applied when first action is created.
            /// </summary>
            private readonly System.Threading.ApartmentState threadApartmentState;

            /// <summary>
            /// Initializes a new instance of the <see cref="ThreadActionPool"/> class.
            /// </summary>
            /// <param name="apartmentState">Background thread apartment state.</param>
            public ThreadActionPool(System.Threading.ApartmentState apartmentState)
            {
                threadApartmentState = apartmentState;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (thread != null)
                {
                    shouldStop.Set();
                    thread.Join();
                    thread = null;
                }
            }

            /// <summary>
            /// Executes the specified action on the thread.
            /// </summary>
            /// <param name="action">Action to be executed.</param>
            public void Execute(Action action)
            {
                // Check if we should create and start background processing thread.
                if (thread == null)
                    lock (this)
                    {
                        if (thread == null)
                        {
                            thread = new System.Threading.Thread(() => ThreadLoop());
                            thread.SetApartmentState(threadApartmentState);
                            thread.Start();
                        }
                    }

                System.Threading.AutoResetEvent syncEvent = threadSyncEvent.Value;
                Exception exception = null;

                lock (actions)
                {
                    actions.Enqueue(() =>
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                        syncEvent.Set();
                    });
                }
                actionAvailable.Set();
                syncEvent.WaitOne();
                if (exception != null)
                {
                    throw new AggregateException(exception);
                }
            }

            /// <summary>
            /// STA thread loop function.
            /// </summary>
            private void ThreadLoop()
            {
                System.Threading.WaitHandle[] handles = new System.Threading.WaitHandle[] { shouldStop, actionAvailable };
                Queue<Action> nextActions = new Queue<Action>();
                bool initializeDkm = initializationForThread.Value;

                while (true)
                {
                    // Wait for next operation
                    int id = System.Threading.WaitHandle.WaitAny(handles);

                    if (handles[id] == shouldStop)
                        break;

                    // Swap actions queue
                    lock (actions)
                    {
                        var actions = this.actions;
                        this.actions = nextActions;
                        nextActions = actions;
                    }

                    // Execute all actions
                    bool initialized = initializationForThread.Value;

                    while (nextActions.Count > 0)
                    {
                        Action action = nextActions.Dequeue();

                        action();
                    }
                }
            }
        }

        /// <summary>
        /// Action pool for executing actions on STA thread.
        /// </summary>
        private static ThreadActionPool staActionPool = new ThreadActionPool(System.Threading.ApartmentState.STA);

        /// <summary>
        /// Executes the specified evaluator on STA thread.
        /// </summary>
        /// <typeparam name="T">The evaluator result type</typeparam>
        /// <param name="evaluator">The evaluator.</param>
        private static T ExecuteOnStaThread<T>(Func<T> evaluator)
        {
            T result = default(T);

            staActionPool.Execute(() =>
            {
                result = evaluator();
            });
            return result;
        }

        /// <summary>
        /// Executes the specified evaluator on DKM initialized thread. It will try to initialize current thread and if it fails it will fall-back to the main thread.
        /// </summary>
        /// <typeparam name="T">The evaluator result type</typeparam>
        /// <param name="evaluator">The evaluator.</param>
        private static void ExecuteOnDkmInitializedThread(Action evaluator)
        {
            if (initializationForThread.Value)
            {
                evaluator();
            }
            else
            {
                staActionPool.Execute(evaluator);
            }
        }

        /// <summary>
        /// Executes the specified evaluator on DKM initialized thread. It will try to initialize current thread and if it fails it will fall-back to the main thread.
        /// </summary>
        /// <typeparam name="T">The evaluator result type</typeparam>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns>The evaluator result.</returns>
        private static T ExecuteOnDkmInitializedThread<T>(Func<T> evaluator)
        {
            if (initializationForThread.Value)
            {
                return evaluator();
            }
            else
            {
                return ExecuteOnStaThread(evaluator);
            }
        }
        #endregion

        #region Messaging with Concord debugging process component (DPE)
        private static TOutput QueryDPE<TOutput>(DkmProcess process, MessageCodes messageCode, object parameter)
        {
            byte[] bytes = MessageSerializer.Serialize(parameter);
            DkmCustomMessage message = DkmCustomMessage.Create(process.Connection, process, VS_SourceId, (int)messageCode, bytes, null);
            DkmCustomMessage response = message.SendLower();
            MessageCodes responseCode = (MessageCodes)response.MessageCode;
            bytes = (byte[])response.Parameter1;

            // Check if we encountered exception during processing
            if (responseCode == MessageCodes.Exception)
            {
                string exceptionText = MessageSerializer.Deserialize<string>(bytes);

                throw new Exception(exceptionText);
            }

            // Check if we got the same message response
            if (responseCode != messageCode)
                throw new Exception($"Unexpected message code: {responseCode}");

            // All good, deserialize response
            return MessageSerializer.Deserialize<TOutput>(bytes);
        }
        #endregion
    }
}
