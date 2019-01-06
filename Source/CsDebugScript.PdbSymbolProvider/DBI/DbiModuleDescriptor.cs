using CsDebugScript.PdbSymbolProvider.Utility;
using CsDebugScript.Engine.Utility;

namespace CsDebugScript.PdbSymbolProvider.DBI
{
    /// <summary>
    /// DBI stream module description.
    /// </summary>
    public class DbiModuleDescriptor
    {
        /// <summary>
        /// Cache of files compiled in this module.
        /// </summary>
        private SimpleCacheStruct<string[]> filesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbiModuleDescriptor"/> class.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="moduleList">Owning module list.</param>
        public DbiModuleDescriptor(IBinaryReader reader, DbiModuleList moduleList)
        {
            ModuleList = moduleList;
            filesCache = SimpleCache.CreateStruct(() =>
            {
                string[] files = new string[NumberOfFiles];

                for (int i = 0; i < files.Length; i++)
                    files[i] = ModuleList.GetFileName(i + StartingFileIndex);
                return files;
            });
            Header = ModuleInfoHeader.Read(reader);
            ModuleName = reader.ReadCString();
            ObjectFileName = reader.ReadCString();

            // Descriptors should be aligned at 4 bytes
            if (reader.Position % 4 != 0)
                reader.Position += 4 - reader.Position % 4;
        }

        /// <summary>
        /// Gets owning module list.
        /// </summary>
        public DbiModuleList ModuleList { get; private set; }

        /// <summary>
        /// Gets first file index from owning module list files array that is compiled into this module.
        /// </summary>
        public int StartingFileIndex { get; internal set; }

        /// <summary>
        /// Gets the header of this module descriptor.
        /// </summary>
        public ModuleInfoHeader Header { get; private set; }

        /// <summary>
        /// The module name. This is usually either a full path to an object file (either directly passed to link.exe or from an archive)
        /// or a string of the form Import:&lt;dll name&gt;.
        /// </summary>
        public string ModuleName { get; private set; }

        /// <summary>
        /// The object file name. In the case of an module that is linked directly passed to link.exe, this is the same as <see cref="ModuleName"/>.
        /// In the case of a module that comes from an archive, this is usually the full path to the archive.
        /// </summary>
        public string ObjectFileName { get; private set; }

        /// <summary>
        /// Gets all files compiled into this module.
        /// </summary>
        public string[] Files => filesCache.Value;

        #region Header data
        /// <summary>
        /// Stream index of module debug info.
        /// </summary>
        public ushort ModuleStreamIndex => Header.ModuleStreamIndex;

        /// <summary>
        /// Size of local symbol debug info in above stream
        /// </summary>
        public uint SymbolDebugInfoByteSize => Header.SymbolDebugInfoByteSize;

        /// <summary>
        /// Size of C11 line number info in above stream
        /// </summary>
        public uint C11LineInfoByteSize => Header.C11LineInfoByteSize;

        /// <summary>
        /// Size of C13 line number info in above stream
        /// </summary>
        public uint C13LineInfoByteSize => Header.C13LineInfoByteSize;

        /// <summary>
        /// Number of files contributing to this module
        /// </summary>
        public uint NumberOfFiles => Header.NumberOfFiles;

        /// <summary>
        /// Name Index for source file name.
        /// </summary>
        public uint SourceFileNameIndex => Header.SourceFileNameIndex;

        /// <summary>
        /// Name Index for path to compiler PDB.
        /// </summary>
        public uint PdbFilePathNameIndex => Header.PdbFilePathNameIndex;
        #endregion
    }
}
