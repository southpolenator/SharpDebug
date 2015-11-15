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
            var methods = type.GetMethods();

            nameFilter = nameFilter.ToLower();
            foreach (var method in methods)
            {
                if (string.IsNullOrEmpty(nameFilter) || method.Name.ToLower().Contains(nameFilter))
                {
                    if (!method.IsPrivate)
                    {
                        yield return method.ToString();
                    }
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
