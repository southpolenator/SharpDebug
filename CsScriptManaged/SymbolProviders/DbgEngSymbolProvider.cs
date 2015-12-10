using System;
using System.Collections.Generic;
using System.Text;
using CsScripts;
using DbgEngManaged;

namespace CsScriptManaged.SymbolProviders
{
    internal class DbgEngSymbolProvider : ISymbolProvider
    {
        /// <summary>
        /// Gets the element type of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeElementTypeId(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                var typedData = GlobalCache.TypedData[Tuple.Create(module.Id, typeId, module.Process.PEB)];
                typedData.Data = module.Process.PEB;
                var result = Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.GetDereference,
                    InData = typedData,
                }).OutData.TypeId;

                return result;
            }
        }

        /// <summary>
        /// Gets the names of all fields of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string[] GetTypeFieldNames(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                List<string> fields = new List<string>();
                uint nameSize;

                try
                {
                    for (uint fieldIndex = 0; ; fieldIndex++)
                    {
                        StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                        Context.Symbols.GetFieldName(module.Id, typeId, fieldIndex, sb, (uint)sb.Capacity, out nameSize);
                        fields.Add(sb.ToString());
                    }
                }
                catch (Exception)
                {
                }

                return fields.ToArray();
            }
        }

        /// <summary>
        /// Gets the field type id and offset of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        public Tuple<uint, int> GetTypeFieldTypeAndOffset(Module module, uint typeId, string fieldName)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                try
                {
                    uint fieldTypeId, fieldOffset;

                    Context.Symbols.GetFieldTypeAndOffsetWide(module.Id, typeId, fieldName, out fieldTypeId, out fieldOffset);
                    return Tuple.Create(fieldTypeId, (int)fieldOffset);
                }
                catch (Exception)
                {
                    return Tuple.Create<uint, int>(0, -1);
                }
            }
        }

        /// <summary>
        /// Gets the name of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public string GetTypeName(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                uint nameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                Context.Symbols.GetTypeName(module.Id, typeId, sb, (uint)sb.Capacity, out nameSize);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the size of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public uint GetTypeSize(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                return Context.Symbols.GetTypeSize(module.Id, typeId);
            }
        }

        /// <summary>
        /// Gets the symbol tag of the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeId">The type identifier.</param>
        public SymTag GetTypeTag(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                return Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.SetFromTypeIdAndU64,
                    InData = new DEBUG_TYPED_DATA()
                    {
                        ModBase = module.Id,
                        TypeId = typeId,
                        Data = module.Process.PEB,
                        Offset = module.Process.PEB,
                    },
                }).OutData.Tag;
            }
        }
    }
}
