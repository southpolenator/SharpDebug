using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CsDebugScript
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
        private readonly Variable[] variables;


        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollection"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public VariableCollection(Variable[] variables)
        {
            this.variables = variables;
        }

        /// <summary>
        /// Gets the <see cref="Variable"/> with the specified name.
        /// </summary>
        /// <param name="name">The variable name.</param>
        public Variable this[string name]
        {
            get
            {
                return this.variables.Single(r => r.GetName() == name);
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
        public IEnumerable<string> Names
        {
            get
            {
                return variables.AsEnumerable().Select(r=>r.GetName());
            }
        }

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
            return Names.Any(r => r == name);
        }
    }
}
