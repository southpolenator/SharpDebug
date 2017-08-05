using CsDebugScript.Engine;
using System.Collections.Generic;
using CsDebugScript.Engine.SymbolProviders;
using System;
using System.IO;

namespace CsDebugScript.DwarfSymbolProvider
{
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

                    return new DwarfSymbolProviderModule(module, compilationUnits, lineNumberPrograms, commonInformationEntries, debugStrings, image.CodeSegmentOffset, image.Is64bit);
                }
                catch
                {
                }
            }
            return null;
        }

        private Dictionary<int, string> ParseDebugStrings(byte[] debugDataStrings)
        {
            using (DwarfMemoryReader reader = new DwarfMemoryReader(debugDataStrings))
            {
                Dictionary<int, string> strings = new Dictionary<int, string>();

                while (!reader.IsEnd)
                {
                    int position = reader.Position;

                    strings.Add(position, reader.ReadAnsiString());
                }

                return strings;
            }
        }

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

        private static DwarfLineNumberProgram[] ParseLineNumberPrograms(byte[] debugLine, ulong codeSegmentOffset)
        {
            using (DwarfMemoryReader debugLineReader = new DwarfMemoryReader(debugLine))
            {
                List<DwarfLineNumberProgram> programs = new List<DwarfLineNumberProgram>();
                int position = 0;

                while (position < debugLine.Length)
                {
                    DwarfLineNumberProgram program = new DwarfLineNumberProgram(debugLineReader, codeSegmentOffset);

                    programs.Add(program);
                    position += program.Length;
                    debugLineReader.Position = position;
                }

                return programs.ToArray();
            }
        }

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
