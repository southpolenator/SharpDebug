using CsDebugScript.CodeGen.CodeWriters;
using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.CodeGen.TypeInstances;
using CsDebugScript.CodeGen.UserTypes.Members;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using DIA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Base class for any exported user type
    /// </summary>
    internal class UserType
    {
        /// <summary>
        /// Lazy cache for the <see cref="TypeName"/> property.
        /// </summary>
        private SimpleCacheStruct<string> typeNameCache;

        /// <summary>
        /// Lazy cache for the <see cref="FullTypeName"/> property.
        /// </summary>
        private SimpleCacheStruct<string> fullTypeNameCache;

        /// <summary>
        /// Lazy cache for the <see cref="ConstructorName"/> property.
        /// </summary>
        private SimpleCacheStruct<string> constructorNameCache;

        /// <summary>
        /// Lazy cache for the <see cref="Namespace"/> property.
        /// </summary>
        private SimpleCacheStruct<string> namespaceCache;

        /// <summary>
        /// Lazy cache for the <see cref="Members"/> property.
        /// </summary>
        private SimpleCacheStruct<UserTypeMember[]> membersCache;

        /// <summary>
        /// Lazy cache for the <see cref="Constructors"/> property.
        /// </summary>
        private SimpleCacheStruct<UserTypeConstructor[]> constructorsCache;

        /// <summary>
        /// Lazy cache for the <see cref="BaseClass"/> and <see cref="BaseClassOffset"/> properties.
        /// </summary>
        private SimpleCacheStruct<Tuple<TypeInstance, int>> baseClassCache;

        /// <summary>
        /// Lazy cache for the <see cref="MemoryBufferOffset"/> property.
        /// </summary>
        private SimpleCacheStruct<int> memoryBufferOffsetCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="xmlType">The XML description of the type.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        /// <param name="factory">User type factory that contains this element.</param>
        public UserType(Symbol symbol, XmlType xmlType, string nameSpace, UserTypeFactory factory)
        {
            Symbol = symbol;
            Factory = factory;
            InnerTypes = new List<UserType>();
            DerivedClasses = new HashSet<UserType>();
            typeNameCache = SimpleCache.CreateStruct(() => GetTypeName());
            fullTypeNameCache = SimpleCache.CreateStruct(() => GetFullTypeName());
            constructorNameCache = SimpleCache.CreateStruct(() => GetConstructorName());
            namespaceCache = SimpleCache.CreateStruct(() => GetNamespace(nameSpace));
            membersCache = SimpleCache.CreateStruct(() => GetMembers().ToArray());
            constructorsCache = SimpleCache.CreateStruct(() => GetConstructors().ToArray());
            baseClassCache = SimpleCache.CreateStruct(() => GetBaseClass(Symbol));
            memoryBufferOffsetCache = SimpleCache.CreateStruct(() => GetMemoryBufferOffset());
        }

        /// <summary>
        /// Gets the symbol we are generating this user type from.
        /// </summary>
        public Symbol Symbol { get; private set; }

        /// <summary>
        /// Gets the XML description of the type.
        /// </summary>
        public XmlType XmlType { get; private set; }

        /// <summary>
        /// Gets the user type factory that contains this element.
        /// </summary>
        public UserTypeFactory Factory { get; protected set; }

        /// <summary>
        /// Gets the "parent" user type where this user type is declared in.
        /// </summary>
        public UserType DeclaredInType { get; private set; }

        /// <summary>
        /// Gets the flag whether we are exporting only static data fields from this user type.
        /// </summary>
        public bool ExportOnlyStaticFields { get; internal set; }

        /// <summary>
        /// Gets the flag whether we are not exporting static data fields from this user type.
        /// </summary>
        public bool DontExportStaticFields { get; internal set; }

        /// <summary>
        /// Gets the list of types declared inside this type.
        /// </summary>
        public List<UserType> InnerTypes { get; private set; }

        /// <summary>
        /// Gets the list of derived classes.
        /// </summary>
        public HashSet<UserType> DerivedClasses { get; private set; }

        /// <summary>
        /// Gets the constructor name suffix. If type is named in such way that .NET doesn't allow it
        /// (for example same way as namespace where it is declared), code will try to append something to it to generate suitable name.
        /// If that name is already present, it will continue it search for new name.
        /// </summary>
        public string ConstructorNameSuffix { get; private set; }

        /// <summary>
        /// Gets the module where this type is declared.
        /// </summary>
        public SymbolProviders.Module Module => Symbol.Module;

        /// <summary>
        /// Gets the code naming used to generate code names.
        /// </summary>
        public ICodeNaming CodeNaming => Factory.CodeNaming;

        /// <summary>
        /// Gets all members this type will provide (constants, static fields, data fields, etc).
        /// </summary>
        public UserTypeMember[] Members => membersCache.Value;

        /// <summary>
        /// Gets all constructors this type will define.
        /// </summary>
        public UserTypeConstructor[] Constructors => constructorsCache.Value;

        /// <summary>
        /// Gets the base class type instance.
        /// </summary>
        public TypeInstance BaseClass => baseClassCache.Value.Item1;

        /// <summary>
        /// Gets the base class offset in bytes.
        /// </summary>
        public int BaseClassOffset => baseClassCache.Value.Item2;

        /// <summary>
        /// Gets the memory buffer offset done by base classes. Every base class that offsets <see cref="DotNetCodeWriter.MemoryBufferOffsetFieldName"/> makes
        /// dealing with field offsets harder, so we need to know how much wee need to return back to get this class offset.
        /// </summary>
        public int MemoryBufferOffset => memoryBufferOffsetCache.Value;

        /// <summary>
        /// Gets the constructor name (it is same as <see cref="TypeName"/>, unless it is template, for templates, we need to trim generic arguments).
        /// </summary>
        public string ConstructorName => constructorNameCache.Value;

        /// <summary>
        /// Gets the namespace this type was declared in.
        /// </summary>
        public string Namespace => namespaceCache.Value;

        /// <summary>
        /// Gets the user type name in generated code.
        /// </summary>
        public string TypeName => typeNameCache.Value;

        /// <summary>
        /// Gets the user type full name in generated code (including namespace).
        /// </summary>
        public string FullTypeName => fullTypeNameCache.Value;

        /// <summary>
        /// Gets the flag whether this user type was declared inside template type (or generics in .NET).
        /// </summary>
        public bool IsDeclaredInsideTemplate
        {
            get
            {
                for (UserType parent = this; parent != null; parent = parent.DeclaredInType)
                    if (parent is SpecializedTemplateUserType || parent is TemplateUserType)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Updates the <see cref="ConstructorNameSuffix"/> property. Invalidates all necessary caches too.
        /// </summary>
        /// <param name="suffix">New value for constructor name suffix.</param>
        public virtual void UpdateConstructorNameSuffix(string suffix)
        {
            ConstructorNameSuffix = suffix;
            constructorNameCache.InvalidateCache();
            typeNameCache.InvalidateCache();
            fullTypeNameCache.InvalidateCache();
            namespaceCache.InvalidateCache();
        }

        /// <summary>
        /// Updates the <see cref="DeclaredInType"/> property and also updates "parents" <see cref="InnerTypes"/> list.
        /// </summary>
        /// <param name="declaredInType">"parent" type where this type was declared.</param>
        public virtual void UpdateDeclaredInType(UserType declaredInType)
        {
            if (DeclaredInType != declaredInType)
            {
                if (DeclaredInType != null)
                    DeclaredInType.InnerTypes.Remove(this);
                if (declaredInType != null)
                    declaredInType.InnerTypes.Add(this);
                DeclaredInType = declaredInType;
                fullTypeNameCache.InvalidateCache();
            }
        }

        /// <summary>
        /// Function that should evaluate <see cref="TypeName"/> property.
        /// </summary>
        /// <returns>User type name.</returns>
        protected virtual string GetTypeName()
        {
            string className = Symbol.Namespaces.Last();

            className = CodeNaming.FixUserNaming(className);
            if (!string.IsNullOrEmpty(ConstructorNameSuffix))
                className += ConstructorNameSuffix;
            return className;
        }

        /// <summary>
        /// Function that should evaluate <see cref="ConstructorName"/> property.
        /// </summary>
        /// <returns>User type constructor name.</returns>
        protected virtual string GetConstructorName()
        {
            string className = Symbol?.Namespaces?.LastOrDefault() ?? TypeName;

            if (this is TemplateUserType || this is SpecializedTemplateUserType)
            {
                int index = className.IndexOf('<');

                if (index >= 0)
                    className = className.Substring(0, index);
            }
            return CodeNaming.FixUserNaming(className);
        }

        /// <summary>
        /// Function that should evaluate <see cref="FullTypeName"/> property.
        /// </summary>
        /// <returns>User type full name.</returns>
        protected virtual string GetFullTypeName()
        {
            if (DeclaredInType != null)
                return DeclaredInType.FullTypeName + "." + TypeName;
            if (!string.IsNullOrEmpty(Namespace))
                return Namespace + "." + TypeName;
            return TypeName;
        }

        /// <summary>
        /// Function that should evaluate <see cref="Namespace"/> property.
        /// </summary>
        /// <param name="constructorNamespace">Namespace parameter of the constructor of this class.</param>
        protected virtual string GetNamespace(string constructorNamespace)
        {
            return CodeNaming.FixUserNaming(constructorNamespace);
        }

        /// <summary>
        /// Function that should evaluate <see cref="Members"/> property.
        /// </summary>
        protected virtual IEnumerable<UserTypeMember> GetMembers()
        {
            // Prepare naming "deduplication" and error fixing
            HashSet<string> usedNames = new HashSet<string>();
            Func<string, string> fixName = (string nameBase) =>
            {
                string name = nameBase;

                for (int i = 0; usedNames.Contains(name); i++)
                    if (i > 0)
                        name = $"{nameBase}_{i}";
                    else
                        name = $"{nameBase}_";
                usedNames.Add(name);
                return name;
            };

            usedNames.Add(ConstructorName);
            foreach (UserType innerType in InnerTypes)
                usedNames.Add(innerType.ConstructorName);

            // Add constants and data fields
            foreach (var field in Symbol.Fields)
            {
                if (field.IsStatic && !field.IsValidStatic)
                    continue;

                if (field.IsStatic && DontExportStaticFields)
                    continue;

                if (!field.IsStatic && ExportOnlyStaticFields)
                    continue;

                string fieldName = fixName(CodeNaming.FixUserNaming(field.Name));

                if (field.LocationType == LocationType.Constant)
                    yield return new ConstantUserTypeMember()
                    {
                        AccessLevel = AccessLevel.Public,
                        Name = fieldName,
                        Type = Factory.GetSymbolTypeInstance(this, field.Type, field.BitSize),
                        Symbol = field,
                        UserType = this,
                    };
                else
                    yield return new DataFieldUserTypeMember()
                    {
                        AccessLevel = AccessLevel.Public,
                        Name = fieldName,
                        Type = Factory.GetSymbolTypeInstance(this, field.Type, field.BitSize),
                        Symbol = field,
                        UserType = this,
                    };
            }

            // Hungarian notation fields
            if (!ExportOnlyStaticFields)
                foreach (UserTypeMember member in GenerateHungarianNotationFields(fixName))
                    yield return member;

            // Base class properties
            if (!ExportOnlyStaticFields && (BaseClass is MultiClassInheritanceTypeInstance || BaseClass is SingleClassInheritanceWithInterfacesTypeInstance))
            {
                Symbol[] baseClasses = Symbol.BaseClasses;
                Symbol[] baseClassesForProperties = BaseClass is SingleClassInheritanceWithInterfacesTypeInstance ? baseClasses.Where(b => b.IsEmpty).ToArray() : baseClasses;
                List<Symbol> baseClassesSorted = baseClasses.OrderBy(s => s.Offset).ThenBy(s => s.Name).ToList();

                foreach (Symbol baseClass in baseClassesForProperties)
                {
                    // Generate simplified base class name
                    TypeInstance type = Factory.GetSymbolTypeInstance(this, baseClass);
                    string baseClassName = CodeNaming.FixUserNaming(type.GetTypeString(truncateNamespace: true));
                    StringBuilder sb = new StringBuilder();
                    int i = baseClassName[0] == '@' ? 1 : 0;

                    for (; i < baseClassName.Length; i++)
                        switch (baseClassName[i])
                        {
                            case '_':
                            case '.':
                                if (i > 0 && baseClassName[i-1] != '_')
                                    sb.Append('_');
                                break;
                            default:
                                sb.Append(baseClassName[i]);
                                break;
                        }
                    while (sb.Length > 0 && sb[sb.Length - 1] == '_')
                        sb.Length--;
                    baseClassName = sb.ToString();

                    // Create property
                    string propertyName = fixName(baseClassesForProperties.Length > 1 ? $"BaseClass_{baseClassName}" : "BaseClass");

                    yield return new BaseClassPropertyUserTypeMember()
                    {
                        AccessLevel = AccessLevel.Public,
                        Name = propertyName,
                        Type = type,
                        Symbol = baseClass,
                        UserType = this,
                        Index = baseClassesSorted.IndexOf(baseClass),
                    };
                }
            }
        }

        /// <summary>
        /// Function that should evaluate <see cref="Constructors"/> property.
        /// </summary>
        protected virtual IEnumerable<UserTypeConstructor> GetConstructors()
        {
            yield return UserTypeConstructor.Static;

            if (!ExportOnlyStaticFields)
            {
                if (this is PhysicalUserType || DerivedClasses.OfType<PhysicalUserType>().Any()
                    || DerivedClasses.Any(dc => dc.Constructors.Contains(UserTypeConstructor.SimplePhysical)))
                {
                    yield return UserTypeConstructor.RegularPhysical;
                    yield return UserTypeConstructor.ComplexPhysical;
                    yield return UserTypeConstructor.SimplePhysical;
                }
                else
                {
                    yield return UserTypeConstructor.Simple;
                }
            }
        }

        /// <summary>
        /// Function that should evaluate <see cref="BaseClass"/> and <see cref="BaseClassOffset"/> properties.
        /// </summary>
        protected virtual Tuple<TypeInstance, int> GetBaseClass(Symbol symbol)
        {
            Symbol[] baseClasses = symbol.BaseClasses;
            TypeInstance baseClass = null;
            int baseClassOffset = 0;

            // Check if we are exporting only static fields
            if (ExportOnlyStaticFields)
            {
                baseClass = new StaticClassTypeInstance(CodeNaming);
                return Tuple.Create(baseClass, baseClassOffset);
            }

            // Check if it is inheriting one of its own template arguments
            foreach (Symbol baseClassSymbol in baseClasses)
            {
                TypeInstance typeInstance = Factory.GetSymbolTypeInstance(this, baseClassSymbol);

                if (typeInstance is TemplateArgumentTypeInstance)
                {
                    baseClass = new MultiClassInheritanceTypeInstance(CodeNaming);
                    return Tuple.Create(baseClass, baseClassOffset);
                }
            }

            // Check if it is recursively inheriting itself
            foreach (Symbol baseClassSymbol in baseClasses)
            {
                UserType userType;

                if (Factory.GetUserType(baseClassSymbol, out userType))
                {
                    if (userType != this && userType is SpecializedTemplateUserType specialization)
                        userType = specialization.TemplateType;
                    if (userType == this || (this is SpecializedTemplateUserType specThis && specThis.TemplateType == userType))
                    {
                        baseClass = new MultiClassInheritanceTypeInstance(CodeNaming);
                        return Tuple.Create(baseClass, baseClassOffset);
                    }
                }
            }

            // Check if it is multi class inheritance
            if (baseClasses.Length > 1)
            {
                int emptyTypes = baseClasses.Count(t => t.IsEmpty);

                if (emptyTypes == baseClasses.Length - 1)
                {
                    UserType userType;
                    Symbol baseClassSymbol = baseClasses.First(t => !t.IsEmpty);

                    if (!baseClassSymbol.IsVirtualInheritance && Factory.GetUserType(baseClassSymbol, out userType) && !(Factory.GetSymbolTypeInstance(this, baseClassSymbol) is TemplateArgumentTypeInstance))
                    {
                        baseClassOffset = baseClassSymbol.Offset;
                        baseClass = new SingleClassInheritanceWithInterfacesTypeInstance(userType, Factory);
                        return Tuple.Create(baseClass, baseClassOffset);
                    }
                }

                baseClass = new MultiClassInheritanceTypeInstance(CodeNaming);
                return Tuple.Create(baseClass, baseClassOffset);
            }

            // Single class inheritance
            if (baseClasses.Length == 1)
            {
                // Check if base class type is virtual inheritance
                Symbol baseClassType = baseClasses[0];

                if (baseClassType.IsVirtualInheritance)
                {
                    baseClass = new MultiClassInheritanceTypeInstance(CodeNaming);
                    return Tuple.Create(baseClass, baseClassOffset);
                }

                // Check if base class type should be transformed
                UserTypeTransformation transformation = Factory.FindTransformation(baseClassType, this);

                if (transformation != null)
                {
                    baseClass = new TransformationTypeInstance(CodeNaming, transformation);
                    return Tuple.Create(baseClass, baseClassOffset);
                }

                // Try to find base class user type
                UserType baseUserType;

                if (Factory.GetUserType(baseClassType, out baseUserType))
                {
                    baseClass = UserTypeInstance.Create(baseUserType, Factory);
                    TemplateTypeInstance genericsInstance = baseClass as TemplateTypeInstance;

                    if (genericsInstance != null && !genericsInstance.CanInstantiate)
                    {
                        // We cannot instantiate the base class, so we must use UserType as the base class.
                        baseClass = new VariableTypeInstance(CodeNaming, false);
                        return Tuple.Create(baseClass, baseClassOffset);
                    }

                    baseClassOffset = baseClassType.Offset;
                    return Tuple.Create(baseClass, baseClassOffset);
                }

                // We weren't able to find base class user type. Continue the search.
                return GetBaseClass(baseClassType);
            }

            // Class doesn't inherit any type
            baseClass = new VariableTypeInstance(CodeNaming, false);
            return Tuple.Create(baseClass, baseClassOffset);
        }

        /// <summary>
        /// Function that should evaluate <see cref="MemoryBufferOffset"/> property.
        /// </summary>
        protected virtual int GetMemoryBufferOffset()
        {
            UserType baseClass = (BaseClass as UserTypeInstance)?.UserType;

            if (baseClass != null)
                return BaseClassOffset + baseClass.MemoryBufferOffset;
            return 0;
        }

        /// <summary>
        /// Try to generate fields based on the Hungarian notation used in class fields.
        /// </summary>
        /// <param name="fixName">Function that generated unique field names with desire to keep existing name.</param>
        protected virtual IEnumerable<HungarianArrayUserTypeMember> GenerateHungarianNotationFields(Func<string, string> fixName)
        {
            // TODO: Add comments to this function and expand XML documentation comment
            const string CounterPrefix = "m_c";
            const string PointerPrefix = "m_p";
            const string ArrayPrefix = "m_rg";
            SymbolField[] fields = Symbol.Fields;
            IEnumerable<SymbolField> counterFields = fields.Where(r => r.Name.StartsWith(CounterPrefix));
            Dictionary<SymbolField, SymbolField> userTypesArrays = new Dictionary<SymbolField, SymbolField>();

            foreach (SymbolField counterField in counterFields)
            {
                if (counterField.Type.BasicType != BasicType.UInt &&
                    counterField.Type.BasicType != BasicType.Int &&
                    counterField.Type.BasicType != BasicType.Long &&
                    counterField.Type.BasicType != BasicType.ULong)
                {
                    continue;
                }

                if (counterField.Type.Tag == CodeTypeTag.Enum)
                {
                    continue;
                }

                string counterNameSurfix = counterField.Name.Substring(CounterPrefix.Length);

                if (string.IsNullOrEmpty(counterNameSurfix))
                {
                    continue;
                }

                foreach (SymbolField pointerField in fields.Where(r => (r.Name.StartsWith(PointerPrefix) || r.Name.StartsWith(ArrayPrefix)) && r.Name.EndsWith(counterNameSurfix)))
                {
                    if ((counterField.IsStatic) != (pointerField.IsStatic))
                    {
                        continue;
                    }

                    if (pointerField.Type.Tag != CodeTypeTag.Pointer)
                    {
                        continue;
                    }

                    if (userTypesArrays.ContainsKey(pointerField))
                    {
                        if (userTypesArrays[pointerField].Name.Length > counterField.Name.Length)
                        {
                            continue;
                        }
                    }

                    userTypesArrays[pointerField] = counterField;
                }
            }

            foreach (var userTypeArray in userTypesArrays)
            {
                SymbolField pointerField = userTypeArray.Key;
                SymbolField counterField = userTypeArray.Value;
                TypeInstance fieldType = Factory.GetSymbolTypeInstance(this, pointerField.Type);

                if (fieldType is ArrayTypeInstance)
                    continue;

                if (fieldType is PointerTypeInstance fieldTypeCodePointer)
                    fieldType = fieldTypeCodePointer.ElementType;

                yield return new HungarianArrayUserTypeMember()
                {
                    AccessLevel = AccessLevel.Public,
                    Name = fixName(CodeNaming.FixUserNaming(pointerField.Name + "Array")),
                    Type = new ArrayTypeInstance(fieldType),
                    UserType = this,
                    PointerFieldName = pointerField.Name,
                    CounterFieldName = counterField.Name,
                };
            }
        }
    }
}
