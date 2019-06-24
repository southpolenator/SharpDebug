using Microsoft.Diagnostics.Runtime;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.ComponentInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpDebug.VS.DPE
{
    internal enum MessageCodes
    {
        Exception,
        ClearCache,
        GetClrRuntimes,
        ClrRuntime_GetThreads,
        ClrRuntime_GetModules,
        ClrRuntime_GetAppDomains,
        ClrRuntime_GetSharedAppDomain,
        ClrRuntime_GetSystemAppDomain,
        ClrRuntime_GetHeapCount,
        ClrRuntime_GetServerGC,
        ClrRuntime_ReadFunctionNameAndDisplacement,
        ClrRuntime_GetInstructionPointerInfo,
        ClrAppDomain_GetModules,
        ClrHeap_GetCanWalkHeap,
        ClrHeap_GetTotalHeapSize,
        ClrHeap_GetObjectType,
        ClrHeap_EnumerateObjects,
        ClrHeap_GetSizeByGeneration,
        ClrThread_GetFrames,
        ClrThread_GetLastException,
        ClrThread_EnumerateStackObjects,
        ClrStackFrame_GetArguments,
        ClrStackFrame_GetLocals,
        ClrModule_GetTypeByName,
        ClrType_GetModule,
        ClrType_GetSimpleData,
        ClrType_GetFields,
        ClrType_GetArrayElementAddress,
        ClrType_GetArrayLength,
        ClrType_GetStaticField,
        ClrStaticField_GetAddress,
        VariableEnumerator_GetNextBatch,
        VariableEnumerator_Dispose,
    }

    /// <summary>
    /// Exported class for communication between DPE process and Visual Studio extension.
    /// </summary>
    public class MessageReceiver : IDkmCustomMessageForwardReceiver
    {
        private volatile int nextRuntimeId = 1;
        private ConcurrentDictionary<int, ClrRuntime> runtimesCache = new ConcurrentDictionary<int, ClrRuntime>();
        private volatile int nextClrTypeId = 1;
        private ConcurrentDictionary<ClrType, int> clrTypeIdsCache = new ConcurrentDictionary<ClrType, int>();
        private ConcurrentDictionary<int, ClrType> clrTypesCache = new ConcurrentDictionary<int, ClrType>();
        private volatile int nextVariableEnumeratorId = 1;
        private ConcurrentDictionary<int, IEnumerator<Tuple<ulong, int>>> variableEnumeratorsCache = new ConcurrentDictionary<int, IEnumerator<Tuple<ulong, int>>>();

        /// <summary>
        /// Sends a message to a listening component which is lower in the hierarchy.
        /// </summary>
        /// <param name="message">Message structure used to pass information between custom debugger backend
        /// components and custom visual studio UI components (packages, add-ins, etc).</param>
        /// <returns>Message sent back from the implementation.</returns>
        public DkmCustomMessage SendLower(DkmCustomMessage message)
        {
            object result;
            byte[] bytes;
            MessageCodes messageCode = (MessageCodes)message.MessageCode;

            try
            {
                bytes = (byte[])message.Parameter1;
                result = ProcessMessage(messageCode, bytes);
            }
            catch (Exception e)
            {
                result = e.ToString();
                messageCode = MessageCodes.Exception;
            }

            bytes = MessageSerializer.Serialize(result);
            return DkmCustomMessage.Create(message.Connection, message.Process, Guid.Empty, (int)messageCode, bytes, null);
        }

        private object ProcessMessage(MessageCodes messageCode, byte[] bytes)
        {
            switch (messageCode)
            {
                case MessageCodes.ClearCache:
                    return ClearCache(MessageSerializer.Deserialize<bool>(bytes));
                case MessageCodes.GetClrRuntimes:
                    return GetClrRuntimes(MessageSerializer.Deserialize<uint>(bytes));
                case MessageCodes.ClrRuntime_GetThreads:
                    return GetClrRuntimeThreads(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrRuntime_GetModules:
                    return GetClrRuntimeModules(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrRuntime_GetAppDomains:
                    return GetClrRuntimeAppDomains(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrRuntime_GetSharedAppDomain:
                    return GetClrRuntimeSharedAppDomain(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrRuntime_GetSystemAppDomain:
                    return GetClrRuntimeSystemAppDomain(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrRuntime_GetHeapCount:
                    return GetClrRuntimeHeapCount(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrRuntime_GetServerGC:
                    return GetClrRuntimeServerGC(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrRuntime_ReadFunctionNameAndDisplacement:
                    return ReadClrRuntimeFunctionNameAndDisplacement(MessageSerializer.Deserialize<Tuple<int, ulong>>(bytes));
                case MessageCodes.ClrRuntime_GetInstructionPointerInfo:
                    return GetClrRuntimeInstructionPointerInfo(MessageSerializer.Deserialize<Tuple<int, ulong>>(bytes));
                case MessageCodes.ClrAppDomain_GetModules:
                    return GetClrAppDomainModules(MessageSerializer.Deserialize<Tuple<int, int>>(bytes));
                case MessageCodes.ClrHeap_GetCanWalkHeap:
                    return GetClrHeapCanWalkHeap(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrHeap_GetTotalHeapSize:
                    return GetClrHeapTotalHeapSize(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrHeap_GetObjectType:
                    return GetClrHeapObjectType(MessageSerializer.Deserialize<Tuple<int, ulong>>(bytes));
                case MessageCodes.ClrHeap_EnumerateObjects:
                    return EnumerateClrHeapObjects(MessageSerializer.Deserialize<Tuple<int, int>>(bytes));
                case MessageCodes.ClrHeap_GetSizeByGeneration:
                    return GetClrHeapSizeByGeneration(MessageSerializer.Deserialize<Tuple<int, int>>(bytes));
                case MessageCodes.ClrThread_GetFrames:
                    return GetClrThreadFrames(MessageSerializer.Deserialize<Tuple<int, uint>>(bytes));
                case MessageCodes.ClrThread_GetLastException:
                    return GetClrThreadLastException(MessageSerializer.Deserialize<Tuple<int, uint>>(bytes));
                case MessageCodes.ClrThread_EnumerateStackObjects:
                    return EnumerateClrThreadStackObjects(MessageSerializer.Deserialize<Tuple<int, uint, int>>(bytes));
                case MessageCodes.ClrStackFrame_GetLocals:
                    return GetClrStackFrameLocals(MessageSerializer.Deserialize<Tuple<int, uint, int>>(bytes));
                case MessageCodes.ClrStackFrame_GetArguments:
                    return GetClrStackFrameArguments(MessageSerializer.Deserialize<Tuple<int, uint, int>>(bytes));
                case MessageCodes.ClrModule_GetTypeByName:
                    return GetClrModuleTypeByName(MessageSerializer.Deserialize<Tuple<int, ulong, string>>(bytes));
                case MessageCodes.ClrType_GetModule:
                    return GetClrTypeModule(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrType_GetSimpleData:
                    return GetClrTypeSimpleData(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrType_GetFields:
                    return GetClrTypeFields(MessageSerializer.Deserialize<int>(bytes));
                case MessageCodes.ClrType_GetArrayElementAddress:
                    return GetClrTypeArrayElementAddress(MessageSerializer.Deserialize<Tuple<int, ulong, int>>(bytes));
                case MessageCodes.ClrType_GetArrayLength:
                    return GetClrTypeArrayLength(MessageSerializer.Deserialize<Tuple<int, ulong>>(bytes));
                case MessageCodes.ClrType_GetStaticField:
                    return GetClrTypeStaticField(MessageSerializer.Deserialize<Tuple<int, string>>(bytes));
                case MessageCodes.ClrStaticField_GetAddress:
                    return GetClrStaticFieldAddress(MessageSerializer.Deserialize<Tuple<int, string, int>>(bytes));
                case MessageCodes.VariableEnumerator_GetNextBatch:
                    return GetVariableEnumeratorNextBatch(MessageSerializer.Deserialize<Tuple<int, int>>(bytes));
                case MessageCodes.VariableEnumerator_Dispose:
                    return DisposeVariableEnumerator(MessageSerializer.Deserialize<int>(bytes));
                default:
                    throw new Exception($"Unknown message code: {messageCode}");
            }
        }

        private bool ClearCache(bool everything)
        {
            runtimesCache.Clear();
            clrTypeIdsCache.Clear();
            clrTypesCache.Clear();
            foreach (IEnumerator<Tuple<ulong, int>> enumerator in variableEnumeratorsCache.Values)
                enumerator.Dispose();
            variableEnumeratorsCache.Clear();
            return everything;
        }

        private Tuple<int, int, int, int, int>[] GetClrRuntimes(uint processId)
        {
            DkmProcess process = GetProcess(processId);
            ClrMdDataReader dataReader = new ClrMdDataReader(process);
            DataTarget dataTarget = DataTarget.CreateFromDataReader(dataReader);
            dataTarget.SymbolLocator = new ClrMdSymbolLocator(process, dataTarget.SymbolLocator);
            ClrRuntime[] clrRuntimes = dataTarget.ClrVersions.Select(clrInfo => clrInfo.CreateRuntime()).ToArray();

            if (clrRuntimes.Length > 0)
            {
                Tuple<int, int, int, int, int>[] runtimes = new Tuple<int, int, int, int, int>[clrRuntimes.Length];

                lock (runtimesCache)
                {
                    for (int i = 0; i < clrRuntimes.Length; i++)
                    {
                        runtimes[i] = Tuple.Create(
                            nextRuntimeId++,
                            clrRuntimes[i].ClrInfo.Version.Major,
                            clrRuntimes[i].ClrInfo.Version.Minor,
                            clrRuntimes[i].ClrInfo.Version.Revision,
                            clrRuntimes[i].ClrInfo.Version.Patch);
                        runtimesCache.TryAdd(runtimes[i].Item1, clrRuntimes[i]);
                    }
                }
                return runtimes;
            }
            return new Tuple<int, int, int, int, int>[0];
        }

        private Tuple<bool, uint, bool, ulong>[] GetClrRuntimeThreads(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            IList<ClrThread> clrThreads = clrRuntime.Threads;
            int[] gcThreadIds = clrRuntime.EnumerateGCThreads().ToArray();
            Tuple<bool, uint, bool, ulong>[] threads = new Tuple<bool, uint, bool, ulong>[clrThreads.Count];

            for (int i = 0; i < clrThreads.Count; i++)
                threads[i] = Tuple.Create(
                    gcThreadIds.Contains((int)clrThreads[i].OSThreadId),
                    clrThreads[i].OSThreadId,
                    clrThreads[i].IsFinalizer,
                    clrThreads[i].AppDomain);
            return threads;
        }

        private ulong[] GetClrRuntimeModules(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];

            return clrRuntime.Modules.Select(m => m.ImageBase).ToArray();
        }

        private Tuple<int, string, ulong, string, string>[] GetClrRuntimeAppDomains(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            IList<ClrAppDomain> clrAppDomains = clrRuntime.AppDomains;
            Tuple<int, string, ulong, string, string>[] appDomains = new Tuple<int, string, ulong, string, string>[clrAppDomains.Count];

            for (int i = 0; i < appDomains.Length; i++)
                appDomains[i] = Tuple.Create(
                    clrAppDomains[i].Id,
                    clrAppDomains[i].Name,
                    clrAppDomains[i].Address,
                    clrAppDomains[i].ApplicationBase,
                    clrAppDomains[i].ConfigurationFile);
            return appDomains;
        }

        private Tuple<int, string, ulong, string, string> GetClrRuntimeSharedAppDomain(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrAppDomain clrAppDomain = clrRuntime.SharedDomain;

            if (clrAppDomain == null)
                Tuple.Create(int.MinValue, string.Empty, 0UL, string.Empty, string.Empty);
            return Tuple.Create(
                clrAppDomain.Id,
                clrAppDomain.Name,
                clrAppDomain.Address,
                clrAppDomain.ApplicationBase,
                clrAppDomain.ConfigurationFile);
        }

        private Tuple<int, string, ulong, string, string> GetClrRuntimeSystemAppDomain(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrAppDomain clrAppDomain = clrRuntime.SystemDomain;

            if (clrAppDomain == null)
                Tuple.Create(int.MinValue, string.Empty, 0UL, string.Empty, string.Empty);
            return Tuple.Create(
                clrAppDomain.Id,
                clrAppDomain.Name,
                clrAppDomain.Address,
                clrAppDomain.ApplicationBase,
                clrAppDomain.ConfigurationFile);
        }

        private int GetClrRuntimeHeapCount(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];

            return clrRuntime.HeapCount;
        }

        private bool GetClrRuntimeServerGC(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];

            return clrRuntime.ServerGC;
        }

        private Tuple<string, ulong> ReadClrRuntimeFunctionNameAndDisplacement(Tuple<int, ulong> input)
        {
            int runtimeId = input.Item1;
            ulong address = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrMethod method = clrRuntime.GetMethodByAddress(address);
            ClrModule clrModule = method.Type.Module;
            string moduleName = clrModule?.Name ?? "???";
            string functionName = moduleName + "!" + method;
            ulong displacement = address - method.NativeCode;

            return Tuple.Create(functionName, displacement);
        }

        private Tuple<ulong, uint, uint> GetClrRuntimeInstructionPointerInfo(Tuple<int, ulong> input)
        {
            int runtimeId = input.Item1;
            ulong instructionPointer = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrMethod method = clrRuntime.GetMethodByAddress(instructionPointer);
            ClrModule clrModule = method.Type.Module;

            return Tuple.Create(clrModule.ImageBase, method.MetadataToken, FindIlOffset(method, instructionPointer));
        }

        private ulong[] GetClrAppDomainModules(Tuple<int, int> input)
        {
            int runtimeId = input.Item1;
            int appDomainId = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrAppDomain clrAppDomain = clrRuntime.AppDomains.First(ad => ad.Id == appDomainId);

            return clrAppDomain.Modules.Select(m => m.ImageBase).ToArray();
        }

        private bool GetClrHeapCanWalkHeap(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];

            return clrRuntime.GetHeap().CanWalkHeap;
        }

        private ulong GetClrHeapTotalHeapSize(int runtimeId)
        {
            ClrRuntime clrRuntime = runtimesCache[runtimeId];

            return clrRuntime.GetHeap().TotalHeapSize;
        }

        private int GetClrHeapObjectType(Tuple<int, ulong> input)
        {
            int runtimeId = input.Item1;
            ulong address = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrType clrType = clrRuntime.GetHeap().GetObjectType(address);

            return GetClrTypeId(clrType);
        }

        private Tuple<int, Tuple<ulong, int>[]> EnumerateClrHeapObjects(Tuple<int, int> input)
        {
            int runtimeId = input.Item1;
            int batchCount = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrHeap clrHeap = clrRuntime.GetHeap();

            return EnumerateVariables(EnumerateClrHeapObjects(clrHeap), batchCount);
        }

        private IEnumerable<Tuple<ulong, int>> EnumerateClrHeapObjects(ClrHeap clrHeap)
        {
            foreach (ulong address in clrHeap.EnumerateObjectAddresses())
            {
                var clrType = clrHeap.GetObjectType(address);

                if (clrType.IsFree)
                    continue;

                int clrTypeId = GetClrTypeId(clrType);

                if (clrType.IsPointer)
                    yield return Tuple.Create(address, clrTypeId);
                else
                    yield return Tuple.Create(address + (uint)IntPtr.Size, clrTypeId);
            }
        }

        private ulong GetClrHeapSizeByGeneration(Tuple<int, int> input)
        {
            int runtimeId = input.Item1;
            int generation = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];

            return clrRuntime.GetHeap().GetSizeByGen(generation);
        }

        private Tuple<int, ulong, ulong, ulong>[] GetClrThreadFrames(Tuple<int, uint> input)
        {
            int runtimeId = input.Item1;
            uint threadSystemId = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrThread clrThread = clrRuntime.Threads.First(t => t.OSThreadId == threadSystemId);
            List<Tuple<int, ulong, ulong, ulong>> frames = new List<Tuple<int, ulong, ulong, ulong>>();

            for (int i = 0; i < clrThread.StackTrace.Count; i++)
                if (clrThread.StackTrace[i].Method != null)
                {
                    ClrStackFrame clrStackFrame = clrThread.StackTrace[i];

                    frames.Add(Tuple.Create(i, clrStackFrame.InstructionPointer, clrStackFrame.StackPointer, clrStackFrame.Module?.ImageBase ?? 0));
                }
            return frames.ToArray();
        }

        private Tuple<ulong, int> GetClrThreadLastException(Tuple<int, uint> input)
        {
            int runtimeId = input.Item1;
            uint threadSystemId = input.Item2;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrThread clrThread = clrRuntime.Threads.First(t => t.OSThreadId == threadSystemId);
            ulong address = clrThread.CurrentException?.Address ?? 0;
            ClrType clrType = clrThread.CurrentException?.Type;

            return Tuple.Create(address, GetClrTypeId(clrType));
        }

        private Tuple<int, Tuple<ulong, int>[]> EnumerateClrThreadStackObjects(Tuple<int, uint, int> input)
        {
            int runtimeId = input.Item1;
            uint threadSystemId = input.Item2;
            int batchCount = input.Item3;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrThread clrThread = clrRuntime.Threads.First(t => t.OSThreadId == threadSystemId);

            return EnumerateVariables(EnumerateClrThreadStackObjects(clrThread), batchCount);
        }

        private IEnumerable<Tuple<ulong, int>> EnumerateClrThreadStackObjects(ClrThread clrThread)
        {
            foreach (ClrRoot root in clrThread.EnumerateStackObjects())
            {
                if (root.Type.IsFree || root.Type.Module == null)
                    continue;

                int clrTypeId = GetClrTypeId(root.Type);
                ulong address = root.Address;

                yield return Tuple.Create(address, clrTypeId);
            }
        }

        private Tuple<ulong, int, string>[] GetClrStackFrameArguments(Tuple<int, uint, int> input)
        {
            int runtimeId = input.Item1;
            uint threadSystemId = input.Item2;
            int stackFrameId = input.Item3;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrThread clrThread = clrRuntime.Threads.First(t => t.OSThreadId == threadSystemId);
            ClrStackFrame clrStackFrame = clrThread.StackTrace[stackFrameId];
            IList<ClrValue> clrValues = clrStackFrame.Arguments;
            string[] names = GetClrArgumentsNames(clrStackFrame);
            Tuple<ulong, int, string>[] variables = new Tuple<ulong, int, string>[clrValues.Count];

            for (int i = 0; i < variables.Length; i++)
            {
                GetClrValueAddressAndCodeTypeId(clrValues[i], out ulong address, out int codeTypeId);
                variables[i] = Tuple.Create(address, codeTypeId, names[i]);
            }
            return variables;
        }

        private Tuple<ulong, int, ulong, uint, uint>[] GetClrStackFrameLocals(Tuple<int, uint, int> input)
        {
            int runtimeId = input.Item1;
            uint threadSystemId = input.Item2;
            int stackFrameId = input.Item3;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrThread clrThread = clrRuntime.Threads.First(t => t.OSThreadId == threadSystemId);
            ClrStackFrame clrStackFrame = clrThread.StackTrace[stackFrameId];
            IList<ClrValue> clrValues = clrStackFrame.Locals;
            Tuple<ulong, int, ulong, uint, uint>[] variables = new Tuple<ulong, int, ulong, uint, uint>[clrValues.Count];
            ulong moduleBase = clrStackFrame.Module.ImageBase;
            uint metadataToken = clrStackFrame.Method.MetadataToken;
            uint ilOffset = FindIlOffset(clrStackFrame);

            for (int i = 0; i < variables.Length; i++)
            {
                GetClrValueAddressAndCodeTypeId(clrValues[i], out ulong address, out int codeTypeId);
                variables[i] = Tuple.Create(address, codeTypeId, moduleBase, metadataToken, ilOffset);
            }
            return variables;
        }

        private void GetClrValueAddressAndCodeTypeId(ClrValue clrValue, out ulong address, out int clrTypeId)
        {
            try
            {
                ClrType clrType = clrValue.Type;

                clrTypeId = GetClrTypeId(clrType);
                address = clrValue.Address;
                if (!clrType.IsPointer)
                    if (clrValue.ElementType == ClrElementType.Class)
                        address += (uint)IntPtr.Size;
            }
            catch
            {
                address = 0;
                clrTypeId = -1;
            }
        }

        private int GetClrTypeId(ClrType clrType)
        {
            int clrTypeId;

            if (clrType == null)
                clrTypeId = -1;
            else if (!clrTypeIdsCache.TryGetValue(clrType, out clrTypeId))
                lock (clrTypeIdsCache)
                {
                    clrTypeId = nextClrTypeId++;
                    clrTypeIdsCache.TryAdd(clrType, clrTypeId);
                    clrTypesCache.TryAdd(clrTypeId, clrType);
                }
            return clrTypeId;
        }

        private int GetClrModuleTypeByName(Tuple<int, ulong, string> input)
        {
            int runtimeId = input.Item1;
            ulong imageBase = input.Item2;
            string typeName = input.Item3;
            ClrRuntime clrRuntime = runtimesCache[runtimeId];
            ClrModule clrModule = clrRuntime.Modules.FirstOrDefault(m => m.ImageBase == imageBase);

            return GetClrTypeId(clrModule.GetTypeByName(typeName));
        }

        private ulong GetClrTypeModule(int clrTypeId)
        {
            ClrType clrType = clrTypesCache[clrTypeId];

            return clrType.Module?.ImageBase ?? 0;
        }

        private Tuple<int, int, int, int, int, int, string> GetClrTypeSimpleData(int clrTypeId)
        {
            ClrType clrType = clrTypesCache[clrTypeId];
            int baseSize = clrType.BaseSize;
            int baseTypeId = GetClrTypeId(clrType.BaseType);
            int componentTypeId = GetClrTypeId(clrType.ComponentType);
            int elementSize = clrType.ElementSize;
            int elementType = (int)clrType.ElementType;
            int booleans = 0;
            string name = clrType.Name;

            if (clrType.HasSimpleValue)
                booleans |= 1;
            if (clrType.IsArray)
                booleans |= 2;
            if (clrType.IsEnum)
                booleans |= 4;
            if (clrType.IsObjectReference)
                booleans |= 8;
            if (clrType.IsPointer)
                booleans |= 16;
            if (clrType.IsPrimitive)
                booleans |= 32;
            if (clrType.IsString)
                booleans |= 64;
            if (clrType.IsValueClass)
                booleans |= 128;
            return Tuple.Create(baseSize, baseTypeId, componentTypeId, elementSize, elementType, booleans, name);
        }

        private Tuple<string, int, int, int>[] GetClrTypeFields(int clrTypeId)
        {
            ClrType clrType = clrTypesCache[clrTypeId];
            IList<ClrInstanceField> clrFields = clrType.Fields;
            Tuple<string, int, int, int>[] fields = new Tuple<string, int, int, int>[clrFields.Count];

            for (int i = 0; i < fields.Length; i++)
            {
                string name = clrFields[i].Name;
                int typeId = GetClrTypeId(clrFields[i].Type);
                int offsetWhenValueClass = (int)clrFields[i].GetAddress(0, true);
                int offsetNotValueClass = (int)clrFields[i].GetAddress(0, false);

                fields[i] = Tuple.Create(name, typeId, offsetWhenValueClass, offsetNotValueClass);
            }
            return fields;
        }

        private ulong GetClrTypeArrayElementAddress(Tuple<int, ulong, int> input)
        {
            int clrTypeId = input.Item1;
            ulong address = input.Item2;
            int index = input.Item3;
            ClrType clrType = clrTypesCache[clrTypeId];

            return clrType.GetArrayElementAddress(address, index);
        }

        private int GetClrTypeArrayLength(Tuple<int, ulong> input)
        {
            int clrTypeId = input.Item1;
            ulong address = input.Item2;
            ClrType clrType = clrTypesCache[clrTypeId];

            return clrType.GetArrayLength(address);
        }

        private int GetClrTypeStaticField(Tuple<int, string> input)
        {
            int clrTypeId = input.Item1;
            string fieldName = input.Item2;
            ClrType clrType = clrTypesCache[clrTypeId];
            ClrStaticField clrStaticField = clrType.GetStaticFieldByName(fieldName);

            return GetClrTypeId(clrStaticField?.Type);
        }

        private ulong GetClrStaticFieldAddress(Tuple<int, string, int> input)
        {
            int clrTypeId = input.Item1;
            string fieldName = input.Item2;
            int clrAppDomainId = input.Item3;
            ClrType clrType = clrTypesCache[clrTypeId];
            ClrStaticField clrStaticField = clrType.GetStaticFieldByName(fieldName);
            ClrAppDomain clrAppDomain = clrType.Module.Runtime.AppDomains.First(ad => ad.Id == clrAppDomainId);

            return clrStaticField.GetAddress(clrAppDomain);
        }

        private Tuple<int, Tuple<ulong, int>[]> EnumerateVariables(IEnumerable<Tuple<ulong, int>> collection, int batchCount)
        {
            int enumeratorId;

            lock (variableEnumeratorsCache)
            {
                enumeratorId = nextVariableEnumeratorId++;
                variableEnumeratorsCache.TryAdd(enumeratorId, collection.GetEnumerator());
            }
            return Tuple.Create(enumeratorId, GetVariableEnumeratorNextBatch(Tuple.Create(enumeratorId, batchCount)));
        }

        private Tuple<ulong, int>[] GetVariableEnumeratorNextBatch(Tuple<int, int> input)
        {
            int enumeratorId = input.Item1;
            int batchCount = input.Item2;
            IEnumerator<Tuple<ulong, int>> enumerator = variableEnumeratorsCache[enumeratorId];
            List<Tuple<ulong, int>> result = new List<Tuple<ulong, int>>();

            while (result.Count < batchCount && enumerator.MoveNext())
                result.Add(enumerator.Current);

            // If we hit the end, dispose enumerator
            if (result.Count < batchCount)
                DisposeVariableEnumerator(enumeratorId);
            return result.ToArray();
        }

        private bool DisposeVariableEnumerator(int enumeratorId)
        {
            IEnumerator<Tuple<ulong, int>> enumerator;

            if (variableEnumeratorsCache.TryRemove(enumeratorId, out enumerator))
            {
                enumerator.Dispose();
                return true;
            }
            return false;
        }

        private static string[] GetClrArgumentsNames(ClrStackFrame frame)
        {
            var imd = frame.Module.MetadataImport;
            var sb = new StringBuilder(64);
            List<string> arguments = new List<string>(frame.Arguments.Count);
            IntPtr paramEnum = IntPtr.Zero;
            uint fetched = 0;
            int paramDef;

            imd.EnumParams(ref paramEnum, (int)frame.Method.MetadataToken, out paramDef, 1, out fetched);
            while (fetched == 1)
            {
                int pmd;
                uint pulSequence, pchName, pdwAttr, pdwCPlusTypeFlag, pcchValue;
                IntPtr ppValue;

                imd.GetParamProps(paramDef, out pmd, out pulSequence, sb, (uint)sb.Capacity, out pchName, out pdwAttr, out pdwCPlusTypeFlag, out ppValue, out pcchValue);
                arguments.Add(sb.ToString());
                sb.Clear();
                imd.EnumParams(ref paramEnum, (int)frame.Method.MetadataToken, out paramDef, 1, out fetched);
            }

            imd.CloseEnum(paramEnum);
            if (arguments.Count == frame.Arguments.Count - 1)
            {
                arguments.Insert(0, "this");
            }

            return arguments.ToArray();
        }

        private static DkmProcess GetProcess(uint processId)
        {
            return DkmProcess.GetProcesses()[(int)processId];
        }

        /// <summary>
        /// Finds the IL offset for the specified frame.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="instructionPointer">The instruction pointer.</param>
        internal static uint FindIlOffset(ClrMethod method, ulong instructionPointer)
        {
            ulong ip = instructionPointer;
            uint last = uint.MaxValue;

            foreach (var item in method.ILOffsetMap)
            {
                if (item.StartAddress > ip)
                    return last;
                if (ip <= item.EndAddress)
                    return (uint)item.ILOffset;
                last = (uint)item.ILOffset;
            }

            return last;
        }

        /// <summary>
        /// Finds the IL offset for the specified frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        private static uint FindIlOffset(ClrStackFrame frame)
        {
            return FindIlOffset(frame.Method, frame.InstructionPointer);
        }
    }
}
