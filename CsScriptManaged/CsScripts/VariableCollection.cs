using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsScripts
{
    /// <summary>
    /// This is helper class for easier navigation through local variables and arguments.
    /// It is not performance optimized and should not be used as dictionary, but only as array of variables.
    /// </summary>
    public class VariableCollection : IReadOnlyList<Variable>, IReadOnlyDictionary<string, Variable>
    {
        /// <summary>
        /// The array of variables
        /// </summary>
        private Variable[] variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollection"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public VariableCollection(Variable[] variables)
        {
            this.variables = variables;
        }

        /// <summary>
        /// Gets the <see cref="Variable"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public Variable this[string key]
        {
            get
            {
                return variables.Where(v => v.GetName() == key).First();
            }
        }

        /// <summary>
        /// Gets the <see cref="Variable"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
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
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        public IEnumerable<string> Keys
        {
            get
            {
                return variables.Select(v => v.GetName());
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        public IEnumerable<Variable> Values
        {
            get
            {
                return variables;
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
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        public bool ContainsKey(string key)
        {
            return Keys.Contains(key);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out Variable value)
        {
            value = variables.Where(v => v.GetName() == key).FirstOrDefault();
            return value != null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator<KeyValuePair<string, Variable>> IEnumerable<KeyValuePair<string, Variable>>.GetEnumerator()
        {
            return variables.ToDictionary(v => v.GetName()).GetEnumerator();
        }
    }
}
