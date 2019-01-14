using CsDebugScript.PdbSymbolProvider;
using CsDebugScript.PdbSymbolProvider.DBI;
using CsDebugScript.PdbSymbolProvider.SymbolRecords;
using CsDebugScript.PdbSymbolProvider.TPI;
using CsDebugScript.PdbSymbolProvider.TypeRecords;
using System.IO;
using System.Linq;
using Xunit;

namespace CsDebugScript.Tests
{
    [Trait("x86", "true")]
    [Trait("x64", "true")]
    public class PdbReaderTests
    {
        [Theory]
        [InlineData("NativeDumpTest.x64.pdb")]
        [InlineData("NativeDumpTest.x64.Release.pdb")]
        [InlineData("NativeDumpTest.x86.pdb")]
        [InlineData("NativeDumpTest.x86.Release.pdb")]
        [InlineData("NativeDumpTest.x64.VS2013.pdb")]
        [InlineData("NativeDumpTest.x64.VS2015.pdb")]
        public void ReadingAllData(string pdbFileName)
        {
            string pdbPath = Path.Combine(DumpInitialization.DefaultDumpPath, pdbFileName);

            using (PdbFile pdbFile = new PdbFile(pdbPath))
            {
                Test(pdbFile);
            }
        }

        private void Test(PdbFile pdbFile)
        {
            Assert.NotEmpty(pdbFile.FreePageMap);
            Assert.NotEmpty(pdbFile.Streams);
            Test(pdbFile.TpiStream);
            Test(pdbFile.IpiStream);
            Test(pdbFile.DbiStream);
            Test(pdbFile.PdbSymbolStream);
        }

        private void Test(DbiStream dbiStream)
        {
            Assert.NotEmpty(dbiStream.Modules);
            Assert.NotEmpty(dbiStream.SectionContributions);
            Assert.NotEmpty(dbiStream.SectionHeaders);
            Assert.NotEmpty(dbiStream.SectionMap);
            if (dbiStream.FpoRecords != null)
                Assert.NotEmpty(dbiStream.FpoRecords);
            Assert.NotNull(dbiStream.ECNames);
        }

        private void Test(SymbolStream symbolStream)
        {
            // Check that getting symbols by kind works correctly
            SymbolRecordKind[] kinds = symbolStream.references.Select(r => r.Kind).Distinct().ToArray();

            foreach (SymbolRecordKind kind in kinds)
                Assert.Equal(symbolStream.references.Count(r => r.Kind == kind), symbolStream[kind].Length);
        }

        private void Test(TpiStream tpiStream)
        {
            Assert.Equal(tpiStream.TypeRecordCount, tpiStream.references.Count);
            Assert.NotEmpty(tpiStream.HashValues);
            if (tpiStream.HashAdjusters != null)
                Assert.NotEmpty(tpiStream.HashAdjusters.Dictionary);

            // Verify that type offsets are correct in references array
            foreach (TypeIndexOffset offset in tpiStream.TypeIndexOffsets)
            {
                var reference = tpiStream.references[(int)offset.Type.ArrayIndex];
                Assert.Equal(offset.Offset, reference.DataOffset - RecordPrefix.Size);
            }

            // Verify that all types can be read
            for (int i = 0; i < tpiStream.references.Count; i++)
                Assert.NotNull(tpiStream[TypeIndex.FromArrayIndex(i)]);

            // Check that getting types by kind works correctly
            TypeLeafKind[] kinds = tpiStream.references.Select(r => r.Kind).Distinct().ToArray();

            foreach (TypeLeafKind kind in kinds)
                Assert.Equal(tpiStream.references.Count(r => r.Kind == kind), tpiStream[kind].Length);
        }
    }
}
