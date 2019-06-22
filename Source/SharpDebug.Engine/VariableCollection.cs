using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpDebug
{
    /// <summary>
    /// This is helper class for easier navigation through local variables and arguments.
    /// It is not performance optimized and should not be used as dictionary, but only as array of variables.
    /// </summary>
    public class VariableCollection : IReadOnlyList<Variable>
    {
        /// <summary>
        /// The array of variables
        /// </summary>
        private Variable[] variables;

        /// <summary>
        /// The variables indexed by name
        /// </summary>
        private Dictionary<string, Variable> variablesByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollection"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public VariableCollection(Variable[] variables)
            : this(variables, variables.Select(v => v.GetName()).ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollection"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="names">The array of variable names. When user type casting happens, some variables might become <c>null</c>.</param>
        public VariableCollection(Variable[] variables, string[] names)
            : this(variables, names, CreateDictionary(variables, names))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollection"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="names">The array of variable names. When user type casting happens, some variables might become <c>null</c>.</param>
        /// <param name="variablesByName">The variables indexed by name.</param>
        public VariableCollection(Variable[] variables, string[] names, Dictionary<string, Variable> variablesByName)
        {
            this.variables = variables;
            this.variablesByName = variablesByName;
            Names = names;
        }

        /// <summary>
        /// Gets the <see cref="Variable"/> with the specified name.
        /// </summary>
        /// <param name="name">The variable name.</param>
        public Variable this[string name]
        {
            get
            {
                return variablesByName[name];
            }
        }

        /// <summary>
        /// Gets the <see cref="Variable"/> at the specified index.
        /// </summary>
        /// <param name="index">The array index.</param>
        public Variable this[int index]
        {
            get
            {
                return variables[index];
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return variables.Length;
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the variables names.
        /// </summary>
        public string[] Names { get; private set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<Variable> GetEnumerator()
        {
            return variables.AsEnumerable().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return variables.GetEnumerator();
        }

        /// <summary>
        /// Determines whether variable with the specified name is in the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        public bool ContainsName(string name)
        {
            return variablesByName.ContainsKey(name);
        }

        /// <summary>
        /// Tries to get variable with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public bool TryGetValue(string name, out Variable value)
        {
            return variablesByName.TryGetValue(name, out value);
        }

        /// <summary>
        /// Creates dictionary of variable names.
        /// </summary>
        /// <param name="variables">Array of variables</param>
        /// <param name="names">The array of variable names. When user type casting happens, some variables might become <c>null</c>.</param>
        private static Dictionary<string, Variable> CreateDictionary(Variable[] variables, string[] names)
        {
            Dictionary<string, Variable> result = new Dictionary<string, Variable>(variables.Length);

            for (int i = variables.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(names[i]) && !result.ContainsKey(names[i]))
                {
                    result.Add(names[i], variables[i]);
                }
            }
            return result;
        }
    }
}
