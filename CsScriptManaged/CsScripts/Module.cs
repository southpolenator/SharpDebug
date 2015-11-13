using CsScriptManaged;
using DbgEngManaged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsScripts
{
    public class Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal Module(Process process, ulong id)
        {
            Id = id;
        }

        /// <summary>
        /// Gets all modules for the current process.
        /// </summary>
        public static Module[] All
        {
            get
            {
                return Process.Current.Modules;
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public ulong Id { get; private set; }

        /// <summary>
        /// Gets the owning process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the offset (address location of module base).
        /// </summary>
        public ulong Offset
        {
            get
            {
                return Id;
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
                return GetName(DebugModname.Module);
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
                return GetName(DebugModname.Image);
            }
        }

        /// <summary>
        /// Gets the name of the loaded image. Unless Microsoft CodeView symbols are present, this is the same as the image name.
        /// </summary>
        public string LoadedImageName
        {
            get
            {
                return GetName(DebugModname.LoadedImage);
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
                return GetName(DebugModname.SymbolFile);
            }
        }

        /// <summary>
        /// Gets the name of the mapped image. In most cases, this is NULL. If the debugger is mapping an image file
        /// (for example, during minidump debugging), this is the name of the mapped image.
        /// </summary>
        public string MappedImageName
        {
            get
            {
                return GetName(DebugModname.MappedImage);
            }
        }

        private string GetName(DebugModname modname)
        {
            uint nameSize;
            StringBuilder sb = new StringBuilder(Constants.MaxFileName);

            Context.Symbols.GetModuleNameStringWide((uint)modname, 0xffffffff, Id, sb, (uint)sb.Capacity, out nameSize);
            return sb.ToString();
        }
    }
}
