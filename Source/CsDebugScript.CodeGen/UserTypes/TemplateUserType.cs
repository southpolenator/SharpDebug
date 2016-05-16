using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenerateUserTypesFromPdb.UserTypes
{
    internal class TemplateUserType : UserType
    {
        private readonly List<string> argumentsSymbols = new List<string>();
        private readonly List<UserType> argumentsUserType = new List<UserType>();

        //  TODO consider new type holding specialized template usertypes.
        //
        public List<TemplateUserType> specializedTypes = new List<TemplateUserType>();

        public TemplateUserType(Symbol symbol, XmlType xmlType, string nameSpace, UserTypeFactory factory)
            : base(symbol, xmlType, nameSpace)
        {
            UpdateArguments(factory);
            ExportStaticFields = false;
        }

        public TemplateUserType TemplateType { get; internal set; }

        public bool UpdateArguments(UserTypeFactory factory)
        {
            this.argumentsSymbols.Clear();
            this.argumentsUserType.Clear();

            string symbolName = Symbol.Namespaces.Last();
            int templateStart = symbolName.IndexOf('<');
            bool result = true;

            if (templateStart > 0)
            {
                var arguments = new List<string>();

                for (int i = templateStart + 1; i < symbolName.Length && symbolName[i] != '>'; i++)
                {
                    var originalyExtractedType = XmlTypeTransformation.ExtractType(symbolName, i);
                    var extractedType = originalyExtractedType.Trim();

                    i += originalyExtractedType.Length;
                    if (string.IsNullOrEmpty(extractedType))
                    {
                        // This can happen only when list is empty
                        if (arguments.Count > 0)
                            throw new NotImplementedException("Unexpected empty template argument in symbol " + symbolName);
                        break;
                    }

                    // Duplicate types should be merged/removed
                    if (arguments.Contains(extractedType))
                        continue;

                    arguments.Add(extractedType);

                    double constant;

                    if (!double.TryParse(extractedType, out constant))
                    {
                        Symbol symbol = GlobalCache.GetSymbol(extractedType, Module);

                        // Check if type is existing type
                        if (symbol == null)
                        {
                            throw new Exception("Wrongly formed template argument");
                        }

                        this.argumentsSymbols.Add(symbol.Name);

                        UserType specializationUserType = null;

                        if (!factory.GetUserType(symbol, out specializationUserType))
                        {
                            if (symbol.Tag != Dia2Lib.SymTagEnum.SymTagEnum && symbol.Tag != Dia2Lib.SymTagEnum.SymTagUDT)
                            {
                                var typeString = GetTypeString(symbol, factory).GetUserTypeString();

                                specializationUserType = new PrimitiveUserType(typeString, symbol);
                            }
                        }

                        this.argumentsUserType.Add(specializationUserType);
                        result = result && specializationUserType != null;
                    }
                }
            }

            // TODO: Unused types should be removed
            return result;
        }

        public Module Module
        {
            get
            {
                return Symbol.Module;
            }
        }

        public override UserType DeclaredInType
        {
            get
            {
                if (this != TemplateType && TemplateType != null)
                    return TemplateType.DeclaredInType;
                return base.DeclaredInType;
            }

            set
            {
                base.DeclaredInType = value;
            }
        }

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
                    if (GenericsArguments == 1)
                    {
                        symbolName += "<" + TemplateArgNameBase + ">";
                    }
                    else if (GenericsArguments > 1)
                    {
                        symbolName += "<";
                        symbolName += string.Join(", ", Enumerable.Range(1, GenericsArguments).Select(t => TemplateArgNameBase + t));
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

        private string TemplateArgNameBase
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

        public string[] ExtractSpecializedTypes()
        {
            List<string> results = new List<string>();
            foreach (string specializedType in argumentsSymbols)
            {
                UserType userType = GlobalCache.GetUserType(specializedType, Module);

                if (userType != null)
                {
                    results.Add(userType.FullClassName);
                    continue;
                }

                results.Add(specializedType);
            }

            //#wrong
            return results.ToArray();
        }

        public Symbol[] ExtractSpecializedSymbols()
        {
            List<Symbol> results = new List<Symbol>();
            foreach (string specializedType in argumentsSymbols)
            {
                results.Add(GlobalCache.GetSymbol(specializedType, Module));
            }

            return results.ToArray();
        }

        public string GetSpecializedType(IEnumerable<string> types)
        {
            if (types.Count() != GenericsArguments)
                throw new Exception("Wrong number of generics arguments");

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

        public string GetSpecializedTypeDefinedInstance()
        {
            string fullClassName = FullClassName;
            string className = ClassName;
            string symbolName = className;

            int templateStart = symbolName.IndexOf('<');

            if (templateStart > 0)
            {
                var types = this.argumentsUserType.Select(r => r is TemplateUserType ? ((TemplateUserType)r).GetSpecializedTypeDefinedInstance() : r.FullClassName);

                symbolName = symbolName.Substring(0, templateStart);
                symbolName += "<";
                symbolName += string.Join(", ", types);
                symbolName += ">";
            }

            return fullClassName.Substring(0, fullClassName.Length - className.Length) + symbolName;
        }

        public bool TryGetArgument(string typeName, out string argument)
        {
            int index = argumentsSymbols.IndexOf(typeName);

            if (index >= 0)
            {
                argument = argumentsSymbols.Count == 1 ? TemplateArgNameBase : TemplateArgNameBase + (index + 1);
                return true;
            }

            TemplateUserType parentType = DeclaredInType as TemplateUserType;

            if (parentType != null)
                return parentType.TryGetArgument(typeName, out argument);

            argument = "";
            return false;
        }

        public override UserTypeTree GetTypeString(Symbol type, UserTypeFactory factory, int bitLength = 0)
        {
            return base.GetTypeString(type, CreateFactory(factory), bitLength);
        }

        protected override UserTypeTree GetBaseTypeString(TextWriter error, Symbol type, UserTypeFactory factory, out int baseClassOffset)
        {
            UserTypeTree baseType = base.GetBaseTypeString(error, type, CreateFactory(factory), out baseClassOffset);

            // Check if base type is template argument. It if is, export it as if it is multi class inheritance.
            UserTypeTreeUserType userBaseType = baseType as UserTypeTreeUserType;
            PrimitiveUserType primitiveUserType = userBaseType != null ? userBaseType.UserType as PrimitiveUserType : null;
            if (userBaseType != null && primitiveUserType != null)
            {
                var dict = GetInheritanceTypeConstraintDictionary(factory);
                string commonBaseClass;

                if (dict.TryGetValue(primitiveUserType.ClassName, out commonBaseClass))
                    return UserTypeTreeUserType.Create(new PrimitiveUserType(commonBaseClass, null), factory);

                baseClassOffset = 0;
                return new UserTypeTreeMultiClassInheritance();
            }

            return baseType;
        }

        protected override void WriteClassComment(IndentedWriter output, int indentation)
        {
            base.WriteClassComment(output, indentation);
            output.WriteLine(indentation, "// ---------------------------------------------------");
            output.WriteLine(indentation, "// Specializations of this class");
            foreach (var type in specializedTypes)
                output.WriteLine(indentation, "//   {0}", type.Symbol.Name);
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


        private enum TypeOfSpecializationType
        {
            Unmatched,
            Anything,
            Variable,
            UserType,
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
                TypeOfSpecializationType specializationType = TypeOfSpecializationType.Unmatched;
                UserType commonType = null;

                foreach (string specializedType in specializedTypes)
                {
                    // Check base type
                    var type = GlobalCache.GetSymbol(specializedType, Module);

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
                        throw new NotImplementedException("Unexpected symbol type " + type.Tag + ". Symbol name: " + specializedType);
                    }

                    // Check if type has user type
                    UserType userType = type.UserType;

                    if (userType == null)
                    {
                        // TODO: This shouldn't happen
                        //specializationType = TypeOfSpecializationType.Variable;
                        //continue;
                        throw new Exception("This should never happen");
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
                    var commonTypeBases = ExtractBaseClasses(commonType);
                    var userTypeBases = ExtractBaseClasses(userType);
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

                string userTypeName;
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
                            userTypeName = new UserTypeTreeGenericsType(templateCommonType, factory).GetUserTypeString();
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

        private static List<UserType> ExtractBaseClasses(UserType userType)
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
                userType = symbol != null ? symbol.UserType : null;
                if (userType != null)
                    userTypes.Add(userType);
            }

            return userTypes;
        }

        private Dictionary<string, string> GetInheritanceTypeConstraintDictionary(UserTypeFactory factory)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
#if false
            string[] commonBaseSpecializationTypes = GetCommonBaseTypesForSpecialization(factory);

            if (commonBaseSpecializationTypes == null || commonBaseSpecializationTypes.All(r => string.IsNullOrEmpty(r)))
#endif
            {
                // no restrictions
                return result;
            }

#if false
            StringBuilder sb = new StringBuilder();
            if (commonBaseSpecializationTypes.Count() == 1)
                result.Add(TemplateArgNameBase, commonBaseSpecializationTypes[0]);
            else
                for (int i = 0; i < commonBaseSpecializationTypes.Count(); i++)
                    if (!string.IsNullOrEmpty(commonBaseSpecializationTypes[i]))
                        result.Add(string.Format("{0}{1}", TemplateArgNameBase, i + 1), commonBaseSpecializationTypes[i]);
            return result;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetInheritanceTypeConstraint(UserTypeFactory factory)
        {
            var dict = GetInheritanceTypeConstraintDictionary(factory);

            return string.Join("", dict.Select(t => string.Format("    where {0} : {1}", t.Key, t.Value)));
        }


        public override UserTypeTree GetFieldType(SymbolField field, UserTypeFactory factory, bool extractingBaseClass, int bitLength = 0)
        {
            if (extractingBaseClass || this.Arguments.Count == 0)
            {
                // Do not match specializations when getting type for base class.
                //
                UserTypeTree baseClassType = GetTypeString(field.Type, factory, bitLength);

                return baseClassType;
            }

            var specializedFields = specializedTypes.Select(r => new Tuple<TemplateUserType, SymbolField>(r, r.Symbol.Fields.FirstOrDefault(q => q.Name == field.Name))).ToArray();

            if (specializedFields.Any(r => r.Item2 == null))
            {
                // TODO
                // Incorrect bucketizing. Field does not exist in all specialization.
                //
                return GetTypeString(field.Type, factory, bitLength);
            }

            if (specializedFields.All(r => r.Item2.Type.Name == field.Type.Name))
            {
                // There is no specialization, all types across the specializations are the same.
                //
                return GetTypeString(field.Type, factory, bitLength);
            }

            //
            UserTypeTree result = GetTypeString(field.Type, factory, bitLength);

            if (result is UserTypeTreeBaseType)
            {
                // Correct result
                //
                UserType baseTypeUserType;

                if (CreateFactory(factory).GetUserType(field.Type, out baseTypeUserType))
                {
                    UserTypeTree tree = UserTypeTreeUserType.Create(baseTypeUserType, factory);

                    if (tree != null)
                    {
                        return tree;
                    }
                }

                // Failed to match the type
                // TODO, look for typedeclared
                // Class is using different types than in template specialization.
                // We cannot support it right now.
                return new UserTypeTreeVariable();
            }

            return result;
        }
    }
}
