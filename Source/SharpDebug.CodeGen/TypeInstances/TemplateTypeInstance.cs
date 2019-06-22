using SharpDebug.CodeGen.UserTypes;
using SharpDebug.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpDebug.CodeGen.TypeInstances
{
    using SymbolProviders;
    using UserType = SharpDebug.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Type instance that represents template user type.
    /// </summary>
    /// <seealso cref="UserTypeInstance" />
    internal class TemplateTypeInstance : UserTypeInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateTypeInstance"/> class.
        /// </summary>
        /// <param name="templateSpecialization">The user type that is either template specialization user type or declared in one.</param>
        /// <param name="factory">The user type factory.</param>
        public TemplateTypeInstance(UserType templateSpecialization, UserTypeFactory factory)
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
            SpecializedArguments = new TypeInstance[DeclaredInTypeHierarchy.Length][];
            for (int j = 0; j < DeclaredInTypeHierarchy.Length; j++)
            {
                // Check if current type in hierarchy is template type
                SpecializedTemplateUserType templateType = DeclaredInTypeHierarchy[j] as SpecializedTemplateUserType;

                if (templateType == null)
                    continue;

                // Try to find specialized arguments for template type
                IReadOnlyList<Symbol> arguments = templateType.TemplateArgumentsAsSymbols;
                TypeInstance[] specializedArguments = new TypeInstance[arguments.Count];

                for (int i = 0; i < arguments.Count; i++)
                {
                    TypeInstance ti = factory.GetSymbolTypeInstance(templateSpecialization, arguments[i]);

                    specializedArguments[i] = ti;
                    if (ti.ContainsUndefinedType())
                        CanInstantiate = false;
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
        public TypeInstance[][] SpecializedArguments { get; private set; }

        /// <summary>
        /// Checks whether this type instance is using undefined type (a.k.a. <see cref="Variable"/> or <see cref="UserType"/>).
        /// </summary>
        /// <returns><c>true</c> if this type instance is using undefined type;<c>false</c> otherwise.</returns>
        public override bool ContainsUndefinedType()
        {
            return !CanInstantiate;
        }

        /// <summary>
        /// Gets the string representing this type instance in generated code.
        /// </summary>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        /// <returns>The string representing this type instance in generated code.</returns>
        public override string GetTypeString(bool truncateNamespace = false)
        {
            StringBuilder sb = new StringBuilder();
            string baseName = DeclaredInTypeHierarchy[0].FullTypeName;
            int templateIndex = baseName.IndexOf('<');

            if (!truncateNamespace)
            {
                sb.Append(templateIndex >= 0 ? baseName.Substring(0, templateIndex) : baseName);
                AppendSpecializedArguments(sb, 0, truncateNamespace);
            }
            for (int i = !truncateNamespace ? 1 : SpecializedArguments.Length - 1; i < SpecializedArguments.Length; i++)
            {
                sb.Append('.');
                baseName = DeclaredInTypeHierarchy[i].TypeName;
                templateIndex = baseName.IndexOf('<');
                sb.Append(templateIndex >= 0 ? baseName.Substring(0, templateIndex) : baseName);
                AppendSpecializedArguments(sb, i, truncateNamespace);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the type of this type instance using the specified type converter.
        /// </summary>
        /// <param name="typeConverter">The type converter interface.</param>
        public override Type GetType(ITypeConverter typeConverter)
        {
            Type genericType = base.GetType(typeConverter);
            Type[] arguments = SpecializedArguments.Where(sa => sa != null).SelectMany(sa => sa).Select(sa => sa.GetType(typeConverter)).ToArray();

            return genericType.MakeGenericType(arguments);
        }

        /// <summary>
        /// Appends specialized arguments of user type.
        /// </summary>
        /// <param name="sb">The string builder where arguments will be appended.</param>
        /// <param name="userTypeIndex">Index of user type which arguments should be appended.</param>
        /// <param name="truncateNamespace">If set to <c>true</c> namespace won't be added to the generated type string.</param>
        private void AppendSpecializedArguments(StringBuilder sb, int userTypeIndex, bool truncateNamespace)
        {
            if (SpecializedArguments[userTypeIndex] != null && SpecializedArguments[userTypeIndex].Length > 0)
            {
                sb.Append('<');
                for (int i = 0; i < SpecializedArguments[userTypeIndex].Length; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append(SpecializedArguments[userTypeIndex][i].GetTypeString(truncateNamespace));
                }
                sb.Append('>');
            }
        }
    }
}
