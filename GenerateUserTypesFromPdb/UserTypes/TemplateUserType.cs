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
    internal class TemplateUserType : UserType
    {
        private List<string> argumentsSymbols = new List<string>();
        private List<UserType> argumentsUserType = new List<UserType>();

        // #fixme, use diferent type
        public List<TemplateUserType> specializedTypes = new List<TemplateUserType>();

        public TemplateUserType(IDiaSession session, IDiaSymbol symbol, XmlType xmlType, string moduleName, UserTypeFactory factory)
            : base(symbol, xmlType, moduleName)
        {
            DiaSession = session;

            UpdateArguments(factory);
        }

        public void UpdateArguments(UserTypeFactory factory)
        {
            this.argumentsSymbols.Clear();
            this.argumentsUserType.Clear();

            string symbolName = Symbol.name;

            int templateStart = symbolName.IndexOf('<');
            var arguments = new List<string>();

            for (int i = templateStart + 1; i < symbolName.Length; i++)
            {
                var extractedType = XmlTypeTransformation.ExtractType(symbolName, i);

                arguments.Add(extractedType.Trim());
                i += extractedType.Length;

                int constant;
                extractedType = extractedType.Trim();

                if (!int.TryParse(extractedType, out constant))
                {
                    IDiaSymbol diaSymbol = DiaSession.GetTypeSymbol(extractedType);

                    // Check if type is existing type
                    if (diaSymbol == null)
                    {
                        throw new Exception("Wrongly formed template argument");
                    }

                    this.argumentsSymbols.Add(TypeToString.GetTypeString(diaSymbol));

                    UserType specializationUserType;
                    if (factory.GetUserType(diaSymbol, out specializationUserType) )
                    {
                        this.argumentsUserType.Add(specializationUserType);
                    }
                    else
                    {
                        this.argumentsUserType.Add(null);
                    }
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

                if (DeclaredInType != null)
                {
                    symbolName = NameHelper.GetFullSymbolNamespaces(symbolName).Last();
                }
                else
                if (Namespace != null)
                {
                    symbolName = symbolName.Substring(NamespaceSymbol.Length + 2);
                }

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
                return argumentsSymbols.Count;
            }
        }

        public List<string> Arguments
        {
            get
            {
                return argumentsSymbols;
            }
        }

        public string[] ExtractSpecializedTypes()
        {
            List<string> results = new List<string>();
            foreach (string specializedType in argumentsSymbols)
            {
                UserType userType;

                if (GlobalCache.UserTypesBySymbolName.TryGetValue(NameHelper.GetLookupNameForSymbol(specializedType), out userType))
                {
                    results.Add(userType.ClassName);
                    continue;
                }

                if (GlobalCache.UserTypesBySymbolName.TryGetValue(specializedType, out userType))
                {
                    results.Add(userType.ClassName);
                    continue;
                }

                results.Add(specializedType);
            }

            //#wrong
            return results.ToArray();
        }

        public IDiaSymbol[] ExtractSpecializedSymbols()
        {
            List<IDiaSymbol> results = new List<IDiaSymbol>();
            foreach (string specializedType in argumentsSymbols)
            {
                UserType userType;

                if (GlobalCache.UserTypesBySymbolName.TryGetValue(specializedType, out userType))
                {
                    results.Add(userType.Symbol);
                }
                else
                {
                    results.Add(DiaSession.GetTypeSymbol(specializedType));
                }
            }

            return results.ToArray();
        }

        public string GetSpecializedType(IEnumerable<string> types)
        {
            if (types.Count() != GenericsArguments)
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

        public string GetSpecializedTypeDefinedInstance()
        {
            string symbolName = FullClassName;

            int templateStart = symbolName.IndexOf('<');

            if (templateStart > 0)
            {
                var types = this.argumentsUserType.Select(r => r is TemplateUserType ? ((TemplateUserType)r).GetSpecializedTypeDefinedInstance() : r.FullClassName);

                symbolName = symbolName.Substring(0, templateStart);
                symbolName += "<";
                symbolName += string.Join(", ", types);
                symbolName += ">";
            }

            return symbolName;
        }


        public bool TryGetArgument(string typeName, out string argument)
        {
            int index = argumentsSymbols.IndexOf(typeName);

            if (index >= 0)
            {
                argument = argumentsSymbols.Count == 1 ? "T" : "T" + (index + 1);
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
            if (baseType is UserTypeTreeUserType && ((UserTypeTreeUserType)baseType).UserType is PrimitiveUserType)
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

        /// <summary>
        /// Checks if given template type can be instantiated.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public bool IsInstantiable(UserTypeFactory factory)
        {
            string symbolName = Symbol.name;

            // Check cache for result
            bool result;
            if (GlobalCache.InstantiableTemplateUserTypes.TryGetValue(symbolName, out result))
            {
                return result;
            }

            //List<string> specializationArgs = NameHelper.GetTemplateSpecializationArguments(symbolName);

            List<string> specializationArgs = this.argumentsSymbols;

            result = true;

            foreach (string arg in specializationArgs)
            {
                bool isTemplateArg = NameHelper.IsTemplateType(arg);

                // Find DiaSymbol for specialization type
                Symbol aa;
                IDiaSymbol argSymbol;
                if (!GlobalCache.DiaSymbolsByName.TryGetValue(arg, out aa))
                {
                    //
                    // TODO, add base symbols to global cache
                    //
                    // Symbol is not cached, use dia to verify is this primitive type
                    argSymbol = DiaSession.GetTypeSymbol(arg);
                }
                else
                {
                    argSymbol = aa.Dia;
                }

                if (argSymbol != null)
                {
                    // Find UserType for specialization
                    UserType argUserType;

                    if (factory.GetUserType(argSymbol, out argUserType))
                    {
                        // Specialization is template, verify the template as well.
                        TemplateUserType argTemplateUserType = argUserType as TemplateUserType;
                        if (argTemplateUserType != null)
                        {
                            // Inner Template cannot be instantiated
                            if (!argTemplateUserType.IsInstantiable(factory))
                            {
                                result = false;
                                break;
                            }
                        }

                        continue;
                    }
                }

                // Can't find symbol, class cannot be instantiated
                result = false;
                break;
            }

            // Cache the result
            GlobalCache.InstantiableTemplateUserTypes.TryAdd(symbolName, result);
            
            return result;
        }


        public string[] GetCommonBaseTypesForSpecialization(UserTypeFactory factory)
        {
            if (!specializedTypes.Any())
            {
                return null;
            }

            string[] results = new string[GenericsArguments];

            for (int i = 0; i < GenericsArguments; i++)
            {
                string[] specializedTypes = this.specializedTypes.Select(r => r.argumentsSymbols[i]).ToArray();

                //
                // TODO, for now just use Variable
                //
                string userTypeName = "Variable";

                foreach (string specializedType in specializedTypes)
                {
                    //
                    // TODO again cache base/primitive dia symbols
                    //
                    if (specializedType == "bool" ||
                        specializedType == "char" ||
                        specializedType == "void" ||
                        specializedType == "short" ||
                        specializedType == "int" ||
                        specializedType == "long long" ||
                        specializedType == "unsigned long long" ||
                        specializedType == "unsigned short" ||
                        specializedType == "unsigned char" || 
                        specializedType == "unsigned int" ||
                        specializedType == "double" ||
                        specializedType == "float")
                    {
                        userTypeName = null;
                        break;
                    }

                    // try lookup type 
                    // TODO
                    // that might not be enough for the enums nested in template types
                    UserType userType;
                    if (factory.TryGetUserType(specializedType, out userType) && userType is EnumUserType)
                    {
                        userTypeName = null;
                        break;
                    }
                }

                results[i] = userTypeName;
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetInheritanceTypeConstraint(UserTypeFactory factory)
        {
            if (Symbol.name.Contains("SequencedWaitInfo"))
            {

            }

            string[] commonBaseSpecializationTypes = GetCommonBaseTypesForSpecialization(factory);

            if (commonBaseSpecializationTypes == null || commonBaseSpecializationTypes.All(r => string.IsNullOrEmpty(r)))
            {
                // no restrictions
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            if (commonBaseSpecializationTypes.Count() == 1)
            {
                sb.Append(string.Format("  where T : {0} ", commonBaseSpecializationTypes[0]));
            }
            else
            {
                for (int i = 0; i < commonBaseSpecializationTypes.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(commonBaseSpecializationTypes[i]))
                    {
                        sb.Append(string.Format("  where T{0} : {1} ", i + 1, commonBaseSpecializationTypes[i]));
                    }
                }
            }

            return sb.ToString();
        }

    }
}