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
        public const string ComputedName = "<computed>";
        private DEBUG_TYPED_DATA typedData;
        private string name;

        internal Variable(string name, _DEBUG_SYMBOL_ENTRY entry)
        {
            this.name = name;
            try
            {
                typedData = GetTypedData(entry.ModuleBase, entry.TypeId, entry.Offset);
            }
            catch (Exception)
            {
                // Fill manually available fields
                typedData.Size = entry.Size;
                typedData.ModBase = entry.ModuleBase;
                typedData.Offset = entry.Offset;
                typedData.TypeId = entry.TypeId;
                typedData.Tag = (SymTag)entry.Tag;
            }
        }

        internal Variable(ulong moduleId, uint typeId, ulong offset, string name = ComputedName)
        {
            this.name = name;
            typedData = GetTypedData(moduleId, typeId, offset);
        }

        private static DEBUG_TYPED_DATA GetTypedData(ulong moduleId, uint typeId, ulong offset)
        {
            return Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
            {
                Operation = ExtTdop.SetFromTypeIdAndU64,
                InData = new DEBUG_TYPED_DATA()
                {
                    ModBase = moduleId,
                    Offset = offset,
                    TypeId = typeId,
                },
            }).OutData;
        }

        internal Variable(DEBUG_TYPED_DATA typedData, string name = ComputedName)
        {
            this.name = name;
            this.typedData = typedData;
        }

        public static Variable FromName(string name)
        {
            ulong offset = Context.Symbols.GetOffsetByNameWide(name);
            uint typeId;
            ulong moduleId;

            Context.Symbols.GetSymbolTypeIdWide(name, out typeId, out moduleId);
            return new Variable(moduleId, typeId, offset, name);
        }

        public static explicit operator bool (Variable v)
        {
            if (!v.GetCodeType().IsSimple && !v.GetCodeType().IsPointer)
            {
                return bool.Parse(v.ToString());
            }

            return v.typedData.Data != 0;
        }

        public static explicit operator byte (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return byte.Parse(v.ToString());
            }

            return (byte)v.typedData.Data;
        }

        public static explicit operator char (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return char.Parse(v.ToString());
            }

            return (char)v.typedData.Data;
        }

        public static explicit operator short (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return short.Parse(v.ToString());
            }

            return (short)v.typedData.Data;
        }

        public static explicit operator ushort (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return ushort.Parse(v.ToString());
            }

            return (ushort)v.typedData.Data;
        }

        public static explicit operator int (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return int.Parse(v.ToString());
            }

            return (int)v.typedData.Data;
        }

        public static explicit operator uint (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return uint.Parse(v.ToString());
            }

            return (uint)v.typedData.Data;
        }

        public static explicit operator long (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return long.Parse(v.ToString());
            }

            return (long)v.typedData.Data;
        }

        public static explicit operator ulong (Variable v)
        {
            if (!v.GetCodeType().IsSimple)
            {
                return ulong.Parse(v.ToString());
            }

            return v.typedData.Data;
        }

        public override string ToString()
        {
            var type = GetCodeType();

            // Check if it is null
            if (type.IsPointer && (typedData.Offset == 0 || typedData.Data == 0))
            {
                return "(null)";
            }

            // ANSI string
            if (type.IsAnsiString)
            {
                uint stringLength;
                StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                Context.DataSpaces.ReadMultiByteStringVirtual(typedData.Offset, Constants.MaxStringReadLength, sb, (uint)sb.Capacity, out stringLength);
                return sb.ToString();
            }

            // Unicode string
            if (type.IsWideString)
            {
                uint stringLength;
                StringBuilder sb = new StringBuilder((int)Constants.MaxStringReadLength);

                Context.DataSpaces.ReadUnicodeStringVirtualWide(typedData.Offset, Constants.MaxStringReadLength * 2, sb, (uint)sb.Capacity, out stringLength);
                return sb.ToString();
            }

            // Simple type
            if (type.IsSimple)
            {
                if (type.Name == "bool" || type.Name == "BOOL")
                {
                    return (typedData.Data != 0).ToString();
                }

                switch (typedData.Size)
                {
                    case 1:
                        return ((byte)typedData.Data).ToString();
                    case 2:
                        return ((short)typedData.Data).ToString();
                    case 4:
                        return ((int)typedData.Data).ToString();
                    case 8:
                        return ((long)typedData.Data).ToString();
                    default:
                        throw new ArgumentException("Incorrect data size " + typedData.Size);
                }
            }

            // TODO: Call custom caster (e.g. std::string, std::wstring)

            // Check if it is pointer
            if (type.IsPointer)
            {
                if (type.Size == 4)
                {
                    return string.Format("0x{0:X4}", typedData.Data);
                }
                else
                {
                    return string.Format("0x{0:X8}", typedData.Data);
                }
            }

            return "{" + type.Name + "}";
        }

        public string GetName()
        {
            return name;
        }

        public DType GetCodeType()
        {
            return new DType(typedData);
        }

        public string GetRuntimeType()
        {
            // TODO: See if it is complex type and try to get VTable
            return "";
        }

        public string[] GetFieldNames()
        {
            List<string> fields = new List<string>();
            uint nameSize;

            try
            {
                for (uint fieldIndex = 0; ; fieldIndex++)
                {
                    StringBuilder sb = new StringBuilder(Constants.MaxSymbolName);

                    Context.Symbols.GetFieldName(typedData.ModBase, typedData.TypeId, fieldIndex, sb, (uint)sb.Capacity, out nameSize);
                    fields.Add(sb.ToString());
                }
            }
            catch (Exception)
            {
            }

            return fields.ToArray();
        }

        public Variable AdjustPointer(int offset)
        {
            if (typedData.Tag != SymTag.PointerType)
            {
                throw new ArgumentException("Variable is not a pointer type, but " + typedData.Tag);
            }

            DEBUG_TYPED_DATA newTypedData = typedData;

            newTypedData.Data += (ulong)offset;
            return new Variable(newTypedData);
        }

        public Variable CastAs(DType type)
        {
            return new Variable(type.ModuleId, type.TypeId, typedData.Offset);
        }

        public Variable CastAs(string newType)
        {
            uint newTypeId = Context.Symbols.GetTypeIdWide(typedData.ModBase, newType);
            int moduleIndex = newType.IndexOf('!');
            ulong moduleId = typedData.ModBase;

            if (moduleIndex > 0)
            {
                string moduleName = newType.Substring(moduleIndex);
                uint index;

                Context.Symbols.GetModuleByModuleName(moduleName, 0, out index, out moduleId);
            }

            return new Variable(moduleId, newTypeId, typedData.Offset);
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

            return new Variable(response.OutData, name);
        }

        public int GetArrayLength()
        {
            var type = GetCodeType();

            if (!type.IsPointer || !type.IsArray)
            {
                throw new ArgumentException("Variable is not an array, but " + type.Name);
            }

            return (int)(type.Size / type.ElementType.Size);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // TODO: Implement
            return base.TryConvert(binder, out result);
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            // TODO: Implement
            return base.TryBinaryOperation(binder, arg, out result);
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            // TODO: Implement
            return base.TryUnaryOperation(binder, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetFieldNames();
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
                throw new ArgumentException("Multidimensional arrays are not supported");
            }

            try
            {
                int index = Convert.ToInt32(indexes[0]);

                if (typedData.Tag == SymTag.PointerType || typedData.Tag == SymTag.ArrayType)
                {
                    var response = Context.Advanced.Request(DebugRequest.ExtTypedDataAnsi, new EXT_TYPED_DATA()
                    {
                        Operation = ExtTdop.GetArrayElement,
                        InData = typedData,
                        In64 = (ulong)index,
                    }).OutData;

                    result = new Variable(response);
                    return true;
                }
            }
            catch (Exception)
            {
                // Index is not a number, fall back to getting member
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
