using Dia2Lib;
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
        public UserTypeFactory(XmlTypeTransformation[] transformations)
        {
            typeTransformations = transformations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeFactory"/> class.
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        public UserTypeFactory(UserTypeFactory factory)
            : this(factory.typeTransformations)
        {
        }

        /// <summary>
        /// Look up for user type based on the specified module and type string.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="typeString">The type string.</param>
        /// <param name="userType">The found user type.</param>
        /// <returns><c>true</c> if user type was found.</returns>
        internal virtual bool GetUserType(Module module, string typeString, out UserType userType)
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
        internal bool ContainsSymbol(Module module, string typeString)
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

            if (symbol.Tag == SymTagEnum.SymTagEnum)
            {
                userType = new EnumUserType(symbol, nameSpace);
            }
            else if (symbol.Tag == SymTagEnum.SymTagExe)
            {
                userType = new GlobalsUserType(symbol, type, nameSpace);
            }
            else if (generationFlags.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
            {
                userType = new PhysicalUserType(symbol, type, nameSpace);
            }
            else
            {
                userType = new UserType(symbol, type, nameSpace);
            }

            symbol.UserType = userType;
            return userType;
        }

        /// <summary>
        /// Adds the symbols to user type factory and generates the user types.
        /// </summary>
        /// <param name="symbols">The template symbols grouped around the same template type.</param>
        /// <param name="type">The XML type description.</param>
        /// <param name="nameSpace">The namespace.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <returns>Generated user types for the specified symbols.</returns>
        internal IEnumerable<UserType> AddSymbols(IEnumerable<Symbol> symbols, XmlType type, string nameSpace, UserTypeGenerationFlags generationFlags)
        {
            if (!type.IsTemplate && symbols.Count() > 1)
                throw new Exception("Type has more than one symbol for " + type.Name);

            if (!type.IsTemplate)
            {
                yield return AddSymbol(symbols.First(), type, nameSpace, generationFlags);
            }
            else
            {
                // Bucketize template user types based on number of template arguments
                var buckets = new Dictionary<int, List<TemplateUserType>>();

                foreach (Symbol symbol in symbols)
                {
                    UserType userType = null;

                    try
                    {
                        // We want to ignore "empty" generic classes (for now)
                        if (symbol.Name == null || symbol.Size == 0)
                            continue;

                        // Generate template user type
                        TemplateUserType templateType = new TemplateUserType(symbol, type, nameSpace, this);

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
                            List<TemplateUserType> templates;

                            symbol.UserType = templateType;
                            if (!buckets.TryGetValue(templateType.AllTemplateArguments.Count, out templates))
                                buckets.Add(templateType.AllTemplateArguments.Count, templates = new List<TemplateUserType>());
                            templates.Add(templateType);
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO: Verify if we need to add this as specialization
                        if (ex.Message != "Wrongly formed template argument")
                            throw;
                    }

                    if (userType != null)
                        yield return userType;
                }

                // Add newly generated types
                foreach (List<TemplateUserType> templatesInBucket in buckets.Values)
                {
                    // TODO: Verify that all templates in the list can be described by the same class (also do check for inner-types)

                    // Sort Templates by Class Name.
                    // This removes ambiguity caused by parallel type processing.
                    //
                    List<TemplateUserType> templates = templatesInBucket.OrderBy(t => t.Symbol.Name.Count(c => c == '*'))
                        .ThenBy(t => t.Symbol.Name.Count(c => c == '<'))
                        .ThenBy(t => t.Symbol.Name).ToList();

                    // Select best suited type for template
                    TemplateUserType template = templates.First();

                    foreach (var specializedTemplate in templates)
                    {
                        var arguments = specializedTemplate.AllTemplateArguments;

                        // Check if all arguments are different
                        if (arguments.Distinct().Count() == arguments.Count())
                        {
                            // Check if all arguments are simple user type
                            bool simpleUserType = true;

                            foreach (var argument in arguments)
                            {
                                var argumentSymbol = GlobalCache.GetSymbol(argument, specializedTemplate.Module);

                                if (argumentSymbol.Tag != SymTagEnum.SymTagUDT || argumentSymbol.Name.Contains("<"))
                                {
                                    simpleUserType = false;
                                    break;
                                }
                            }

                            if (simpleUserType)
                            {
                                template = specializedTemplate;
                                break;
                            }

                            // Check if none of the arguments is template user type
                            bool noneIsTemplate = true;

                            foreach (var argument in arguments)
                            {
                                var argumentSymbol = GlobalCache.GetSymbol(argument, specializedTemplate.Module);

                                if (argumentSymbol.Tag == SymTagEnum.SymTagUDT && argumentSymbol.Name.Contains("<"))
                                {
                                    noneIsTemplate = false;
                                    break;
                                }
                            }

                            if (noneIsTemplate)
                            {
                                template = specializedTemplate;
                                continue;
                            }
                        }

                        // This one is as good as any...
                    }

                    // Move all types under the selected type
                    foreach (var specializedTemplate in templates)
                    {
                        template.SpecializedTypes.Add(specializedTemplate);
                        specializedTemplate.TemplateType = template;
                    }

                    yield return template;
                }
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
        internal IEnumerable<UserType> ProcessTypes(IEnumerable<UserType> userTypes, Dictionary<Symbol, string> symbolNamespaces)
        {
            ConcurrentBag<UserType> newTypes = new ConcurrentBag<UserType>();

            // Split user types that have static members in more than one module
            Parallel.ForEach(Partitioner.Create(userTypes), (userType) =>
            {
                if (!userType.ExportStaticFields)
                    return;

                Symbol[] symbols = GlobalCache.GetSymbolStaticFieldsSymbols(userType.Symbol).ToArray();

                if (symbols.Length == 1)
                    return;

                bool foundSameNamespace = false;

                foreach (var symbol in symbols)
                {
                    string nameSpace = symbol.Module.Namespace;

                    if (userType.Namespace != nameSpace)
                        newTypes.Add(new UserType(symbol, null, nameSpace) { ExportDynamicFields = false });
                    else
                        foundSameNamespace = true;
                }

                userType.ExportStaticFields = foundSameNamespace;
            });

            // Find parent type/namespace
            Dictionary<string, UserType> namespaceTypes = new Dictionary<string, UserType>();

            foreach (UserType userType in userTypes)
            {
                Symbol symbol = userType.Symbol;

                if (symbol.Tag != SymTagEnum.SymTagUDT && symbol.Tag != SymTagEnum.SymTagEnum)
                    continue;

                string symbolName = symbol.Name;
                List<string> namespaces = symbol.Namespaces;

                if (namespaces.Count == 1)
                {
                    // Class is not defined in namespace nor in type.
                    continue;
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

                    // Put type under exported template type (TODO: Remove this when template types start checking subtypes)
                    var templateType = namespaceUserType as TemplateUserType;

                    if (templateType != null)
                        namespaceUserType = templateType.TemplateType;
                    if (namespaceUserType == null)
                    {
                        namespaceUserType = new NamespaceUserType(new string[] { namespaces[i] }, previousNamespaceUserType == null ? symbolNamespaces[symbol] : null);
                        if (previousNamespaceUserType != null)
                            namespaceUserType.UpdateDeclaredInType(previousNamespaceUserType);
                        namespaceTypes.Add(currentNamespace, namespaceUserType);
                        newTypes.Add(namespaceUserType);
                    }

                    previousNamespaceUserType = namespaceUserType;
                }

                userType.UpdateDeclaredInType(previousNamespaceUserType);
            }

            // Update Class Name if it has duplicate with the namespace it is declared in
            foreach (UserType userType in newTypes.Concat(userTypes))
            {
                userType.ClassName = userType.OriginalClassName;
                if (userType.DeclaredInType != null && userType.OriginalClassName == userType.DeclaredInType.ClassName)
                {
                    userType.ClassName += "_";
                }

                TemplateUserType templateUserType = userType as TemplateUserType;

                if (templateUserType != null)
                {
                    foreach (UserType specializedUserType in templateUserType.SpecializedTypes)
                    {
                        specializedUserType.ClassName = userType.ClassName;
                    }
                }
            }

            // Remove duplicate types from exported template types (TODO: Remove this when template types start checking subtypes)
            foreach (UserType userType in userTypes)
            {
                TemplateUserType templateType = userType as TemplateUserType;

                if (templateType == null)
                    continue;

                HashSet<string> uniqueTypes = new HashSet<string>();

                foreach (var innerType in templateType.InnerTypes.ToArray())
                {
                    string className;

                    if (!(innerType is NamespaceUserType))
                        className = innerType.ClassName;
                    else
                        className = innerType.Namespace;
                    if (uniqueTypes.Contains(className))
                        templateType.InnerTypes.Remove(innerType);
                    else
                        uniqueTypes.Add(className);
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
                    foreach (UserType specializedUserType in templateUserType.SpecializedTypes)
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
                TemplateUserType templateUserType = baseClassUserType as TemplateUserType;

                if (templateUserType != null)
                    baseClassUserType = templateUserType.TemplateType;

                if (baseClassUserType != null)
                    baseClassUserType.AddDerivedClass(userType);
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
                    return tr.TransformType(inputType, ownerUserType.ClassName, typeConverter);
                }

                UserType userType;

                if (GetUserType(type.Module, inputType, out userType))
                {
                    return userType.NonSpecializedFullClassName;
                }

                Symbol symbol = type.Module.GetSymbol(inputType);

                if (symbol != null)
                {
                    if ((symbol.Tag == SymTagEnum.SymTagBaseType)
                        || (symbol.Tag == SymTagEnum.SymTagPointerType && symbol.ElementType.Tag == SymTagEnum.SymTagBaseType)
                        || (symbol.Tag == SymTagEnum.SymTagArrayType && symbol.ElementType.Tag == SymTagEnum.SymTagBaseType))
                    {
                        return ownerUserType.GetSymbolTypeTree(symbol, null).GetTypeString();
                    }
                }

                return "Variable";
            };

            return new UserTypeTransformation(transformation, typeConverter, ownerUserType, type);
        }
    }
}
