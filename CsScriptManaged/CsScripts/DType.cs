using CsScriptManaged;
using DbgEngManaged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsScripts
{
    public class DType
    {
        private DEBUG_TYPED_DATA typedData;

        internal DType(DEBUG_TYPED_DATA typedData)
        {
            this.typedData = typedData;
        }

        internal DType(ulong moduleId, uint typeId, ulong offset = 0, SymTag tag = SymTag.Null)
        {
            try
            {
                typedData = Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.SetFromTypeIdAndU64,
                    InData = new DEBUG_TYPED_DATA()
                    {
                        ModBase = moduleId,
                        TypeId = typeId,
                        Offset = 350978832288,
                    },
                }).OutData;
            }
            catch (Exception)
            {
                typedData.ModBase = moduleId;
                typedData.TypeId = typeId;
                typedData.Offset = offset;
                typedData.Tag = tag;
            }
        }

        public ulong ModuleId
        {
            get
            {
                return typedData.ModBase;
            }
        }

        public uint TypeId
        {
            get
            {
                return typedData.TypeId;
            }
        }

        internal SymTag Tag
        {
            get
            {
                return typedData.Tag;
            }
        }

        public DType BaseType
        {
            get
            {
                if (typedData.BaseTypeId < Constants.MaxBaseTypeId && typedData.BaseTypeId != TypeId)
                {
                    return new DType(ModuleId, typedData.BaseTypeId);
                }
                else if (Tag == SymTag.ArrayType || Tag == SymTag.PointerType)
                {
                    try
                    {
                        return new DType(Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                        {
                            Operation = ExtTdop.GetDereference,
                            InData = typedData,
                        }).OutData);
                    }
                    catch (Exception)
                    {
                    }
                }

                return this;
            }
        }

        public string Name
        {
            get
            {
                uint nameSize;
                StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                Context.Symbols.GetTypeName(ModuleId, TypeId, sb, (uint)sb.Capacity, out nameSize);
                return sb.ToString();
            }
        }

        public uint Size
        {
            get
            {
                return Context.Symbols.GetTypeSize(ModuleId, TypeId);
            }
        }

        public bool IsEnum
        {
            get
            {
                return typedData.Tag == SymTag.Enum;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
