using Dia2Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb.UserTypes
{
    class UserTypeFactory
    {
        protected XmlTypeTransformation[] typeTransformations;

        public UserTypeFactory(XmlTypeTransformation[] transformations)
        {
            typeTransformations = transformations;
        }

        public UserTypeFactory(UserTypeFactory factory)
            : this(factory.typeTransformations)
        {
        }

        internal virtual bool TryGetUserType(Module module, string typeString, out UserType userType)
        {
            userType = GlobalCache.GetUserType(typeString, module);
            return userType != null;
        }

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

        internal UserType AddSymbol(Symbol symbol, XmlType type, string nameSpace, UserTypeGenerationFlags generationOptions)
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
            else if (generationOptions.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
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

        internal IEnumerable<UserType> AddSymbols(IEnumerable<Symbol> symbols, XmlType type, string nameSpace, UserTypeGenerationFlags generationOptions)
        {
            if (!type.IsTemplate && symbols.Any())
                throw new Exception("Type has more than one symbol for " + type.Name);

            if (!type.IsTemplate)
            {
                yield return AddSymbol(symbols.First(), type, nameSpace, generationOptions);
            }
            else
            {
                var buckets = new Dictionary<int, List<TemplateUserType>>();

                foreach (Symbol symbol in symbols)
                {
                    UserType userType = null;

                    try
                    {
                        // We want to ignore "empty" generic classes (for now)
                        if (symbol.Name == null || symbol.Size == 0)
                            continue;

                        TemplateUserType templateType = new TemplateUserType(symbol, type, nameSpace, this);

                        int templateArgs = templateType.GenericsArguments;

#if false // TODO: Check if we want to use simple user type instead of template user type
                        if (templateArgs == 0)
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
                            if (!buckets.TryGetValue(templateArgs, out templates))
                                buckets.Add(templateArgs, templates = new List<TemplateUserType>());
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
                foreach (var templates in buckets.Values)
                {
                    // TODO: Verify that all templates in the list can be described by the same class (also check for subtypes)

                    // Move all types under the first type
                    TemplateUserType template = templates.First();

                    foreach (var specializedTemplate in templates)
                    {
                        template.specializedTypes.Add(specializedTemplate);
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
        /// <param name="userTypes">The user types.</param>
        /// <param name="symbolNamespaces">The symbol namespaces.</param>
        /// <returns></returns>
        internal IEnumerable<UserType> ProcessTypes(IEnumerable<UserType> userTypes, Dictionary<Symbol, string> symbolNamespaces)
        {
            ConcurrentBag<UserType> newTypes = new ConcurrentBag<UserType>();

            // Split user types that have static members in more than one module
            Parallel.ForEach(Partitioner.Create(userTypes), (userType) =>
            {
                if (!userType.ExportStaticFields)
                    return;

                SymbolField[] staticMembers = GlobalCache.GetSymbolStaticFields(userType.Symbol).ToArray();
                HashSet<Symbol> symbols = new HashSet<Symbol>();

                foreach (var field in staticMembers)
                    symbols.Add(field.ParentType);

                if (symbols.Count == 1)
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
                            namespaceUserType.SetDeclaredInType(previousNamespaceUserType);
                        namespaceTypes.Add(currentNamespace, namespaceUserType);
                        newTypes.Add(namespaceUserType);
                    }

                    previousNamespaceUserType = namespaceUserType;
                }

                userType.SetDeclaredInType(previousNamespaceUserType);
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

            return newTypes;
        }

        internal UserTypeTransformation FindTransformation(Symbol type, UserType ownerUserType)
        {
            string originalFieldTypeString = type.Name;
            var transformation = typeTransformations.Where(t => t.Matches(originalFieldTypeString)).FirstOrDefault();

            if (transformation == null)
                return null;

            Func<string, string> typeConverter = null;

            typeConverter = (inputType) =>
            {
                UserType userType;

                if (TryGetUserType(type.Module, inputType, out userType))
                {
                    return userType.FullClassName;
                }

                var tr = typeTransformations.Where(t => t.Matches(inputType)).FirstOrDefault();

                if (tr != null)
                {
                    return tr.TransformType(inputType, ownerUserType.ClassName, typeConverter);
                }

                return "Variable";
            };

            return new UserTypeTransformation(transformation, typeConverter, ownerUserType, type);
        }

        internal bool ContainsSymbol(Module module, string typeString)
        {
            UserType userType;

            return TryGetUserType(module, typeString, out userType);
        }
    }
}
