using CsScriptManaged;
using DbgEngManaged;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CsScripts
{
    public class Variable : DynamicObject
    {
        private DEBUG_TYPED_DATA typedData;
        private string name;

        internal Variable(string name, _DEBUG_SYMBOL_ENTRY entry)
        {
            this.name = name;
            if (entry.Offset > 0)
            {
                typedData = Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.SetFromTypeIdAndU64,
                    InData = new DEBUG_TYPED_DATA()
                    {
                        ModBase = entry.ModuleBase,
                        Offset = entry.Offset,
                        TypeId = entry.TypeId,
                    },
                }).OutData;
            }
            else
            {
                typedData.Size = entry.Size;
                typedData.Offset = entry.Offset;
            }
        }

        internal Variable(string name, DEBUG_TYPED_DATA typedData)
        {
            this.name = name;
            this.typedData = typedData;
        }

        public static explicit operator byte (Variable v)
        {
            return (byte)(short)v;
        }

        public static explicit operator short (Variable v)
        {
            return (short)(int)v;
        }

        public static explicit operator int (Variable v)
        {
            return (int)(long)v;
        }

        public static explicit operator long (Variable v)
        {
            uint read;
            uint size = v.typedData.Size;
            IntPtr pointer = Marshal.AllocHGlobal((int)size);

            try
            {
                Context.Symbols.ReadTypedDataVirtual(v.typedData.Offset, v.typedData.ModBase, v.typedData.TypeId, pointer, size, out read);

                switch (size)
                {
                    case 1:
                        return Marshal.ReadByte(pointer);
                    case 2:
                        return Marshal.ReadInt16(pointer);
                    case 4:
                        return Marshal.ReadInt32(pointer);
                    case 8:
                        return Marshal.ReadInt64(pointer);
                    default:
                        throw new Exception("Unexpected variable size");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        public string GetName()
        {
            return name;
        }

        public string[] GetFieldNames()
        {
            List<string> fields = new List<string>();
            uint nameSize;

            try
            {
                for (uint fieldIndex = 0; ; fieldIndex++)
                {
                    StringBuilder sb = new StringBuilder();

                    Context.Symbols.GetFieldName(typedData.ModBase, typedData.TypeId, fieldIndex, sb, 1024, out nameSize);
                    fields.Add(sb.ToString());
                }
            }
            catch (Exception)
            {
            }

            return fields.ToArray();
        }

        public Variable[] GetFields()
        {
            string[] fieldNames = GetFieldNames();
            Variable[] fields = new Variable[fieldNames.Length];

            for (int i = 0; i < fieldNames.Length; i++)
            {
                fields[i] = GetField(fieldNames[i]);
            }

            return fields;
        }

        public Variable GetField(string name)
        {
            var response = Context.Advanced.RequestExtended(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
            {
                Operation = ExtTdop.GetField,
                InData = typedData,
                InStrIndex = (uint)Marshal.SizeOf<EXT_TYPED_DATA>(),
            }, name);

            return new Variable(name, response.OutData);
        }

        private bool TryGetMember(string name, out object result)
        {
            try
            {
                result = GetField(name);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length > 1)
            {
                throw new Exception("Multidimensional arrays are not supported");
            }

            if (typedData.Tag == SymTag.PointerType || typedData.Tag == SymTag.ArrayType)
            {
                int index = (int)indexes[0];
                var response = Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                {
                    Operation = ExtTdop.GetArrayElement,
                    InData = typedData,
                    In64 = (ulong)index,
                }).OutData;

                result = new Variable("<computed>", response);
                return true;
            }

            return TryGetMember(indexes[0].ToString(), out result);
        }

        #region Not allowed setters/deleters
        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            throw new UnauthorizedAccessException();
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            throw new UnauthorizedAccessException();
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new UnauthorizedAccessException();
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            throw new UnauthorizedAccessException();
        }
        #endregion
    }
}
