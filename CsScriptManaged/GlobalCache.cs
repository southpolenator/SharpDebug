using CsScripts;
using DbgEngManaged;
using System;
using System.Collections.Generic;

namespace CsScriptManaged
{
    /// <summary>
    /// Helper class for caching global objects.
    /// </summary>
    public class GlobalCache
    {
        /// <summary>
        /// The processes
        /// </summary>
        public static GlobalCache<Tuple<uint, uint>, Process> Processes = new GlobalCache<Tuple<uint, uint>, Process>(CreateProcess);

        /// <summary>
        /// The typed data
        /// </summary>
        public static GlobalCache<Tuple<ulong, uint, ulong>, DEBUG_TYPED_DATA> TypedData = new GlobalCache<Tuple<ulong, uint, ulong>, DEBUG_TYPED_DATA>(GetTypedData);

        /// <summary>
        /// The list of simple caches that should be invalidated after medatada is removed so that new metadata can create new caches...
        /// </summary>
        internal static List<SimpleCache<Variable[]>> VariablesUserTypeCastedFields = new List<SimpleCache<Variable[]>>();

        /// <summary>
        /// The list of global caches that should be invalidated after medatada is removed so that new metadata can create new caches...
        /// </summary>
        internal static List<GlobalCache<string, Variable>> VariablesUserTypeCastedFieldsByName = new List<GlobalCache<string, Variable>>();

        /// <summary>
        /// The List of user type casted variable collections that should be invalidated after metadata is removed so that new metadata can create new caches...
        /// </summary>
        internal static List<SimpleCache<VariableCollection>> UserTypeCastedVariableCollections = new List<SimpleCache<VariableCollection>>();

        /// <summary>
        /// Creates the process.
        /// </summary>
        /// <param name="processKey">The process key.</param>
        private static Process CreateProcess(Tuple<uint, uint> processKey)
        {
            return new Process(processKey.Item1, processKey.Item2);
        }

        /// <summary>
        /// Gets the typed data.
        /// </summary>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="offset">The offset.</param>
        private static DEBUG_TYPED_DATA GetTypedData(Tuple<ulong, uint, ulong> typedDataId)
        {
            return Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
            {
                Operation = ExtTdop.SetFromTypeIdAndU64,
                InData = new DEBUG_TYPED_DATA()
                {
                    ModBase = typedDataId.Item1,
                    TypeId = typedDataId.Item2,
                    Offset = typedDataId.Item3,
                },
            }).OutData;
        }
    }

    /// <summary>
    /// Helper class for caching global objects.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class GlobalCache<TKey, TValue>
    {
        /// <summary>
        /// The populate action
        /// </summary>
        private Func<TKey, TValue> populateAction;

        /// <summary>
        /// The cached values
        /// </summary>
        private Dictionary<TKey, TValue> values = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalCache{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="populateAction">The populate action.</param>
        public GlobalCache(Func<TKey, TValue> populateAction)
        {
            this.populateAction = populateAction;
        }

        /// <summary>
        /// Clears this cache.
        /// </summary>
        public void Clear()
        {
            values.Clear();
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                return values.Values;
            }
        }

        /// <summary>
        /// Gets the number of entries inside the cache.
        /// </summary>
        public int Count
        {
            get
            {
                return values.Count;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;

                if (!values.TryGetValue(key, out value))
                {
                    value = populateAction(key);
                    values.Add(key, value);
                }

                return value;
            }

            set
            {
                values[key] = value;
            }
        }

        /// <summary>
        /// Gets the existing value in the cache associated with the specified key. Value won't be populated if it is not in cache.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter. This parameter
        /// is passed uninitialized.</param>
        /// <returns>
        ///   <c>true</c> if the <see cref="GlobalCache{TKey, TValue}" /> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetExistingValue(TKey key, out TValue value)
        {
            return values.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the value in the cache associated with the specified key. Value will be populated if it is not in cache.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the value parameter. This parameter
        /// is passed uninitialized.</param>
        /// <returns>
        ///   <c>true</c> if the <see cref="GlobalCache{TKey, TValue}" /> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(TKey typeName, out TValue userType)
        {
            try
            {
                userType = this[typeName];
                return true;
            }
            catch (Exception)
            {
                userType = default(TValue);
                return false;
            }
        }
    }
}
