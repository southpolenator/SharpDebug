using Dia2Lib;
using GenerateUserTypesFromPdb.UserTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb
{
    class TemplateUserType : UserType
    {
        private List<string> arguments = new List<string>();

        // #fixme, use diferent type
        public List<TemplateUserType> specializedTypes = new List<TemplateUserType>();

        public TemplateUserType(IDiaSession session, IDiaSymbol symbol, XmlType xmlType, string moduleName, UserTypeFactory factory)
            : base(symbol, xmlType, moduleName)
        {
            string symbolName = symbol.name;
            int templateStart = symbolName.IndexOf('<');
            var arguments = new List<string>();

            DiaSession = session;
            for (int i = templateStart + 1; i < symbolName.Length; i++)
            {
                var extractedType = XmlTypeTransformation.ExtractType(symbolName, i);

                arguments.Add(extractedType.Trim());
                i += extractedType.Length;

                int constant;
                extractedType = extractedType.Trim();

                if (!int.TryParse(extractedType, out constant))
                {
                    var type = session.GetTypeSymbol(extractedType);

                    // Check if type is existing type
                    if (type == null)
                        throw new Exception("Wrongly formed template argument");

                    this.arguments.Add(TypeToString.GetTypeString(type));
                }
            }

            // TODO: Unused types should be removed
        }

        public IDiaSession DiaSession { get; private set; }

        protected override bool ExportStaticFields { get { return false; } }

        public override string ClassName
        {
            get
            {
                string symbolName = Symbol.name;
                string newSymbolName = symbolName.Replace("::", ".");

                if (symbolName != newSymbolName)
                {
                    symbolName = newSymbolName;
                }

                if (DeclaredInType != null)
                    symbolName = symbolName.Substring(DeclaredInType.FullClassName.Length + 1);

                int templateStart = symbolName.IndexOf('<');

                if (templateStart > 0)
                {
                    symbolName = symbolName.Substring(0, templateStart);
                    if (GenericsArguments == 1)
                    {
                        symbolName += "<T>";
                    }
                    else if (GenericsArguments > 1)
                    {
                        symbolName += "<";
                        symbolName += string.Join(", ", Enumerable.Range(1, GenericsArguments).Select(t => "T" + t));
                        symbolName += ">";
                    }
                }

                return symbolName;
            }
        }

        public int GenericsArguments
        {
            get
            {
                return arguments.Count;
            }
        }

        public string[] ExtractSpecializedTypes()
        {
            return arguments.ToArray();
        }

        public string GetSpecializedType(string[] types)
        {
            if (types == null)
            {
            }

            if (types.Length != GenericsArguments)
                throw new Exception("Wrong number of generics arguments");

            string symbolName = FullClassName;
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

        public bool TryGetArgument(string typeName, out string argument)
        {
            int index = arguments.IndexOf(typeName);

            if (index >= 0)
            {
                argument = arguments.Count == 1 ? "T" : "T" + (index + 1);
                return true;
            }

            argument = "";
            return false;
        }

        public override UserTypeTree GetTypeString(IDiaSymbol type, UserTypeFactory factory, ulong bitLength = 0)
        {
            return base.GetTypeString(type, CreateFactory(factory), bitLength);
        }

        protected override UserTypeTree GetBaseTypeString(TextWriter error, IDiaSymbol type, UserTypeFactory factory)
        {
            UserTypeTree baseType = base.GetBaseTypeString(error, type, CreateFactory(factory));

            // Check if base type is template argument. It if is, export it as if it is multi class inheritance.
            if (baseType is UserTypeTreeUserType && ((UserTypeTreeUserType)baseType).UserType is FakeUserType)
                return new UserTypeTreeMultiClassInheritance();
            return baseType;
        }

        internal override bool Matches(IDiaSymbol type, UserTypeFactory factory)
        {
            return base.Matches(type, factory);
        }

        internal override bool Matches(string typeString, UserTypeFactory factory)
        {
            if (typeString.Contains('<') && typeString.EndsWith(">"))
            {
                var typeStringStart = typeString.Substring(0, typeString.IndexOf('<'));

                if (string.IsNullOrEmpty(typeStringStart))
                {
                    // do not match unnamed templates
                    return false;
                }

                if (!ClassName.StartsWith(typeStringStart))
                    return false;

                IDiaSymbol diaTypeSymbol = DiaSession.GetTypeSymbol(typeString);

                //#fixme
                if (diaTypeSymbol == null)
                {
                    return false;
                }

                var templateType = new TemplateUserType(DiaSession, diaTypeSymbol, new XmlType() { Name = typeString }, ModuleName, factory);

                return Matches(this, templateType, factory);
            }

            return base.Matches(typeString, factory);
        }

        internal static bool Matches(TemplateUserType template1, TemplateUserType template2, UserTypeFactory factory)
        {
            // Verify that all fields are of the same type
            var t1 = template1.Symbol.name;
            var t2 = template2.Symbol.name;
            var f1 = template1.ExtractFields(factory, UserTypeGenerationFlags.None).OrderBy(f => f.FieldName).ToArray();
            var f2 = template2.ExtractFields(factory, UserTypeGenerationFlags.None).OrderBy(f => f.FieldName).ToArray();

            if (f1.Length != f2.Length)
                return false;
            for (int i = 0; i < f1.Length; i++)
                if (f1[i].FieldName != f2[i].FieldName || f1[i].FieldType != f2[i].FieldType)
                    return false;
            return true;
        }

        private UserTypeFactory CreateFactory(UserTypeFactory factory)
        {
            var templateFactory = factory as TemplateUserTypeFactory;

            if (templateFactory != null)
            {
                if (templateFactory.TemplateType != this)
                    return CreateFactory(templateFactory.OriginalFactory);
                return templateFactory;
            }

            return new TemplateUserTypeFactory(factory, this);
        }
    }


    class TemplateUserTypeFactory : UserTypeFactory
    {
        public TemplateUserTypeFactory(UserTypeFactory factory, TemplateUserType templateType)
            : base(factory)
        {
            TemplateType = templateType;
            OriginalFactory = factory;
        }

        public TemplateUserType TemplateType { get; private set; }

        public UserTypeFactory OriginalFactory { get; private set; }

        internal override bool GetUserType(IDiaSymbol type, out UserType userType)
        {
            string argumentName;
            string typeString = TypeToString.GetTypeString(type);

            if (TryGetArgument(typeString, out argumentName))
            {
                userType = new FakeUserType(argumentName);
                return true;
            }

            return base.GetUserType(type, out userType);
        }

        internal override bool TryGetUserType(string typeString, out UserType userType)
        {
            string argumentName;

            if (TryGetArgument(typeString, out argumentName))
            {
                userType = new FakeUserType(argumentName);
                return true;
            }

            return base.TryGetUserType(typeString, out userType);
        }

        private bool TryGetArgument(string typeString, out string argumentName)
        {
            if (TemplateType.TryGetArgument(typeString, out argumentName))
            {
                return true;
            }

            if (typeString == "wchar_t")
            {
                if (TemplateType.TryGetArgument("unsigned short", out argumentName))
                    return true;
            }
            else if (typeString == "unsigned short")
            {
                if (TemplateType.TryGetArgument("whcar_t", out argumentName))
                    return true;
            }
            else if (typeString == "unsigned long long")
            {
                if (TemplateType.TryGetArgument("unsigned __int64", out argumentName))
                    return true;
            }
            else if (typeString == "unsigned __int64")
            {
                if (TemplateType.TryGetArgument("unsigned long long", out argumentName))
                    return true;
            }
            else if (typeString == "long long")
            {
                if (TemplateType.TryGetArgument("__int64", out argumentName))
                    return true;
            }
            else if (typeString == "__int64")
            {
                if (TemplateType.TryGetArgument("long long", out argumentName))
                    return true;
            }

            return false;
        }
    }

}
