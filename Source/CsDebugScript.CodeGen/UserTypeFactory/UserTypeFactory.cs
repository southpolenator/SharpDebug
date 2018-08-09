using CsDebugScript.CodeGen.CodeWriters;
using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.CodeGen.TypeInstances;
using CsDebugScript.Engine;
using DIA;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class representing user type factory. It is being used to find types.
    /// </summary>
    internal class UserTypeFactory
    {
        /// <summary>
        /// The list of available type transformations
        /// </summary>
        protected XmlTypeTransformation[] typeTransformations;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeFactory"/> class.
        /// </summary>
        /// <param name="transformations">The transformations.</param>
        /// <param name="codeWriter">The code writer used to output generated code.</param>
        public UserTypeFactory(XmlTypeTransformation[] transformations, ICodeWriter codeWriter)
        {
            typeTransformations = transformations;
            CodeWriter = codeWriter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeFactory"/> class.
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        public UserTypeFactory(UserTypeFactory factory)
            : this(factory.typeTransformations, factory.CodeWriter)
        {
        }

        /// <summary>
        /// Gets the code writer that is used to output generated code.
        /// </summary>
        public ICodeWriter CodeWriter { get; private set; }

        /// <summary>
        /// Look up for user type based on the specified module and type string.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeString">The type string.</param>
        /// <param name="userType">The found user type.</param>
        /// <returns><c>true</c> if user type was found.</returns>
        internal virtual bool GetUserType(SymbolProviders.Module module, string typeString, out UserType userType)
        {
            userType = GlobalCache.GetUserType(typeString, module);
            return userType != null;
        }

        /// <summary>
        /// Look up for user type based on the specified symbol.
        /// </summary>
        /// <param name="type">The symbol.</param>
        /// <param name="userType">The found user type.</param>
        /// <returns><c>true</c> if user type was found.</returns>
        internal virtual bool GetUserType(Symbol type, out UserType userType)
        {
            userType = GlobalCache.GetUserType(type);
            if (!(userType is TemplateUserType))
                return userType != null;

            TemplateUserType specializedUserType = (TemplateUserType)userType;

            if (specializedUserType != null)
                userType = specializedUserType;
            else
            {
                // We could not find the specialized template.
                // Return null in this case.
                userType = null;
            }

            return userType != null;
        }

        /// <summary>
        /// Determines whether the factory contains the specified type.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeString">The type string.</param>
        /// <returns><c>true</c> if user type was found.</returns>
        internal bool ContainsSymbol(SymbolProviders.Module module, string typeString)
        {
            UserType userType;

            return GetUserType(module, typeString, out userType);
        }

        /// <summary>
        /// Adds the symbol to user type factory and generates the user type.
        /// </summary>
        /// <param name="symbol">The non-template symbol.</param>
        /// <param name="type">The XML type description.</param>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <returns>Generated user type for the specified symbol.</returns>
        internal UserType AddSymbol(Symbol symbol, XmlType type, string nameSpace, UserTypeGenerationFlags generationFlags)
        {
            UserType userType;

            if (symbol.Tag == CodeTypeTag.Enum)
            {
                userType = new EnumUserType(symbol, nameSpace, this);
            }
            else if (symbol.Tag == CodeTypeTag.ModuleGlobals)
            {
                userType = new GlobalsUserType(symbol, type, nameSpace, this);
            }
            else if (generationFlags.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
            {
                userType = new PhysicalUserType(symbol, type, nameSpace, this);
            }
            else
            {
                userType = new UserType(symbol, type, nameSpace, this);
            }

            symbol.UserType = userType;
            return userType;
        }

        /// <summary>
        /// Adds template symbols to user type factory and generates template user types.
        /// </summary>
        /// <param name="symbols">The template symbols grouped around the same template type.</param>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <returns>Generated user types for the specified symbols.</returns>
        internal IEnumerable<UserType> AddTemplateSymbols(IEnumerable<Symbol> symbols, string nameSpace, UserTypeGenerationFlags generationFlags)
        {
            // Bucketize template user types based on number of template arguments
            var buckets = new Dictionary<int, List<SpecializedTemplateUserType>>();

            foreach (Symbol symbol in symbols)
            {
                UserType userType = null;

                // We want to ignore "empty" generic classes (for now)
                if (symbol.Name == null || symbol.Size == 0)
                    continue;

                // Generate template user type
                SpecializedTemplateUserType templateType = new SpecializedTemplateUserType(symbol, null, nameSpace, this);

                if (!templateType.WronglyFormed)
                {
#if false // TODO: Verify if we want to use simple user type instead of template user type
                    if (templateType.AllTemplateArguments.Count == 0)
                    {
                        // Template does not have arguments that can be used by generic
                        // Make it specialized type
                        userType = this.AddSymbol(symbol, null, moduleName, generationOptions);
                    }
                    else
#endif
                    {
                        List<SpecializedTemplateUserType> templates;

                        symbol.UserType = templateType;
                        if (!buckets.TryGetValue(templateType.AllTemplateArguments.Count, out templates))
                            buckets.Add(templateType.AllTemplateArguments.Count, templates = new List<SpecializedTemplateUserType>());
                        templates.Add(templateType);
                    }
                }

                if (userType != null)
                    yield return userType;
            }

            // Add newly generated types
            foreach (List<SpecializedTemplateUserType> templatesInBucket in buckets.Values)
            {
                // TODO: Verify that all templates in the list can be described by the same class (also do check for inner-types)

                // Sort Templates by Class Name.
                // This removes ambiguity caused by parallel type processing.
                //
                List<SpecializedTemplateUserType> templates = templatesInBucket.OrderBy(t => t.Symbol.Name.Count(c => c == '*'))
                    .ThenBy(t => t.Symbol.Name.Count(c => c == '<'))
                    .ThenBy(t => t.Symbol.Name).ToList();

                // Select best suited type for template
                SpecializedTemplateUserType template = templates.First();

                foreach (var specializedTemplate in templates)
                {
                    var arguments = specializedTemplate.AllTemplateArguments;

                    // Check if all arguments are different
                    if (arguments.Distinct().Count() == arguments.Count())
                    {
                        // Check if all arguments are simple user type
                        bool simpleUserType = true;

                        foreach (var argument in arguments)
                            if ((argument.Tag != CodeTypeTag.Class && argument.Tag != CodeTypeTag.Structure && argument.Tag != CodeTypeTag.Union) || argument.Name.Contains("<"))
                            {
                                simpleUserType = false;
                                break;
                            }

                        if (simpleUserType)
                        {
                            template = specializedTemplate;
                            break;
                        }

                        // Check if none of the arguments is template user type
                        bool noneIsTemplate = true;

                        foreach (var argument in arguments)
                            if (argument != null && (argument.Tag == CodeTypeTag.Class || argument.Tag == CodeTypeTag.Structure || argument.Tag == CodeTypeTag.Union) && argument.Name.Contains("<"))
                            {
                                noneIsTemplate = false;
                                break;
                            }

                        if (noneIsTemplate)
                        {
                            template = specializedTemplate;
                            continue;
                        }
                    }

                    // This one is as good as any...
                }

                yield return new TemplateUserType(template, templates, this);
            }
        }

        /// <summary>
        /// Post process the types:
        ///  - If UserType has static members in more than one module, split it into multiple user types.
        ///  - Find parent type/namespace.
        /// </summary>
        /// <param name="userTypes">The list of user types.</param>
        /// <param name="symbolNamespaces">The symbol namespaces.</param>
        /// <returns>Newly generated user types.</returns>
        internal IEnumerable<UserType> ProcessTypes(IEnumerable<UserType> userTypes, Dictionary<Symbol, string> symbolNamespaces, string commonNamespace)
        {
            ConcurrentBag<UserType> newTypes = new ConcurrentBag<UserType>();

            // Collect all constants used by template types
            Dictionary<string, Symbol> constantsDictionary = new Dictionary<string, Symbol>();

            foreach (TemplateUserType templateType in userTypes.OfType<TemplateUserType>())
                foreach (SpecializedTemplateUserType specialization in templateType.Specializations)
                    foreach (Symbol symbol in specialization.AllTemplateArguments)
                    {
                        if (symbol.Tag != CodeTypeTag.TemplateArgumentConstant)
                            continue;
                        if (!constantsDictionary.ContainsKey(symbol.Name))
                            constantsDictionary.Add(symbol.Name, symbol);
                    }

            // Create user types for template type constants
            if (constantsDictionary.Count > 0)
            {
                // Create namespace that will contain all constants (TemplateConstants)
                NamespaceUserType templateConstants = new NamespaceUserType(new[] { "TemplateConstants" }, commonNamespace, this);

                newTypes.Add(templateConstants);

                // Foreach constant, create new user type
                foreach (Symbol symbol in constantsDictionary.Values)
                {
                    symbol.UserType = new TemplateArgumentConstantUserType(symbol, this);
                    symbol.UserType.UpdateDeclaredInType(templateConstants);
                    newTypes.Add(symbol.UserType);
                }
            }

            // Assign generated user types to template type constant arguments
            foreach (TemplateUserType templateType in userTypes.OfType<TemplateUserType>())
                foreach (SpecializedTemplateUserType specialization in templateType.Specializations)
                    foreach (Symbol symbol in specialization.AllTemplateArguments)
                    {
                        if (symbol.Tag != CodeTypeTag.TemplateArgumentConstant || symbol.UserType != null)
                            continue;
                        symbol.UserType = constantsDictionary[symbol.Name].UserType;
                    }

            // Split user types that have static members in more than one module
            Parallel.ForEach(Partitioner.Create(userTypes), (userType) =>
            {
                if (userType.DontExportStaticFields)
                    return;

                Symbol[] symbols = GlobalCache.GetSymbolStaticFieldsSymbols(userType.Symbol).ToArray();

                if (symbols.Length == 1)
                    return;

                bool foundSameNamespace = false;

                foreach (var symbol in symbols)
                {
                    string nameSpace = symbol.Module.Namespace;

                    if (userType.Namespace != nameSpace)
                        newTypes.Add(new UserType(symbol, null, nameSpace, this) { ExportOnlyStaticFields = true });
                    else
                        foundSameNamespace = true;
                }

                if (!foundSameNamespace)
                    userType.DontExportStaticFields = true;
            });

            // TODO: This needs to happen before template types creation. We need to verify that all types declared in template type are matched correctly.
            // Find parent type/namespace
            Dictionary<string, UserType> namespaceTypes = new Dictionary<string, UserType>();
            Action<UserType> findParentType = (UserType userType) =>
            {
                Symbol symbol = userType.Symbol;
                string symbolName = symbol.Name;
                List<string> namespaces = symbol.Namespaces;

                if (namespaces.Count == 1)
                {
                    // Class is not defined in namespace nor in type.
                    return;
                }

                StringBuilder currentNamespaceSB = new StringBuilder();
                UserType previousNamespaceUserType = null;

                for (int i = 0; i < namespaces.Count - 1; i++)
                {
                    if (i > 0)
                        currentNamespaceSB.Append("::");
                    currentNamespaceSB.Append(namespaces[i]);

                    string currentNamespace = currentNamespaceSB.ToString();
                    UserType namespaceUserType;

                    if (!namespaceTypes.TryGetValue(currentNamespace, out namespaceUserType))
                        namespaceUserType = GlobalCache.GetUserType(currentNamespace, symbol.Module);

                    if (namespaceUserType == null)
                    {
                        namespaceUserType = new NamespaceUserType(new string[] { namespaces[i] }, previousNamespaceUserType == null ? symbolNamespaces[symbol] : null, this);
                        if (previousNamespaceUserType != null)
                            namespaceUserType.UpdateDeclaredInType(previousNamespaceUserType);
                        namespaceTypes.Add(currentNamespace, namespaceUserType);
                        newTypes.Add(namespaceUserType);
                    }

                    previousNamespaceUserType = namespaceUserType;
                }

                userType.UpdateDeclaredInType(previousNamespaceUserType);
            };

            foreach (UserType userType in userTypes)
            {
                Symbol symbol = userType.Symbol;

                if (symbol.Tag != CodeTypeTag.Class && symbol.Tag != CodeTypeTag.Structure && symbol.Tag != CodeTypeTag.Union && symbol.Tag != CodeTypeTag.Enum)
                    continue;

                if (userType is TemplateUserType templateType)
                {
                    foreach (SpecializedTemplateUserType specializedType in templateType.Specializations)
                        findParentType(specializedType);
                    templateType.UpdateDeclaredInType(templateType.SpecializedRepresentative.DeclaredInType);

                    // TODO: Once we fix how we pick specialized representative this won't be needed. Specialized representative needs to have all inner types.
                    if (templateType.SpecializedRepresentative.DeclaredInType is TemplateUserType t)
                        templateType.UpdateDeclaredInType(t);
                    if (templateType.SpecializedRepresentative.DeclaredInType is SpecializedTemplateUserType t2)
                        templateType.UpdateDeclaredInType(t2.TemplateType.SpecializedRepresentative);
                }
                else
                    findParentType(userType);
            }

            // Update Class Name if it has duplicate with the namespace it is declared in
            foreach (UserType userType in newTypes.Concat(userTypes))
            {
                if (userType.DeclaredInType != null && userType.TypeName == userType.DeclaredInType.TypeName)
                {
                    // Since changing user type name can generate duplicate name, we need to adjust name to be unique.
                    int counter = 0;

                    do
                    {
                        if (counter > 0)
                            userType.UpdateConstructorNameSuffix($"_{counter}");
                        else
                            userType.UpdateConstructorNameSuffix("_");
                        counter++;
                    }
                    while (userType.DeclaredInType.InnerTypes.Where(t => t != userType).Any(t => t.TypeName == userType.TypeName));
                }
            }

            // Find all derived classes
            foreach (UserType userType in userTypes)
            {
                // We are doing this only for UDTs
                if (userType is EnumUserType || userType is GlobalsUserType || userType is NamespaceUserType)
                    continue;

                // For template user types, we want to remember all specializations
                TemplateUserType templateUserType = userType as TemplateUserType;

                if (templateUserType != null)
                {
                    foreach (SpecializedTemplateUserType specializedUserType in templateUserType.Specializations)
                    {
                        AddDerivedClassToBaseClasses(specializedUserType);
                    }
                }
                else
                {
                    AddDerivedClassToBaseClasses(userType);
                }
            }

            // Merge namespaces when possible
            foreach (UserType userType in newTypes)
            {
                NamespaceUserType nameSpace = userType as NamespaceUserType;

                if (nameSpace == null)
                {
                    continue;
                }

                nameSpace.MergeIfPossible();
            }

            // Remove empty namespaces after merge
            List<UserType> removedUserTypes = new List<UserType>();

            foreach (UserType userType in newTypes)
            {
                NamespaceUserType nameSpace = userType as NamespaceUserType;

                if (nameSpace == null)
                {
                    continue;
                }

                if (nameSpace.InnerTypes.Count == 0)
                {
                    removedUserTypes.Add(nameSpace);
                }
            }

            return newTypes.Except(removedUserTypes);
        }

        /// <summary>
        /// Adds the specified user type as derived class to all its base classes.
        /// </summary>
        /// <param name="userType">The user type.</param>
        private void AddDerivedClassToBaseClasses(UserType userType)
        {
            IEnumerable<Symbol> allBaseClasses = userType.Symbol.GetAllBaseClasses();

            foreach (Symbol baseClass in allBaseClasses)
            {
                UserType baseClassUserType = GlobalCache.GetUserType(baseClass);
                SpecializedTemplateUserType templateUserType = baseClassUserType as SpecializedTemplateUserType;

                if (templateUserType != null)
                    baseClassUserType = templateUserType.TemplateType;

                if (baseClassUserType != null)
                    baseClassUserType.DerivedClasses.Add(userType);
            }
        }

        /// <summary>
        /// Tries to match transformation for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="ownerUserType">The owner user type.</param>
        /// <returns>Transformation if matched one is found; otherwise null.</returns>
        internal UserTypeTransformation FindTransformation(Symbol type, UserType ownerUserType)
        {
            // Find first transformation that matches the specified type
            string originalFieldTypeString = type.Name;
            var transformation = typeTransformations.FirstOrDefault(t => t.Matches(originalFieldTypeString));

            if (transformation == null)
                return null;

            // Create type converter function for the transformation
            Func<string, string> typeConverter = null;

            typeConverter = (inputType) =>
            {
                var tr = typeTransformations.FirstOrDefault(t => t.Matches(inputType));

                if (tr != null)
                {
                    return tr.TransformType(inputType, typeConverter);
                }

                UserType userType;

                if (GetUserType(type.Module, inputType, out userType))
                {
                    return userType.FullTypeName;
                }

                Symbol symbol = type.Module.GetSymbol(inputType);

                if (symbol != null)
                {
                    if ((symbol.Tag == CodeTypeTag.BuiltinType)
                        || (symbol.Tag == CodeTypeTag.Pointer && symbol.ElementType.Tag == CodeTypeTag.BuiltinType)
                        || (symbol.Tag == CodeTypeTag.Array && symbol.ElementType.Tag == CodeTypeTag.BuiltinType))
                    {
                        return ownerUserType.Factory.GetSymbolTypeInstance(ownerUserType, symbol).GetTypeString();
                    }
                }

                return new VariableTypeInstance(CodeWriter).GetTypeString();
            };

            return new UserTypeTransformation(transformation, typeConverter, ownerUserType, type);
        }

        /// <summary>
        /// Gets the type instance for the specified symbol.
        /// </summary>
        /// <param name="parentType">The user type from which this symbol comes from (examples: field type, template type...).</param>
        /// <param name="symbol">The original type.</param>
        /// <param name="bitLength">Number of bits used for this symbol.</param>
        internal virtual TypeInstance GetSymbolTypeInstance(UserType parentType, Symbol symbol, int bitLength = 0)
        {
            switch (symbol.Tag)
            {
                case CodeTypeTag.BuiltinType:
                    if (bitLength == 1)
                        return new BasicTypeInstance(CodeWriter, typeof(bool));
                    switch (symbol.BasicType)
                    {
                        case BasicType.Bit:
                        case BasicType.Bool:
                            return new BasicTypeInstance(CodeWriter, typeof(bool));
                        case BasicType.Char:
                        case BasicType.WChar:
                        case BasicType.Char16:
                        case BasicType.Char32:
                            return new BasicTypeInstance(CodeWriter, typeof(char));
                        case BasicType.BSTR:
                            return new BasicTypeInstance(CodeWriter, typeof(string));
                        case BasicType.Void:
                        case BasicType.NoType:
                            return new BasicTypeInstance(CodeWriter, typeof(VoidType));
                        case BasicType.Float:
                            return new BasicTypeInstance(CodeWriter, symbol.Size <= 4 ? typeof(float) : typeof(double));
                        case BasicType.Int:
                        case BasicType.Long:
                            switch (symbol.Size)
                            {
                                case 0:
                                    return new BasicTypeInstance(CodeWriter, typeof(VoidType));
                                case 1:
                                    return new BasicTypeInstance(CodeWriter, typeof(sbyte));
                                case 2:
                                    return new BasicTypeInstance(CodeWriter, typeof(short));
                                case 4:
                                    return new BasicTypeInstance(CodeWriter, typeof(int));
                                case 8:
                                    return new BasicTypeInstance(CodeWriter, typeof(long));
                                default:
                                    throw new Exception($"Unexpected type length {symbol.Size}");
                            }

                        case BasicType.UInt:
                        case BasicType.ULong:
                            switch (symbol.Size)
                            {
                                case 0:
                                    return new BasicTypeInstance(CodeWriter, typeof(VoidType));
                                case 1:
                                    return new BasicTypeInstance(CodeWriter, typeof(byte));
                                case 2:
                                    return new BasicTypeInstance(CodeWriter, typeof(ushort));
                                case 4:
                                    return new BasicTypeInstance(CodeWriter, typeof(uint));
                                case 8:
                                    return new BasicTypeInstance(CodeWriter, typeof(ulong));
                                default:
                                    throw new Exception($"Unexpected type length {symbol.Size}");
                            }

                        case BasicType.Hresult:
                            return new BasicTypeInstance(CodeWriter, typeof(uint)); // TODO: Create Hresult type

                        default:
                            throw new Exception($"Unexpected basic type {symbol.BasicType}");
                    }

                case CodeTypeTag.Pointer:
                    {
                        Symbol pointerType = symbol.ElementType;
                        UserType pointerUserType;

                        // When exporting pointer from Global Modules, always export types as code pointer.
                        if (parentType is GlobalsUserType && GetUserType(pointerType, out pointerUserType))
                        {
                            return new PointerTypeInstance(UserTypeInstance.Create(pointerUserType, this));
                        }

                        TypeInstance innerType = GetSymbolTypeInstance(parentType, pointerType);

                        if (innerType is TemplateArgumentTypeInstance)
                            return new PointerTypeInstance(innerType);
                        switch (pointerType.Tag)
                        {
                            case CodeTypeTag.BuiltinType:
                            case CodeTypeTag.Enum:
                                {
                                    if ((innerType as BasicTypeInstance)?.BasicType == typeof(VoidType))
                                        return new BasicTypeInstance(CodeWriter, typeof(NakedPointer));
                                    return new PointerTypeInstance(innerType);
                                }

                            case CodeTypeTag.Class:
                            case CodeTypeTag.Structure:
                            case CodeTypeTag.Union:
                                return innerType;
                            default:
                                return new PointerTypeInstance(innerType);
                        }
                    }

                case CodeTypeTag.Enum:
                case CodeTypeTag.Class:
                case CodeTypeTag.Structure:
                case CodeTypeTag.Union:
                case CodeTypeTag.TemplateArgumentConstant:
                    {
                        // Try to apply transformation on the type
                        UserTypeTransformation transformation = FindTransformation(symbol, parentType);

                        if (transformation != null)
                        {
                            return new TransformationTypeInstance(CodeWriter, transformation);
                        }

                        // Try to find user type that represents current type
                        UserType userType;

                        if (GetUserType(symbol, out userType))
                        {
                            TypeInstance type = UserTypeInstance.Create(userType, this);
                            TemplateTypeInstance genericsTree = type as TemplateTypeInstance;

                            if (genericsTree != null && !genericsTree.CanInstantiate)
                                return new VariableTypeInstance(CodeWriter);
                            return type;
                        }

                        // We were unable to find user type. If it is enum, use its basic type
                        if (symbol.Tag == CodeTypeTag.Enum)
                        {
                            return new BasicTypeInstance(CodeWriter, EnumUserType.GetEnumBasicType(symbol));
                        }

                        // Fall-back to Variable
                        return new VariableTypeInstance(CodeWriter);
                    }

                case CodeTypeTag.Array:
                    return new ArrayTypeInstance(GetSymbolTypeInstance(parentType, symbol.ElementType));

                case CodeTypeTag.Function:
                    return new FunctionTypeInstance(CodeWriter);

                case CodeTypeTag.BaseClass:
                    {
                        symbol = symbol.Module.FindGlobalTypeWildcard(symbol.Name).Single();
                        return GetSymbolTypeInstance(parentType, symbol, bitLength);
                    }

                default:
                    throw new Exception("Unexpected type tag " + symbol.Tag);
            }
        }
    }
}
