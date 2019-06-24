using SharpDebug.Engine.Native;
using SharpDebug.Engine.SymbolProviders;
using DbgEng;
using SharpUtilities;
using System;

namespace SharpDebug.Engine.Debuggers.DbgEngDllHelpers
{
    /// <summary>
    /// Symbol provider that is being implemented over DbgEng.dll.
    /// </summary>
    internal class DbgEngSymbolProvider : PerModuleSymbolProvider
    {
        /// <summary>
        /// The typed data
        /// </summary>
        internal static readonly DictionaryCache<Tuple<ulong, uint, ulong>, DEBUG_TYPED_DATA> typedData = new DictionaryCache<Tuple<ulong, uint, ulong>, DEBUG_TYPED_DATA>(GetTypedData);

        /// <summary>
        /// Initializes a new instance of the <see cref="DbgEngSymbolProvider"/> class.
        /// </summary>
        public DbgEngSymbolProvider(DbgEngDll dbgEngDll)
        {
            DbgEngDll = dbgEngDll;
        }

        /// <summary>
        /// Gets the DbgEngDll debugger engine.
        /// </summary>
        public DbgEngDll DbgEngDll { get; private set; }

        /// <summary>
        /// Loads symbol provider module from the specified module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>Interface for symbol provider module</returns>
        public override ISymbolProviderModule LoadModule(Module module)
        {
            return new DbgEngSymbolProviderModule(this, module);
        }

        /// <summary>
        /// Gets the typed data.
        /// </summary>
        /// <param name="typedDataId">The typed data identifier.</param>
        private static DEBUG_TYPED_DATA GetTypedData(Tuple<ulong, uint, ulong> typedDataId)
        {
            var dbgEngDll = (DbgEngDll)Context.Debugger;

            return dbgEngDll.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
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
