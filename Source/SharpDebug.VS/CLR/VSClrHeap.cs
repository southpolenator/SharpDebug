using SharpDebug.CLR;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpDebug.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of <see cref="IClrHeap"/>.
    /// </summary>
    internal class VSClrHeap : IClrHeap
    {
        /// <summary>
        /// The cache of <see cref="CanWalkHeap"/>.
        /// </summary>
        private SimpleCache<bool> canWalkHeapCache;

        /// <summary>
        /// The cache of <see cref="TotalHeapSize"/>.
        /// </summary>
        private SimpleCache<ulong> totalHeapSizeCache;

        /// <summary>
        /// Cache of types by virtual table address.
        /// </summary>
        private DictionaryCache<ulong, VSClrType> typesByAddressCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdHeap"/> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        public VSClrHeap(VSClrRuntime runtime)
        {
            VSRuntime = runtime;
            canWalkHeapCache = SimpleCache.Create(() => Proxy.GetClrHeapCanWalkHeap(VSRuntime.Process.Id, VSRuntime.Id));
            totalHeapSizeCache = SimpleCache.Create(() => Proxy.GetClrHeapTotalHeapSize(VSRuntime.Process.Id, VSRuntime.Id));
            typesByAddressCache = new DictionaryCache<ulong, VSClrType>((a) => default(VSClrType));
        }

        /// <summary>
        /// Gets the Visual Studio implementation of the runtime.
        /// </summary>
        public VSClrRuntime VSRuntime { get; private set; }

        /// <summary>
        /// Gets the Visual Studio debugger proxy.
        /// </summary>
        public VSDebuggerProxy Proxy => VSRuntime.Proxy;

        /// <summary>
        /// Gets the runtime.
        /// </summary>
        public IClrRuntime Runtime => VSRuntime;

        /// <summary>
        /// Returns <c>true</c> if the GC heap is in a consistent state for heap enumeration.
        /// This will return <c>false</c> if the process was stopped in the middle of a GC,
        /// which can cause the GC heap to be unwalkable.
        /// </summary>
        /// <remarks>
        /// You may still attempt to walk the heap if this function returns <c>false</c>, but you
        /// will likely only be able to partially walk each segment.
        /// </remarks>
        public bool CanWalkHeap => canWalkHeapCache.Value;

        /// <summary>
        /// Gets the total size of the heap. It is defined as the sum of the length of all segments.
        /// </summary>
        public ulong TotalHeapSize => totalHeapSizeCache.Value;

        /// <summary>
        /// Number of elements to be retrieved in a batch.
        /// </summary>
        internal const int EnumerationBatchSize = 1000;

        /// <summary>
        /// Enumerates all objects on the heap.
        /// </summary>
        /// <returns>An enumeration of all objects on the heap.</returns>
        public IEnumerable<Variable> EnumerateObjects()
        {
            Tuple<int, Tuple<ulong, int>[]> firstBatch = Proxy.EnumerateClrHeapObjects(VSRuntime.Process.Id, VSRuntime.Id, EnumerationBatchSize);

            return EnumerateVariables(VSRuntime, firstBatch);
        }

        /// <summary>
        /// Enumerates variables from the remote connection.
        /// </summary>
        /// <param name="runtime">The Visual Studio implementation of the runtime.</param>
        /// <param name="firstBatch">Tuple of enumeration id and first batch elements.</param>
        internal static IEnumerable<Variable> EnumerateVariables(VSClrRuntime runtime, Tuple<int, Tuple<ulong, int>[]> firstBatch)
        {
            VSDebuggerProxy proxy = runtime.Proxy;
            uint processId = runtime.Process.Id;
            int enumerationId = firstBatch.Item1;
            Tuple<ulong, int>[] batch = firstBatch.Item2;
            bool destroyed = batch.Length == EnumerationBatchSize;

            try
            {
                while (batch.Length > 0)
                {
                    foreach (Tuple<ulong, int> tuple in batch)
                    {
                        IClrType clrType = runtime.GetClrType(tuple.Item2);

                        if (clrType != null)
                        {
                            ulong address = tuple.Item1;
                            CodeType codeType = runtime.Process.FromClrType(clrType);
                            Variable variable;

                            if (codeType.IsPointer)
                                variable = Variable.CreatePointerNoCast(codeType, address);
                            else
                                variable = Variable.CreateNoCast(codeType, address);

                            // TODO: Can we get already upcast address and clr type from the remote connection?
                            yield return Variable.UpcastClrVariable(variable);
                        }
                    }

                    if (destroyed)
                        break;
                    batch = proxy.GetVariableEnumeratorNextBatch(processId, enumerationId, EnumerationBatchSize);
                    destroyed = batch.Length == EnumerationBatchSize;
                }
            }
            finally
            {
                if (!destroyed)
                    proxy.DisposeVariableEnumerator(processId, enumerationId);
            }
        }

        /// <summary>
        /// Gets the type of the object at the specified address.
        /// </summary>
        /// <param name="objectAddress">The object address.</param>
        public IClrType GetObjectType(ulong objectAddress)
        {
            try
            {
                ulong vtablePointer = VSRuntime.Process.ReadPointer(objectAddress);
                VSClrType type;

                if (!typesByAddressCache.TryGetExistingValue(vtablePointer, out type))
                    lock (typesByAddressCache)
                        if (!typesByAddressCache.TryGetExistingValue(vtablePointer, out type))
                        {
                            type = VSRuntime.GetClrType(Proxy.GetClrHeapObjectType(VSRuntime.Process.Id, VSRuntime.Id, objectAddress));
                            typesByAddressCache[vtablePointer] = type;
                        }
                return type;
            }
            catch
            {
                return VSRuntime.GetClrType(Proxy.GetClrHeapObjectType(VSRuntime.Process.Id, VSRuntime.Id, objectAddress));
            }
        }

        /// <summary>
        /// Get the size by generation 0, 1, 2, 3. The large object heap is Gen 3 here.
        /// The sum of all of these should add up to the TotalHeapSize.
        /// </summary>
        /// <param name="generation">The generation.</param>
        public ulong GetSizeByGeneration(int generation)
        {
            return Proxy.GetClrHeapSizeByGeneration(VSRuntime.Process.Id, VSRuntime.Id, generation);
        }
    }
}
