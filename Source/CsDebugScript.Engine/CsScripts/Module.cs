using CsScriptManaged;
using CsScriptManaged.SymbolProviders;
using CsScriptManaged.Utility;
using System;
using System.Linq;

namespace CsScripts
{
    /// <summary>
    /// Represents the version of a Module.
    /// </summary>
    public struct ModuleVersion
    {
        /// <summary>
        /// In a version 'A.B.C.D', this field represents 'A'.
        /// </summary>
        public int Major;

        /// <summary>
        /// In a version 'A.B.C.D', this field represents 'B'.
        /// </summary>
        public int Minor;

        /// <summary>
        /// In a version 'A.B.C.D', this field represents 'C'.
        /// </summary>
        public int Revision;

        /// <summary>
        /// In a version 'A.B.C.D', this field represents 'D'.
        /// </summary>
        public int Patch;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Revision, Patch);
        }
    }

    /// <summary>
    /// Module of the debugging process.
    /// </summary>
    public class Module
    {
        /// <summary>
        /// The module name
        /// </summary>
        private SimpleCache<string> name;

        /// <summary>
        /// The image name
        /// </summary>
        private SimpleCache<string> imageName;

        /// <summary>
        /// The loaded image name
        /// </summary>
        private SimpleCache<string> loadedImageName;

        /// <summary>
        /// The symbol file name
        /// </summary>
        private SimpleCache<string> symbolFileName;

        /// <summary>
        /// The mapped image name
        /// </summary>
        private SimpleCache<string> mappedImageName;

        /// <summary>
        /// The module version
        /// </summary>
        private SimpleCache<ModuleVersion> moduleVersion;

        /// <summary>
        /// The CLR module
        /// </summary>
        private SimpleCache<Microsoft.Diagnostics.Runtime.ClrModule> clrModule;

        /// <summary>
        /// The CLR PDB reader
        /// </summary>
        private SimpleCache<Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader> clrPdbReader;

        /// <summary>
        /// The timestamp and size
        /// </summary>
        private SimpleCache<Tuple<DateTime, ulong>> timestampAndSize;

        /// <summary>
        /// The next fake code type identifier
        /// </summary>
        private int nextFakeCodeTypeId = -1;

        /// <summary>
        /// The symbol provider module cache
        /// </summary>
        internal ISymbolProviderModule SymbolProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Module" /> class.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The module address.</param>
        internal Module(Process process, ulong address)
        {
            Address = address;
            Process = process;
            name = SimpleCache.Create(() =>
            {
                string name = Context.Debugger.GetModuleName(this);

                Process.UpdateModuleByNameCache(this, name);
                return name;
            });
            imageName = SimpleCache.Create(() => Context.Debugger.GetModuleImageName(this));
            loadedImageName = SimpleCache.Create(() => Context.Debugger.GetModuleLoadedImage(this));
            symbolFileName = SimpleCache.Create(() => Context.Debugger.GetModuleSymbolFile(this));
            mappedImageName = SimpleCache.Create(() => Context.Debugger.GetModuleMappedImage(this));
            moduleVersion = SimpleCache.Create(() =>
            {
                ModuleVersion version = new ModuleVersion();

                Context.Debugger.GetModuleVersion(this, out version.Major, out version.Minor, out version.Revision, out version.Patch);
                return version;
            });
            timestampAndSize = SimpleCache.Create(() => Context.Debugger.GetModuleTimestampAndSize(this));
            clrModule = SimpleCache.Create(() => Process.ClrRuntimes.SelectMany(r => r.ClrRuntime.Modules).Where(m => m.ImageBase == Address).FirstOrDefault());
            clrPdbReader = SimpleCache.Create(() =>
            {
                try
                {
                    string pdbPath = ClrModule.Runtime.DataTarget.SymbolLocator.FindPdb(ClrModule.Pdb);

                    if (!string.IsNullOrEmpty(pdbPath))
                    {
                        return new Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader(pdbPath);
                    }
                }
                catch (Exception)
                {
                }

                return null;
            });
            TypesByName = new DictionaryCache<string, CodeType>(GetTypeByName);
            TypesById = new DictionaryCache<uint, CodeType>(GetTypeById);
            GlobalVariables = new DictionaryCache<string, Variable>(GetGlobalVariable);
            UserTypeCastedGlobalVariables = new DictionaryCache<string, Variable>((name) =>
            {
                Variable variable = Process.CastVariableToUserType(GlobalVariables[name]);

                if (UserTypeCastedGlobalVariables.Count == 0)
                {
                    GlobalCache.VariablesUserTypeCastedFieldsByName.Add(UserTypeCastedGlobalVariables);
                }

                return variable;
            });
        }

        /// <summary>
        /// Gets the array of all modules for the current process.
        /// </summary>
        public static Module[] All
        {
            get
            {
                return Process.Current.Modules;
            }
        }

        /// <summary>
        /// Types by the name
        /// </summary>
        internal DictionaryCache<string, CodeType> TypesByName { get; private set; }

        /// <summary>
        /// Types by the identifier
        /// </summary>
        internal DictionaryCache<uint, CodeType> TypesById { get; private set; }

        /// <summary>
        /// Cache of global variables.
        /// </summary>
        internal DictionaryCache<string, Variable> GlobalVariables { get; private set; }

        /// <summary>
        /// Cache of user type casted global variables.
        /// </summary>
        internal DictionaryCache<string, Variable> UserTypeCastedGlobalVariables { get; private set; }

        /// <summary>
        /// Gets the address.
        /// </summary>
        public ulong Address { get; private set; }

        /// <summary>
        /// Gets the owning process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the offset (address location of the module base).
        /// </summary>
        public ulong Offset
        {
            get
            {
                return Address;
            }
        }

        /// <summary>
        /// Gets the module name. This is usually just the file name without the extension. In a few cases,
        /// the module name differs significantly from the file name.
        /// </summary>
        public string Name
        {
            get
            {
                return name.Value;
            }

            internal set
            {
                name.Value = value;
            }
        }

        /// <summary>
        /// Gets the name of the image. This is the name of the executable file, including the extension.
        /// Typically, the full path is included in user mode but not in kernel mode.
        /// </summary>
        public string ImageName
        {
            get
            {
                return imageName.Value;
            }
        }

        /// <summary>
        /// Gets the name of the loaded image. Unless Microsoft CodeView symbols are present, this is the same as the image name.
        /// </summary>
        public string LoadedImageName
        {
            get
            {
                return loadedImageName.Value;
            }
        }

        /// <summary>
        /// Gets the name of the symbol file. The path and name of the symbol file. If no symbols have been loaded,
        /// this is the name of the executable file instead.
        /// </summary>
        public string SymbolFileName
        {
            get
            {
                return symbolFileName.Value;
            }
        }

        /// <summary>
        /// Gets the module version.
        /// </summary>
        public ModuleVersion ModuleVersion
        {
            get
            {
                return moduleVersion.Value;
            }
        }

        /// <summary>
        /// Gets the name of the mapped image. In most cases, this is null. If the debugger is mapping an image file
        /// (for example, during minidump debugging), this is the name of the mapped image.
        /// </summary>
        public string MappedImageName
        {
            get
            {
                return mappedImageName.Value;
            }
        }

        /// <summary>
        /// Gets the DateTime of module creation.
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return timestampAndSize.Value.Item1;
            }
        }

        /// <summary>
        /// Gets the size in bytes.
        /// </summary>
        public ulong Size
        {
            get
            {
                return timestampAndSize.Value.Item2;
            }
        }

        /// <summary>
        /// Gets the CLR module.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrModule ClrModule
        {
            get
            {
                return clrModule.Value;
            }
        }

        /// <summary>
        /// Gets the CLR PDB reader.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader ClrPdbReader
        {
            get
            {
                return clrPdbReader.Value;
            }
        }

        /// <summary>
        /// Gets the global or static variable.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <returns>Variable if found</returns>
        /// <exception cref="System.ArgumentException">Variable name contains wrong module name. Don't add it manually, it will be added automatically.</exception>
        public Variable GetVariable(string name)
        {
            int moduleIndex = name.IndexOf('!');

            if (moduleIndex > 0)
            {
                if (string.Compare(name, 0, Name, 0, Math.Max(Name.Length, moduleIndex), true) != 0)
                {
                    throw new ArgumentException("Variable name contains wrong module name. Don't add it manually, it will be added automatically.");
                }

                name = name.Substring(moduleIndex + 1);
            }

            return UserTypeCastedGlobalVariables[name];
        }

        /// <summary>
        /// Gets the CLR static variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="appDomain">The application domain.</param>
        /// <returns>Static variable if found</returns>
        public Variable GetClrVariable(string name, CsScriptManaged.CLR.AppDomain appDomain)
        {
            int variableNameIndex = name.LastIndexOf('.');
            string typeName = name.Substring(0, variableNameIndex);
            var clrType = ClrModule.GetTypeByName(typeName);

            if (clrType == null)
            {
                throw new Exception("CLR type not found " + typeName);
            }

            string variableName = name.Substring(variableNameIndex + 1);
            var staticField = clrType.GetStaticFieldByName(variableName);

            if (staticField == null)
            {
                throw new Exception("Field " + staticField + " wasn't found in CLR type " + typeName);
            }

            var address = staticField.GetAddress(appDomain.ClrAppDomain);

            return Variable.CreateNoCast(FromClrType(clrType), address, variableName);
        }

        #region Cache filling functions
        /// <summary>
        /// Gets the global variable by the name.
        /// </summary>
        /// <param name="name">The name.</param>
        private Variable GetGlobalVariable(string name)
        {
            // Check if it is CLR variable
            if (name.Contains("."))
            {
                return GetClrVariable(name, Process.CurrentCLRAppDomain);
            }
            else
            {
                uint typeId = Context.SymbolProvider.GetGlobalVariableTypeId(this, name);
                var codeType = TypesById[typeId];
                ulong address = Context.SymbolProvider.GetGlobalVariableAddress(this, name);

                return Variable.CreateNoCast(codeType, address, name, name);
            }
        }

        /// <summary>
        /// Gets the type with the specified name.
        /// </summary>
        /// <param name="name">The type name.</param>
        private CodeType GetTypeByName(string name)
        {
            int moduleIndex = name.IndexOf('!');

            if (moduleIndex > 0)
            {
                if (string.Compare(name.Substring(0, moduleIndex), Name, true) != 0)
                {
                    throw new ArgumentException("Type name contains wrong module name. Don't add it manually, it will be added automatically.");
                }

                name = name.Substring(moduleIndex + 1);
            }

            CodeType codeType = null;

            if (clrModule.Cached && ClrModule != null)
            {
                codeType = GetClrTypeByName(name);
            }

            try
            {
                if (codeType == null)
                {
                    uint typeId = Context.SymbolProvider.GetTypeId(this, name);

                    codeType = TypesById[typeId];
                }
            }
            catch (Exception)
            {
                if (ClrModule != null)
                {
                    codeType = GetClrTypeByName(name);
                }
            }

            if (codeType == null)
            {
                throw new Exception(string.Format("Type '{0}' wasn't found", name));
            }

            return codeType;
        }

        /// <summary>
        /// Gets the type with the specified name.
        /// </summary>
        /// <param name="name">The CLR type name.</param>
        private CodeType GetClrTypeByName(string name)
        {
            try
            {
                // Try to find code type inside CLR module
                var clrType = ClrModule.GetTypeByName(name);

                if (clrType != null)
                {
                    // Create a code type
                    return new ClrCodeType(this, clrType);
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        /// <summary>
        /// Gets the type with the specified identifier.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        private CodeType GetTypeById(uint typeId)
        {
            return new NativeCodeType(this, typeId, Context.SymbolProvider.GetTypeTag(this, typeId), Context.SymbolProvider.GetTypeBasicType(this, typeId));
        }

        /// <summary>
        /// Gets the next fake code type identifier.
        /// </summary>
        internal uint GetNextFakeCodeTypeId()
        {
            return (uint)System.Threading.Interlocked.Decrement(ref nextFakeCodeTypeId);
        }

        /// <summary>
        /// Determines whether the specified code type identifier is fake.
        /// </summary>
        /// <param name="codeTypeId">The code type identifier.</param>
        internal bool IsFakeCodeTypeId(uint codeTypeId)
        {
            return codeTypeId >= (uint)nextFakeCodeTypeId;
        }

        /// <summary>
        /// Creates CodeType from the CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type.</param>
        internal CodeType FromClrType(Microsoft.Diagnostics.Runtime.ClrType clrType)
        {
            return Process.FromClrType(clrType);
        }
        #endregion
    }
}
