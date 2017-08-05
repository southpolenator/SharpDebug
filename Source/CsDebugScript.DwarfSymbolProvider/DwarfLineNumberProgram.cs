using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CsDebugScript.DwarfSymbolProvider
{
    internal class DwarfLineNumberProgram
    {
        // TODO: Remove reading using structure
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct LineNumberProgramHeader
        {
            public const int MaximumOperationsPerInstruction = 1;

            public uint UnitLength; // 12 byte in DWARF-64
            public ushort Version;
            public uint HeaderLength; // 8 byte in DWARF-64
            public byte MinimumInstructionLength;
            // byte MaximumOperationsPerInstruction; (// not in DWARF 2
            public byte DefaultIsStatement;
            public sbyte LineBase;
            public byte LineRange;
            public byte OperationCodeBase;
            // LEB128 StandardOperationCodeLengths[OperationCodeBase];
            // string IncludeDirectories[] // zero byte terminated
            // FileInfo Files[] // zero byte terminated
        }

        internal class DwarfFile
        {
            public string Name { get; set; }

            public string Directory { get; set; }

            public string Path { get; set; }

            public uint LastModification { get; set; }

            public uint Length { get; set; }

            public List<LineInfo> Lines { get; set; } = new List<LineInfo>();

            public override string ToString()
            {
                return Name;
            }
        }

        internal class LineInfo
        {
            public DwarfFile File { get; set; }

            public uint Address { get; set; }

            public uint Line { get; set; }

            public uint Column { get; set; }
        }

        internal class DwarfLineParsingState
        {
            public DwarfFile File { get; set; }

            public uint Address { get; set; }

            public uint OperationIndex { get; set; }

            public uint Line { get; set; }

            public uint Column { get; set; }

            public bool IsStatement { get; set; }

            public bool IsBasicBlock { get; set; }

            public bool IsSequenceEnd { get; set; }

            public bool IsPrologueEnd { get; set; }

            public bool IsEpilogueEnd { get; set; }

            public uint Isa { get; set; }

            public uint Discriminator { get; set; }

            public LineNumberProgramHeader Header { get; private set; }

            public DwarfLineParsingState(LineNumberProgramHeader header, DwarfFile defaultFile)
            {
                Header = header;
                Reset(defaultFile);
            }

            public void Reset(DwarfFile defaultFile)
            {
                Address = 0;
                OperationIndex = 0;
                File = defaultFile;
                Line = 1;
                Column = 0;
                IsStatement = Header.DefaultIsStatement != 0;
                IsBasicBlock = false;
                IsSequenceEnd = false;
                IsPrologueEnd = false;
                IsEpilogueEnd = false;
                Isa = 0;
                Discriminator = 0;
            }

            public void AdvanceAddress(int operationAdvance)
            {
                int addressAdvance = Header.MinimumInstructionLength * (((int)OperationIndex + operationAdvance) / LineNumberProgramHeader.MaximumOperationsPerInstruction);

                Address += (uint)addressAdvance;
                OperationIndex = (OperationIndex + (uint)operationAdvance) % LineNumberProgramHeader.MaximumOperationsPerInstruction;
            }

            public void AddCurrentLineInfo()
            {
                File.Lines.Add(new LineInfo()
                {
                    File = File,
                    Address = Address,
                    Column = Column,
                    Line = Line,
                });
            }
        }

        public DwarfLineNumberProgram(DwarfMemoryReader debugLine, ulong codeSegmentOffset)
        {
            Header = debugLine.ReadStructure<LineNumberProgramHeader>(debugLine.Position);
            Files = ReadData(debugLine, (uint)codeSegmentOffset);
        }

        public List<DwarfFile> Files { get; private set; }

        internal LineNumberProgramHeader Header { get; private set; }

        internal int Length
        {
            get
            {
                return (int)Header.UnitLength + Marshal.SizeOf(Header.UnitLength.GetType());
            }
        }

        private List<DwarfFile> ReadData(DwarfMemoryReader debugLine, uint codeSegmentOffset)
        {
            int beginPosition = debugLine.Position;
            int endPosition = debugLine.Position + Length;
            uint[] operationCodeLengths = new uint[Header.OperationCodeBase];

            debugLine.Position = beginPosition + Marshal.SizeOf<LineNumberProgramHeader>();
            operationCodeLengths[0] = 0;
            for (int i = 1; i < operationCodeLengths.Length && debugLine.Position < endPosition; i++)
            {
                operationCodeLengths[i] = debugLine.LEB128();
            }

            // Read directories
            List<string> directories = new List<string>();

            while (debugLine.Position < endPosition && debugLine.Peek() != 0)
            {
                string directory = debugLine.ReadAnsiString();

                directory = directory.Replace('/', Path.DirectorySeparatorChar);
                directories.Add(directory);
            }
            debugLine.ReadByte(); // Skip zero termination byte

            // Read files
            List<DwarfFile> files = new List<DwarfFile>();

            while (debugLine.Position < endPosition && debugLine.Peek() != 0)
            {
                files.Add(ReadFile(debugLine, directories));
            }
            debugLine.ReadByte(); // Skip zero termination byte

            // Parse lines
            DwarfLineParsingState state = new DwarfLineParsingState(Header, files.FirstOrDefault());
            uint lastAddress = 0;

            while (debugLine.Position < endPosition)
            {
                byte operationCode = debugLine.ReadByte();

                if (operationCode >= operationCodeLengths.Length)
                {
                    // Special operation code
                    int adjustedOperationCode = operationCode - Header.OperationCodeBase;
                    int operationAdvance = adjustedOperationCode / Header.LineRange;
                    state.AdvanceAddress(operationAdvance);
                    int lineAdvance = Header.LineBase + (adjustedOperationCode % Header.LineRange);
                    state.Line += (uint)lineAdvance;
                    state.AddCurrentLineInfo();
                    state.IsBasicBlock = false;
                    state.IsPrologueEnd = false;
                    state.IsEpilogueEnd = false;
                    state.Discriminator = 0;
                }
                else
                {
                    switch ((DwarfLineNumberStandardOpcode)operationCode)
                    {
                        case DwarfLineNumberStandardOpcode.Extended:
                            {
                                uint extendedLength = debugLine.LEB128();
                                int newPosition = debugLine.Position + (int)extendedLength;
                                DwarfLineNumberExtendedOpcode extendedCode = (DwarfLineNumberExtendedOpcode)debugLine.ReadByte();

                                switch (extendedCode)
                                {
                                    case DwarfLineNumberExtendedOpcode.EndSequence:
                                        lastAddress = state.Address;
                                        state.IsSequenceEnd = true;
                                        state.AddCurrentLineInfo();
                                        state.Reset(files.FirstOrDefault());
                                        break;
                                    case DwarfLineNumberExtendedOpcode.SetAddress:
                                        {
                                            state.Address = debugLine.ReadUint();
                                            if (state.Address == 0)
                                            {
                                                state.Address = lastAddress;
                                            }
                                            state.OperationIndex = 0;
                                        }
                                        break;
                                    case DwarfLineNumberExtendedOpcode.DefineFile:
                                        state.File = ReadFile(debugLine, directories);
                                        files.Add(state.File);
                                        break;
                                    case DwarfLineNumberExtendedOpcode.SetDiscriminator:
                                        state.Discriminator = debugLine.LEB128();
                                        break;
                                    default:
                                        throw new Exception($"Unsupported DwarfLineNumberExtendedOpcode: {extendedCode}");
                                }
                                debugLine.Position = newPosition;
                            }
                            break;
                        case DwarfLineNumberStandardOpcode.Copy:
                            state.AddCurrentLineInfo();
                            state.IsBasicBlock = false;
                            state.IsPrologueEnd = false;
                            state.IsEpilogueEnd = false;
                            state.Discriminator = 0;
                            break;
                        case DwarfLineNumberStandardOpcode.AdvancePc:
                            state.AdvanceAddress((int)debugLine.LEB128());
                            break;
                        case DwarfLineNumberStandardOpcode.AdvanceLine:
                            state.Line += debugLine.SLEB128();
                            break;
                        case DwarfLineNumberStandardOpcode.SetFile:
                            state.File = files[(int)debugLine.LEB128() - 1];
                            break;
                        case DwarfLineNumberStandardOpcode.SetColumn:
                            state.Column = debugLine.LEB128();
                            break;
                        case DwarfLineNumberStandardOpcode.NegateStmt:
                            state.IsStatement = !state.IsStatement;
                            break;
                        case DwarfLineNumberStandardOpcode.SetBasicBlock:
                            state.IsBasicBlock = true;
                            break;
                        case DwarfLineNumberStandardOpcode.ConstAddPc:
                            state.AdvanceAddress((255 - Header.OperationCodeBase) / Header.LineRange);
                            break;
                        case DwarfLineNumberStandardOpcode.FixedAdvancePc:
                            state.Address += debugLine.ReadUshort();
                            state.OperationIndex = 0;
                            break;
                        case DwarfLineNumberStandardOpcode.SetPrologueEnd:
                            state.IsPrologueEnd = true;
                            break;
                        case DwarfLineNumberStandardOpcode.SetEpilogueBegin:
                            state.IsEpilogueEnd = true;
                            break;
                        case DwarfLineNumberStandardOpcode.SetIsa:
                            state.Isa = debugLine.LEB128();
                            break;
                        default:
                            throw new Exception($"Unsupported DwarfLineNumberStandardOpcode: {(DwarfLineNumberStandardOpcode)operationCode}");
                    }
                }
            }

            // Fix lines in files...
            foreach (DwarfFile file in files)
            {
                file.Lines = file.Lines.Where(l => l.Address >= codeSegmentOffset).ToList();
                for (int i = 0; i < file.Lines.Count; i++)
                {
                    file.Lines[i].Address -= codeSegmentOffset;
                }
            }
            return files;
        }

        private static DwarfFile ReadFile(DwarfMemoryReader debugLine, List<string> directories)
        {
            string name = debugLine.ReadAnsiString();
            int directoryIndex = (int)debugLine.LEB128();
            uint lastModification = debugLine.LEB128();
            uint length = debugLine.LEB128();
            string directory = directoryIndex > 0 ? directories[directoryIndex - 1] : null;
            string path = name;

            try
            {
                path = string.IsNullOrEmpty(directory) || Path.IsPathRooted(path) ? name : Path.Combine(directory, name);
            }
            catch
            {
            }

            return new DwarfFile()
            {
                Name = name,
                Directory = directory,
                Path = path,
                LastModification = lastModification,
                Length = length,
            };
        }
    }
}
