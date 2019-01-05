using CsDebugScript.CodeGen;
using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers.DbgEngDllHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace CsDebugScript
{
    /// <summary>
    /// Compiles and executes scripts
    /// </summary>
    public static class ScriptExecution
    {
        /// <summary>
        /// Deafult transformations that are being applied when using CodeGen.
        /// </summary>
        public static readonly XmlTypeTransformation[] DefaultTransformations = FixTransformations(new[]
        {
            new XmlTypeTransformation()
            {
                OriginalType = "std::basic_string<char,${char_traits},${allocator}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.@string",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::basic_string<wchar_t,${char_traits},${allocator}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.wstring",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::basic_string<unsigned short,${char_traits},${allocator}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.wstring",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::basic_string<char>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.@string",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::basic_string<wchar_t>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.wstring",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::basic_string<unsigned short>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.wstring",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::vector<${T},${allocator}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.vector<${T}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::list<${T},${allocator}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.list<${T}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::map<${TKey},${TValue},${comparator},${allocator}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.map<${TKey},${TValue}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::unordered_map<${TKey},${TValue},${hasher},${keyEquality},${allocator}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.unordered_map<${TKey},${TValue}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::pair<${TFirst},${TSecond}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.pair<${TFirst},${TSecond}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::any",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.any",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::array<${T},${Length}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.array<${T}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::optional<${T}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.optional<${T}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::shared_ptr<${T}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.shared_ptr<${T}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::weak_ptr<${T}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.weak_ptr<${T}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1},${T2}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1},${T2}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1},${T2},${T3}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1},${T2},${T3}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1},${T2},${T3},${T4}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1},${T2},${T3},${T4}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1},${T2},${T3},${T4},${T5}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1},${T2},${T3},${T4},${T5}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1},${T2},${T3},${T4},${T5},${T6}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1},${T2},${T3},${T4},${T5},${T6}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1},${T2},${T3},${T4},${T5},${T6},${T7}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1},${T2},${T3},${T4},${T5},${T6},${T7}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::variant<${T1},${T2},${T3},${T4},${T5},${T6},${T7},${T8}>",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.variant<${T1},${T2},${T3},${T4},${T5},${T6},${T7},${T8}>",
            },
            new XmlTypeTransformation()
            {
                OriginalType = "std::filesystem::path",
                NewType = "CsDebugScript.CommonUserTypes.NativeTypes.std.filesystem.path",
            },
        });

        private static XmlTypeTransformation[] FixTransformations(XmlTypeTransformation[] originalTransformations)
        {
            List<XmlTypeTransformation> transformations = new List<XmlTypeTransformation>();

            foreach (XmlTypeTransformation transformation in originalTransformations)
            {
                transformations.Add(transformation);
                if (transformation.OriginalType.StartsWith("std::filesystem::"))
                {
                    // Adding CLANG namespace duplicates
                    transformations.Add(new XmlTypeTransformation()
                    {
                        OriginalType = "std::__1::__fs::filesystem::" + transformation.OriginalType.Substring(17),
                        NewType = transformation.NewType,
                    });
                }
                else if (transformation.OriginalType.StartsWith("std::"))
                {
                    // Adding GCC namespace duplicate
                    transformations.Add(new XmlTypeTransformation()
                    {
                        OriginalType = "std::__cxx11::" + transformation.OriginalType.Substring(5),
                        NewType = transformation.NewType,
                    });

                    // Adding CLANG namespace duplicates
                    transformations.Add(new XmlTypeTransformation()
                    {
                        OriginalType = "std::__1::" + transformation.OriginalType.Substring(5),
                        NewType = transformation.NewType,
                    });
                }
            }

            return transformations.ToArray();
        }

        /// <summary>
        /// Resolves the path for the specified base file path.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="baseFilePath">The base file path.</param>
        /// <returns>Resolved path if file exists or string.Empty.</returns>
        private static string ResolvePath(string reference, string baseFilePath)
        {
            if (Path.IsPathRooted(reference))
            {
                return File.Exists(reference) ? reference : string.Empty;
            }

            // Try to find in path relative to the base file path
            string path = string.Empty;

            if (!string.IsNullOrEmpty(baseFilePath))
            {
                string folder = baseFilePath;

                if (!Path.IsPathRooted(baseFilePath))
                {
                    folder = Path.GetFullPath(baseFilePath);
                }

                folder = Path.GetDirectoryName(folder);
                path = Path.Combine(folder, reference);
                path = Path.GetFullPath(path);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Try to find in path relative to the current working directory
            path = Path.GetFullPath(reference);
            if (File.Exists(path))
            {
                return path;
            }

            // Try to find path relative to current assembly location
            try
            {
                string folder = typeof(ScriptExecution).Assembly.Location;

                if (!Path.IsPathRooted(baseFilePath))
                {
                    folder = Path.GetFullPath(baseFilePath);
                }

                folder = Path.GetDirectoryName(folder);
                path = Path.Combine(folder, reference);
                path = Path.GetFullPath(path);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            catch
            {
            }

            // Look into search folders
            foreach (string folder in ScriptCompiler.SearchFolders)
            {
                path = Path.Combine(folder, reference);
                path = Path.GetFullPath(path);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts the importing options to CodeGen configuration.
        /// </summary>
        /// <param name="options">The importing options.</param>
        /// <returns>CodeGen configuration</returns>
        private static XmlConfig ConvertOptionsToCodeGenConfig(ImportUserTypeOptions options)
        {
            // User types
            List<XmlType> types = new List<XmlType>();

            foreach (string userTypeName in options.UserTypes)
            {
                types.Add(new XmlType()
                {
                    Name = userTypeName,
                    ExportDependentTypes = options.ImportDependentTypes,
                });
            }

            // Modules
            List<XmlModule> modules = new List<XmlModule>();

            foreach (string moduleName in options.Modules)
            {
                Module module = Module.All.First(m => m.Name == moduleName);
                string symbolsPath = Context.SymbolProvider.GetModuleSymbolsPath(module);

                if (!File.Exists(symbolsPath))
                    continue;
                modules.Add(new XmlModule()
                {
                    Name = moduleName,
                    SymbolsPath = symbolsPath,
                });
            }

            // Check if we are using direct class access.
            bool useDirectClassAccess = !(Context.SymbolProvider is DbgEngSymbolProvider);

            // Create configuration
            return new XmlConfig()
            {
                Types = types.ToArray(),
                Modules = modules.ToArray(),
                UseDirectClassAccess = useDirectClassAccess,
                CompressedOutput = true,
                ForceUserTypesToNewInsteadOfCasting = true,
                GeneratePhysicalMappingOfUserTypes = useDirectClassAccess,
                MultiFileExport = false,
                Transformations = DefaultTransformations,
                GenerateAssemblyWithILWriter = options.UseILCodeWriter,
            };
        }

        /// <summary>
        /// Helper class to do metadata reference resolver for files.
        /// </summary>
        /// <seealso cref="Microsoft.CodeAnalysis.MetadataReferenceResolver" />
        internal class MetadataResolver : MetadataReferenceResolver
        {
            /// <summary>
            /// The previous metadata resolver
            /// </summary>
            private MetadataReferenceResolver previousResolver;

            /// <summary>
            /// The generated CodeGen assemblies
            /// </summary>
            private Dictionary<string, ImportUserTypeAssembly> codeGenAssemblies = new Dictionary<string, ImportUserTypeAssembly>();

            /// <summary>
            /// Initializes a new instance of the <see cref="MetadataResolver"/> class.
            /// </summary>
            /// <param name="previousResolver">The previous resolver.</param>
            public MetadataResolver(MetadataReferenceResolver previousResolver)
            {
                this.previousResolver = previousResolver;
            }

            /// <summary>
            /// True to instruct the compiler to invoke <see cref="M:Microsoft.CodeAnalysis.MetadataReferenceResolver.ResolveMissingAssembly(Microsoft.CodeAnalysis.MetadataReference,Microsoft.CodeAnalysis.AssemblyIdentity)" /> for each assembly reference that
            /// doesn't match any of the assemblies explicitly referenced by the <see cref="T:Microsoft.CodeAnalysis.Compilation" /> (via <see cref="P:Microsoft.CodeAnalysis.Compilation.ExternalReferences" />, or #r directives.
            /// </summary>
            public override bool ResolveMissingAssemblies
            {
                get
                {
                    return previousResolver.ResolveMissingAssemblies;
                }
            }

            /// <summary>
            /// Resolves a missing assembly reference.
            /// </summary>
            /// <param name="definition">The metadata definition (assembly or module) that declares assembly reference <paramref name="referenceIdentity" /> in its list of dependencies.</param>
            /// <param name="referenceIdentity">Identity of the assembly reference that couldn't be resolved against metadata references explicitly specified to in the compilation.</param>
            /// <returns>
            /// Resolved reference or null if the identity can't be resolved.
            /// </returns>
            public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
            {
                return previousResolver.ResolveMissingAssembly(definition, referenceIdentity);
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="other">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object other)
            {
                return other is MetadataResolver;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public override int GetHashCode()
            {
                return 0;
            }

            /// <summary>
            /// Resolves the reference.
            /// </summary>
            /// <param name="reference">The reference.</param>
            /// <param name="baseFilePath">The base file path.</param>
            /// <param name="properties">The properties.</param>
            /// <returns></returns>
            public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
            {
                // Check if we are referencing CodeGen assembly
                ImportUserTypeAssembly codeGenAssembly;

                if (codeGenAssemblies.TryGetValue(reference, out codeGenAssembly))
                {
                    using (MemoryStream stream = new MemoryStream(codeGenAssembly.AssemblyBytes))
                    {
                        return ImmutableArray.Create(MetadataReference.CreateFromStream(stream, properties, null, codeGenAssembly.AssemblyPath));
                    }
                }

                // Check the previous resolver
                var result = previousResolver.ResolveReference(reference, baseFilePath, properties);

                if (result.Length > 0)
                {
                    return result;
                }

                // Try to use file resolver
                try
                {
                    string path = ResolvePath(reference, baseFilePath);

                    if (!string.IsNullOrEmpty(path))
                    {
                        return ImmutableArray.Create(MetadataReference.CreateFromFile(path, properties));
                    }
                }
                catch
                {
                }

                // Check if reference holds xml for CodeGen
                ImportUserTypeOptions options = ImportUserTypeOptions.ParseString(reference);

                if (options != null)
                {
                    foreach (ImportUserTypeAssembly assembly in codeGenAssemblies.Values)
                    {
                        if (assembly.Options.Equals(options))
                        {
                            // TODO: Compare that used PDBs have same GUID.
                            codeGenAssembly = assembly;
                            break;
                        }
                    }

                    if (codeGenAssembly == null)
                    {
                        codeGenAssembly = GenerateAssembly(options);
                        AddAssembly(codeGenAssembly);
                    }

                    using (MemoryStream stream = new MemoryStream(codeGenAssembly.AssemblyBytes))
                    {
                        return ImmutableArray.Create(MetadataReference.CreateFromStream(stream, properties, null, codeGenAssembly.AssemblyPath));
                    }
                }

                return ImmutableArray<PortableExecutableReference>.Empty;
            }

            /// <summary>
            /// Adds the assembly to the list.
            /// </summary>
            /// <param name="assembly">The assembly.</param>
            internal void AddAssembly(ImportUserTypeAssembly assembly)
            {
                codeGenAssemblies.Add(assembly.AssemblyPath, assembly);
            }

            /// <summary>
            /// Generates the assembly from the specified importing options.
            /// </summary>
            /// <param name="options">The importing options.</param>
            internal ImportUserTypeAssembly GenerateAssembly(ImportUserTypeOptions options)
            {
                // Generate CodeGen configuration
                string assemblyPath = Path.Combine(Path.GetTempPath(), "CsDebugScript.CodeGen.Assemblies", Guid.NewGuid().ToString() + ".dll");
                XmlConfig codeGenConfig = ConvertOptionsToCodeGenConfig(options);

                Directory.CreateDirectory(Path.GetDirectoryName(assemblyPath));
                codeGenConfig.GeneratedAssemblyName = assemblyPath;

                // Execute code generation
                using (var engineModuleProvider = new EngineSymbolProviderModuleProviderWithPdbReader(Process.Current, options.UsePdbReaderWhenPossible))
                {
                    Generator generator = new Generator(engineModuleProvider);
                    generator.GenerateAssembly(codeGenConfig, assemblyPath);

                    byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);

                    // Add generated file to be loaded after execution
                    return new ImportUserTypeAssembly()
                    {
                        AssemblyBytes = assemblyBytes,
                        AssemblyPath = assemblyPath,
                        Options = options,
                    };
                }
            }
        }

        /// <summary>
        /// Helper class that provides ability to return PDB reader when possible or just falls back to engine module provider otherwise.
        /// </summary>
        private class EngineSymbolProviderModuleProviderWithPdbReader : EngineSymbolProviderModuleProvider, IDisposable
        {
            /// <summary>
            /// List of PDB module that we need to dispose when needed.
            /// </summary>
            private List<PdbSymbolProvider.PdbModule> pdbModules = new List<PdbSymbolProvider.PdbModule>();

            /// <summary>
            /// Initializes a new instance of the <see cref="EngineSymbolProviderModuleProviderWithPdbReader"/> class.
            /// </summary>
            /// <param name="process"></param>
            /// <param name="usePdbReaderWhenPossible"></param>
            public EngineSymbolProviderModuleProviderWithPdbReader(Process process, bool usePdbReaderWhenPossible)
                : base(process)
            {
                UsePdbReaderWhenPossible = usePdbReaderWhenPossible;
            }

            /// <summary>
            /// Gets or sets a value indicating whether PDB reader should be used when possible.
            /// </summary>
            public bool UsePdbReaderWhenPossible { get; private set; }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                foreach (PdbSymbolProvider.PdbModule pdbModule in pdbModules)
                    pdbModule.Dispose();
            }

            /// <summary>
            /// Opens the module for the specified XML module description.
            /// </summary>
            /// <param name="xmlModule">The XML module description.</param>
            public override CodeGen.SymbolProviders.Module Open(XmlModule xmlModule)
            {
                try
                {
                    if (UsePdbReaderWhenPossible && xmlModule?.SymbolsPath != null && Path.GetExtension(xmlModule.SymbolsPath).ToLower() == ".pdb")
                    {
                        PdbSymbolProvider.PdbModule pdbModule = new PdbSymbolProvider.PdbModule(xmlModule);

                        lock (pdbModules)
                        {
                            pdbModules.Add(pdbModule);
                        }
                        return pdbModule;
                    }
                }
                catch
                {
                }
                return base.Open(xmlModule);
            }
        }

        /// <summary>
        /// Helper class to do source reference resolver for files.
        /// </summary>
        /// <seealso cref="Microsoft.CodeAnalysis.MetadataReferenceResolver" />
        internal class SourceResolver : SourceReferenceResolver
        {
            /// <summary>
            /// The original source resolver
            /// </summary>
            private SourceReferenceResolver originalSourceResolver;

            /// <summary>
            /// The generated CodeGen code
            /// </summary>
            private Dictionary<string, ImportUserTypeCode> codeGenCode = new Dictionary<string, ImportUserTypeCode>();

            /// <summary>
            /// Initializes a new instance of the <see cref="SourceResolver"/> class.
            /// </summary>
            /// <param name="originalSourceResolver">The original source resolver.</param>
            public SourceResolver(SourceReferenceResolver originalSourceResolver)
            {
                this.originalSourceResolver = originalSourceResolver;
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="other">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object other)
            {
                return other is SourceResolver;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
            /// </returns>
            public override int GetHashCode()
            {
                return 1;
            }

            /// <summary>
            /// Normalizes specified source path with respect to base file path.
            /// </summary>
            /// <param name="path">The source path to normalize. May be absolute or relative.</param>
            /// <param name="baseFilePath">Path of the source file that contains the <paramref name="path" /> (may also be relative), or null if not available.</param>
            /// <returns>
            /// Normalized path, or null if <paramref name="path" /> can't be normalized. The resulting path doesn't need to exist.
            /// </returns>
            public override string NormalizePath(string path, string baseFilePath)
            {
                // Try to see if it is import user type options
                ImportUserTypeOptions options = ImportUserTypeOptions.ParseString(path);

                if (options != null)
                {
                    return options.Serialize();
                }

                // Normalize path
                string result = originalSourceResolver.NormalizePath(path, baseFilePath);

                return result;
            }

            /// <summary>
            /// Resolves specified path with respect to base file path.
            /// </summary>
            /// <param name="path">The path to resolve. May be absolute or relative.</param>
            /// <param name="baseFilePath">Path of the source file that contains the <paramref name="path" /> (may also be relative), or null if not available.</param>
            /// <returns>
            /// Normalized path, or null if the file can't be resolved.
            /// </returns>
            public override string ResolveReference(string path, string baseFilePath)
            {
                // Try to see if it is import user type options
                ImportUserTypeOptions options = ImportUserTypeOptions.ParseString(path);

                if (options != null)
                {
                    return options.Serialize();
                }

                // Do resolve reference
                string result = originalSourceResolver.ResolveReference(path, baseFilePath);

                if (string.IsNullOrEmpty(result))
                {
                    result = ResolvePath(path, baseFilePath);
                }

                return result;
            }

            /// <summary>
            /// Opens a <see cref="T:System.IO.Stream" /> that allows reading the content of the specified file.
            /// </summary>
            /// <param name="resolvedPath">Path returned by <see cref="M:Microsoft.CodeAnalysis.SourceReferenceResolver.ResolveReference(System.String,System.String)" />.</param>
            /// <returns></returns>
            public override Stream OpenRead(string resolvedPath)
            {
                ImportUserTypeCode code;

                if (codeGenCode.TryGetValue(resolvedPath, out code))
                {
                    return new MemoryStream(Encoding.UTF8.GetBytes(code.Code));
                }

                ImportUserTypeOptions options = ImportUserTypeOptions.ParseString(resolvedPath);

                if (options != null)
                {
                    code = GenerateCode(options);
                    AddCode(code);
                    return new MemoryStream(Encoding.UTF8.GetBytes(code.Code));
                }

                return originalSourceResolver.OpenRead(resolvedPath);
            }

            /// <summary>
            /// Adds the code to the list.
            /// </summary>
            /// <param name="code">The code.</param>
            internal void AddCode(ImportUserTypeCode code)
            {
                codeGenCode.Add(code.Options.Serialize(), code);
            }

            /// <summary>
            /// Generates the code from the specified importing options.
            /// </summary>
            /// <param name="options">The importing options.</param>
            internal ImportUserTypeCode GenerateCode(ImportUserTypeOptions options)
            {
                XmlConfig codeGenConfig = ConvertOptionsToCodeGenConfig(options);

                codeGenConfig.GenerateNamespaceAsStaticClass = true;

                // Execute code generation
                IModuleProvider moduleProvider = new EngineSymbolProviderModuleProvider(Process.Current);
                Generator generator = new Generator(moduleProvider);
                string code = generator.GenerateScriptCode(codeGenConfig);

                // Add generated code to be loaded after execution
                return new ImportUserTypeCode()
                {
                    Code = code,
                    Options = options,
                };
            }
        }

        /// <summary>
        /// Executes the specified script.
        /// </summary>
        /// <param name="path">The script path.</param>
        /// <param name="arguments">Script arguments.</param>
        public static void Execute(string path, params string[] arguments)
        {
            try
            {
                var scriptBase = new ScriptBase();
                var scriptOptions = ScriptOptions.Default.WithImports(ScriptCompiler.DefaultUsings).AddReferences(ScriptCompiler.DefaultAssemblyReferences);
                var originalSourceResolver = scriptOptions.SourceResolver;
                var originalMetadataResolver = scriptOptions.MetadataResolver;

                scriptOptions = scriptOptions.WithMetadataResolver(new MetadataResolver(originalMetadataResolver));
                scriptOptions = scriptOptions.WithSourceResolver(new SourceResolver(originalSourceResolver));

                var argsCode = Convert(arguments);
                var scriptState = CSharpScript.RunAsync(argsCode, scriptOptions, scriptBase).Result;

                // TODO: What about loading and clearing metadata?
                scriptState = scriptState.ContinueWithAsync(string.Format(@"#load ""{0}""", path)).Result;
            }
            catch (CompilationErrorException ex)
            {
                Console.Error.WriteLine("Compile errors:");
                foreach (var error in ex.Diagnostics)
                {
                    Console.Error.WriteLine($"  {error}");
                }
            }
        }

        /// <summary>
        /// Converts the specified arguments to the script code so that they can be loaded there.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static string Convert(params string[] args)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;

            sb.Append("string[] args = new string[] { ");
            foreach (var arg in args)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append("@\"");
                sb.Append(arg.Replace("\"", "\"\""));
                sb.Append("\"");
            }

            if (!first)
            {
                sb.Append(" ");
            }

            sb.Append("};");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Options class that determines how user types are imported from modules.
    /// </summary>
    /// <seealso cref="System.IEquatable{T}" />
    public class ImportUserTypeOptions : IEquatable<ImportUserTypeOptions>
    {
        /// <summary>
        /// Gets or sets the list of user types to be imported.
        /// </summary>
        public List<string> UserTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of modules to be imported.
        /// </summary>
        public List<string> Modules { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether type dependencies should be imported.
        /// </summary>
        public bool ImportDependentTypes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether assembly should be generated by emitting IL.
        /// </summary>
        public bool UseILCodeWriter { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether PDB reader (<see cref="PdbSymbolProvider.PdbModuleProvider"/>) should be used when possible.
        /// </summary>
        public bool UsePdbReaderWhenPossible { get; set; } = false;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return (UserTypes?.GetHashCode() ?? 0) ^ (Modules?.GetHashCode() ?? 0) ^ (ImportDependentTypes ? 13 : 0) ^ (UseILCodeWriter ? 121 : 0) ^ (UsePdbReaderWhenPossible ? 1453 : 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ImportUserTypeOptions);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ImportUserTypeOptions other)
        {
            return UserTypes.SequenceEqual(other.UserTypes)
                && Modules.SequenceEqual(other.Modules)
                && ImportDependentTypes == other.ImportDependentTypes
                && UseILCodeWriter == other.UseILCodeWriter;
        }

        /// <summary>
        /// Parses the specified string as importing options.
        /// </summary>
        /// <param name="jsonInput">The JSON input.</param>
        /// <returns>Importing options if string was parsed; <c>null</c> otherwise</returns>
        internal static ImportUserTypeOptions ParseString(string jsonInput)
        {
            if (jsonInput?.ToLower() == "[AutoUserTypes]".ToLower())
                return new ImportUserTypeOptions()
                {
                    ImportDependentTypes = false,
                    UseILCodeWriter = true,
                    UsePdbReaderWhenPossible = true,
                    UserTypes = new List<string>(),
                    Modules = Process.Current.Modules.Select(m => m.Name).ToList(),
                };

            try
            {
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                ImportUserTypeOptions options = jsonSerializer.Deserialize<ImportUserTypeOptions>(jsonInput);

                return options;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Serializes this instance to JSON.
        /// </summary>
        internal string Serialize()
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

            UserTypes?.Sort();
            Modules?.Sort();
            return jsonSerializer.Serialize(this);
        }
    }

    /// <summary>
    /// Imported user types as script code
    /// </summary>
    internal class ImportUserTypeCode
    {
        /// <summary>
        /// Gets or sets the importing options.
        /// </summary>
        public ImportUserTypeOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the script code.
        /// </summary>
        public string Code { get; set; }
    }

    /// <summary>
    /// Imported user types as compiled assembly
    /// </summary>
    internal class ImportUserTypeAssembly
    {
        /// <summary>
        /// Gets or sets the importing options.
        /// </summary>
        public ImportUserTypeOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the assembly bytes.
        /// </summary>
        public byte[] AssemblyBytes { get; set; }

        /// <summary>
        /// Gets or sets the assembly path.
        /// </summary>
        public string AssemblyPath { get; set; }
    }
}
