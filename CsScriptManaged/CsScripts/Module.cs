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
        private Module(ulong id)
        {
            Id = id;
        }

        public static Module[] All
        {
            get
            {
                List<Module> modules = new List<Module>();

                try
                {
                    for (uint i = 0; ; i++)
                    {
                        ulong moduleId = Context.Symbols.GetModuleByIndex(i);

                        modules.Add(new Module(moduleId));
                    }
                }
                catch (Exception)
                {
                }

                return modules.ToArray();
            }
        }

        public ulong Id { get; private set; }

        public string Name
        {
            get
            {
                return GetName(DebugModname.Module);
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
