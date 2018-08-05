using System;
using System.Collections.Generic;
using System.Linq;
using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.CodeGen.TypeInstances;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class that represents namespace for the user types
    /// </summary>
    /// <seealso cref="UserType" />
    internal class NamespaceUserType : UserType
    {
        /// <summary>
        /// The list of namespaces represented by this instance
        /// </summary>
        private List<string> namespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceUserType"/> class.
        /// </summary>
        /// <param name="innerNamespaces">The list of inner namespaces (e.g. chrono in std::chrono).</param>
        /// <param name="topLevelNamespace">The top level namespace (e.g. module name).</param>
        /// <param name="factory">User type factory that contains this element.</param>
        internal NamespaceUserType(IEnumerable<string> innerNamespaces, string topLevelNamespace, UserTypeFactory factory)
            : base(symbol: null, xmlType: null, nameSpace: null, factory: factory)
        {
            namespaces = innerNamespaces.Select(s => CodeWriter.FixUserNaming(s)).ToList();
            if (topLevelNamespace != null)
                namespaces.Insert(0, topLevelNamespace);
        }

        /// <summary>
        /// Gets the list of namespaces this class represents.
        /// </summary>
        public IReadOnlyList<string> Namespaces => namespaces;

        /// <summary>
        /// Merges namespace with "parent" ones if possible (only if all are namespaces).
        /// </summary>
        internal void MergeIfPossible()
        {
            bool canBeMerged = true;

            for (UserType parentNamespace = DeclaredInType; parentNamespace != null && canBeMerged; parentNamespace = parentNamespace.DeclaredInType)
                canBeMerged = parentNamespace is NamespaceUserType;

            if (canBeMerged && DeclaredInType != null)
            {
                for (UserType parentNamespace = DeclaredInType; parentNamespace != null; parentNamespace = parentNamespace.DeclaredInType)
                    namespaces.Insert(0, parentNamespace.Namespace);
                UpdateConstructorNameSuffix(ConstructorNameSuffix); // Invalidate cache of name properties
                UpdateDeclaredInType(null);
            }
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.TypeName"/> property.
        /// </summary>
        /// <returns>User type name.</returns>
        protected override string GetTypeName()
        {
            if (!string.IsNullOrEmpty(ConstructorNameSuffix))
                return namespaces.Last() + ConstructorNameSuffix;
            return namespaces.Last();
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.FullTypeName"/> property.
        /// </summary>
        /// <returns>User type full name.</returns>
        protected override string GetFullTypeName()
        {
            if (DeclaredInType != null)
                return $"{DeclaredInType.FullTypeName}.{Namespace}";
            return Namespace;
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.Namespace"/> property.
        /// </summary>
        /// <param name="constructorNamespace">Namespace parameter of the constructor of this class.</param>
        protected override string GetNamespace(string constructorNamespace)
        {
            return string.Join(".", namespaces);
        }

        /// <summary>
        /// Function that should evaluate <see cref="UserType.BaseClass"/> and <see cref="UserType.BaseClassOffset"/> properties.
        /// </summary>
        protected override Tuple<TypeInstance, int> GetBaseClass(Symbol symbol)
        {
            return Tuple.Create<TypeInstance, int>(new StaticClassTypeInstance(CodeWriter), 0);
        }
    }
}
