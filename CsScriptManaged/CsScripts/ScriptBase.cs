using System;
using System.Linq;
using System.Dynamic;

namespace CsScripts
{
    public class ScriptBase
    {
        public class ModulesHelper : DynamicObject
        {
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                Module module = Module.All.FirstOrDefault(m => string.Compare(binder.Name, m.Name, true) == 0);

                if (module != null)
                {
                    result = new ModuleGlobalsHelper(module);
                    return true;
                }

                result = null;
                return false;
            }
        }

        public class ModuleGlobalsHelper : DynamicObject
        {
            private Module module;

            public ModuleGlobalsHelper(Module module)
            {
                this.module = module;
            }

            public static explicit operator Module (ModuleGlobalsHelper helper)
            {
                return helper.module;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                try
                {
                    result = module.GetVariable(binder.Name);
                    return true;
                }
                catch (Exception)
                {
                    var property = module.GetType().GetProperty(binder.Name);

                    if (property != null)
                    {
                        result = property.GetValue(module);
                        return true;
                    }

                    result = null;
                    return false;
                }
            }
        }

        protected dynamic Modules = new ModulesHelper();

        public static void write(object obj)
        {
            Console.Write(obj);
        }

        public static void write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        public static void Write(object obj)
        {
            Console.Write(obj);
        }

        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        public static void writeln(object obj)
        {
            Console.WriteLine(obj);
        }

        public static void writeln(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static void WriteLine(object obj)
        {
            Console.WriteLine(obj);
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
