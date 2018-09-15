using CsDebugScript.Engine;
using CsDebugScript.Exceptions;
using System;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Implementation of std::any
    /// </summary>
    [UserType(TypeName = "std::any")]
    [UserType(TypeName = "std::__1::any")]
    public class any : UserType
    {
        private interface IAny
        {
            /// <summary>
            /// Gets the flag indicating if this instance has value set.
            /// </summary>
            bool HasValue { get; }

            /// <summary>
            /// Gets the value stored in this instance. If <see cref="HasValue"/> is <c>false</c> this will be <c>null</c>.
            /// </summary>
            Variable Value { get; }
        }

        /// <summary>
        /// libstdc++ 7 implementation of std::any
        /// </summary>
        internal class LibStdCpp7 : IAny
        {
            private const string InternalManagerNameStart = "std::any::_Manager_internal<";
            private const string InternalManagerNameEnd = ">::_S_manage";
            private const string ExternalManagerNameStart = "std::any::_Manager_external<";
            private const string ExternalManagerNameEnd = ">::_S_manage";

            /// <summary>
            /// Code type extracted data
            /// </summary>
            private class ExtractedData
            {
                /// <summary>
                /// Offset of manager pointer field.
                /// </summary>
                public int ManagerPointerOffset;

                /// <summary>
                /// Offset of data pointer field.
                /// </summary>
                public int DataPointerOffset;

                /// <summary>
                /// Offset of buffer field.
                /// </summary>
                public int BufferOffset;

                /// <summary>
                /// Code type of std::any.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// Name of the manager function.
            /// </summary>
            private string managerName;

            /// <summary>
            /// Value code type.
            /// </summary>
            private CodeType valueCodeType;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp7"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public LibStdCpp7(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                ulong managerAddress = data.Process.ReadPointer(address + (uint)data.ManagerPointerOffset);
                if (managerAddress != 0)
                {
                    Tuple<string, ulong> nameAndOffset = Context.SymbolProvider.GetSymbolNameByAddress(data.Process, managerAddress);

                    if (nameAndOffset != null)
                        managerName = nameAndOffset.Item1;
                }
                if (HasValue)
                {
                    string codeTypeName;

                    if (IsInternal)
                    {
                        string moduleName = managerName.Substring(0, managerName.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Substring(moduleName.Length + InternalManagerNameStart.Length, managerName.Length - moduleName.Length - InternalManagerNameStart.Length - InternalManagerNameEnd.Length);
                    }
                    else if (IsExternal)
                    {
                        string moduleName = managerName.Substring(0, managerName.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Substring(moduleName.Length + ExternalManagerNameStart.Length, managerName.Length - moduleName.Length - ExternalManagerNameStart.Length - ExternalManagerNameEnd.Length);
                    }
                    else
                        throw new NotImplementedException();
                    valueCodeType = CodeType.Create(codeTypeName.Trim());
                }
            }

            /// <summary>
            /// Gets the flag indicating if this instance has value set.
            /// </summary>
            public bool HasValue => managerName != null;

            /// <summary>
            /// Gets the value stored in this instance. If <see cref="HasValue"/> is <c>false</c> this will be <c>null</c>.
            /// </summary>
            public Variable Value
            {
                get
                {
                    if (!HasValue)
                        return null;

                    if (IsInternal)
                        return Variable.Create(valueCodeType, address + (uint)data.BufferOffset);
                    else if (IsExternal)
                        return Variable.Create(valueCodeType, data.Process.ReadPointer(address + (uint)data.DataPointerOffset));
                    else
                        throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Checks if value is stored in internal buffer.
            /// </summary>
            internal bool IsInternal => managerName?.Contains(InternalManagerNameStart) ?? false;

            /// <summary>
            /// Checks if value is stored in external buffer (allocated in pointer field).
            /// </summary>
            internal bool IsExternal => managerName?.Contains(ExternalManagerNameStart) ?? false;

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _M_storage
                // | _M_ptr
                // | _M_buffer
                // _M_manager
                CodeType _M_storage, _M_ptr, _M_buffer, _M_manager;

                if (!codeType.GetFieldTypes().TryGetValue("_M_storage", out _M_storage) || !codeType.GetFieldTypes().TryGetValue("_M_manager", out _M_manager))
                    return null;
                if (!_M_storage.GetFieldTypes().TryGetValue("_M_ptr", out _M_ptr))
                    return null;
                if (!_M_storage.GetFieldTypes().TryGetValue("_M_buffer", out _M_buffer))
                    return null;
                return new ExtractedData
                {
                    ManagerPointerOffset = codeType.GetFieldOffset("_M_manager"),
                    DataPointerOffset = codeType.GetFieldOffset("_M_storage") + _M_storage.GetFieldOffset("_M_ptr"),
                    BufferOffset = codeType.GetFieldOffset("_M_storage") + _M_storage.GetFieldOffset("_M_buffer"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Clang libc++ implementation of std::any
        /// </summary>
        internal class ClangLibCpp : IAny
        {
            private const string SmallHandlerNameStart = "std::__1::__any_imp::_SmallHandler<";
            private const string SmallHandlerNameEnd = ">::__handle";
            private const string LargeHandlerNameStart = "std::__1::__any_imp::_LargeHandler<";
            private const string LargeHandlerNameEnd = ">::__handle";

            /// <summary>
            /// Code type extracted data
            /// </summary>
            private class ExtractedData
            {
                /// <summary>
                /// Offset of manager pointer field.
                /// </summary>
                public int ManagerPointerOffset;

                /// <summary>
                /// Offset of data pointer field.
                /// </summary>
                public int DataPointerOffset;

                /// <summary>
                /// Offset of buffer field.
                /// </summary>
                public int BufferOffset;

                /// <summary>
                /// Code type of std::any.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// Name of the manager function.
            /// </summary>
            private string managerName;

            /// <summary>
            /// Value code type.
            /// </summary>
            private CodeType valueCodeType;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClangLibCpp"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public ClangLibCpp(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                ulong managerAddress = data.Process.ReadPointer(address + (uint)data.ManagerPointerOffset);
                if (managerAddress != 0)
                {
                    Tuple<string, ulong> nameAndOffset = Context.SymbolProvider.GetSymbolNameByAddress(data.Process, managerAddress);

                    if (nameAndOffset != null)
                        managerName = nameAndOffset.Item1;
                }
                if (HasValue)
                {
                    string codeTypeName;
                    string handlerCodeTypeName;

                    if (IsSmall)
                    {
                        string moduleName = managerName.Substring(0, managerName.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Substring(moduleName.Length + SmallHandlerNameStart.Length, managerName.Length - moduleName.Length - SmallHandlerNameStart.Length - SmallHandlerNameEnd.Length);
                        handlerCodeTypeName = $"{moduleName}std::__1::__any_imp::_SmallHandler<{codeTypeName.Substring(moduleName.Length)}>";
                    }
                    else if (IsLarge)
                    {
                        string moduleName = managerName.Substring(0, managerName.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Substring(moduleName.Length + LargeHandlerNameStart.Length, managerName.Length - moduleName.Length - LargeHandlerNameStart.Length - LargeHandlerNameEnd.Length);
                        handlerCodeTypeName = $"{moduleName}std::__1::__any_imp::_LargeHandler<{codeTypeName.Substring(moduleName.Length)}>";
                    }
                    else
                        throw new NotImplementedException();

                    CodeType handlerCodeType = CodeType.Create(handlerCodeTypeName);

                    valueCodeType = handlerCodeType.TemplateArguments[0] as CodeType ?? CodeType.Create(codeTypeName.Trim());
                }
            }

            /// <summary>
            /// Gets the flag indicating if this instance has value set.
            /// </summary>
            public bool HasValue => managerName != null;

            /// <summary>
            /// Gets the value stored in this instance. If <see cref="HasValue"/> is <c>false</c> this will be <c>null</c>.
            /// </summary>
            public Variable Value
            {
                get
                {
                    if (!HasValue)
                        return null;

                    if (IsSmall)
                        return Variable.Create(valueCodeType, address + (uint)data.BufferOffset);
                    else if (IsLarge)
                        return Variable.Create(valueCodeType, data.Process.ReadPointer(address + (uint)data.DataPointerOffset));
                    else
                        throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Checks if value is stored in internal buffer.
            /// </summary>
            internal bool IsSmall => managerName?.Contains(SmallHandlerNameStart) ?? false;

            /// <summary>
            /// Checks if value is stored in external buffer (allocated in pointer field).
            /// </summary>
            internal bool IsLarge => managerName?.Contains(LargeHandlerNameStart) ?? false;


            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // __s
                // | __ptr
                // | __buf
                // __h
                CodeType __s, __ptr, __buf, __h;

                if (!codeType.GetFieldTypes().TryGetValue("__s", out __s) || !codeType.GetFieldTypes().TryGetValue("__h", out __h))
                    return null;
                if (!__s.GetFieldTypes().TryGetValue("__ptr", out __ptr))
                    return null;
                if (!__s.GetFieldTypes().TryGetValue("__buf", out __buf))
                    return null;
                return new ExtractedData
                {
                    ManagerPointerOffset = codeType.GetFieldOffset("__h"),
                    DataPointerOffset = codeType.GetFieldOffset("__s") + __s.GetFieldOffset("__ptr"),
                    BufferOffset = codeType.GetFieldOffset("__s") + __s.GetFieldOffset("__buf"),
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// Microsoft Visual Studio implementation of std::any
        /// </summary>
        internal class VisualStudio : IAny
        {
            /// <summary>
            /// Bit mask used to extract <see cref="Representation"/> from <see cref="typeData"/>.
            /// </summary>
            private const ulong RepresentationMask = 3;

            /// <summary>
            /// Bit mask used to extract <see cref="TypeInfoAddress"/> from <see cref="typeData"/> for 32bit builds.
            /// </summary>
            private const ulong RepresentationMaskFlipped_x86 = 0xFFFFFFFC;

            /// <summary>
            /// Bit mask used to extract <see cref="TypeInfoAddress"/> from <see cref="typeData"/> for 64bit builds.
            /// </summary>
            private const ulong RepresentationMaskFlipped_x64 = 0xFFFFFFFFFFFFFFFC;

            /// <summary>
            /// RTTI signature used during extraction of <see cref="CodeType"/> from type_info name.
            /// </summary>
            private const string rttiSignature = " `RTTI Type Descriptor'";

            /// <summary>
            /// Internal representation type of std::any.
            /// </summary>
            private enum Representation
            {
                Trivial,
                Big,
                Small,
            }

            /// <summary>
            /// Code type extracted data
            /// </summary>
            private class ExtractedData
            {
                /// <summary>
                /// Offset of trivial data structure.
                /// </summary>
                public int TrivialDataOffset;

                /// <summary>
                /// Offset of small storage data structure.
                /// </summary>
                public int SmallStorageDataOffset;

                /// <summary>
                /// Offset of big storage pointer data field.
                /// </summary>
                public int BigStoragePointerOffset;

                /// <summary>
                /// Offset of type data field.
                /// </summary>
                public int TypeDataOffset;

                /// <summary>
                /// Representation mask constant.
                /// </summary>
                public ulong RepresentationMaskFlipped;

                /// <summary>
                /// Code type of std::any.
                /// </summary>
                public CodeType CodeType;

                /// <summary>
                /// Process where code type comes from.
                /// </summary>
                public Process Process;
            }

            /// <summary>
            /// Code type extracted data.
            /// </summary>
            private ExtractedData data;

            /// <summary>
            /// Address of variable.
            /// </summary>
            private ulong address;

            /// <summary>
            /// TypeData field value.
            /// </summary>
            private ulong typeData;

            /// <summary>
            /// Value code type.
            /// </summary>
            private CodeType valueCodeType;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            /// <param name="savedData">Data returned from VerifyCodeType function.</param>
            public VisualStudio(Variable variable, object savedData)
            {
                data = (ExtractedData)savedData;
                address = variable.GetPointerAddress();
                typeData = data.Process.ReadPointer(address + (uint)data.TypeDataOffset);
                if (HasValue)
                {
                    Tuple<string, ulong> nameAndOffset = Context.SymbolProvider.GetSymbolNameByAddress(data.Process, TypeInfoAddress);

                    if (nameAndOffset != null)
                    {
                        string name = nameAndOffset.Item1;

                        if (name.EndsWith(rttiSignature))
                        {
                            name = name.Substring(0, name.Length - rttiSignature.Length);
                            valueCodeType = CodeType.Create(name);
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the flag indicating if this instance has value set.
            /// </summary>
            public bool HasValue => typeData != 0;

            /// <summary>
            /// Gets the value stored in this instance. If <see cref="HasValue"/> is <c>false</c> this will be <c>null</c>.
            /// </summary>
            public Variable Value
            {
                get
                {
                    if (!HasValue)
                        return null;
                    switch (StorageType)
                    {
                        case Representation.Trivial:
                            return Variable.Create(valueCodeType, address + (uint)data.TrivialDataOffset);
                        case Representation.Small:
                            return Variable.Create(valueCodeType, address + (uint)data.SmallStorageDataOffset);
                        case Representation.Big:
                            return Variable.Create(valueCodeType, data.Process.ReadPointer(address + (uint)data.BigStoragePointerOffset));
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            /// <summary>
            /// Gets the address of type_info pointer (it is pointer to RTTI structure).
            /// </summary>
            private ulong TypeInfoAddress => typeData & data.RepresentationMaskFlipped;

            /// <summary>
            /// Gets the representation type of this instance.
            /// </summary>
            private Representation StorageType => (Representation)(typeData & RepresentationMask);

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            /// <returns>Extracted data object or <c>null</c> if fails.</returns>
            internal static object VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _Storage
                // | _TrivialData
                // | _SmallStorage
                //   | _Data
                //   | _RTTI
                // | _BigStorage
                //   | _Padding
                //   | _Ptr
                //   | _RTTI
                // | _TypeData
                // _Dummy
                CodeType _Storage, _TrivialData, _SmallStorage, _Data, _RTTI, _BigStorage, _Padding, _Ptr, _RTTI2, _TypeData, _Dummy;

                if (!codeType.GetFieldTypes().TryGetValue("_Storage", out _Storage) || !codeType.GetFieldTypes().TryGetValue("_Dummy", out _Dummy))
                    return null;
                if (!_Storage.GetFieldTypes().TryGetValue("_TrivialData", out _TrivialData))
                    return null;
                if (!_Storage.GetFieldTypes().TryGetValue("_SmallStorage", out _SmallStorage) || !_SmallStorage.GetFieldTypes().TryGetValue("_Data", out _Data) || !_SmallStorage.GetFieldTypes().TryGetValue("_RTTI", out _RTTI))
                    return null;
                if (!_Storage.GetFieldTypes().TryGetValue("_BigStorage", out _BigStorage) || !_BigStorage.GetFieldTypes().TryGetValue("_Padding", out _Padding) || !_BigStorage.GetFieldTypes().TryGetValue("_Ptr", out _Ptr) || !_BigStorage.GetFieldTypes().TryGetValue("_RTTI", out _RTTI2))
                    return null;
                if (!_Storage.GetFieldTypes().TryGetValue("_TypeData", out _TypeData))
                    return null;
                return new ExtractedData()
                {
                    BigStoragePointerOffset = codeType.GetFieldOffset("_Storage") + _Storage.GetFieldOffset("_BigStorage") + _BigStorage.GetFieldOffset("_Ptr"),
                    SmallStorageDataOffset = codeType.GetFieldOffset("_Storage") + _Storage.GetFieldOffset("_SmallStorage") + _SmallStorage.GetFieldOffset("_Data"),
                    TrivialDataOffset = codeType.GetFieldOffset("_Storage") + _Storage.GetFieldOffset("_TrivialData"),
                    TypeDataOffset = codeType.GetFieldOffset("_Storage") + _Storage.GetFieldOffset("_TypeData"),
                    RepresentationMaskFlipped = codeType.Module.PointerSize == 4 ? RepresentationMaskFlipped_x86 : RepresentationMaskFlipped_x64,
                    CodeType = codeType,
                    Process = codeType.Module.Process,
                };
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IAny> typeSelector = new TypeSelector<IAny>(new[]
        {
            new Tuple<Type, Func<CodeType, object>>(typeof(VisualStudio), VisualStudio.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(LibStdCpp7), LibStdCpp7.VerifyCodeType),
            new Tuple<Type, Func<CodeType, object>>(typeof(ClangLibCpp), ClangLibCpp.VerifyCodeType),
        });

        /// <summary>
        /// The instance used to read variable data
        /// </summary>
        private IAny instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="any"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::any</exception>
        public any(Variable variable)
            : base(variable)
        {
            instance = typeSelector.SelectType(variable);
            if (instance == null)
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::any");
            }
        }

        /// <summary>
        /// Gets the flag indicating if this instance has value set.
        /// </summary>
        public bool HasValue => instance.HasValue;

        /// <summary>
        /// Gets the value stored in this instance. If <see cref="HasValue"/> is <c>false</c> this will be <c>null</c>.
        /// </summary>
        [ForceDefaultVisualizerAtttribute]
        public Variable Value => instance.Value;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!HasValue)
                return "empty";
            return Value.ToString();
        }
    }
}
