using CsDebugScript.Engine.Utility;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.CallStack;
using Microsoft.VisualStudio.Debugger.Evaluation;
using Microsoft.VisualStudio.Debugger.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using Dia2Lib;

namespace CsDebugScript.VS
{
    /// <summary>
    /// Visual Studio Debugger Proxy object to default AppDomain
    /// </summary>
    /// <seealso cref="System.MarshalByRefObject" />
    internal class VSDebuggerProxy : MarshalByRefObject
    {
        public const string AppDomainDataName = "VSDebuggerProxy.Value";

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
        /// Initializes the <see cref="VSDebugger"/> class.
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
                catch (Exception)
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSDebugger"/> class.
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
            DkmStackWalkFrame frame = ExecuteOnMainThread(() => DkmStackFrame.ExtractFromDTEObject(VSContext.DTE.Debugger.CurrentStackFrame));
            DkmStackWalkFrame[] frames = threads[threadId].Frames.Value;

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

                    return pdbFileId.PdbName;
                }

                return module.FullName;
            });
        }

        public object GetModuleDiaSession(uint moduleId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmModuleInstance module = GetModule(moduleId);

                return module.Module.GetSymbolInterface(typeof(IDiaSession).GUID);
            });
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

        public Engine.Native.ImageFileMachine GetProcessActualProcessorType(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                switch (process.SystemInformation.ProcessorArchitecture)
                {
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64:
                        return Engine.Native.ImageFileMachine.AMD64;
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM:
                        return Engine.Native.ImageFileMachine.ARM;
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL:
                        return string.IsNullOrEmpty(process.SystemInformation.SystemWow64Directory)
                            || process.SystemInformation.SystemDirectory == process.SystemInformation.SystemWow64Directory
                                ? Engine.Native.ImageFileMachine.I386 : Engine.Native.ImageFileMachine.AMD64;
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

        public Engine.Native.ImageFileMachine GetProcessEffectiveProcessorType(uint processId)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                switch (process.SystemInformation.ProcessorArchitecture)
                {
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64:
                        return (process.SystemInformation.Flags & Microsoft.VisualStudio.Debugger.DefaultPort.DkmSystemInformationFlags.Is64Bit) != 0 ? Engine.Native.ImageFileMachine.AMD64 : Engine.Native.ImageFileMachine.I386;
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_ARM:
                        return Engine.Native.ImageFileMachine.ARM;
                    case DkmProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL:
                        return Engine.Native.ImageFileMachine.I386;
                    default:
                        throw new NotImplementedException("Unexpected DkmProcessorArchitecture");
                }
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

                threads[(int)threadId].Frames.Value = frames.ToArray();

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
                        switch ((Dia2Lib.CV_HREG_e)frames[i].Registers.UnwoundRegisters[j].Identifier)
                        {
                            case Dia2Lib.CV_HREG_e.CV_AMD64_EBP:
                            case Dia2Lib.CV_HREG_e.CV_AMD64_RBP:
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

        public string ReadAnsiString(uint processId, ulong address, int length)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return System.Text.ASCIIEncoding.Default.GetString(process.ReadMemoryString(address, DkmReadMemoryFlags.None, 1, length));
            });
        }

        public string ReadUnicodeString(uint processId, ulong address, int length)
        {
            return ExecuteOnDkmInitializedThread(() =>
            {
                DkmProcess process = GetProcess(processId);

                return System.Text.UnicodeEncoding.Default.GetString(process.ReadMemoryString(address, DkmReadMemoryFlags.None, 2, length));
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
            processes.Clear();
            threads.Clear();
            modules.Clear();
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

        private DkmModuleInstance GetModule(uint id)
        {
            lock (modules)
            {
                return modules[(int)id];
            }
        }

        #region Executing evaluators on correct thread
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
                ExecuteOnMainThread(evaluator);
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
                return ExecuteOnMainThread(evaluator);
            }
        }

        /// <summary>
        /// Executes the specified evaluator on main thread.
        /// </summary>
        /// <typeparam name="T">The evaluator result type</typeparam>
        /// <param name="evaluator">The evaluator.</param>
        private static void ExecuteOnMainThread(Action evaluator)
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(evaluator);
            }
            else
            {
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    bool initialized = initializationForThread.Value;

                    evaluator();
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
        }

        /// <summary>
        /// Executes the specified evaluator on main thread.
        /// </summary>
        /// <typeparam name="T">The evaluator result type</typeparam>
        /// <param name="evaluator">The evaluator.</param>
        /// <returns>The evaluator result.</returns>
        private static T ExecuteOnMainThread<T>(Func<T> evaluator)
        {
            if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null)
            {
                return System.Windows.Application.Current.Dispatcher.Invoke(evaluator);
            }
            else
            {
                T result = default(T);

                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    bool initialized = initializationForThread.Value;

                    result = evaluator();
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
                return result;
            }
        }
        #endregion
    }
}
