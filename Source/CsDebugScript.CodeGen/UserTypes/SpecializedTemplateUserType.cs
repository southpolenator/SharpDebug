using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsDebugScript.CodeGen.SymbolProviders;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// User type that represents specialization of template user type. For example: MyType&lt;int&gt;
    /// </summary>
    /// <seealso cref="UserType" />
    internal class SpecializedTemplateUserType : UserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecializedTemplateUserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="xmlType">The XML description of the type.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        /// <param name="factory">User type factory that contains this element.</param>
        public SpecializedTemplateUserType(Symbol symbol, XmlType xmlType, string nameSpace, UserTypeFactory factory)
            : base(symbol, xmlType, nameSpace, factory)
        {
            Factory = CreateFactory(factory);
            OriginalFactory = factory;

            // Enumerate all template arguments as strings
            List<Symbol> allTemplateArguments = new List<Symbol>();
            List<Symbol> templateArgumentsAsSymbols = new List<Symbol>();

            for (int i = 0; i < Symbol.Namespaces.Count - 1; i++)
                if (!ParseTemplateArguments(Module, Symbol.Namespaces[i], allTemplateArguments))
                {
                    WronglyFormed = true;
                    break;
                }
            if (!WronglyFormed)
            {
                WronglyFormed = !ParseTemplateArguments(Module, Symbol.Namespaces.Last(), templateArgumentsAsSymbols);
                allTemplateArguments.AddRange(templateArgumentsAsSymbols);
            }
            if (!WronglyFormed)
            {
                AllTemplateArguments = allTemplateArguments.Select(s => s.Name).ToList();
                TemplateArgumentsAsSymbols = templateArgumentsAsSymbols;
            }
        }

        /// <summary>
        /// Gets the flag representing if this specialized template type can be formed out of user types or something is unknown
        /// and <see cref="Variable"/> needs to be used in its arguments.
        /// </summary>
        public bool WronglyFormed { get; private set; }

        /// <summary>
        /// Gets the original user type factory that contains this type.
        /// </summary>
        public UserTypeFactory OriginalFactory { get; private set; }

        /// <summary>
        /// Template user type which is generalization of this user type.
        /// </summary>
        public TemplateUserType TemplateType { get; internal set; }

        /// <summary>
        /// Gets the list of template arguments parsed as symbols (<see cref="Symbol"/>).
        /// </summary>
        public IReadOnlyList<Symbol> TemplateArgumentsAsSymbols { get; private set; }

        /// <summary>
        /// Gets the list of all parsed template arguments (including "parent" user types) as strings.
        /// </summary>
        public IReadOnlyList<string> AllTemplateArguments { get; private set; }

        /// <summary>
        /// Gets the number of template arguments.
        /// </summary>
        public int NumberOfTemplateArguments => TemplateArgumentsAsSymbols.Count;

        /// <summary>
        /// Tries to match the specified type name against template arguments.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="argumentName">The found argument name.</param>
        /// <returns><c>true</c> if template argument was matched.</returns>
        public bool TryGetTemplateArgument(string typeName, out string argumentName)
        {
            // Does it belong to our template arguments?
            int index = ((List<Symbol>)TemplateArgumentsAsSymbols).FindIndex(s => s.Name == typeName);

            if (index >= 0)
            {
                argumentName = GetTemplateArgumentName(index);
                return true;
            }

            // Does it belong to one of the "parent" template types?
            UserType parentType = DeclaredInType;

            while (parentType != null)
            {
                SpecializedTemplateUserType templateParentType = parentType as SpecializedTemplateUserType;

                if (templateParentType != null)
                    return templateParentType.TryGetTemplateArgument(typeName, out argumentName);
                parentType = parentType.DeclaredInType;
            }

            // Template argument wasn't found
            argumentName = "";
            return false;
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.TypeName"/> property.
        /// </summary>
        /// <returns>User type name.</returns>
        protected override string GetTypeName()
        {
            string className = Symbol.Namespaces.Last();
            int index = className.IndexOf('<');

            if (index > 0)
                className = className.Substring(0, index);
            className = CodeWriter.FixUserNaming(className);
            if (!string.IsNullOrEmpty(ConstructorNameSuffix))
                className += ConstructorNameSuffix;
            if (NumberOfTemplateArguments > 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(className);
                sb.Append('<');
                foreach (Symbol arg in TemplateArgumentsAsSymbols)
                {
                    if (arg.UserType != null)
                        sb.Append(arg.UserType.FullTypeName);
                    else
                        sb.Append(OriginalFactory.GetSymbolTypeInstance(this, arg).GetTypeString());
                    sb.Append(',');
                    sb.Append(' ');
                }
                sb.Length -= 2;
                sb.Append('>');
                return sb.ToString();
            }
            return className;
        }

        /// <summary>
        /// Gets template argument name for the specified template argument index.
        /// </summary>
        /// <param name="argumentIndex">Template argument index.</param>
        /// <returns>String representing template argument name.</returns>
        internal string GetTemplateArgumentName(int argumentIndex)
        {
            if (argumentIndex < 0 || argumentIndex >= NumberOfTemplateArguments)
                throw new ArgumentOutOfRangeException(nameof(argumentIndex));

            return NumberOfTemplateArguments == 1 ? TemplateArgumentsNameBase : TemplateArgumentsNameBase + (argumentIndex + 1);
        }

        /// <summary>
        /// Parses template arguments for the specified symbol name.
        /// </summary>
        /// <param name="module">Module that contains this symbol.</param>
        /// <param name="symbolName">The symbol name.</param>
        /// <param name="result">List of symbols that will contain parsed template arguments.</param>
        /// <returns><c>true</c> if all template arguments were parsed correctly and found in the global cache.</returns>
        internal static bool ParseTemplateArguments(SymbolProviders.Module module, string symbolName, List<Symbol> result)
        {
            int templateStart = symbolName.IndexOf('<');

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
                    if (!module.IsConstant(extractedType))
                    {
                        // Check if type is existing type (symbol)
                        Symbol symbol = GlobalCache.GetSymbol(extractedType, module);

                        if (symbol == null)
                        {
                            // TODO: Check if this is unused argument and we should ignore it.
                            return false;
                        }
                        result.Add(symbol);
                    }
                }
            }
            return true;
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
                    if (parent is TemplateUserType || parent is SpecializedTemplateUserType)
                        sb.Append('i');
                    parent = parent.DeclaredInType;
                }

                return sb.ToString();
            }
        }

        public bool UpdateTemplateArguments(UserTypeFactory userTypeFactory)
        {
            // TODO: This looks like it is not needed, verify with some huge example PDBs.
            //throw new NotImplementedException();
            return true;
        }
    }
}
