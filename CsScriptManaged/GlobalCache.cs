using CsScriptManaged.Native;
using CsScriptManaged.Utility;
using CsScripts;
using DbgEngManaged;
using System;
using System.Collections.Generic;

namespace CsScriptManaged
{
    /// <summary>
    /// Helper class for caching global objects.
    /// </summary>
    internal class GlobalCache
    {
        /// <summary>
        /// The processes
        /// </summary>
        internal static DictionaryCache<uint, Process> Processes = new DictionaryCache<uint, Process>(CreateProcess);

        /// <summary>
        /// The typed data
        /// </summary>
        internal static DictionaryCache<Tuple<ulong, uint, ulong>, DEBUG_TYPED_DATA> TypedData = new DictionaryCache<Tuple<ulong, uint, ulong>, DEBUG_TYPED_DATA>(GetTypedData);

        /// <summary>
        /// The list of simple caches that should be invalidated after medatada is removed so that new metadata can create new caches...
        /// </summary>
        internal static List<SimpleCache<Variable[]>> VariablesUserTypeCastedFields = new List<SimpleCache<Variable[]>>();

        /// <summary>
        /// The list of global caches that should be invalidated after medatada is removed so that new metadata can create new caches...
        /// </summary>
        internal static List<DictionaryCache<string, Variable>> VariablesUserTypeCastedFieldsByName = new List<DictionaryCache<string, Variable>>();

        /// <summary>
        /// The List of user type casted variable collections that should be invalidated after metadata is removed so that new metadata can create new caches...
        /// </summary>
        internal static List<SimpleCache<VariableCollection>> UserTypeCastedVariableCollections = new List<SimpleCache<VariableCollection>>();

        /// <summary>
        /// The user type casted variables that should be invalidated after metadata is removed so that new metadata can create new caches...
        /// </summary>
        internal static List<DictionaryCache<Variable, Variable>> UserTypeCastedVariables = new List<DictionaryCache<Variable, Variable>>();

        /// <summary>
        /// Creates the process.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        private static Process CreateProcess(uint processId)
        {
            return new Process(processId);
        }

        /// <summary>
        /// Gets the typed data.
        /// </summary>
        /// <param name="typedDataId">The typed data identifier.</param>
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
}
