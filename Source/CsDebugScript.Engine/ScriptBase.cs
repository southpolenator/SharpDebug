using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;

namespace CsDebugScript
{
    /// <summary>
    /// Base class for all C# scripts.
    /// </summary>
    public class ScriptBase
    {
        /// <summary>
        /// Helper class for making Modules be dynamic object inside the scripts.
        /// </summary>
        private class ModulesDynamicObject : DynamicObject
        {
            /// <summary>
            /// Tries the get member.
            /// </summary>
            /// <param name="binder">The binder.</param>
            /// <param name="result">The result.</param>
            /// <returns><c>true</c> if member is found; <c>false</c> otherwise</returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                Module module = Module.All.FirstOrDefault(m => string.Compare(binder.Name, m.Name, true) == 0);

                if (module != null)
                {
                    result = new ModuleGlobalsDynamicObject(module);
                    return true;
                }

                result = null;
                return false;
            }
        }

        /// <summary>
        /// Helper class for making one Module be dynamic object inside the scripts (after getting it from Modules dynamic object).
        /// </summary>
        private class ModuleGlobalsDynamicObject : DynamicObject
        {
            /// <summary>
            /// The module
            /// </summary>
            private Module module;

            /// <summary>
            /// Initializes a new instance of the <see cref="ModuleGlobalsDynamicObject"/> class.
            /// </summary>
            /// <param name="module">The module.</param>
            public ModuleGlobalsDynamicObject(Module module)
            {
                this.module = module;
            }

            /// <summary>
            /// Performs an implicit conversion from <see cref="ModuleGlobalsDynamicObject"/> to <see cref="Module"/>.
            /// </summary>
            /// <param name="helper">The helper.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator Module(ModuleGlobalsDynamicObject helper)
            {
                return helper.module;
            }

            /// <summary>
            /// Tries the get member.
            /// </summary>
            /// <param name="binder">The binder.</param>
            /// <param name="result">The result.</param>
            /// <returns><c>true</c> if member is found; <c>false</c> otherwise</returns>
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

        /// <summary>
        /// Helper class for making Globals be dynamic object inside the scripts.
        /// </summary>
        private class GlobalsDynamicObject : DynamicObject
        {
            /// <summary>
            /// Tries the get member.
            /// </summary>
            /// <param name="binder">The binder.</param>
            /// <param name="result">The result.</param>
            /// <returns><c>true</c> if member is found; <c>false</c> otherwise</returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                try
                {
                    result = Process.Current.GetGlobal(binder.Name);
                    return true;
                }
                catch (Exception)
                {
                    result = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// The Modules dynamic object. You can use this dynamic variable to easily access Modules and afterwards global variables.
        /// </summary>
        public dynamic Modules = new ModulesDynamicObject();

        /// <summary>
        /// The Globals dynamic object. You can use this dynamic variable to easily access global variables.
        /// </summary>
        public dynamic Globals = new GlobalsDynamicObject();

        /// <summary>
        /// Gets the array of all processes being debugged.
        /// </summary>
        public static Process[] Processes
        {
            get
            {
                return Process.All;
            }
        }

        /// <summary>
        /// Gets the array of all threads in the current process.
        /// </summary>
        public static Thread[] Threads
        {
            get
            {
                return Process.Current.Threads;
            }
        }

        /// <summary>
        /// Gets the array of all frames in the current thread.
        /// </summary>
        public static StackFrame[] Frames
        {
            get
            {
                return StackTrace.Current.Frames;
            }
        }

        /// <summary>
        /// Gets the variable collection of arguemnts from current stack frame.
        /// </summary>
        public static VariableCollection Arguments
        {
            get
            {
                return StackFrame.Current.Arguments;
            }
        }

        /// <summary>
        /// Gets the variable collection of local variables from current stack frame.
        /// </summary>
        public static VariableCollection Locals
        {
            get
            {
                return StackFrame.Current.Locals;
            }
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void write(object obj)
        {
            Console.Write(obj);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void write<T>(IEnumerable<T> objects)
        {
            Console.Write(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void Write(object obj)
        {
            Console.Write(obj);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void Write<T>(IEnumerable<T> objects)
        {
            Console.Write(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void writeln(object obj)
        {
            Console.WriteLine(obj);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void writeln(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void writeln<T>(IEnumerable<T> objects)
        {
            Console.WriteLine(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        public static void writeln()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void WriteLine(object obj)
        {
            Console.WriteLine(obj);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void WriteLine<T>(IEnumerable<T> objects)
        {
            Console.WriteLine(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        public static void WriteLine()
        {
            Console.WriteLine();
        }
    }
}
