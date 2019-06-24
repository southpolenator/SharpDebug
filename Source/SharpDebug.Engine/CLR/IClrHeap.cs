using System.Collections.Generic;

namespace SharpDebug.CLR
{
    /// <summary>
    /// CLR code Heap interface is an abstraction for the whole GC Heap. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public interface IClrHeap
    {
        /// <summary>
        /// Gets the runtime.
        /// </summary>
        IClrRuntime Runtime { get; }

        /// <summary>
        /// Returns <c>true</c> if the GC heap is in a consistent state for heap enumeration.
        /// This will return <c>false</c> if the process was stopped in the middle of a GC,
        /// which can cause the GC heap to be unwalkable.
        /// </summary>
        /// <remarks>
        /// You may still attempt to walk the heap if this function returns <c>false</c>, but you
        /// will likely only be able to partially walk each segment.
        /// </remarks>
        bool CanWalkHeap { get; }

        /// <summary>
        /// Gets the total size of the heap. It is defined as the sum of the length of all segments.
        /// </summary>
        ulong TotalHeapSize { get; }

        /// <summary>
        /// Enumerates all objects on the heap.
        /// </summary>
        /// <returns>An enumeration of all objects on the heap.</returns>
        IEnumerable<Variable> EnumerateObjects();

        /// <summary>
        /// Get the size by generation 0, 1, 2, 3. The large object heap is Gen 3 here.
        /// The sum of all of these should add up to the TotalHeapSize.
        /// </summary>
        /// <param name="generation">The generation.</param>
        ulong GetSizeByGeneration(int generation);

        /// <summary>
        /// Gets the type of the object at the specified address.
        /// </summary>
        /// <param name="objectAddress">The object address.</param>
        IClrType GetObjectType(ulong objectAddress);
    }
}
