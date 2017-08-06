using CsDebugScript.Engine;
using System.Collections.Generic;
using CsDebugScript.Engine.SymbolProviders;
using System.IO;

namespace CsDebugScript.DwarfSymbolProvider
{
    /// <summary>
    /// DWARF symbol provider that can be used with the <see cref="Context"/>.
    /// </summary>
    /// <seealso cref="CsDebugScript.Engine.SymbolProviders.PerModuleSymbolProvider" />
    public class DwarfSymbolProvider : PerModuleSymbolProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DwarfSymbolProvider"/> class.
        /// </summary>
        /// <param name="fallbackSymbolProvider">The fall-back symbol provider.</param>
        public DwarfSymbolProvider(ISymbolProvider fallbackSymbolProvider = null)
            : base(fallbackSymbolProvider)
        {
        }

        /// <summary>
        /// Loads symbol provider module from the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>Interface for symbol provider module</returns>
        public override ISymbolProviderModule LoadModule(Module module)
        {
            // Try to load PDB file into our own DIA session
            string location = File.Exists(module.MappedImageName) ? module.MappedImageName : module.ImageName;

            if (File.Exists(location))
            {
                try
                {
                    IDwarfImage image = new PeImage(location);
                    var debugStrings = ParseDebugStrings(image.DebugDataStrings);
                    var compilationUnits = ParseCompilationUnits(image.DebugData, image.DebugDataDescription, debugStrings, image.CodeSegmentOffset);
                    var lineNumberPrograms = ParseLineNumberPrograms(image.DebugLine, image.CodeSegmentOffset);
                    var commonInformationEntries = ParseCommonInformationEntries(image.DebugFrame, image.Is64bit);

                    return new DwarfSymbolProviderModule(module, compilationUnits, lineNumberPrograms, commonInformationEntries, image.CodeSegmentOffset, image.Is64bit);
                }
                catch
                {
                }
            }
            return null;
        }

        /// <summary>
        /// Parses the debug data strings.
        /// </summary>
        /// <param name="debugDataStrings">The debug data strings.</param>
        /// <returns>Dictionary of strings located by position in the stream</returns>
        private Dictionary<int, string> ParseDebugStrings(byte[] debugDataStrings)
        {
            using (DwarfMemoryReader reader = new DwarfMemoryReader(debugDataStrings))
            {
                Dictionary<int, string> strings = new Dictionary<int, string>();

                while (!reader.IsEnd)
                {
                    int position = reader.Position;

                    strings.Add(position, reader.ReadString());
                }

                return strings;
            }
        }

        /// <summary>
        /// Parses the compilation units.
        /// </summary>
        /// <param name="debugData">The debug data.</param>
        /// <param name="debugDataDescription">The debug data description.</param>
        /// <param name="debugStrings">The debug strings.</param>
        /// <param name="codeSegmentOffset">The code segment offset.</param>
        private static DwarfCompilationUnit[] ParseCompilationUnits(byte[] debugData, byte[] debugDataDescription, Dictionary<int, string> debugStrings, ulong codeSegmentOffset)
        {
            using (DwarfMemoryReader debugDataReader = new DwarfMemoryReader(debugData))
            using (DwarfMemoryReader debugDataDescriptionReader = new DwarfMemoryReader(debugDataDescription))
            {
                List<DwarfCompilationUnit> compilationUnits = new List<DwarfCompilationUnit>();

                while (!debugDataReader.IsEnd)
                {
                    DwarfCompilationUnit compilationUnit = new DwarfCompilationUnit(debugDataReader, debugDataDescriptionReader, debugStrings, codeSegmentOffset);

                    compilationUnits.Add(compilationUnit);
                }

                return compilationUnits.ToArray();
            }
        }

        /// <summary>
        /// Parses the line number programs.
        /// </summary>
        /// <param name="debugLine">The debug line.</param>
        /// <param name="codeSegmentOffset">The code segment offset.</param>
        private static DwarfLineNumberProgram[] ParseLineNumberPrograms(byte[] debugLine, ulong codeSegmentOffset)
        {
            using (DwarfMemoryReader debugLineReader = new DwarfMemoryReader(debugLine))
            {
                List<DwarfLineNumberProgram> programs = new List<DwarfLineNumberProgram>();

                while (!debugLineReader.IsEnd)
                {
                    DwarfLineNumberProgram program = new DwarfLineNumberProgram(debugLineReader, codeSegmentOffset);

                    programs.Add(program);
                }

                return programs.ToArray();
            }
        }

        /// <summary>
        /// Parses the common information entries.
        /// </summary>
        /// <param name="debugFrame">The debug frame.</param>
        /// <param name="is64bit">Set to <c>true</c> if image is 64 bit.</param>
        private static DwarfCommonInformationEntry[] ParseCommonInformationEntries(byte[] debugFrame, bool is64bit)
        {
            using (DwarfMemoryReader debugFrameReader = new DwarfMemoryReader(debugFrame))
            {
                byte defaultAddressSize = (byte)(is64bit ? 8 : 4);

                return DwarfCommonInformationEntry.ParseAll(debugFrameReader, defaultAddressSize);
            }
        }
    }
}
