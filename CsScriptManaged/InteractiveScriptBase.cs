using CsScripts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsScriptManaged
{
    /// <summary>
    /// Base class for interactive script commands
    /// </summary>
    public class InteractiveScriptBase : ScriptBase
    {
        /// <summary>
        /// The interactive script variables
        /// </summary>
        public dynamic _Interactive_Script_Variables_;

        /// <summary>
        /// Gets the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        public IEnumerable<string> GetCommands(string nameFilter = "")
        {
            Type type = GetType();

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
        /// Gets the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
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
        /// Lists the variables.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        public void ListVariables(string nameFilter = "")
        {
            nameFilter = nameFilter.ToLower();
            foreach (var variable in (IDictionary<string, object>)_Interactive_Script_Variables_)
            {
                if (string.IsNullOrEmpty(nameFilter) || variable.Key.ToLower() == nameFilter)
                {
                    Console.WriteLine("  {0} [{1}] = {2}", variable.Key, variable.Value.GetType(), variable.Value);
                }
            }
        }
    }
}
