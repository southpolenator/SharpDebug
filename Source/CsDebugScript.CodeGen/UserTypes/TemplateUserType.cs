using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.CodeGen.UserTypes.Members;
using System.Collections.Generic;
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
        /// Initializes a new instance of the <see cref="TemplateUserType"/> class.
        /// </summary>
        /// <param name="template">Specialized template user type that will be used as representative for generating code.</param>
        /// <param name="specializations"></param>
        /// <param name="factory">User type factory that contains this element.</param>
        public TemplateUserType(SpecializedTemplateUserType template, List<SpecializedTemplateUserType> specializations, UserTypeFactory factory)
            : base(template.Symbol, null, template.Namespace, factory)
        {
            Specializations = specializations;
            SpecializedRepresentative = template;
            foreach (SpecializedTemplateUserType specialization in specializations)
                specialization.TemplateType = this;
        }

        /// <summary>
        /// Gets the specialized template user type that will be used as representative for generating code.
        /// </summary>
        public SpecializedTemplateUserType SpecializedRepresentative { get; private set; }

        /// <summary>
        /// Gets the list of all specialized template user types that this user type generalizes.
        /// </summary>
        public IReadOnlyList<SpecializedTemplateUserType> Specializations { get; private set; }

        /// <summary>
        /// Updates the <see cref="UserType.ConstructorNameSuffix"/> property. Invalidates all necessary caches too.
        /// </summary>
        /// <param name="suffix">New value for constructor name suffix.</param>
        public override void UpdateConstructorNameSuffix(string suffix)
        {
            base.UpdateConstructorNameSuffix(suffix);
            foreach (UserType specialization in Specializations)
                specialization.UpdateConstructorNameSuffix(suffix);
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
            if (SpecializedRepresentative.NumberOfTemplateArguments > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(className);
                sb.Append('<');
                for (int i = 0; i < SpecializedRepresentative.NumberOfTemplateArguments; i++)
                {
                    sb.Append(SpecializedRepresentative.GetTemplateArgumentName(i));
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
        /// Function that should evaluate <see cref="UserType.Members"/> property.
        /// </summary>
        protected override IEnumerable<UserTypeMember> GetMembers()
        {
            UserTypeMember[] members = SpecializedRepresentative.Members;

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] is ConstantUserTypeMember originalConstant)
                {
                    // Verify that value of this constant is the same in all specializations
                    bool same = true;

                    foreach (SpecializedTemplateUserType specialization in Specializations)
                    {
                        UserTypeMember m = specialization.Members.FirstOrDefault(mm => mm.Name == originalConstant.Name);

                        if (m == null)
                            continue;

                        if (!(m is ConstantUserTypeMember constant) || originalConstant.Value.ToString() != constant.Value.ToString())
                        {
                            same = false;
                            break;
                        }
                    }

                    // If constant is not the same in all specializations, it needs to be read from the code type.
                    if (!same)
                        yield return new DataFieldUserTypeMember()
                        {
                            AccessLevel = originalConstant.AccessLevel,
                            Symbol = originalConstant.Symbol,
                            Name = originalConstant.Name,
                            Type = originalConstant.Type,
                            UserType = originalConstant.UserType,
                        };
                    else
                        yield return members[i];
                }
                else
                    yield return members[i];
            }
        }
    }
}
