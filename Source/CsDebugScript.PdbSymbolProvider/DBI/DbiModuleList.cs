using CsDebugScript.PdbSymbolProvider.Utility;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// List of modules read from DBI stream.
    /// </summary>
    public class DbiModuleList : IReadOnlyList<DbiModuleDescriptor>
    {
        /// <summary>
        /// Dictionary cache of file names.
        /// </summary>
        private DictionaryCache<uint, string> fileNameCache;

        /// <summary>
        /// Cache of module list.
        /// </summary>
        private SimpleCacheStruct<List<DbiModuleDescriptor>> modulesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbiModuleList"/> class.
        /// </summary>
        /// <param name="moduleInfoStream">Module info stream binary reader.</param>
        /// <param name="fileInfoStream">File info stream binary reader.</param>
        public DbiModuleList(IBinaryReader moduleInfoStream, IBinaryReader fileInfoStream)
        {
            ModuleInfoStream = moduleInfoStream;
            modulesCache = SimpleCache.CreateStruct(() =>
            {
                var descriptors = new List<DbiModuleDescriptor>();
                while (ModuleInfoStream.BytesRemaining > 0)
                    descriptors.Add(new DbiModuleDescriptor(ModuleInfoStream, this));
                return descriptors;
            });

            FileInfoStream = fileInfoStream;
            if (fileInfoStream.Length > 0)
            {
                // Header:
                //   ushort NumModules;
                //   ushort NumSourceFiles;
                // Following this header the File Info Substream is laid out as follows:
                //   ushort ModIndices[NumModules];
                //   ushort ModFileCounts[NumModules];
                //   uint FileNameOffsets[NumSourceFiles];
                //   char Names[][NumSourceFiles];
                // with the caveat that `NumSourceFiles` cannot be trusted, so
                // it is computed by summing the `ModFileCounts` array.
                ushort modulesCount = fileInfoStream.ReadUshort();
                ushort headerSourceFilesCount = fileInfoStream.ReadUshort();

                // First is an array of `NumModules` module indices. This does not seem to be
                // used for anything meaningful, so we ignore it.
                ushort[] moduleIndexes = fileInfoStream.ReadUshortArray(modulesCount);

                ushort[] moduleFileCounts = fileInfoStream.ReadUshortArray(modulesCount);

                // Compute the real number of source files. We can't trust the value in
                // of headerSourceFilesCount because it is an ushort, and the sum of all
                // source file counts might be larger than an ushort. So we compute the real
                // count by summing up the individual counts.
                int sourceFilesCount = 0;

                for (int i = 0; i < moduleFileCounts.Length; i++)
                    sourceFilesCount += moduleFileCounts[i];

                // In the reference implementation, this array is where the pointer documented
                // at the definition of ModuleInfoHeader::FileNameOffs points to.  Note that
                // although the field in ModuleInfoHeader is ignored this array is not, as it
                // is the authority on where each filename begins in the names buffer.
                FileNameOffsets = fileInfoStream.ReadUintArray(sourceFilesCount);
                FileNamesStream = fileInfoStream.ReadSubstream();
                fileNameCache = new DictionaryCache<uint, string>((uint namesOffset) =>
                {
                    FileNamesStream.Position = namesOffset;
                    return FileNamesStream.ReadCString();
                });

                if (Modules.Count != modulesCount)
                    throw new Exception("Inconsistent number of modules");

                int nextFileIndex = 0;

                for (int i = 0; i < modulesCount; i++)
                {
                    Modules[i].StartingFileIndex = nextFileIndex;
                    nextFileIndex += moduleFileCounts[i];
                }
            }
        }

        /// <summary>
        /// Gets total number of source files in all modules.
        /// </summary>
        public int SourceFileCount => FileNameOffsets.Length;

        /// <summary>
        /// Gets number of modules in this list.
        /// </summary>
        public int ModuleCount => Modules.Count;

        /// <summary>
        /// Gets module info stream binary reader.
        /// </summary>
        public IBinaryReader ModuleInfoStream { get; private set; }

        /// <summary>
        /// Gets file info stream binary reader.
        /// </summary>
        public IBinaryReader FileInfoStream { get; private set; }

        /// <summary>
        /// Gets the list of modules.
        /// </summary>
        internal List<DbiModuleDescriptor> Modules => modulesCache.Value;

        /// <summary>
        /// Gets the file names stream binary reader.
        /// </summary>
        internal IBinaryReader FileNamesStream { get; private set; }

        /// <summary>
        /// Gets file name offsets in file names stream.
        /// </summary>
        internal uint[] FileNameOffsets { get; private set; }

        #region IReadOnlyList implementation
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => Modules.Count;

        /// <summary>
        /// Gets the <see cref="DbiModuleDescriptor"/> at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public DbiModuleDescriptor this[int index] => Modules[index];

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<DbiModuleDescriptor> GetEnumerator()
        {
            return Modules.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Modules.GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Gets file name at the specified position
        /// </summary>
        /// <param name="fileIndex">Index in all files list.</param>
        public string GetFileName(int fileIndex)
        {
            return fileNameCache[FileNameOffsets[fileIndex]];
        }
    }
}
