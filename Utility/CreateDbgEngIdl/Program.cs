using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CreateDbgEngIdl
{
    class Program
    {
        static string StripWin32Defs(string line)
        {
            line = Regex.Replace(line, "_(In|Out)_(reads|writes|writes_to)_(bytes_|)(opt_|)[(][^)]*[)]", "");
            return line.Replace("PCSTR ", "LPStr ").Replace("PSTR ", "LPStr ").Replace("PCWSTR ", "LPWStr ")
                       .Replace("PWSTR ", "LPWStr ").Replace("PULONG64 ", "unsigned __int64 * ").Replace("ULONG64 ", "unsigned __int64 ").Replace("LONG64 ", "__int64 ").Replace("PULONG ", "unsigned long * ")
                       .Replace("ULONG ", "unsigned long ").Replace("PBOOL ", "bool * ").Replace("BOOL ", "bool ").Replace("va_list ", "char * ").Replace("...", "SAFEARRAY(VARIANT) parameters").Replace("FARPROC ", "void * ").Replace("FARPROC*", "void **")
                       .Replace("UCHAR ", "unsigned char ").Replace("CHAR ", "char ").Replace("USHORT ", "unsigned short ").Replace("PVOID ", "void * ").Replace("UINT ", "unsigned int ")
                       .Replace("IN ", "").Replace("__in ", "").Replace("_In_ ", "").Replace("__out ", "").Replace("_Out_ ", "").Replace("OUT ", "").Replace("__out_opt ", "").Replace("_Out_opt_ ", "").Replace("OPTIONAL ", "")
                       .Replace("__in_opt ", "").Replace("_In_opt_ ", "").Replace("__reserved ", "").Replace("__inout ", "").Replace("_Inout_ ", "").Replace("_Reserved_ ", "");
        }

        static string RemoveSpaces(string line)
        {
            string newLine = line.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");

            do
            {
                line = newLine;
                newLine = line.Replace("  ", " ");
            }
            while (line != newLine);
            return newLine.Trim();
        }

        public enum MachineType
        {
            Native = 0,
            I386 = 0x014c,
            Itanium = 0x0200,
            x64 = 0x8664
        }

        public static MachineType GetMachineType(string fileName)
        {
            const int PE_POINTER_OFFSET = 60;
            const int MACHINE_OFFSET = 4;
            byte[] data = File.ReadAllBytes(fileName);

            // dos header is 64 bytes, last element, long (4 bytes) is the address of the PE header
            int PE_HEADER_ADDR = BitConverter.ToInt32(data, PE_POINTER_OFFSET);
            int machineUint = BitConverter.ToUInt16(data, PE_HEADER_ADDR + MACHINE_OFFSET);
            return (MachineType)machineUint;
        }

        static readonly string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        static readonly string[] HeaderFiles = new string[]
            {
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.15063.0\um\dbgeng.h"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.14393.0\um\dbgeng.h"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.10586.0\um\dbgeng.h"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.10240.0\um\dbgeng.h"),
                Path.Combine(ProgramFiles, @"Windows Kits\8.1\Include\um\DbgEng.h"),
            };
        static readonly string[] MidlPaths = new string[]
            {
                Path.Combine(ProgramFiles, @"Windows Kits\10\bin\x64\midl.exe"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\bin\x86\midl.exe"),
                Path.Combine(ProgramFiles, @"Windows Kits\8.1\bin\x64\midl.exe"),
                Path.Combine(ProgramFiles, @"Windows Kits\8.1\bin\x86\midl.exe"),
            };
        static readonly string[] UmPaths = new string[]
            {
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.15063.0\um"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.14393.0\um"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.10586.0\um"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.10240.0\um"),
                Path.Combine(ProgramFiles, @"Windows Kits\8.1\Include\um"),
            };
        static readonly string[] SharedPaths = new string[]
            {
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.15063.0\shared"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.14393.0\shared"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.10586.0\shared"),
                Path.Combine(ProgramFiles, @"Windows Kits\10\Include\10.0.10240.0\shared"),
                Path.Combine(ProgramFiles, @"Windows Kits\8.1\Include\shared"),
            };
        static readonly string[] VCBinPaths = new string[]
            {
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio 14.0\VC\bin"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio 12.0\VC\bin"),
            };
        static readonly string[] VCDiaSdkIdlPaths = new string[]
            {
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio\2017\Enterprise\DIA SDK\idl"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio\2017\Professional\DIA SDK\idl"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio\2017\Community\DIA SDK\idl"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio 14.0\DIA SDK\idl"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio 12.0\DIA SDK\idl"),
            };
        static readonly string[] VCDiaSdkIncludePaths = new string[]
            {
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio\2017\Enterprise\DIA SDK\include"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio\2017\Professional\DIA SDK\include"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio\2017\Community\DIA SDK\include"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio 14.0\DIA SDK\include"),
                Path.Combine(ProgramFiles, @"Microsoft Visual Studio 12.0\DIA SDK\include"),
            };
        static readonly string[] TlbImpPaths = new string[]
            {
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\x64\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.2 Tools\x64\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.2 Tools\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\x64\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\x64\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\x64\TlbImp.exe"),
                Path.Combine(ProgramFiles, @"Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\TlbImp.exe"),
            };

        static string FindFile(string fileName, params string[] paths)
        {
            foreach (string path in paths)
                if (File.Exists(path))
                    return path;

            throw new Exception(fileName + " not found. Have you installed Windows Kit?");
        }

        static string FindExe(string fileName, params string[] paths)
        {
            foreach (string path in paths)
                if (File.Exists(path))
                {
                    MachineType machineType = GetMachineType(path);

                    if (machineType == MachineType.x64 && !Environment.Is64BitOperatingSystem)
                        continue;

                    return path;
                }

            throw new Exception(fileName + " not found. Have you installed Windows Kit?");
        }

        static string FindFolder(string folderName, params string[] paths)
        {
            foreach (string path in paths)
                if (Directory.Exists(path))
                    return path;

            throw new Exception(folderName + " not found. Have you installed Windows Kit?");
        }

        static int Main(string[] args)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Directory.CreateDirectory(tempFolder);
            Directory.SetCurrentDirectory(tempFolder);

            try
            {
                // Find paths
                string HeaderFile = FindFile("DbgEng.h", HeaderFiles);
                string MidlPath = FindExe("midl.exe", MidlPaths);
                string UmPath = FindFolder("um", UmPaths);
                string SharedPath = FindFolder("um", SharedPaths);
                string VCBinPath = FindFolder("VC\\bin", VCBinPaths);
                string TlbImpPath = FindExe("TlbImp.exe", TlbImpPaths);
                string DiaIncludePath = FindFolder("include", VCDiaSdkIncludePaths);
                string DiaIdlPath = FindFolder("idl", VCDiaSdkIdlPaths);

                // Generate IDL file
                Regex defineDeclarationRegex = new Regex(@"#define\s+([^(]+)\s+((0x|)[0-9a-fA-F]+)$");
                Regex typedefStructOneLineRegex = new Regex(@"typedef struct.*;");
                Regex typedefStructMultiLineRegex = new Regex(@"typedef struct[^;]+");
                Regex typedefInterfaceOneLineRegex = new Regex("typedef interface DECLSPEC_UUID[(]\"([^\"]+)\"");
                Regex declareInterfaceRegex = new Regex(@"DECLARE_INTERFACE_[(](\w+)");
                Dictionary<string, string> uuids = new Dictionary<string, string>();
                Dictionary<string, string> references = new Dictionary<string, string>();
                StringBuilder outputString = new StringBuilder();
                Dictionary<string, string> constants = new Dictionary<string, string>();

                using (StreamWriter output = new StreamWriter("output.idl"))
                using (StreamReader reader = new StreamReader(HeaderFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        Match match;

                        // Check if it is enum definition
                        match = defineDeclarationRegex.Match(line);
                        if (match != null && match.Success && match.Groups[1].ToString().ToUpper() != "INTERFACE" && match.Groups[2].Length <= 10)
                        {
                            constants.Add(match.Groups[1].Value, match.Groups[2].Value);
                            continue;
                        }

                        // Check if it is typedef of one-line struct
                        match = typedefStructOneLineRegex.Match(line);
                        if (match != null && match.Success)
                        {
                            outputString.AppendLine(string.Format("    {0}", line));
                            continue;
                        }

                        // Check if it is typedef of multi-line struct
                        match = typedefStructMultiLineRegex.Match(line);
                        if (match != null && match.Success)
                        {
                            int brackets = 0;

                            outputString.AppendLine();
                            while (!reader.EndOfStream)
                            {
                                string newLine = StripWin32Defs(RemoveSpaces(line));

                                brackets -= line.Count(c => c == '}');
                                outputString.AppendLine(string.Format("    {0}{1}", new string(' ', brackets * 4), newLine));
                                brackets += line.Count(c => c == '{');
                                if (brackets == 0 && newLine.EndsWith(";"))
                                    break;
                                line = reader.ReadLine();
                            }
                            outputString.AppendLine();

                            continue;
                        }

                        // Check if it is typedef of interface UUID
                        match = typedefInterfaceOneLineRegex.Match(line);
                        if (match != null && match.Success)
                        {
                            string uuid = match.Groups[1].Value;
                            string definition = "";

                            while (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                definition += line;
                                if (definition.Contains(';'))
                                    break;
                            }

                            // Check if definition contains interface pointer type
                            match = Regex.Match(definition, @"(\w+)\s*\*\s+(\w+);");
                            if (match == null || !match.Success)
                            {
                                continue;
                            }

                            // Save definition
                            string interfaceName = match.Groups[1].Value;
                            string interfaceReference = match.Groups[2].Value;

                            uuids.Add(interfaceName, uuid);
                            references.Add(interfaceReference, interfaceName);
                            continue;
                        }

                        // Check if it is interface declaration
                        match = declareInterfaceRegex.Match(line);
                        if (match != null && match.Success)
                        {
                            string interfaceName = match.Groups[1].Value;
                            string uuid = uuids[interfaceName];
                            StringBuilder definitionBuilder = new StringBuilder();
                            List<Tuple<string, string, string>> interfaceMethods = new List<Tuple<string, string, string>>();

                            while (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                definitionBuilder.AppendLine(line);
                                if (line.StartsWith("};"))
                                    break;
                            }

                            string definition = definitionBuilder.ToString();

                            // Remove comments
                            definition = Regex.Replace(definition, "/[*].*[*]/", "");
                            definition = Regex.Replace(definition, "//[^\n]*", "");

                            // Extract methods
                            string[] methods = definition.Split(";".ToCharArray());

                            foreach (string method in methods)
                            {
                                match = Regex.Match(method, @"[(]([^)]+)[)].*[(](([^)]*[)]?)*)[)] PURE");
                                if (match != null && match.Success)
                                {
                                    string[] methodReturnValueAndName = match.Groups[1].Value.Split(",".ToCharArray());
                                    string methodName = methodReturnValueAndName.Length > 1 ? methodReturnValueAndName[1].Trim() : methodReturnValueAndName[0].Trim();
                                    string returnValue = methodReturnValueAndName.Length > 1 ? methodReturnValueAndName[0].Trim() : "HRESULT";
                                    string parametersString = match.Groups[2].Value;

                                    if (methodName != "QueryInterface" && methodName != "AddRef" && methodName != "Release")
                                    {
                                        // Clean parameters
                                        StringBuilder parameters = new StringBuilder();

                                        parametersString = RemoveSpaces(parametersString.Replace("THIS_", "").Replace("THIS", ""));
                                        if (parametersString.Length > 0)
                                        {
                                            // Check if there are SAL notation parenthesis that have comma inside and replace with semicolon
                                            var matches = Regex.Matches(parametersString, "[(][^)]*[)]");

                                            foreach (Match m in matches)
                                            {
                                                parametersString = parametersString.Replace(m.Value, m.Value.Replace(",", ":"));
                                            }

                                            // Fix parameters
                                            bool optionalStarted = false;
                                            string[] parametersArray = parametersString.Split(",".ToCharArray());
                                            int outParameters = 0;
                                            bool forbidOptional = false;

                                            if (parametersString.Contains("..."))
                                            {
                                                returnValue = "[vararg] " + returnValue;
                                                forbidOptional = true;
                                            }

                                            for (int i = 0; i < parametersArray.Length; i++)
                                            {
                                                string parameter = parametersArray[i];

                                                if (parameters.Length != 0)
                                                    parameters.Append(", ");

                                                bool outAttribute = Regex.IsMatch(parameter, @"OUT\s|_Out_[a-zA-Z_]*([(][^)]*[)])?\s|__out\w*\s|__inout|_Inout\w*\s");
                                                bool optionalAttribute = (Regex.IsMatch(parameter, @"_(In|Out)[a-zA-Z_]*opt[a-zA-Z_]*([(][^)]*[)])?\s|OPTIONAL") || optionalStarted) && !forbidOptional;
                                                Match sizeAttribute = Regex.Match(parameter, @"_(In|Out)_(reads_|writes_)(bytes_|)(opt_|)[(]([^)]*)[)]");
                                                Match maxAttribute = Regex.Match(parameter, @"_Out_writes_to_(opt_|)[(]([^):]*):([^)]*)[)]");
                                                bool convertToArray = false;

                                                if (Regex.IsMatch(parameter, @"[(][^)]*[)]") && (sizeAttribute == null || sizeAttribute.Success == false) && (maxAttribute == null || maxAttribute.Success == false))
                                                {
                                                    throw new Exception("Not all SAL attributes are parsed");
                                                }

                                                if (outAttribute)
                                                    outParameters++;
                                                optionalStarted = optionalAttribute;
                                                parameters.Append('[');
                                                parameters.Append(outAttribute ? "out" : "in");
                                                if (optionalAttribute)
                                                    parameters.Append(",optional");
                                                if (sizeAttribute != null && sizeAttribute.Success)
                                                {
                                                    parameters.Append(",size_is(");
                                                    parameters.Append(sizeAttribute.Groups[5].Value);
                                                    parameters.Append(")");
                                                    convertToArray = true;
                                                }
                                                else if (maxAttribute != null && maxAttribute.Success)
                                                {
                                                    parameters.Append(",size_is(");
                                                    parameters.Append(maxAttribute.Groups[2].Value);
                                                    parameters.Append(")");
                                                    convertToArray = true;
                                                }

                                                if (outParameters == 1 && outAttribute && i == parametersArray.Length - 1 && !optionalAttribute)
                                                    parameters.Append(",retval");
                                                parameters.Append("] ");
                                                foreach (var reference in references)
                                                {
                                                    parameter = parameter.Replace(reference.Key + " ", reference.Value + "* ");
                                                    parameter = parameter.Replace(reference.Key + "*", reference.Value + "**");
                                                }

                                                parameter = RemoveSpaces(StripWin32Defs(parameter));
                                                if (convertToArray)
                                                {
                                                    int pointerIndex = parameter.LastIndexOf('*');

                                                    if (pointerIndex >= 0)
                                                    {
                                                        parameter = RemoveSpaces(parameter.Remove(pointerIndex, 1) + "[]");
                                                    }
                                                    else if (!parameter.StartsWith("LPStr") && !parameter.StartsWith("LPWStr"))
                                                    {
                                                        parameter += "[]";
                                                    }
                                                }

                                                parameters.Append(parameter);
                                            }
                                        }

                                        interfaceMethods.Add(Tuple.Create(returnValue, methodName, parameters.ToString()));
                                    }
                                }
                                else if (method.Trim() != "" && method.Trim() != "}")
                                {
                                    throw new Exception("Wrong method parsing");
                                }
                            }

                            // Print interface definition
                            outputString.AppendLine(string.Format(@"    ///////////////////////////////////////////////////////////
    [
        object,
        uuid({0}),
        helpstring(""{1}"")
    ]
    interface {1} : IUnknown
    {{", uuid, interfaceName));
                            foreach (var method in interfaceMethods)
                            {
                                outputString.AppendLine(string.Format("        {0} {1}({2});", method.Item1, method.Item2, method.Item3));
                            }

                            outputString.AppendLine("    };");
                            outputString.AppendLine();
                            continue;
                        }

                        // TODO: Unknown line
                    }

                    // Write constants
                    var remainingConstants = constants.ToArray();
                    var usedConstants = new HashSet<string>();
                    WriteConstants(outputString, usedConstants, ref remainingConstants, "DEBUG_REQUEST_", "DebugRequest", (s) => s.StartsWith("DEBUG_LIVE_USER_NON_INVASIVE"));
                    WriteConstants(outputString, usedConstants, ref remainingConstants, "DEBUG_SCOPE_GROUP_");
                    WriteConstants(outputString, usedConstants, ref remainingConstants, "DEBUG_OUTPUT_", "DebugOutput", null, (s) => !s.StartsWith("DEBUG_OUTPUT_SYMBOLS_") && !s.StartsWith("DEBUG_OUTPUT_IDENTITY_DEFAULT"));
                    WriteConstants(outputString, usedConstants, ref remainingConstants, "DEBUG_MODNAME_");
                    WriteConstants(outputString, usedConstants, ref remainingConstants, "DEBUG_OUTCTL_");
                    WriteConstants(outputString, usedConstants, ref remainingConstants, "DEBUG_EXECUTE_");
                    WriteConstants(outputString, usedConstants, ref remainingConstants, "", "Defines");

                    // Write file header
                    FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

                    output.WriteLine(@"import ""oaidl.idl"";
import ""ocidl.idl"";

#if (__midl >= 501)
midl_pragma warning( disable: 2362 )
#endif

[
    uuid({0}),
    helpstring(""DbgEng Type Library""),
    version({1}.{2}),
]
library DbgEng
{{
    importlib(""stdole32.tlb"");
    importlib(""stdole2.tlb"");

    ///////////////////////////////////////////////////////////
    // interface forward declaration", Guid.NewGuid(), fileVersion.ProductMajorPart, fileVersion.ProductMinorPart);
                    foreach (var uuid in uuids)
                    {
                        output.WriteLine("    interface {0};", uuid.Key);
                    }

                    output.WriteLine(@"
    ///////////////////////////////////////////////////////////
    // missing structs
    enum {EXCEPTION_MAXIMUM_PARAMETERS = 15}; // maximum number of exception parameters

    typedef struct _EXCEPTION_RECORD64 {
        DWORD    ExceptionCode;
        DWORD ExceptionFlags;
        LONGLONG ExceptionRecord;
        LONGLONG ExceptionAddress;
        DWORD NumberParameters;
        DWORD __unusedAlignment;
        LONGLONG ExceptionInformation[EXCEPTION_MAXIMUM_PARAMETERS];
    } EXCEPTION_RECORD64, *PEXCEPTION_RECORD64;

    typedef struct _IMAGE_FILE_HEADER {
        WORD    Machine;
        WORD    NumberOfSections;
        DWORD   TimeDateStamp;
        DWORD   PointerToSymbolTable;
        DWORD   NumberOfSymbols;
        WORD    SizeOfOptionalHeader;
        WORD    Characteristics;
    } IMAGE_FILE_HEADER, *PIMAGE_FILE_HEADER;

    typedef struct _IMAGE_DATA_DIRECTORY {
        DWORD   VirtualAddress;
        DWORD   Size;
    } IMAGE_DATA_DIRECTORY, *PIMAGE_DATA_DIRECTORY;

    enum { IMAGE_NUMBEROF_DIRECTORY_ENTRIES  = 16};

    typedef struct _IMAGE_OPTIONAL_HEADER64 {
        WORD        Magic;
        BYTE        MajorLinkerVersion;
        BYTE        MinorLinkerVersion;
        DWORD       SizeOfCode;
        DWORD       SizeOfInitializedData;
        DWORD       SizeOfUninitializedData;
        DWORD       AddressOfEntryPoint;
        DWORD       BaseOfCode;
        ULONGLONG   ImageBase;
        DWORD       SectionAlignment;
        DWORD       FileAlignment;
        WORD        MajorOperatingSystemVersion;
        WORD        MinorOperatingSystemVersion;
        WORD        MajorImageVersion;
        WORD        MinorImageVersion;
        WORD        MajorSubsystemVersion;
        WORD        MinorSubsystemVersion;
        DWORD       Win32VersionValue;
        DWORD       SizeOfImage;
        DWORD       SizeOfHeaders;
        DWORD       CheckSum;
        WORD        Subsystem;
        WORD        DllCharacteristics;
        ULONGLONG   SizeOfStackReserve;
        ULONGLONG   SizeOfStackCommit;
        ULONGLONG   SizeOfHeapReserve;
        ULONGLONG   SizeOfHeapCommit;
        DWORD       LoaderFlags;
        DWORD       NumberOfRvaAndSizes;
        IMAGE_DATA_DIRECTORY DataDirectory[IMAGE_NUMBEROF_DIRECTORY_ENTRIES];
    } IMAGE_OPTIONAL_HEADER64, *PIMAGE_OPTIONAL_HEADER64;

    typedef struct _IMAGE_NT_HEADERS64 {
        DWORD Signature;
        IMAGE_FILE_HEADER FileHeader;
        IMAGE_OPTIONAL_HEADER64 OptionalHeader;
    } IMAGE_NT_HEADERS64, *PIMAGE_NT_HEADERS64;

    struct _WINDBG_EXTENSION_APIS32 {
        DWORD NotSupported;
    };

    struct _WINDBG_EXTENSION_APIS64 {
        DWORD NotSupported;
    };

    struct _MEMORY_BASIC_INFORMATION64 {
        ULONGLONG BaseAddress;
        ULONGLONG AllocationBase;
        DWORD     AllocationProtect;
        DWORD     __alignment1;
        ULONGLONG RegionSize;
        DWORD     State;
        DWORD     Protect;
        DWORD     Type;
        DWORD     __alignment2;
    };

");

                    output.WriteLine();
                    output.WriteLine("    ///////////////////////////////////////////////////////////");
                    output.WriteLine(outputString);
                    output.WriteLine("};");
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(MidlPath);
                string midlPlatform = IntPtr.Size == 4 ? "/win32 /robust" : "/x64";

                startInfo.Arguments = string.Format(@"/I""{0}"" /I""{1}"" output.idl /tlb output.tlb {2}", UmPath, SharedPath, midlPlatform);
                startInfo.UseShellExecute = false;
                startInfo.EnvironmentVariables["Path"] = VCBinPath + ";" + startInfo.EnvironmentVariables["Path"];
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }

#if X64
            string tlbimpPlatform = "x64";
#elif X86
            string tlbimpPlatform = "x86";
#else
                string tlbimpPlatform = "Agnostic";
#endif

                startInfo = new ProcessStartInfo(TlbImpPath);
                startInfo.Arguments = @"output.tlb /machine:" + tlbimpPlatform;
                startInfo.UseShellExecute = false;
                startInfo.EnvironmentVariables["Path"] = VCBinPath + ";" + startInfo.EnvironmentVariables["Path"];
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }

                // Create Dia2Lib.tlb
                startInfo = new ProcessStartInfo(MidlPath);
                startInfo.Arguments = $"/I \"{DiaIncludePath}\" /I \"{DiaIdlPath}\" /I \"{UmPath}\" /I \"{SharedPath}\" dia2.idl /tlb dia2lib.tlb";
                startInfo.UseShellExecute = false;
                startInfo.EnvironmentVariables["Path"] = VCBinPath + ";" + startInfo.EnvironmentVariables["Path"];
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }

                // Create Dia2Lib.dll
                startInfo = new ProcessStartInfo(TlbImpPath);
                startInfo.Arguments = "dia2lib.tlb";
                startInfo.UseShellExecute = false;
                startInfo.EnvironmentVariables["Path"] = VCBinPath + ";" + startInfo.EnvironmentVariables["Path"];
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }

                // Copy files to output folder
                string[] filesToCopy = new[] { "Dia2Lib.dll", "DbgEng.dll" };

                foreach (string fileToCopy in filesToCopy)
                {
                    string inputFile = Path.Combine(tempFolder, fileToCopy);
                    string outputFile = Path.Combine(outputFolder, fileToCopy);

                    if (File.Exists(outputFile))
                    {
                        File.Delete(outputFile);
                    }
                    File.Move(inputFile, outputFile);
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(outputFolder);
                Directory.Delete(tempFolder, true);
            }
            return 0;
        }

        private static void WriteConstants(StringBuilder outputString, HashSet<string> usedConstants, ref KeyValuePair<string,string>[] remainingConstants, string prefix, string constantsName = null, Func<string, bool> additionalAcceptanceConstantFilter = null, Func<string, bool> additionalRejectConstantFilter = null)
        {
            if (string.IsNullOrEmpty(constantsName))
                constantsName = FormatEnumName(prefix, "");

            var constants = remainingConstants.Where(k => (k.Key.StartsWith(prefix) || (additionalAcceptanceConstantFilter != null && additionalAcceptanceConstantFilter(k.Key))) && (additionalRejectConstantFilter == null || additionalRejectConstantFilter(k.Key))).ToArray();
            remainingConstants = remainingConstants.Except(constants).ToArray();

            outputString.AppendLine("    enum " + constantsName);
            outputString.AppendLine("    {");
            foreach (var c in constants)
            {
                string cname = FormatEnumName(c.Key, prefix);

                while (usedConstants.Contains(cname))
                    cname += "_";
                usedConstants.Add(cname);

                outputString.AppendFormat("        {0} = {1},", cname, c.Value);
                outputString.AppendLine();
            }

            outputString.AppendLine("    };");
        }

        private static string FormatEnumName(string key, string prefix)
        {
            if (key.StartsWith(prefix))
                key = key.Substring(prefix.Length);
            key = key.Trim().ToLower();

            StringBuilder sb = new StringBuilder();
            bool makeUpper = true;

            foreach (char c in key)
            {
                if (c == '_')
                    makeUpper = true;
                else if (makeUpper)
                {
                    sb.Append(char.ToUpper(c));
                    makeUpper = false;
                }
                else
                {
                    sb.Append(c);
                    makeUpper = false;
                }
            }

            return sb.ToString();
        }
    }
}
