using CsDebugScript.CodeGen.TypeTrees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents template user type. For example: MyType&lt;T&gt;
    /// </summary>
    /// <seealso cref="UserType" />
    internal class TemplateUserType : UserType
    {
        /// <summary>
        /// The list of template arguments stored as symbols
        /// </summary>
        private List<Symbol> templateArgumentsAsSymbols = new List<Symbol>();

        /// <summary>
        /// The list of template arguments stored as user types
        /// </summary>
        private List<UserType> templateArgumentsAsUserTypes = new List<UserType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateUserType" /> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="xmlType">The XML description of the type.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        /// <param name="factory">The user type factory.</param>
        public TemplateUserType(Symbol symbol, XmlType xmlType, string nameSpace, UserTypeFactory factory)
            : base(symbol, xmlType, nameSpace)
        {
            UpdateTemplateArguments(factory);
            ExportStaticFields = false;
        }

        /// <summary>
        /// Gets or sets the template type used for all specializations (SpecializedTypes).
        /// TODO: Consider using new instance of type for holding specializations (instead of choosing one like we do today).
        /// </summary>
        public TemplateUserType TemplateType { get; internal set; }

        /// <summary>
        /// Gets the list of specializations (specialized types).
        /// </summary>
        public List<TemplateUserType> SpecializedTypes { get; private set; } = new List<TemplateUserType>();

        /// <summary>
        /// Gets the full name of the class (specialized version), including namespace and "parent" type it is declared into.
        /// This specialized version of FullClassName returns it with original specialization.
        /// </summary>
        internal override string SpecializedFullClassName
        {
            get
            {
                string className = GetSpecializedStringVersion(false);

                if (DeclaredInType != null)
                {
                    UserType userType = DeclaredInType;
                    TemplateUserType templateUserType = userType as TemplateUserType;

                    if (templateUserType != null)
                    {
                        string specializationNamespace = Symbol.Namespaces[Symbol.Namespaces.Count - 2];
                        bool updated = false;

                        // Find correct specialization of the template user type
                        foreach (TemplateUserType specializationType in templateUserType.SpecializedTypes)
                        {
                            if (specializationType.Symbol.Namespaces.Last() == specializationNamespace)
                            {
                                userType = specializationType;
                                updated = true;
                                break;
                            }
                        }

#if false
                        // TODO: This doesn't help because inner types of different buckets are not correctly assigned...
                        if (!updated)
                        {
                            // Try full new search for the specialized parent type
                            List<string> namespaces = Symbol.Namespaces;
                            StringBuilder currentNamespaceSB = new StringBuilder();

                            for (int i = 0; i < namespaces.Count - 1; i++)
                            {
                                if (i > 0)
                                    currentNamespaceSB.Append("::");
                                currentNamespaceSB.Append(namespaces[i]);
                            }

                            userType = GlobalCache.GetUserType(currentNamespaceSB.ToString(), Module);
                            updated = userType != null;
                        }
#endif

                        if (!updated)
                        {
                            // There is no such specialization that we need?!?
                            // We should fall back to no arguments at all
                            throw new Exception("Specialization is not possible");
                        }
                    }

                    return string.Format("{0}.{1}", userType.SpecializedFullClassName, className);
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    return string.Format("{0}.{1}", Namespace, className);
                }

                return string.Format("{0}", className);
            }
        }

        /// <summary>
        /// Gets the full name of the class (non-specialized version), including namespace and "parent" type it is declared into.
        /// This non-specialized version of FullClassName returns it with template being trimmed to just &lt;&gt;.
        /// </summary>
        internal override string NonSpecializedFullClassName
        {
            get
            {
                string className = ClassName;

                int templateStart = className.IndexOf('<');

                if (templateStart > 0)
                    className = className.Substring(0, templateStart) + "<>";

                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.NonSpecializedFullClassName, className);
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    return string.Format("{0}.{1}", Namespace, className);
                }

                return string.Format("{0}", className);
            }
        }

        /// <summary>
        /// Updates the template arguments (symbols and user types).
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        /// <returns><c>true</c> if all template arguments are resolved as user types.</returns>
        public bool UpdateTemplateArguments(UserTypeFactory factory)
        {
            string symbolName = Symbol.Namespaces.Last();
            int templateStart = symbolName.IndexOf('<');
            bool result = true;

            templateArgumentsAsSymbols.Clear();
            templateArgumentsAsUserTypes.Clear();
            if (templateStart > 0)
            {
                // Parse template arguments
                List<string> arguments = new List<string>();

                for (int i = templateStart + 1; i < symbolName.Length && symbolName[i] != '>'; i++)
                {
                    string originalyExtractedType = XmlTypeTransformation.ExtractType(symbolName, i);
                    string extractedType = originalyExtractedType.Trim();

                    i += originalyExtractedType.Length;
                    if (string.IsNullOrEmpty(extractedType))
                    {
                        // This can happen only when list is empty
                        if (arguments.Count > 0)
                            throw new NotImplementedException("Unexpected empty template argument in symbol " + symbolName);
                        break;
                    }

                    arguments.Add(extractedType);

                    // Try to see if argument is number (constants are removed from the template arguments as they cannot be used in C#)
                    double constant;

                    if (!double.TryParse(extractedType, out constant))
                    {
                        // Check if type is existing type (symbol)
                        Symbol symbol = GlobalCache.GetSymbol(extractedType, Module);

                        if (symbol == null)
                            throw new Exception("Wrongly formed template argument");
                        templateArgumentsAsSymbols.Add(symbol);

                        // Try to get user type for the symbol
                        UserType specializationUserType = null;

                        if (!factory.GetUserType(symbol, out specializationUserType))
                        {
                            if (symbol.Tag != Dia2Lib.SymTagEnum.SymTagEnum && symbol.Tag != Dia2Lib.SymTagEnum.SymTagUDT)
                            {
                                var typeString = GetSymbolTypeTree(symbol, factory).GetTypeString();

                                specializationUserType = new TemplateArgumentUserType(typeString, symbol);
                            }
                        }

                        templateArgumentsAsUserTypes.Add(specializationUserType);
                        result = result && specializationUserType != null;
                    }
                }
            }

            // TODO: Unused types should be removed
            return result;
        }

        /// <summary>
        /// Gets the module where symbol is located.
        /// </summary>
        public Module Module
        {
            get
            {
                return Symbol.Module;
            }
        }

        /// <summary>
        /// Gets the "parent" user type where this user type is declared in.
        /// </summary>
        public override UserType DeclaredInType
        {
            get
            {
                if (this != TemplateType && TemplateType != null)
                    return TemplateType.DeclaredInType;
                return base.DeclaredInType;
            }
        }

        /// <summary>
        /// Gets the class name for this user type. Class name doesn't contain namespace.
        /// </summary>
        public override string ClassName
        {
            get
            {
                string symbolName = Symbol.Name;

                if (DeclaredInType != null)
                {
                    symbolName = Symbol.Namespaces.Last();
                }

                int templateStart = symbolName.IndexOf('<');

                if (templateStart > 0)
                {
                    symbolName = symbolName.Substring(0, templateStart);
                    if (NumberOfTemplateArguments == 1)
                    {
                        symbolName += "<" + TemplateArgumentsNameBase + ">";
                    }
                    else if (NumberOfTemplateArguments > 1)
                    {
                        symbolName += "<";
                        symbolName += string.Join(", ", Enumerable.Range(1, NumberOfTemplateArguments).Select(t => TemplateArgumentsNameBase + t));
                        symbolName += ">";
                    }
                    else
                    {
                        symbolName += "_T_";
                    }
                }

                return symbolName;
            }
        }

        /// <summary>
        /// Gets the number of template arguments.
        /// </summary>
        public int NumberOfTemplateArguments
        {
            get
            {
                return templateArgumentsAsSymbols.Count;
            }
        }

        /// <summary>
        /// Gets the template arguments as parsed strings.
        /// </summary>
        public IEnumerable<string> TemplateArguments
        {
            get
            {
                return templateArgumentsAsSymbols.Select(s => s.Name);
            }
        }

        /// <summary>
        /// Gets the template arguments as symbols.
        /// </summary>
        public IReadOnlyList<Symbol> TemplateArgumentsAsSymbols
        {
            get
            {
                return templateArgumentsAsSymbols;
            }
        }

        /// <summary>
        /// Gets the template arguments name base.
        /// For single template user type this will be T.
        /// For inner template user types, this will be Ti, Tii, etc.
        /// </summary>
        private string TemplateArgumentsNameBase
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append('T');
                UserType parent = DeclaredInType;

                while (parent != null)
                {
                    if (parent is TemplateUserType)
                        sb.Append('i');
                    parent = parent.DeclaredInType;
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the specialized string version of this template user type based on the specified types.
        /// </summary>
        /// <param name="types">The types to be used as template arguments.</param>
        public string GetSpecializedStringVersion(string[] types)
        {
            if (types.Length != NumberOfTemplateArguments)
                throw new Exception("Wrong number of generics arguments");

            // TODO: Consider using ConstructorName instead of ClassName
            // TODO: Why is this function using ClassName and one with no arguments is using FullClassName?
            string className = ClassName;
            string symbolName = className;
            int templateStart = symbolName.IndexOf('<');

            if (templateStart > 0)
            {
                symbolName = symbolName.Substring(0, templateStart);
                symbolName += "<";
                symbolName += string.Join(", ", types);
                symbolName += ">";
            }

            return symbolName;
        }

        /// <summary>
        /// Gets the specialized string version of this template user type.
        /// </summary>
        /// <param name="useFullClassName">if set to <c>true</c> FullClassName will be used when generating specialized string version. If set to <c>false</c>, ClassName will be used.</param>
        public string GetSpecializedStringVersion(bool useFullClassName = true)
        {
            string fullClassName = useFullClassName ? FullClassName : ClassName;
            string className = ClassName;
            string symbolName = className;

            int templateStart = symbolName.IndexOf('<');

            if (templateStart > 0)
            {
                IEnumerable<string> types = templateArgumentsAsUserTypes.Select(r => r.SpecializedFullClassName);

                symbolName = symbolName.Substring(0, templateStart);
                symbolName += "<";
                symbolName += string.Join(", ", types);
                symbolName += ">";
            }

            return fullClassName.Substring(0, fullClassName.Length - className.Length) + symbolName;
        }

        /// <summary>
        /// Tries to match the specified type name against template arguments.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="argumentName">The found argument name.</param>
        /// <returns><c>true</c> if template argument was matched.</returns>
        public bool TryGetTemplateArgument(string typeName, out string argumentName)
        {
            // Does it belong to our template arguments?
            int index = templateArgumentsAsSymbols.FindIndex(s => s.Name == typeName);

            if (index >= 0)
            {
                argumentName = NumberOfTemplateArguments == 1 ? TemplateArgumentsNameBase : TemplateArgumentsNameBase + (index + 1);
                return true;
            }

            // Does it belong to one of the "parent" template types?
            UserType parentType = DeclaredInType;

            while (parentType != null)
            {
                TemplateUserType templateParentType = parentType as TemplateUserType;

                if (templateParentType != null)
                    return templateParentType.TryGetTemplateArgument(typeName, out argumentName);
                parentType = parentType.DeclaredInType;
            }

            // Template argument wasn't found
            argumentName = "";
            return false;
        }

        /// <summary>
        /// Gets the type tree for the specified type (symbol).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="bitLength">Number of bits used for this symbol.</param>
        internal override TypeTree GetSymbolTypeTree(Symbol type, UserTypeFactory factory, int bitLength = 0)
        {
            return base.GetSymbolTypeTree(type, CreateFactory(factory), bitLength);
        }

        /// <summary>
        /// Gets the type tree for the base class.
        /// If class has multi inheritance, it can return MultiClassInheritanceTypeTree or SingleClassInheritanceWithInterfacesTypeTree.
        /// </summary>
        /// <param name="error">The error text writer.</param>
        /// <param name="type">The type for which we are getting base class.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="baseClassOffset">The base class offset.</param>
        protected override TypeTree GetBaseClassTypeTree(TextWriter error, Symbol type, UserTypeFactory factory, out int baseClassOffset)
        {
            TypeTree baseType = base.GetBaseClassTypeTree(error, type, CreateFactory(factory), out baseClassOffset);

            // Check if base type is template argument. It if is, export it as if it is multi class inheritance.
            UserTypeTree userBaseType = baseType as UserTypeTree;
            TemplateArgumentUserType primitiveUserType = userBaseType != null ? userBaseType.UserType as TemplateArgumentUserType : null;
            if (userBaseType != null && primitiveUserType != null)
            {
                var dict = GetGenericTypeConstraintsDictionary(factory);
                string commonBaseClass;

                if (dict.TryGetValue(primitiveUserType.ClassName, out commonBaseClass))
                    return UserTypeTree.Create(new TemplateArgumentUserType(commonBaseClass, null), factory);

                baseClassOffset = 0;
                return new MultiClassInheritanceTypeTree();
            }

            return baseType;
        }

        /// <summary>
        /// Writes the class comment on the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The current indentation.</param>
        protected override void WriteClassComment(IndentedWriter output, int indentation)
        {
            base.WriteClassComment(output, indentation);
            output.WriteLine(indentation, "// ---------------------------------------------------");
            output.WriteLine(indentation, "// Specializations of this class");
            foreach (var type in SpecializedTypes)
                output.WriteLine(indentation, "//   {0}", type.Symbol.Name);
        }

        /// <summary>
        /// Creates the user type factory based on this template user type.
        /// </summary>
        /// <param name="factory">The original user type factory.</param>
        private UserTypeFactory CreateFactory(UserTypeFactory factory)
        {
            // Check if we are trying to create factory from factory that we already created
            var templateFactory = factory as TemplateUserTypeFactory;

            if (templateFactory != null)
            {
                if (templateFactory.TemplateType != this)
                    return CreateFactory(templateFactory.OriginalFactory);

                // TODO: Verify if we want to keep existing template factory or we want to add our type too
                return templateFactory;
            }

            return new TemplateUserTypeFactory(factory, this);
        }

        private enum TypeOfSpecializationType
        {
            Unmatched,
            Anything,
            Variable,
            UserType,
        }

        /// <summary>
        /// Gets the common base types for all specializations.
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        private string[] GetCommonBaseTypesForAllSpecializations(UserTypeFactory factory)
        {
            // If we don't have specializations, we cannot continue
            if (!SpecializedTypes.Any())
            {
                return null;
            }

            // Do this for every template argument
            string[] results = new string[NumberOfTemplateArguments];

            for (int i = 0; i < NumberOfTemplateArguments; i++)
            {
                // Get all specializations for current template argument
                Symbol[] specializedSymbols = SpecializedTypes.Select(r => r.templateArgumentsAsSymbols[i]).ToArray();
                TypeOfSpecializationType specializationType = TypeOfSpecializationType.Unmatched;
                UserType commonType = null;

                foreach (Symbol type in specializedSymbols)
                {
                    // Check base type
                    if (type.Tag == Dia2Lib.SymTagEnum.SymTagBaseType || type.Tag == Dia2Lib.SymTagEnum.SymTagEnum)
                        if (type.Name != "void")
                        {
                            specializationType = TypeOfSpecializationType.Anything;
                            break;
                        }
                        else
                        {
                            specializationType = TypeOfSpecializationType.Variable;
                            continue;
                        }

                    // Check pointer, array and function types, they inherit Variable
                    if (type.Tag == Dia2Lib.SymTagEnum.SymTagPointerType || type.Tag == Dia2Lib.SymTagEnum.SymTagArrayType || type.Tag == Dia2Lib.SymTagEnum.SymTagFunctionType)
                    {
                        specializationType = TypeOfSpecializationType.Variable;
                        continue;
                    }

                    if (type.Tag != Dia2Lib.SymTagEnum.SymTagUDT)
                    {
                        throw new NotImplementedException("Unexpected symbol type " + type.Tag + ". Symbol name: " + type.Name);
                    }

                    // Check if type has user type
                    UserType userType = type.UserType;

                    if (userType == null)
                    {
                        // TODO: This shouldn't happen
                        specializationType = TypeOfSpecializationType.Variable;
                        continue;
                    }

                    if (specializationType == TypeOfSpecializationType.Variable)
                        continue;

                    // If user type is template, get parent template type (one that describes all specializations)
                    var templateType = userType as TemplateUserType;

                    if (templateType != null)
                        userType = templateType.TemplateType;

                    if (specializationType == TypeOfSpecializationType.Unmatched)
                    {
                        specializationType = TypeOfSpecializationType.UserType;
                        commonType = userType;
                        continue;
                    }

                    // Try to find common type for commonType and userType
                    var commonTypeBases = ExtractAllBaseClasses(commonType);
                    var userTypeBases = ExtractAllBaseClasses(userType);
                    bool found = false;

                    foreach (var ct in commonTypeBases)
                    {
                        foreach (var ut in userTypeBases)
                            if (ut == ct)
                            {
                                found = true;
                                commonType = ut;
                                break;
                            }

                        if (found)
                            break;
                    }

                    if (!found)
                        specializationType = TypeOfSpecializationType.Variable;
                }

                // Save result based on specialization type
                string userTypeName = null;
                var templateCommonType = commonType as TemplateUserType;

                switch (specializationType)
                {
                    case TypeOfSpecializationType.Anything:
                        userTypeName = null;
                        break;
                    case TypeOfSpecializationType.Variable:
                        userTypeName = "Variable";
                        break;
                    case TypeOfSpecializationType.UserType:
                        if (templateCommonType != null)
                        {
                            // Common specialization is template type. In order to use it, we need to take specialization from this type
                            // and not the one that engine picked up as generalization.
                            UserType templateArgumentUserType = templateArgumentsAsUserTypes[i];
                            List<UserType> baseClasses = ExtractAllBaseClasses(templateArgumentUserType);

                            foreach (UserType baseClass in baseClasses)
                            {
                                TemplateUserType templateBaseClass = baseClass as TemplateUserType;

                                if (templateBaseClass != null && templateCommonType == templateBaseClass.TemplateType)
                                {
                                    templateCommonType = templateBaseClass;
                                    break;
                                }
                            }

                            // In order to use template as specialization, we need to have all arguments coming from our template arguments.
                            // If not, we cannot trust them and should continue with the base classes.
                            var tree = new TemplateTypeTree(templateCommonType, factory);
                            bool ok = true;

                            do
                            {
                                // Check if all arguments are coming from our template arguments.
                                ok = true;
                                foreach (var args in tree.SpecializedArguments)
                                    if (args != null)
                                        foreach (var arg in args)
                                            if (!(arg is TemplateArgumentTreeType) && !(arg is UserTypeTree && ((UserTypeTree)arg).UserType is TemplateArgumentUserType))
                                            {
                                                ok = false;
                                                break;
                                            }

                                if (!ok)
                                {
                                    // Find base class that we should continue with
                                    UserType nextBaseClass = null;
                                    Symbol symbol = templateCommonType.Symbol;

                                    while (nextBaseClass == null)
                                    {
                                        if (symbol.BaseClasses == null || symbol.BaseClasses.Length == 0)
                                        {
                                            // We have finished all
                                            break;
                                        }

                                        if (symbol.BaseClasses.Length > 1)
                                        {
                                            // We cannot match common type with multi-inheritance
                                            break;
                                        }

                                        symbol = symbol.BaseClasses[0];
                                        nextBaseClass = symbol?.UserType;
                                    }

                                    // No base class, use Variable
                                    if (nextBaseClass == null)
                                    {
                                        userTypeName = "Variable";
                                        break;
                                    }
                                    else if (nextBaseClass is TemplateUserType)
                                    {
                                        // Base class is template, continue with checks
                                        templateCommonType = (TemplateUserType)nextBaseClass;
                                        tree = new TemplateTypeTree(templateCommonType, factory);
                                    }
                                    else
                                    {
                                        // Base class is not template, so we can stop testing it.
                                        userTypeName = nextBaseClass.FullClassName;
                                        break;
                                    }
                                }
                            }
                            while (!ok);

                            // All checks passed for this template user type, use it.
                            if (ok)
                                userTypeName = tree.GetTypeString();
                        }
                        else
                            userTypeName = commonType.FullClassName;
                        break;
                    case TypeOfSpecializationType.Unmatched:
                    default:
                        throw new NotImplementedException("Unexpected specialization type " + specializationType + " for template type " + ClassName);
                }

                results[i] = userTypeName;
            }

            return results;
        }

        /// <summary>
        /// Extracts all base classes for the specified user type.
        /// </summary>
        /// <param name="userType">The user type.</param>
        private static List<UserType> ExtractAllBaseClasses(UserType userType)
        {
            var userTypes = new List<UserType>();
            Symbol symbol = userType.Symbol;

            userTypes.Add(userType);
            while (symbol != null)
            {
                var baseClasses = symbol.BaseClasses;

                if (baseClasses == null || baseClasses.Length == 0)
                {
                    // We have finished all
                    break;
                }

                if (baseClasses.Length > 1)
                {
                    // We cannot match common type with multi-inheritance
                    break;
                }

                symbol = baseClasses[0];
                userType = symbol?.UserType;
                if (userType != null)
                    userTypes.Add(userType);
            }

            return userTypes;
        }

        /// <summary>
        /// Gets the dictionary of generic type constraints per template argument (that has constraint).
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        private Dictionary<string, string> GetGenericTypeConstraintsDictionary(UserTypeFactory factory)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
#if false
            string[] commonBaseSpecializationTypes = GetCommonBaseTypesForAllSpecializations(factory);

            if (commonBaseSpecializationTypes == null || commonBaseSpecializationTypes.All(r => string.IsNullOrEmpty(r)))
#endif
            {
                // no restrictions
                return result;
            }

#if false
            StringBuilder sb = new StringBuilder();
            if (commonBaseSpecializationTypes.Count() == 1)
                result.Add(TemplateArgumentsNameBase, commonBaseSpecializationTypes[0]);
            else
                for (int i = 0; i < commonBaseSpecializationTypes.Count(); i++)
                    if (!string.IsNullOrEmpty(commonBaseSpecializationTypes[i]))
                        result.Add(string.Format("{0}{1}", TemplateArgumentsNameBase, i + 1), commonBaseSpecializationTypes[i]);
            return result;
#endif
        }

        /// <summary>
        /// Gets the list of generic type constraints.
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        protected override IEnumerable<string> GetGenericTypeConstraints(UserTypeFactory factory)
        {
            var dict = GetGenericTypeConstraintsDictionary(CreateFactory(factory));

            return dict.Select(t => string.Format("where {0} : {1}", t.Key, t.Value));
        }

        /// <summary>
        /// Gets the type tree for the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="extractingBaseClass">if set to <c>true</c> user type field is being generated for getting base class.</param>
        /// <param name="bitLength">Number of bits used for this symbol.</param>
        protected override TypeTree GetFieldTypeTree(SymbolField field, UserTypeFactory factory, bool extractingBaseClass, int bitLength = 0)
        {
            // Do not match specializations when getting type for base class.
            if (extractingBaseClass || NumberOfTemplateArguments == 0)
                return GetSymbolTypeTree(field.Type, factory, bitLength);

            // Check field in all specializations
            var specializedFields = SpecializedTypes.Select(r => new Tuple<TemplateUserType, SymbolField>(r, r.Symbol.Fields.FirstOrDefault(q => q.Name == field.Name))).ToArray();

            if (specializedFields.Any(r => r.Item2 == null))
            {
                // TODO: Incorrect bucketization. Field does not exist in all specialization.
                return GetSymbolTypeTree(field.Type, factory, bitLength);
            }

            if (specializedFields.All(r => r.Item2.Type.Name == field.Type.Name))
            {
                // There is no specialization, all types across the specializations are the same.
                return GetSymbolTypeTree(field.Type, factory, bitLength);
            }

            // Try to get type tree
            TypeTree result = GetSymbolTypeTree(field.Type, factory, bitLength);

            if (result is BasicTypeTree)
            {
                // Basic type tree is not challenged against template arguments, so try to do that.
                UserType basicUserType;

                if (CreateFactory(factory).GetUserType(field.Type, out basicUserType))
                {
                    TypeTree tree = UserTypeTree.Create(basicUserType, factory);

                    if (tree != null)
                    {
                        return tree;
                    }
                }

                // Failed to match the type
                // TODO: Look for typedeclared. Class is using different types than in template specialization. We cannot support it right now.
                return new VariableTypeTree();
            }

            return result;
        }
    }
}
