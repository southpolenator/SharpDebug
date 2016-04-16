using CsScripts;
using System.Collections.Generic;

namespace CsScriptManaged.CLR
{
    /// <summary>
    /// CLR code Heap is a abstraction for the whole GC Heap. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public class Heap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Heap"/> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="clrHeap">The CLR heap.</param>
        internal Heap(Runtime runtime, Microsoft.Diagnostics.Runtime.ClrHeap clrHeap)
        {
            Runtime = runtime;
            ClrHeap = clrHeap;
        }

        /// <summary>
        /// Gets the runtime.
        /// </summary>
        public Runtime Runtime { get; private set; }

        /// <summary>
        /// Returns <c>true</c> if the GC heap is in a consistent state for heap enumeration.
        /// This will return <c>false</c> if the process was stopped in the middle of a GC,
        /// which can cause the GC heap to be unwalkable.
        /// </summary>
        /// <remarks>
        /// You may still attempt to walk the heap if this function returns <c>false</c>, but you
        /// will likely only be able to partially walk each segment.
        /// </remarks>
        public bool CanWalkHeap
        {
            get
            {
                return ClrHeap.CanWalkHeap;
            }
        }

        /// <summary>
        /// Gets the total size of the heap. It is defined as the sum of the length of all segments.
        /// </summary>
        public ulong TotalHeapSize
        {
            get
            {
                return ClrHeap.TotalHeapSize;
            }
        }

        /// <summary>
        /// Gets the CLR heap.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrHeap ClrHeap { get; private set; }

        /// <summary>
        /// Enumerates all objects on the heap.
        /// </summary>
        /// <returns>An enumeration of all objects on the heap.</returns>
        public IEnumerable<Variable> EnumerateObjects()
        {
            foreach (ulong address in ClrHeap.EnumerateObjectAddresses())
            {
                var clrType = ClrHeap.GetObjectType(address);
                var codeType = Runtime.Process.FromClrType(clrType);

                yield return Variable.Create(codeType, address);
            }
        }

        /// <summary>
        /// Get the size by generation 0, 1, 2, 3. The large object heap is Gen 3 here.
        /// The sum of all of these should add up to the TotalHeapSize.
        /// </summary>
        /// <param name="generation">The generation.</param>
        public ulong GetSizeByGeneration(int generation)
        {
            return ClrHeap.GetSizeByGen(generation);
        }
    }
}
