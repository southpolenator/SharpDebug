using CsScriptManaged;
using CsScriptManaged.SymbolProviders;
using CsScriptManaged.Utility;
using System;

namespace CsScripts
{
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
        /// The next fake code type identifier
        /// </summary>
        private uint nextFakeCodeTypeId = uint.MaxValue;

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
        /// Gets the global variable by the name.
        /// </summary>
        /// <param name="name">The name.</param>
        private Variable GetGlobalVariable(string name)
        {
            uint typeId = Context.SymbolProvider.GetGlobalVariableTypeId(this, name);
            var codeType = TypesById[typeId];
            ulong address = Context.SymbolProvider.GetGlobalVariableAddress(this, name);

            return Variable.CreateNoCast(codeType, address, name, name);
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
        #region Cache filling functions

        /// <summary>
        /// Gets the type with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
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

            uint typeId = Context.SymbolProvider.GetTypeId(this, name);

            return TypesById[typeId];
        }

        /// <summary>
        /// Gets the type with the specified identifier.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        private CodeType GetTypeById(uint typeId)
        {
            return new CodeType(this, typeId, Context.SymbolProvider.GetTypeTag(this, typeId), Context.SymbolProvider.GetTypeBasicType(this, typeId));
        }

        /// <summary>
        /// Gets the next fake code type identifier.
        /// </summary>
        internal uint GetNextFakeCodeTypeId()
        {
            return nextFakeCodeTypeId--;
        }
        #endregion
    }
}
