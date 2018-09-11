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
        public class LibStdCpp7 : IAny
        {
            private const string InternalManagerNameStart = "std::any::_Manager_internal<";
            private const string InternalManagerNameEnd = ">::_S_manage";
            private const string ExternalManagerNameStart = "std::any::_Manager_external<";
            private const string ExternalManagerNameEnd = ">::_S_manage";

            /// <summary>
            /// Cache of _M_storage field.
            /// </summary>
            private UserMember<Variable> storage;

            /// <summary>
            /// Cache of _M_storage._M_ptr field.
            /// </summary>
            private UserMember<Variable> pointer;

            /// <summary>
            /// Cache of _M_storage._M_buffer field.
            /// </summary>
            private UserMember<Variable> buffer;

            /// <summary>
            /// Cache of _M_manager field.
            /// </summary>
            private UserMember<Variable> manager;

            /// <summary>
            /// Cache of manager function name.
            /// </summary>
            private UserMember<string> managerName;

            /// <summary>
            /// Cache of value code type.
            /// </summary>
            private UserMember<CodeType> codeType;

            /// <summary>
            /// Initializes a new instance of the <see cref="LibStdCpp7"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public LibStdCpp7(Variable variable)
            {
                storage = UserMember.Create(() => variable.GetField("_M_storage"));
                pointer = UserMember.Create(() => storage.Value.GetField("_M_ptr"));
                buffer = UserMember.Create(() => storage.Value.GetField("_M_buffer"));
                manager = UserMember.Create(() => variable.GetField("_M_manager"));
                managerName = UserMember.Create(() =>
                {
                    Tuple<string, ulong> nameAndOffset = Context.SymbolProvider.GetSymbolNameByAddress(storage.Value.GetCodeType().Module.Process, manager.Value.GetPointerAddress());

                    if (nameAndOffset == null)
                        return null;
                    return nameAndOffset.Item1;
                });
                codeType = UserMember.Create(() =>
                {
                    string codeTypeName;

                    if (IsInternal)
                    {
                        string moduleName = managerName.Value.Substring(0, managerName.Value.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Value.Substring(moduleName.Length + InternalManagerNameStart.Length, managerName.Value.Length - moduleName.Length - InternalManagerNameStart.Length - InternalManagerNameEnd.Length);
                    }
                    else if (IsExternal)
                    {
                        string moduleName = managerName.Value.Substring(0, managerName.Value.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Value.Substring(moduleName.Length + ExternalManagerNameStart.Length, managerName.Value.Length - moduleName.Length - ExternalManagerNameStart.Length - ExternalManagerNameEnd.Length);
                    }
                    else
                        throw new NotImplementedException();

                    return CodeType.Create(codeTypeName.Trim());
                });
            }

            /// <summary>
            /// Gets the flag indicating if this instance has value set.
            /// </summary>
            public bool HasValue => !manager.Value.IsNull();

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
                        return Variable.Create(codeType.Value, buffer.Value.GetPointerAddress());
                    else if (IsExternal)
                        return Variable.Create(codeType.Value, pointer.Value.GetPointerAddress());
                    else
                        throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Checks if value is stored in internal buffer.
            /// </summary>
            internal bool IsInternal => managerName.Value?.Contains(InternalManagerNameStart) ?? false;

            /// <summary>
            /// Checks if value is stored in external buffer (allocated in pointer field).
            /// </summary>
            internal bool IsExternal => managerName.Value?.Contains(ExternalManagerNameStart) ?? false;


            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // _M_storage
                // | _M_ptr
                // | _M_buffer
                // _M_manager
                CodeType _M_storage, _M_ptr, _M_buffer, _M_manager;

                if (!codeType.GetFieldTypes().TryGetValue("_M_storage", out _M_storage) || !codeType.GetFieldTypes().TryGetValue("_M_manager", out _M_manager))
                    return false;
                if (!_M_storage.GetFieldTypes().TryGetValue("_M_ptr", out _M_ptr))
                    return false;
                if (!_M_storage.GetFieldTypes().TryGetValue("_M_buffer", out _M_buffer))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Clang libc++ implementation of std::any
        /// </summary>
        public class ClangLibCpp : IAny
        {
            private const string SmallHandlerNameStart = "std::__1::__any_imp::_SmallHandler<";
            private const string SmallHandlerNameEnd = ">::__handle";
            private const string LargeHandlerNameStart = "std::__1::__any_imp::_LargeHandler<";
            private const string LargeHandlerNameEnd = ">::__handle";

            /// <summary>
            /// Cache of __s field.
            /// </summary>
            private UserMember<Variable> storage;

            /// <summary>
            /// Cache of __s.__ptr field.
            /// </summary>
            private UserMember<Variable> pointer;

            /// <summary>
            /// Cache of __s.__buf field.
            /// </summary>
            private UserMember<Variable> buffer;

            /// <summary>
            /// Cache of __h field.
            /// </summary>
            private UserMember<Variable> manager;

            /// <summary>
            /// Cache of manager function name.
            /// </summary>
            private UserMember<string> managerName;

            /// <summary>
            /// Cache of value code type.
            /// </summary>
            private UserMember<CodeType> codeType;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClangLibCpp"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public ClangLibCpp(Variable variable)
            {
                storage = UserMember.Create(() => variable.GetField("__s"));
                pointer = UserMember.Create(() => storage.Value.GetField("__ptr"));
                buffer = UserMember.Create(() => storage.Value.GetField("__buf"));
                manager = UserMember.Create(() => variable.GetField("__h"));
                managerName = UserMember.Create(() =>
                {
                    Tuple<string, ulong> nameAndOffset = Context.SymbolProvider.GetSymbolNameByAddress(storage.Value.GetCodeType().Module.Process, manager.Value.GetPointerAddress());

                    if (nameAndOffset == null)
                        return null;
                    return nameAndOffset.Item1;
                });
                codeType = UserMember.Create(() =>
                {
                    string codeTypeName;
                    string handlerCodeTypeName;

                    if (IsSmall)
                    {
                        string moduleName = managerName.Value.Substring(0, managerName.Value.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Value.Substring(moduleName.Length + SmallHandlerNameStart.Length, managerName.Value.Length - moduleName.Length - SmallHandlerNameStart.Length - SmallHandlerNameEnd.Length);
                        handlerCodeTypeName = $"{moduleName}std::__1::__any_imp::_SmallHandler<{codeTypeName.Substring(moduleName.Length)}>";
                    }
                    else if (IsLarge)
                    {
                        string moduleName = managerName.Value.Substring(0, managerName.Value.IndexOf('!') + 1);

                        codeTypeName = moduleName + managerName.Value.Substring(moduleName.Length + LargeHandlerNameStart.Length, managerName.Value.Length - moduleName.Length - LargeHandlerNameStart.Length - LargeHandlerNameEnd.Length);
                        handlerCodeTypeName = $"{moduleName}std::__1::__any_imp::_LargeHandler<{codeTypeName.Substring(moduleName.Length)}>";
                    }
                    else
                        throw new NotImplementedException();

                    CodeType handlerCodeType = CodeType.Create(handlerCodeTypeName);

                    return handlerCodeType.TemplateArguments[0] as CodeType ?? CodeType.Create(codeTypeName.Trim());
                });
            }

            /// <summary>
            /// Gets the flag indicating if this instance has value set.
            /// </summary>
            public bool HasValue => !manager.Value.IsNull();

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
                        return Variable.Create(codeType.Value, buffer.Value.GetPointerAddress());
                    else if (IsLarge)
                        return Variable.Create(codeType.Value, pointer.Value.GetPointerAddress());
                    else
                        throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Checks if value is stored in internal buffer.
            /// </summary>
            internal bool IsSmall => managerName.Value?.Contains(SmallHandlerNameStart) ?? false;

            /// <summary>
            /// Checks if value is stored in external buffer (allocated in pointer field).
            /// </summary>
            internal bool IsLarge => managerName.Value?.Contains(LargeHandlerNameStart) ?? false;


            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
            {
                // We want to have this kind of hierarchy
                // __s
                // | __ptr
                // | __buf
                // __h
                CodeType __s, __ptr, __buf, __h;

                if (!codeType.GetFieldTypes().TryGetValue("__s", out __s) || !codeType.GetFieldTypes().TryGetValue("__h", out __h))
                    return false;
                if (!__s.GetFieldTypes().TryGetValue("__ptr", out __ptr))
                    return false;
                if (!__s.GetFieldTypes().TryGetValue("__buf", out __buf))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Microsoft Visual Studio implementation of std::any
        /// </summary>
        public class VisualStudio : IAny
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
            /// Cache of _Storage field.
            /// </summary>
            private UserMember<Variable> storage;

            /// <summary>
            /// Cache of _Storage._TrivialData field.
            /// </summary>
            private UserMember<Variable> trivialData;

            /// <summary>
            /// Cache of _Storage._SmallStorage field.
            /// </summary>
            private UserMember<Variable> smallStorage;

            /// <summary>
            /// Cache of _Storage._BigStorage field.
            /// </summary>
            private UserMember<Variable> bigStorage;

            /// <summary>
            /// Cache of _Storage._TypeData field.
            /// </summary>
            private UserMember<ulong> typeData;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisualStudio"/> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public VisualStudio(Variable variable)
            {
                storage = UserMember.Create(() => variable.GetField("_Storage"));
                trivialData = UserMember.Create(() => storage.Value.GetField("_TrivialData"));
                smallStorage = UserMember.Create(() => storage.Value.GetField("_SmallStorage"));
                bigStorage = UserMember.Create(() => storage.Value.GetField("_BigStorage"));
                typeData = UserMember.Create(() => (ulong)storage.Value.GetField("_TypeData"));
            }

            /// <summary>
            /// Gets the flag indicating if this instance has value set.
            /// </summary>
            public bool HasValue => typeData.Value != 0;

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
                            return Variable.Create(CodeType, trivialData.Value.GetPointerAddress());
                        case Representation.Small:
                            return Variable.Create(CodeType, smallStorage.Value.GetField("_Data").GetPointerAddress());
                        case Representation.Big:
                            return Variable.Create(CodeType, bigStorage.Value.GetField("_Ptr").GetPointerAddress());
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            /// <summary>
            /// Gets the address of type_info pointer (it is pointer to RTTI structure).
            /// </summary>
            private ulong TypeInfoAddress => typeData.Value & (storage.Value.GetCodeType().Module.PointerSize == 4 ? RepresentationMaskFlipped_x86 : RepresentationMaskFlipped_x64);

            /// <summary>
            /// Gets the representation type of this instance.
            /// </summary>
            private Representation StorageType => (Representation)(typeData.Value & RepresentationMask);

            /// <summary>
            /// Gets the code type of value stored in this instance.
            /// </summary>
            private CodeType CodeType
            {
                get
                {
                    Tuple<string, ulong> nameAndOffset = Context.SymbolProvider.GetSymbolNameByAddress(storage.Value.GetCodeType().Module.Process, TypeInfoAddress);

                    if (nameAndOffset == null)
                        return null;

                    string name = nameAndOffset.Item1;

                    if (!name.EndsWith(rttiSignature))
                        return null;

                    name = name.Substring(0, name.Length - rttiSignature.Length);
                    return CodeType.Create(name);
                }
            }

            /// <summary>
            /// Verifies if the specified code type is correct for this class.
            /// </summary>
            /// <param name="codeType">The code type.</param>
            internal static bool VerifyCodeType(CodeType codeType)
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
                    return false;
                if (!_Storage.GetFieldTypes().TryGetValue("_TrivialData", out _TrivialData))
                    return false;
                if (!_Storage.GetFieldTypes().TryGetValue("_SmallStorage", out _SmallStorage) || !_SmallStorage.GetFieldTypes().TryGetValue("_Data", out _Data) || !_SmallStorage.GetFieldTypes().TryGetValue("_RTTI", out _RTTI))
                    return false;
                if (!_Storage.GetFieldTypes().TryGetValue("_BigStorage", out _BigStorage) || !_BigStorage.GetFieldTypes().TryGetValue("_Padding", out _Padding) || !_BigStorage.GetFieldTypes().TryGetValue("_Ptr", out _Ptr) || !_BigStorage.GetFieldTypes().TryGetValue("_RTTI", out _RTTI2))
                    return false;
                if (!_Storage.GetFieldTypes().TryGetValue("_TypeData", out _TypeData))
                    return false;
                return true;
            }
        }

        /// <summary>
        /// The type selector
        /// </summary>
        private static TypeSelector<IAny> typeSelector = new TypeSelector<IAny>(new[]
        {
            new Tuple<Type, Func<CodeType, bool>>(typeof(VisualStudio), VisualStudio.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(LibStdCpp7), LibStdCpp7.VerifyCodeType),
            new Tuple<Type, Func<CodeType, bool>>(typeof(ClangLibCpp), ClangLibCpp.VerifyCodeType),
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
