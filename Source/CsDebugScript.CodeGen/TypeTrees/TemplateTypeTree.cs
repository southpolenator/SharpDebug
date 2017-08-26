using CsDebugScript.CodeGen.UserTypes;
using CsDebugScript.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsDebugScript.CodeGen.TypeTrees
{
    using SymbolProviders;
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Type tree that represents template user type.
    /// </summary>
    /// <seealso cref="UserTypeTree" />
    internal class TemplateTypeTree : UserTypeTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTypeTree"/> class.
        /// </summary>
        /// <param name="templateSpecialization">The template specialization user type.</param>
        /// <param name="factory">The user type factory.</param>
        public TemplateTypeTree(UserType templateSpecialization, UserTypeFactory factory)
            : base(templateSpecialization)
        {
            // Get all "parent" types
            UserType type = templateSpecialization;
            List<UserType> declaredInList = new List<UserType>();

            while (type != null)
            {
                declaredInList.Add(type);
                type = type.DeclaredInType;
            }

            declaredInList.Reverse();
            DeclaredInTypeHierarchy = declaredInList.ToArray();

            // Extract all template types and check if we can instantiate this instance
            CanInstantiate = true;
            SpecializedArguments = new TypeTree[DeclaredInTypeHierarchy.Length][];
            for (int j = 0; j < DeclaredInTypeHierarchy.Length; j++)
            {
                // Check if current type in hierarchy is template type
                TemplateUserType templateType = DeclaredInTypeHierarchy[j] as TemplateUserType;

                if (templateType == null)
                    continue;

                // Try to find specialized arguments for template type
                IReadOnlyList<Symbol> arguments = templateType.TemplateArgumentsAsSymbols;
                TypeTree[] specializedArguments = new TypeTree[arguments.Count];

                for (int i = 0; i < arguments.Count; i++)
                {
                    UserType userType;

                    factory.GetUserType(arguments[i], out userType);
                    if (userType != null)
                    {
                        specializedArguments[i] = UserTypeTree.Create(userType, factory);
                        TemplateTypeTree templateTypeTree = specializedArguments[i] as TemplateTypeTree;

                        if (templateTypeTree != null && !templateTypeTree.CanInstantiate)
                            CanInstantiate = false;
                    }
                    else
                    {
                        // TODO: Check why do we go one more round trip through module for getting argument symbol
                        Symbol symbol = templateSpecialization.Symbol.Module.GetSymbol(arguments[i].Name);

                        if (symbol.Tag != CodeTypeTag.BuiltinType)
                        {
                            // Built-in types can be used for specialization
                            CanInstantiate = false;
                        }

                        // #fixme can't deal with it
                        specializedArguments[i] = templateType.GetSymbolTypeTree(arguments[i], factory);
                    }
                }

                SpecializedArguments[j] = specializedArguments;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be instantiated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can be instantiated; otherwise, <c>false</c>.
        /// </value>
        public bool CanInstantiate { get; private set; }

        /// <summary>
        /// Gets the array of user types in DeclaredInType hierarchy.
        /// </summary>
        public UserType[] DeclaredInTypeHierarchy { get; private set; }

        /// <summary>
        /// Gets the array of arrays of specialized arguments for each type in DeclaredInType hierarchy.
        /// </summary>
        public TypeTree[][] SpecializedArguments { get; private set; }

        /// <summary>
        /// Gets the string representing this type tree in C# code.
        /// </summary>
        /// <param name="truncateNamespace">if set to <c>true</c> namespace will be truncated from generating type string.</param>
        /// <returns>
        /// The string representing this type tree in C# code.
        /// </returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            StringBuilder sb = new StringBuilder();

            if (!truncateNamespace && DeclaredInTypeHierarchy[0].Namespace != null)
            {
                sb.Append(DeclaredInTypeHierarchy[0].Namespace);
                sb.Append('.');
            }

            for (int j = 0; j < DeclaredInTypeHierarchy.Length; j++)
            {
                UserType userType = DeclaredInTypeHierarchy[j];
                TemplateUserType templateType = userType as TemplateUserType;
                NamespaceUserType namespaceType = userType as NamespaceUserType;

                if (templateType != null)
                    sb.Append(templateType.GetSpecializedStringVersion(SpecializedArguments[j].Select(t => t.GetTypeString(truncateNamespace)).ToArray()));
                else if (namespaceType != null)
                {
                    if (j == 0 || truncateNamespace)
                        continue;
                    sb.Append(namespaceType.Namespace);
                }
                else
                    sb.Append(userType.ClassName);
                sb.Append('.');
            }

            sb.Length--;
            return sb.ToString();
        }
    }
}
