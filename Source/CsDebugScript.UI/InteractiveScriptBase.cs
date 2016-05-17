using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Scripting;

namespace CsDebugScript
{
    /// <summary>
    /// Helper for dumping objects using InteractiveScriptBase.ObjectWriter.
    /// </summary>
    public static class InteractiveScriptBaseExtensions
    {
        /// <summary>
        /// Outputs the specified object using InteractiveScriptBase.ObjectWriter.
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void Dump(this object obj)
        {
            InteractiveScriptBase interactiveScript = InteractiveScriptBase.Current;

            if (interactiveScript == null)
                throw new NotImplementedException("Calling Dump() is only supported while using interactive scripting");

            interactiveScript.Dump(obj);
        }
    }

    /// <summary>
    /// Base class for interactive script commands
    /// </summary>
    public class InteractiveScriptBase : ScriptBase
    {
        /// <summary>
        /// Gets the interactive script base of the script that is currently executing.
        /// </summary>
        public static InteractiveScriptBase Current { get; internal set; }

        /// <summary>
        /// Gets or sets the object writer using in interactive scripting mode.
        /// </summary>
        public IObjectWriter ObjectWriter { get; set; }

        /// Gets or sets the internal object writer. It is used for writing objects to host window.
        internal IObjectWriter _InternalObjectWriter_ { get; set; }

        /// <summary>
        /// Stops interactive scripting execution. You can use this simply by entering it as command in interactive scripting mode.
        /// </summary>
        public object quit
        {
            get
            {
                throw new ExitRequestedException();
            }
        }

        /// <summary>
        /// Stops interactive scripting execution. You can use this simply by entering it as command in interactive scripting mode.
        /// </summary>
        public object q
        {
            get
            {
                return quit;
            }
        }

        /// <summary>
        /// Stops interactive scripting execution. You can use this simply by entering it as command in interactive scripting mode.
        /// </summary>
        public object exit
        {
            get
            {
                return quit;
            }
        }

        /// <summary>
        /// The interactive script base type for next compile iteration
        /// </summary>
        internal Type _InteractiveScriptBaseType_;

        /// <summary>
        /// The Roslyn script state
        /// </summary>
        internal ScriptState<object> _ScriptState_;

        /// <summary>
        /// Outputs the specified object using ObjectWriter.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Dump(object obj)
        {
            obj = ObjectWriter.Output(obj);
            _InternalObjectWriter_.Output(obj);
        }

        private IEnumerable<string> GetCommands(Type type, System.Reflection.BindingFlags additionalBinding, string nameFilter = "")
        {
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | additionalBinding | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);

            nameFilter = nameFilter.ToLower();
            foreach (var method in methods)
            {
                if (method.DeclaringType != type || method.IsSpecialName)
                    continue;
                if (string.IsNullOrEmpty(nameFilter) || method.Name.ToLower().Contains(nameFilter))
                {
                    yield return method.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <returns>Enumeration of available commands</returns>
        public IEnumerable<string> GetCommands(string nameFilter = "")
        {
            Type type = GetType();

            while (type != null)
            {
                foreach (var command in GetCommands(type, type == GetType() ? System.Reflection.BindingFlags.NonPublic : System.Reflection.BindingFlags.Default, nameFilter))
                    yield return command;

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Lists the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <param name="signatureFilter">The signature filter.</param>
        public void ListCommands(string nameFilter = "", string signatureFilter = "")
        {
            var commands = GetCommands(nameFilter);

            if (!string.IsNullOrEmpty(signatureFilter))
            {
                signatureFilter = signatureFilter.ToLower();
                commands = commands.Where(c => c.ToLower().Contains(signatureFilter));
            }

            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }
        }

        /// <summary>
        /// Gets the available commands including all base classes.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <returns>Enumeration of available commands.</returns>
        public IEnumerable<string> GetAllCommands(string nameFilter = "")
        {
            Type type = GetType();

            while (type != typeof(object))
            {
                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);

                nameFilter = nameFilter.ToLower();
                foreach (var method in methods)
                {
                    if (method.DeclaringType != type)
                        continue;
                    if (string.IsNullOrEmpty(nameFilter) || method.Name.ToLower().Contains(nameFilter))
                    {
                        yield return method.ToString();
                    }
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Lists the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <param name="signatureFilter">The signature filter.</param>
        public void ListAllCommands(string nameFilter = "", string signatureFilter = "")
        {
            var commands = GetAllCommands(nameFilter);

            if (!string.IsNullOrEmpty(signatureFilter))
            {
                signatureFilter = signatureFilter.ToLower();
                commands = commands.Where(c => c.ToLower().Contains(signatureFilter));
            }

            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }
        }

        /// <summary>
        /// Gets stored variables in current interactive execution.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        public IDictionary<string, object> GetVariables(string nameFilter = "")
        {
            IEnumerable<string> variableNames = _ScriptState_.Variables.Select(v => v.Name).Distinct();

            if (!string.IsNullOrEmpty(nameFilter))
            {
                nameFilter = nameFilter.ToLower();
                variableNames = variableNames.Where(v => v.ToLower() == nameFilter);
            }

            Dictionary<string, object> variables = new Dictionary<string, object>();

            foreach (var variableName in variableNames)
            {
                variables.Add(variableName, _ScriptState_.GetVariable(variableName).Value);
            }

            return variables;
        }

        /// <summary>
        /// Lists stored variables in current interactive execution.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        public void ListVariables(string nameFilter = "")
        {
            foreach (var variable in GetVariables(nameFilter))
            {
                Console.WriteLine("  {0} [{1}] = {2}", variable.Key, variable.Value.GetType(), variable.Value);
            }
        }

        /// <summary>
        /// Changes the base class for interactive scripting.
        /// </summary>
        /// <typeparam name="T">Type of the new base class.</typeparam>
        public void ChangeBaseClass<T>()
            where T : InteractiveScriptBase
        {
            ChangeBaseClass(typeof(T));
        }

        /// <summary>
        /// Changes the base class for interactive scripting.
        /// </summary>
        /// <param name="newBaseClassType">Type of the new base class.</param>
        /// <remarks>Base class type must inherit InteractiveScriptBase.</remarks>
        public void ChangeBaseClass(Type newBaseClassType)
        {
            if (typeof(InteractiveScriptBase).IsAssignableFrom(newBaseClassType))
            {
                _InteractiveScriptBaseType_ = newBaseClassType;
            }
            else
            {
                throw new ArgumentException(nameof(newBaseClassType));
            }
        }
    }
}
