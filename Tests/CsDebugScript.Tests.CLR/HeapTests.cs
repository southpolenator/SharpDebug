using CsDebugScript.CLR;
using System.Linq;
using Xunit;

namespace CsDebugScript.Tests.CLR
{
    [Collection("CLR Types")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class HeapTests
    {
        [Fact]
        public void HeapSize()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();
            IClrHeap heap = runtime.Heap;

            Assert.True(heap.TotalHeapSize > 0);

            ulong generationSizes = 0;

            for (int generation = 0; generation <= 3; generation++)
            {
                generationSizes += heap.GetSizeByGeneration(generation);
            }
            Assert.Equal(heap.TotalHeapSize, generationSizes);
        }

        [Fact]
        public void HeapEnumeration()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();
            IClrHeap heap = runtime.Heap;

            Assert.NotNull(heap);
            Assert.NotNull(runtime.GCThreads);
            Assert.True(runtime.HeapCount > 0);
            Assert.NotNull(runtime.ToString());
            Assert.True(heap.CanWalkHeap);

            int count = 0;

            foreach (Variable variable in heap.EnumerateObjects())
            {
                Assert.NotNull(variable);
                count++;
            }

            Assert.True(count > 0);
        }
    }

    [Collection("CLR Types Server")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class HeapTestsServer
    {
        [Fact]
        public void ServerSegmentTests()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            Assert.True(runtime.ServerGC);
        }
    }

    [Collection("CLR Types Workstation")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class HeapTestsWorkstation
    {
        [Fact]
        public void WorkstationSegmentTests()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            Assert.False(runtime.ServerGC);
        }
    }
}
