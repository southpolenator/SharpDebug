using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsScripts;
using DbgEngManaged;

namespace CsScriptManaged.SymbolProviders
{
    internal class DbgEngSymbolProvider : ISymbolProvider
    {
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

        public uint GetTypeSize(Module module, uint typeId)
        {
            using (ProcessSwitcher switcher = new ProcessSwitcher(module.Process))
            {
                return Context.Symbols.GetTypeSize(module.Id, typeId);
            }
        }

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
