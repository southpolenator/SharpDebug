using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    [Flags]
    enum UserTypeGenerationFlags
    {
        None = 0,
        SingleLineProperty = 1,
        GenerateFieldTypeInfoComment = 2,
        UseClassFieldsFromDiaSymbolProvider = 4,
        ForceUserTypesToNewInsteadOfCasting = 8,
        CacheUserTypeFields = 16,
        CacheStaticUserTypeFields = 32,
        LazyCacheUserTypeFields = 64,
        GeneratePhysicalMappingOfUserTypes = 128,
    }

    static class StringExtensions
    {
        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }

    static class MoreDiaExtensions
    {
        static Dictionary<string, IDiaSymbol> basicTypes;

        public static IDiaSymbol GetTypeSymbol(this IDiaSession session, string name)
        {
            if (basicTypes == null)
            {
                var types = session.globalScope.GetChildren(SymTagEnum.SymTagBaseType);

                basicTypes = new Dictionary<string, IDiaSymbol>();
                foreach (var type in types)
                {
                    try
                    {
                        string typeString = TypeToString.GetTypeString(type);

                        if (!basicTypes.ContainsKey(typeString))
                        {
                            basicTypes.Add(typeString, type);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            string originalName = name;
            IDiaSymbol symbol;

            int pointer = 0;

            while (name.EndsWith("*"))
            {
                pointer++;
                name = name.Substring(0, name.Length - 1);
            }

            name = name.Trim();
            if (name.EndsWith(" const"))
                name = name.Substring(0, name.Length - 6);
            if (name.StartsWith("enum "))
                name = name.Substring(5);

            if (name == "unsigned __int64")
                name = "unsigned long long";
            else if (name == "__int64")
                name = "long long";
            else if (name == "long")
                name = "int";
            else if (name == "unsigned long")
                name = "unsigned int";

            if (!basicTypes.TryGetValue(name, out symbol))
                symbol = session.globalScope.GetChild(name);

            if (symbol == null)
                Console.WriteLine("   '{0}' not found", originalName);
            else
                for (int i = 0; i < pointer; i++)
                    symbol = symbol.objectPointerType;
            return symbol;
        }
    }

    abstract class UserTypeTree
    {
        public abstract string GetUserTypeString();

        public override string ToString()
        {
            return GetUserTypeString();
        }
    }

    class UserTypeTreeUserType : UserTypeTree
    {
        public UserTypeTreeUserType(UserType userType)
        {
            UserType = userType;
        }

        public UserType UserType { get; private set; }

        public override string GetUserTypeString()
        {
            return UserType.FullClassName;
        }

        internal static UserTypeTree Create(UserType userType, UserTypeFactory factory)
        {
            var templateType = userType as TemplateUserType;

            if (templateType != null)
                return new UserTypeTreeGenericsType(templateType, factory);
            return new UserTypeTreeUserType(userType);
        }
    }

    class UserTypeTreeBaseType : UserTypeTree
    {
        public UserTypeTreeBaseType(string baseType)
        {
            BaseType = baseType;
        }

        public string BaseType { get; private set; }

        public override string GetUserTypeString()
        {
            return BaseType;
        }
    }

    class UserTypeTreeGenericsType : UserTypeTreeUserType
    {
        public UserTypeTreeGenericsType(TemplateUserType genericsType, UserTypeFactory factory)
            : base(genericsType)
        {
            GenericsType = genericsType;

            string[] arguments = genericsType.ExtractSpecializedTypes();

            SpecializedArguments = new UserTypeTree[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                var symbol = genericsType.DiaSession.GetTypeSymbol(arguments[i]);

                SpecializedArguments[i] = genericsType.GetTypeString(symbol, factory);
            }
        }

        public TemplateUserType GenericsType { get; private set; }
        public UserTypeTree[] SpecializedArguments { get; private set; }

        public override string GetUserTypeString()
        {
            return GenericsType.GetSpecializedType(SpecializedArguments.Select(t => t.GetUserTypeString()).ToArray());
        }
    }

    class UserTypeTreeEnum : UserTypeTreeUserType
    {
        public UserTypeTreeEnum(EnumUserType userType)
            : base(userType)
        {
        }

        public EnumUserType EnumUserType
        {
            get
            {
                return (EnumUserType)UserType;
            }
        }
    }

    class UserTypeTreeCodePointer : UserTypeTree
    {
        public UserTypeTreeCodePointer(UserTypeTree innerType)
        {
            InnerType = innerType;
        }

        public UserTypeTree InnerType { get; private set; }

        public override string GetUserTypeString()
        {
            return string.Format("CodePointer<{0}>", InnerType.GetUserTypeString());
        }
    }

    class UserTypeTreeCodeArray : UserTypeTree
    {
        public UserTypeTreeCodeArray(UserTypeTree innerType)
        {
            InnerType = innerType;
        }

        public UserTypeTree InnerType { get; private set; }

        public override string GetUserTypeString()
        {
            return string.Format("CodeArray<{0}>", InnerType.GetUserTypeString());
        }
    }

    class UserTypeTreeCodeFunction : UserTypeTree
    {
        public override string GetUserTypeString()
        {
            return "CodeFunction";
        }
    }

    class UserTypeTreeVariable : UserTypeTree
    {
        private bool isJustVariable;

        public UserTypeTreeVariable(bool isJustVariable = true)
        {
            this.isJustVariable = isJustVariable;
        }

        public override string GetUserTypeString()
        {
            return isJustVariable ? "Variable" : "UserType";
        }
    }

    class UserTypeTreeMultiClassInheritance : UserTypeTreeVariable
    {
        public UserTypeTreeMultiClassInheritance()
            : base(false)
        {
        }
    }

    class UserTypeTreeTransformation : UserTypeTree
    {
        public UserTypeTreeTransformation(UserTypeTransformation transformation)
        {
            Transformation = transformation;
        }

        public UserTypeTransformation Transformation { get; private set; }

        public override string GetUserTypeString()
        {
            return Transformation.TransformType();
        }
    }

    class UserTypeField
    {
        public string SimpleFieldValue { get; set; }

        public string FieldName { get; set; }

        public string FieldType { get; set; }

        public string PropertyName { get; set; }

        public string ConstructorText { get; set; }

        public string FieldTypeInfoComment { get; set; }

        public bool Static { get; set; }

        public bool UseUserMember { get; set; }

        public bool CacheResult { get; set; }

        public void WriteFieldCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options)
        {
            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
                output.WriteLine(indentation, FieldTypeInfoComment);
            if (UseUserMember && CacheResult)
                output.WriteLine(indentation, "private {0}UserMember<{1}> {2};", Static ? "static " : "", FieldType, FieldName);
            else if (CacheResult)
                output.WriteLine(indentation, "private {0}{1} {2};", Static ? "static " : "", FieldType, FieldName);
            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                output.WriteLine();
        }

        public void WriteConstructorCode(IndentedWriter output, int indentation)
        {
            if (UseUserMember && CacheResult)
                output.WriteLine(indentation, "{0} = UserMember.Create(() => {1});", FieldName, ConstructorText);
            else if (CacheResult)
                output.WriteLine(indentation, "{0} = {1};", FieldName, ConstructorText);
        }

        public void WritePropertyCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options, ref bool firstField)
        {
            if (options.HasFlag(UserTypeGenerationFlags.SingleLineProperty) && CacheResult)
            {
                if (firstField)
                {
                    output.WriteLine();
                    firstField = false;
                }

                if (UseUserMember)
                    output.WriteLine(indentation, "public {0}{1} {2} {{ get {{ return {3}.Value; }} }}", Static ? "static " : "", FieldType, PropertyName, FieldName);
                else
                    output.WriteLine(indentation, "public {0}{1} {2} {{ get {{ return {3}; }} }}", Static ? "static " : "", FieldType, PropertyName, FieldName);
            }
            else
            {
                output.WriteLine();
                if (!UseUserMember && !CacheResult && options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
                    output.WriteLine(indentation, FieldTypeInfoComment);
                output.WriteLine(indentation, "public {0}{1} {2}", Static ? "static " : "", FieldType, PropertyName);
                output.WriteLine(indentation++, "{{");
                output.WriteLine(indentation, "get");
                output.WriteLine(indentation++, "{{");
                if (UseUserMember && CacheResult)
                    output.WriteLine(indentation, "return {0}.Value;", FieldName);
                else if (CacheResult)
                    output.WriteLine(indentation, "return {0};", FieldName);
                else
                    output.WriteLine(indentation, "return {0};", ConstructorText);
                output.WriteLine(--indentation, "}}");
                output.WriteLine(--indentation, "}}");
            }
        }
    }

    class UserTypeConstructor
    {
        public string Arguments { get; set; } = "";

        public string BaseClassInitialization { get; set; } = "";

        public bool ContainsFieldDefinitions { get; set; }

        public bool Static { get; set; }

        public void WriteCode(IndentedWriter output, int indentation, UserTypeField[] fields, string constructorName, bool exportStaticFields)
        {
            if (Static)
            {
                output.WriteLine();
                output.WriteLine(indentation, "static {0}()", constructorName);
                output.WriteLine(indentation++, "{{");
                if (ContainsFieldDefinitions)
                    foreach (var field in fields)
                    {
                        if ((field.Static && !exportStaticFields && field.FieldTypeInfoComment != null) || (!field.CacheResult && !field.UseUserMember))
                            continue;
                        if (field.Static)
                            field.WriteConstructorCode(output, indentation);
                    }
                output.WriteLine(--indentation, "}}");
            }
            else
            {
                output.WriteLine();
                output.WriteLine(indentation, "public {0}({1})", constructorName, Arguments);
                if (!string.IsNullOrEmpty(BaseClassInitialization))
                    output.WriteLine(indentation + 1, ": {0}", BaseClassInitialization);
                output.WriteLine(indentation++, "{{");
                if (ContainsFieldDefinitions)
                    foreach (var field in fields)
                    {
                        if (!field.CacheResult && !field.UseUserMember)
                            continue;
                        if (!field.Static)
                            field.WriteConstructorCode(output, indentation);
                    }
                output.WriteLine(--indentation, "}}");
            }
        }
    }

    class UserTypeTransformation
    {
        private XmlTypeTransformation transformation;
        private Func<string, string> typeConverter;
        private UserType ownerUserType;
        private IDiaSymbol type;

        public UserTypeTransformation(XmlTypeTransformation transformation, Func<string, string> typeConverter, UserType ownerUserType, IDiaSymbol type)
        {
            this.transformation = transformation;
            this.typeConverter = typeConverter;
            this.ownerUserType = ownerUserType;
            this.type = type;
        }

        internal string TransformType()
        {
            string originalFieldTypeString = TypeToString.GetTypeString(type);

            return transformation.TransformType(originalFieldTypeString, ownerUserType.ClassName, typeConverter);
        }

        internal string TransformConstructor(string field, string fieldOffset)
        {
            string originalFieldTypeString = TypeToString.GetTypeString(type);

            return transformation.TransformConstructor(originalFieldTypeString, field, fieldOffset, ownerUserType.ClassName, typeConverter);
        }
    }

    class UserTypeFactory
    {
        protected List<UserType> userTypes = new List<UserType>();
        protected XmlTypeTransformation[] typeTransformations;

        public UserTypeFactory(XmlTypeTransformation[] transformations)
        {
            typeTransformations = transformations;
        }

        public UserTypeFactory(UserTypeFactory factory)
            : this(factory.typeTransformations)
        {
            userTypes.AddRange(factory.userTypes);
        }

        public IEnumerable<UserType> Symbols
        {
            get
            {
                return userTypes;
            }
        }

        internal virtual bool GetUserType(string typeString, out UserType userType)
        {
            userType = userTypes.FirstOrDefault(t => t.Matches(typeString, this));
            return userType != null;
        }

        internal virtual bool GetUserType(IDiaSymbol type, out UserType userType)
        {
            userType = userTypes.FirstOrDefault(t => t.Matches(type, this));
            return userType != null;
        }

        internal void AddSymbol(IDiaSymbol symbol, XmlType type, string moduleName, UserTypeGenerationFlags generationOptions)
        {
            if (type == null)
            {
                userTypes.Add(new EnumUserType(symbol, moduleName));
            }
            else if (generationOptions.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
            {
                userTypes.Add(new PhysicalUserType(symbol, type, moduleName));
            }
            else
            {
                userTypes.Add(new UserType(symbol, type, moduleName));
            }
        }

        internal void AddSymbol(IDiaSession session, IDiaSymbol[] symbols, XmlType type, string moduleName, UserTypeGenerationFlags generationOptions)
        {
            if (!type.IsTemplate && symbols.Length > 1)
                throw new Exception("Type has more than one symbol for " + type.Name);

            if (!type.IsTemplate)
                AddSymbol(symbols[0], type, moduleName, generationOptions);
            else
            {
                // Create template user type for every symbol
                var templates = new List<TemplateUserType>();

                for (int i = 0; i < symbols.Length; i++)
                    try
                    {
                        if (symbols[i].length > 0) // We want to ignore "empty" generic classes (for now)
                            templates.Add(new TemplateUserType(session, symbols[i], type, moduleName, this));
                    }
                    catch (Exception)
                    {
                    }

                // Bucketize user types for number of generics arguments in generated code
                var buckets = new Dictionary<int, TemplateUserType>();

                foreach (var template in templates)
                {
                    int args = template.GenericsArguments;
                    TemplateUserType previousTemplate;

                    if (!buckets.TryGetValue(args, out previousTemplate))
                        buckets.Add(args, template);
                    else
                    {
                        if (!TemplateUserType.Matches(template, previousTemplate, this))
                            throw new Exception("Templates are not matching field names and types");
                    }
                }

                // Add newly generated types
                foreach (var template in buckets.Values)
                    userTypes.Add(template);
            }
        }

        internal void ProcessTypes()
        {
            foreach (var userType in userTypes)
            {
                var fullClassName = userType.FullClassName;
                int lastIndex = fullClassName.LastIndexOf('.');
                var parentTypeName = lastIndex > 0 ? fullClassName.Substring(0, lastIndex) : null;
                if (parentTypeName != userType.Namespace)
                {
                    var parentType = userTypes.First(t => t.FullClassName == parentTypeName);

                    if (parentType != null)
                    {
                        userType.SetDeclaredInType(parentType);
                    }
                    else
                    {
                        throw new Exception("Unsupported namespace of class " + userType.Symbol.name);
                    }
                }
            }
        }

        internal bool ContainsSymbol(IDiaSymbol type)
        {
            UserType userType;

            return GetUserType(type, out userType);
        }

        internal UserTypeTransformation FindTransformation(IDiaSymbol type, UserType ownerUserType)
        {
            string originalFieldTypeString = TypeToString.GetTypeString(type);
            var transformation = typeTransformations.Where(t => t.Matches(originalFieldTypeString)).FirstOrDefault();

            if (transformation == null)
                return null;

            Func<string, string> typeConverter = null;

            typeConverter = (inputType) =>
            {
                UserType userType;

                if (GetUserType(inputType, out userType))
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

        internal bool ContainsSymbol(string typeString)
        {
            UserType userType;

            return GetUserType(typeString, out userType);
        }
    }

    class FakeUserType : UserType
    {
        private string typeName;

        public FakeUserType(string typeName)
            : base(null, null, null)
        {
            this.typeName = typeName;
        }

        public override string ClassName
        {
            get
            {
                return typeName;
            }
        }

        public override string FullClassName
        {
            get
            {
                return typeName;
            }
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

        internal override bool GetUserType(string typeString, out UserType userType)
        {
            string argumentName;

            if (TryGetArgument(typeString, out argumentName))
            {
                userType = new FakeUserType(argumentName);
                return true;
            }

            return base.GetUserType(typeString, out userType);
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

    class TemplateUserType : UserType
    {
        private List<string> arguments = new List<string>();

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
            if (typeString.Contains('<'))
            {
                var typeStringStart = typeString.Substring(0, typeString.IndexOf('<'));

                if (!ClassName.StartsWith(typeStringStart))
                    return false;

                var templateType = new TemplateUserType(DiaSession, DiaSession.GetTypeSymbol(typeString), new XmlType() { Name = typeString }, ModuleName, factory);

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

    class UserType
    {
        public UserType(IDiaSymbol symbol, XmlType xmlType, string moduleName)
        {
            Symbol = symbol;
            XmlType = xmlType;
            ModuleName = moduleName;
            InnerTypes = new List<UserType>();
            Usings = new HashSet<string>(new string[] { "CsScripts" });
        }

        public IDiaSymbol Symbol { get; private set; }

        public string ModuleName { get; private set; }

        public string Namespace { get; set; }

        public UserType DeclaredInType { get; private set; }

        public List<UserType> InnerTypes { get; private set; }

        public HashSet<string> Usings { get; private set; }

        public XmlType XmlType { get; private set; }

        protected virtual bool ExportStaticFields { get { return true; } }

        public virtual string ClassName
        {
            get
            {
                string symbolName = Symbol.name.Replace("::", ".").Replace('<', '_').Replace('>', '_').Replace(' ', '_').Replace(',', '_').Replace("__", "_").Replace("__", "_").TrimEnd('_');

                if (DeclaredInType != null)
                {
                    if (Namespace != null)
                        symbolName = Namespace + "." + symbolName;
                    return symbolName.Substring(DeclaredInType.FullClassName.Length + 1);
                }

                return symbolName;
            }
        }

        public virtual string ConstructorName
        {
            get
            {
                string className = ClassName;

                if (className.Contains('<'))
                    return className.Substring(0, className.IndexOf('<'));
                return className;
            }
        }

        public virtual string FullClassName
        {
            get
            {
                if (DeclaredInType != null)
                    return string.Format("{0}.{1}", DeclaredInType.FullClassName, ClassName);
                if (Namespace != null)
                    return string.Format("{0}.{1}", Namespace, ClassName);
                return ClassName;
            }
        }

        protected virtual UserTypeField ExtractField(IDiaSymbol field, UserTypeFactory factory, UserTypeGenerationFlags options, bool extractingBaseClass = false)
        {
            var symbol = Symbol;
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool isStatic = (DataKind)field.dataKind == DataKind.StaticMember;
            UserTypeTree fieldType = GetTypeString(field.type, factory, field.length);
            string fieldName = field.name;
            string gettingField = "variable.GetField";
            string simpleFieldValue;

            if (isStatic)
            {
                simpleFieldValue = string.Format("Process.Current.GetGlobal(\"{0}!{1}::{2}\")", ModuleName, Symbol.name, fieldName);
            }
            else
            {
                if (useThisClass)
                {
                    gettingField = "thisClass.Value.GetClassField";
                }

                simpleFieldValue = string.Format("{0}(\"{1}\")", gettingField, fieldName);
            }

            UserTypeField userTypeField = ExtractField(field, fieldType, factory, simpleFieldValue, gettingField, isStatic, options, extractingBaseClass);
            UserTypeTransformation transformation = factory.FindTransformation(field.type, this);

            if (transformation != null)
            {
                string newFieldTypeString = transformation.TransformType();
                string fieldOffset = string.Format("{0}.GetFieldOffset(\"{1}\")", useThisClass ? "variable" : "thisClass.Value", field.name);

                if (isStatic)
                {
                    fieldOffset = "<Field offset was used on a static member>";
                }

                userTypeField.ConstructorText = transformation.TransformConstructor(userTypeField.SimpleFieldValue, fieldOffset);
                userTypeField.FieldType = newFieldTypeString;
            }

            if (extractingBaseClass)
            {
                if (fieldType is UserTypeTreeUserType && ((UserTypeTreeUserType)fieldType).UserType is FakeUserType)
                    userTypeField.ConstructorText = string.Format("CastAs<{0}>()", fieldType.GetUserTypeString());
                else if (useThisClass)
                    userTypeField.ConstructorText = userTypeField.ConstructorText.Replace("thisClass.Value.GetClassField", "GetBaseClass");
                else
                    userTypeField.ConstructorText = userTypeField.ConstructorText.Replace("variable.GetField", "GetBaseClass");
            }

            return userTypeField;
        }

        protected virtual UserTypeField ExtractField(IDiaSymbol field, UserTypeTree fieldType, UserTypeFactory factory, string simpleFieldValue, string gettingField, bool isStatic, UserTypeGenerationFlags options, bool extractingBaseClass)
        {
            bool forceUserTypesToNewInsteadOfCasting = options.HasFlag(UserTypeGenerationFlags.ForceUserTypesToNewInsteadOfCasting);
            bool cacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields);
            bool cacheStaticUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheStaticUserTypeFields);
            bool lazyCacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields);
            string fieldName = field.name;
            string castingTypeString = GetCastingString(fieldType);
            string constructorText;
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool castWithNewInsteadOfCasting = forceUserTypesToNewInsteadOfCasting && factory.ContainsSymbol(castingTypeString);
            var fieldTypeString = fieldType.GetUserTypeString();

            if (string.IsNullOrEmpty(castingTypeString))
            {
                constructorText = simpleFieldValue;
            }
            else if (fieldType is UserTypeTreeEnum)
            {
                constructorText = string.Format("({0})(ulong){1}", castingTypeString, simpleFieldValue);
            }
            else if (castingTypeString == "string")
            {
                constructorText = string.Format("{0}.ToString()", simpleFieldValue);
            }
            else if (fieldType is UserTypeTreeBaseType && castingTypeString != "NakedPointer")
            {
                constructorText = string.Format("({0}){1}", castingTypeString, simpleFieldValue);
            }
            else if (fieldType is UserTypeTreeBaseType || fieldType is UserTypeTreeCodeFunction || fieldType is UserTypeTreeCodeArray || fieldType is UserTypeTreeCodePointer || castWithNewInsteadOfCasting)
            {
                constructorText = string.Format("new {0}({1})", castingTypeString, simpleFieldValue);
            }
            else
            {
                if (isStatic || !useThisClass)
                {
                    constructorText = string.Format("{1}.CastAs<{0}>()", castingTypeString, simpleFieldValue);
                }
                else
                {
                   constructorText = string.Format("{0}<{1}>(\"{2}\")", gettingField, castingTypeString, fieldName);
                }
            }

            return new UserTypeField
            {
                ConstructorText = constructorText,
                FieldName = "_" + fieldName,
                FieldType = fieldTypeString,
                FieldTypeInfoComment = string.Format("// {0} {1};", TypeToString.GetTypeString(field.type), fieldName),
                PropertyName = fieldName,
                Static = isStatic,
                UseUserMember = lazyCacheUserTypeFields,
                CacheResult = cacheUserTypeFields || (isStatic && cacheStaticUserTypeFields),
                SimpleFieldValue = simpleFieldValue,
            };
        }

        internal void UpdateUserTypes(UserTypeFactory factory, UserTypeGenerationFlags generationOptions)
        {
            var fields = Symbol.GetChildren(SymTagEnum.SymTagData);

            foreach (var field in fields)
            {
                var type = field.type;

                if ((SymTagEnum)type.symTag == SymTagEnum.SymTagEnum)
                {
                    if (!factory.ContainsSymbol(type))
                    {
                        factory.AddSymbol(type, null, ModuleName, generationOptions);
                    }
                }
            }
        }

        internal IEnumerable<UserTypeField> ExtractFields(UserTypeFactory factory, UserTypeGenerationFlags options)
        {
            var symbol = Symbol;
            var fields = symbol.GetChildren(SymTagEnum.SymTagData);
            bool hasNonStatic = false;
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);

            foreach (var field in fields)
            {
                if (IsFieldFiltered(field))
                    continue;

                var userField = ExtractField(field, factory, options);

                yield return userField;
                hasNonStatic = hasNonStatic || !userField.Static;
            }

            foreach (var field in GetAutoGeneratedFields(hasNonStatic, useThisClass))
                yield return field;
        }

        protected virtual IEnumerable<UserTypeField> GetAutoGeneratedFields(bool hasNonStatic, bool useThisClass)
        {
            if (hasNonStatic && useThisClass)
            {
                yield return new UserTypeField
                {
                    ConstructorText = string.Format("variable.GetBaseClass(baseClassString)"),
                    FieldName = "thisClass",
                    FieldType = "Variable",
                    FieldTypeInfoComment = null,
                    PropertyName = null,
                    Static = false,
                    UseUserMember = true,
                    CacheResult = true,
                };
            }

            if (hasNonStatic && useThisClass)
            {
                yield return new UserTypeField
                {
                    ConstructorText = string.Format("GetBaseClassString(typeof({0}))", FullClassName),
                    FieldName = "baseClassString",
                    FieldType = "string",
                    FieldTypeInfoComment = null,
                    PropertyName = null,
                    Static = true,
                    UseUserMember = false,
                    CacheResult = true,
                };
            }
        }

        public virtual void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            var symbol = Symbol;
            var moduleName = ModuleName;
            var fields = ExtractFields(factory, options).OrderBy(f => !f.Static).ThenBy(f => f.FieldName).ToArray();
            bool hasStatic = fields.Where(f => f.Static).Any(), hasNonStatic = fields.Where(f => !f.Static).Any();
            UserTypeTree baseType = GetBaseTypeString(error, symbol, factory);
            var baseClasses = symbol.GetBaseClasses().ToArray();

            if (DeclaredInType == null)
            {
                if (Usings.Count > 0)
                {
                    foreach (var u in Usings.OrderBy(s => s))
                        output.WriteLine(indentation, "using {0};", u);
                    output.WriteLine();
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    output.WriteLine(indentation, "namespace {0}", Namespace);
                    output.WriteLine(indentation++, "{{");
                }
            }

            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
            {
                if (baseClasses.Length > 0)
                    output.WriteLine(indentation, "// {0} is inherited from:", ClassName);
                foreach (var type in baseClasses)
                    output.WriteLine(indentation, "//   {0}", type.name);
            }
            output.WriteLine(indentation, @"[UserType(ModuleName = ""{0}"", TypeName = ""{1}"")]", moduleName, XmlType.Name);
            output.WriteLine(indentation, @"public partial class {0} : {1}", ClassName, baseType);
            output.WriteLine(indentation++, @"{{");

            foreach (var field in fields)
            {
                if ((field.Static && !ExportStaticFields && field.FieldTypeInfoComment != null) || (!field.CacheResult && !field.UseUserMember))
                    continue;
                field.WriteFieldCode(output, indentation, options);
            }

            var constructors = GenerateConstructors();

            foreach (var constructor in constructors)
                constructor.WriteCode(output, indentation, fields, ConstructorName, ExportStaticFields);

            bool firstField = true;
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.PropertyName) || (field.Static && !ExportStaticFields && field.FieldTypeInfoComment != null))
                    continue;
                field.WritePropertyCode(output, indentation, options, ref firstField);
            }

            // Inner types
            foreach (var innerType in InnerTypes)
            {
                output.WriteLine();
                innerType.WriteCode(output, error, factory, options, indentation);
            }

            if (baseType is UserTypeTreeMultiClassInheritance)
            {
                // Write all properties for getting base classes
                foreach (var type in baseClasses)
                {
                    var field = ExtractField(type, factory, options, true);

                    field.PropertyName = field.PropertyName.Replace(" ", "").Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace("__", "_").TrimEnd('_');
                    output.WriteLine();
                    if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(field.FieldTypeInfoComment))
                        output.WriteLine(indentation, "// Property for getting base class: {0}", type.name);
                    if (baseClasses.Length == 1)
                        output.WriteLine(indentation, "public {0} BaseClass", field.FieldType);
                    else
                        output.WriteLine(indentation, "public {0} BaseClass_{1}", field.FieldType, field.PropertyName);
                    output.WriteLine(indentation++, "{{");
                    output.WriteLine(indentation, "get");
                    output.WriteLine(indentation++, "{{");
                    output.WriteLine(indentation, "return {0};", field.ConstructorText);
                    output.WriteLine(--indentation, "}}");
                    output.WriteLine(--indentation, "}}");
                }
            }

            // Class end
            output.WriteLine(--indentation, @"}}");

            if (DeclaredInType == null && !string.IsNullOrEmpty(Namespace))
            {
                output.WriteLine(--indentation, "}}");
            }
        }

        protected virtual IEnumerable<UserTypeConstructor> GenerateConstructors()
        {
            yield return new UserTypeConstructor()
            {
                ContainsFieldDefinitions = true,
                Static = true,
            };

            yield return new UserTypeConstructor()
            {
                Arguments = "Variable variable",
                BaseClassInitialization = "base(variable)",
                ContainsFieldDefinitions = true,
                Static = false,
            };
        }

        private bool IsFieldFiltered(IDiaSymbol field)
        {
            return (XmlType.IncludedFields.Count > 0 && !XmlType.IncludedFields.Contains(field.name))
                || XmlType.ExcludedFields.Contains(field.name);
        }

        public void SetDeclaredInType(UserType declaredInType)
        {
            DeclaredInType = declaredInType;
            declaredInType.InnerTypes.Add(this);
            foreach (var u in Usings)
            {
                declaredInType.Usings.Add(u);
            }
        }

        protected virtual string GetCastingString(UserTypeTree typeTree)
        {
            if (typeTree is UserTypeTreeVariable)
                return "";
            return typeTree.GetUserTypeString();
        }

        protected virtual UserTypeTree GetBaseTypeString(TextWriter error, IDiaSymbol type, UserTypeFactory factory)
        {
            var baseClasses = type.GetBaseClasses().ToArray();

            if (baseClasses.Length > 1)
            {
                return new UserTypeTreeMultiClassInheritance();
            }

            if (baseClasses.Length == 1)
            {
                type = baseClasses[0];

                UserType userType;

                if (factory.GetUserType(type, out userType))
                {
                    return UserTypeTreeUserType.Create(userType, factory);
                }

                var transformation = factory.FindTransformation(type, this);

                if (transformation != null)
                {
                    return new UserTypeTreeTransformation(transformation);
                }

                return GetBaseTypeString(error, type, factory);
            }

            return new UserTypeTreeVariable(false);
        }

        public virtual UserTypeTree GetTypeString(IDiaSymbol type, UserTypeFactory factory, ulong bitLength = 0)
        {
            UserType fakeUserType;

            if (factory.GetUserType(type, out fakeUserType) && fakeUserType is FakeUserType)
            {
                return new UserTypeTreeUserType(fakeUserType);
            }

            switch ((SymTagEnum)type.symTag)
            {
                case SymTagEnum.SymTagBaseType:
                    if (bitLength == 1)
                        return new UserTypeTreeBaseType("bool");
                    switch ((BasicType)type.baseType)
                    {
                        case BasicType.Bit:
                        case BasicType.Bool:
                            return new UserTypeTreeBaseType("bool");
                        case BasicType.Char:
                        case BasicType.WChar:
                            return new UserTypeTreeBaseType("char");
                        case BasicType.BSTR:
                            return new UserTypeTreeBaseType("string");
                        case BasicType.Void:
                            return new UserTypeTreeBaseType("void");
                        case BasicType.Float:
                            return new UserTypeTreeBaseType(type.length <= 4 ? "float" : "double");
                        case BasicType.Int:
                        case BasicType.Long:
                            switch (type.length)
                            {
                                case 0:
                                    return new UserTypeTreeBaseType("void");
                                case 1:
                                    return new UserTypeTreeBaseType("sbyte");
                                case 2:
                                    return new UserTypeTreeBaseType("short");
                                case 4:
                                    return new UserTypeTreeBaseType("int");
                                case 8:
                                    return new UserTypeTreeBaseType("long");
                                default:
                                    throw new Exception("Unexpected type length " + type.length);
                            }

                        case BasicType.UInt:
                        case BasicType.ULong:
                            switch (type.length)
                            {
                                case 0:
                                    return new UserTypeTreeBaseType("void");
                                case 1:
                                    return new UserTypeTreeBaseType("byte");
                                case 2:
                                    return new UserTypeTreeBaseType("ushort");
                                case 4:
                                    return new UserTypeTreeBaseType("uint");
                                case 8:
                                    return new UserTypeTreeBaseType("ulong");
                                default:
                                    throw new Exception("Unexpected type length " + type.length);
                            }

                        case BasicType.Hresult:
                            return new UserTypeTreeBaseType("Hresult");
                        default:
                            throw new Exception("Unexpected basic type " + (BasicType)type.baseType);
                    }

                case SymTagEnum.SymTagPointerType:
                    {
                        IDiaSymbol pointerType = type.type;

                        if (factory.GetUserType(pointerType, out fakeUserType) && fakeUserType is FakeUserType)
                        {
                            return new UserTypeTreeCodePointer(new UserTypeTreeUserType(fakeUserType));
                        }

                        switch ((SymTagEnum)pointerType.symTag)
                        {
                            case SymTagEnum.SymTagBaseType:
                            case SymTagEnum.SymTagEnum:
                                {
                                    string innerType = GetTypeString(pointerType, factory).GetUserTypeString();

                                    if (innerType == "void")
                                        return new UserTypeTreeBaseType("NakedPointer");
                                    if (innerType == "char")
                                        return new UserTypeTreeBaseType("string");
                                    return new UserTypeTreeCodePointer(GetTypeString(pointerType, factory));
                                }

                            case SymTagEnum.SymTagUDT:
                                return GetTypeString(pointerType, factory);
                            default:
                                return new UserTypeTreeCodePointer(GetTypeString(pointerType, factory));
                        }
                    }

                case SymTagEnum.SymTagUDT:
                case SymTagEnum.SymTagEnum:
                    {
                        UserType userType;

                        if (factory.GetUserType(type, out userType))
                        {
                            if (userType is EnumUserType)
                                return new UserTypeTreeEnum((EnumUserType)userType);
                            return UserTypeTreeUserType.Create(userType, factory);
                        }

                        if ((SymTagEnum)type.symTag == SymTagEnum.SymTagEnum)
                        {
                            return new UserTypeTreeBaseType("uint");
                        }

                        var transformation = factory.FindTransformation(type, this);

                        if (transformation != null)
                        {
                            return new UserTypeTreeTransformation(transformation);
                        }

                        return new UserTypeTreeVariable();
                    }

                case SymTagEnum.SymTagArrayType:
                    return new UserTypeTreeCodeArray(GetTypeString(type.type, factory));

                case SymTagEnum.SymTagFunctionType:
                    return new UserTypeTreeCodeFunction();

                default:
                    throw new Exception("Unexpected type tag " + (SymTagEnum)type.symTag);
            }
        }

        internal virtual bool Matches(string typeString, UserTypeFactory factory)
        {
            return Symbol.name == typeString;
        }

        internal virtual bool Matches(IDiaSymbol type, UserTypeFactory factory)
        {
            return Matches(TypeToString.GetTypeString(type), factory);
        }
    }

    class PhysicalUserType : UserType
    {
        public PhysicalUserType(IDiaSymbol symbol, XmlType xmlType, string moduleName)
            : base(symbol, xmlType, moduleName)
        {
        }

        protected override IEnumerable<UserTypeField> GetAutoGeneratedFields(bool hasNonStatic, bool useThisClass)
        {
            if (hasNonStatic && useThisClass)
            {
                // TODO: Remove this in the future
                yield return new UserTypeField
                {
                    ConstructorText = string.Format("variable.GetBaseClass(baseClassString)"),
                    FieldName = "thisClass",
                    FieldType = "Variable",
                    FieldTypeInfoComment = null,
                    PropertyName = null,
                    Static = false,
                    UseUserMember = true,
                    CacheResult = true,
                };
            }

            yield return new UserTypeField
            {
                ConstructorText = string.Format("GetBaseClassString(typeof({0}))", FullClassName),
                FieldName = "baseClassString",
                FieldType = "string",
                FieldTypeInfoComment = null,
                PropertyName = null,
                Static = true,
                UseUserMember = false,
                CacheResult = true,
            };
        }

        protected override IEnumerable<UserTypeConstructor> GenerateConstructors()
        {
            yield return new UserTypeConstructor()
            {
                ContainsFieldDefinitions = true,
                Static = true,
            };

            yield return new UserTypeConstructor()
            {
                Arguments = "Variable variable",
                BaseClassInitialization = string.Format("this(variable.GetBaseClass(baseClassString), Debugger.ReadMemory(variable.GetCodeType().Module.Process, variable.GetBaseClass(baseClassString).GetPointerAddress(), {0}), 0, variable.GetBaseClass(baseClassString).GetPointerAddress())", Symbol.length),
                ContainsFieldDefinitions = false,
                Static = false,
            };

            yield return new UserTypeConstructor()
            {
                Arguments = "Variable variable, byte[] buffer, int offset, ulong bufferAddress",
                BaseClassInitialization = "base(variable, buffer, offset, bufferAddress)",
                ContainsFieldDefinitions = true,
                Static = false,
            };
        }

        protected override UserTypeField ExtractField(IDiaSymbol field, UserTypeTree fieldType, UserTypeFactory factory, string simpleFieldValue, string gettingField, bool isStatic, UserTypeGenerationFlags options, bool extractingBaseClass)
        {
            if (!isStatic)
            {
                bool lazyCacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields);
                bool cacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields);
                bool cacheStaticUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheStaticUserTypeFields);
                string constructorText = "";
                string fieldName = field.name;
                string fieldTypeString = fieldType.GetUserTypeString();
                UserTypeTreeBaseType baseType = fieldType as UserTypeTreeBaseType;
                UserTypeTreeCodeArray codeArrayType = fieldType as UserTypeTreeCodeArray;
                UserTypeTreeUserType userType = fieldType as UserTypeTreeUserType;
                bool isEmbedded = (SymTagEnum)field.type.symTag != SymTagEnum.SymTagPointerType;

                if (baseType != null)
                {
                    if (baseType.BaseType == "string")
                    {
                        ulong charSize = field.type.type.length;

                        constructorText = string.Format("ReadString(GetCodeType().Module.Process, ReadPointer(memoryBuffer, memoryBufferOffset + {0}, {1}), {2})", field.offset, field.type.length, charSize);
                    }
                    else if (baseType.BaseType != "NakedPointer")
                    {
                        if ((LocationType)field.locationType == LocationType.BitField)
                        {
                            ulong bits = field.length;
                            ulong bitsOffset = field.bitPosition;

                            constructorText = string.Format("Read{0}(memoryBuffer, memoryBufferOffset + {1}, {2}, {3})", baseType.GetUserTypeString().UppercaseFirst(), field.offset, field.length, field.bitPosition);
                        }
                        else
                            constructorText = string.Format("Read{0}(memoryBuffer, memoryBufferOffset + {1})", baseType.GetUserTypeString().UppercaseFirst(), field.offset);
                    }
                }
                else if (codeArrayType != null)
                {
                    if (codeArrayType.InnerType is UserTypeTreeBaseType)
                    {
                        baseType = (UserTypeTreeBaseType)codeArrayType.InnerType;
                        if (baseType != null && baseType.BaseType != "string" && baseType.BaseType != "NakedPointer")
                        {
                            ulong arraySize = field.type.length;
                            ulong elementSize = field.type.type.length;

                            if (baseType.BaseType == "char")
                                constructorText = string.Format("Read{0}Array(memoryBuffer, memoryBufferOffset + {1}, {2}, {3})", baseType.GetUserTypeString().UppercaseFirst(), field.offset, arraySize / elementSize, elementSize);
                            else
                                constructorText = string.Format("Read{0}Array(memoryBuffer, memoryBufferOffset + {1}, {2})", baseType.GetUserTypeString().UppercaseFirst(), field.offset, arraySize / elementSize);
                            fieldTypeString = baseType.GetUserTypeString() + "[]";
                        }
                    }
                }
                else if (userType != null)
                {
                    if (!(userType.UserType is EnumUserType) && !(userType.UserType is TemplateUserType) && !extractingBaseClass)
                    {
                        string thisClassCodeType = "thisClass.Value.GetCodeType()";

                        if (!isEmbedded)
                        {
                            string fieldAddress = string.Format("ReadPointer(memoryBuffer, memoryBufferOffset + {0}, {1})", field.offset, field.type.length);
                            string fieldVariable = string.Format("Variable.CreatePointer({0}.GetClassFieldType(\"{1}\"), {2}, \"{1}\")", thisClassCodeType, fieldName, fieldAddress);

                            constructorText = string.Format("CastAs<{1}>({0})", fieldVariable, fieldTypeString);
                        }
                        else
                        {
                            string fieldAddress = string.Format("memoryBufferAddress + (ulong)(memoryBufferOffset + {0})", field.offset);
                            string fieldVariable = string.Format("Variable.Create({0}.GetClassFieldType(\"{1}\"), {2}, \"{1}\")", thisClassCodeType, fieldName, fieldAddress);

                            constructorText = string.Format("new {0}({1}, memoryBuffer, memoryBufferOffset + {2}, memoryBufferAddress)", fieldTypeString, fieldVariable, field.offset);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(constructorText))
                    return new UserTypeField()
                    {
                        ConstructorText = constructorText,
                        FieldName = "_" + fieldName,
                        FieldType = fieldTypeString,
                        FieldTypeInfoComment = string.Format("// {0} {1};", TypeToString.GetTypeString(field.type), fieldName),
                        PropertyName = fieldName,
                        Static = isStatic,
                        UseUserMember = lazyCacheUserTypeFields,
                        CacheResult = cacheUserTypeFields || (isStatic && cacheStaticUserTypeFields),
                        SimpleFieldValue = simpleFieldValue,
                    };
            }

            return base.ExtractField(field, fieldType, factory, simpleFieldValue, gettingField, isStatic, options, extractingBaseClass);
        }
    }

    class EnumUserType : UserType
    {
        public EnumUserType(IDiaSymbol symbol, string moduleName)
            : base(symbol, new XmlType() { Name = symbol.name }, moduleName)
        {
        }

        public override void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            if (DeclaredInType == null)
            {
                if (!string.IsNullOrEmpty(Namespace))
                {
                    output.WriteLine(indentation, "namespace {0}", Namespace);
                    output.WriteLine(indentation++, "{{");
                }
            }

            output.WriteLine(indentation, @"public enum {0}", ClassName);
            output.WriteLine(indentation++, @"{{");

            foreach (var enumValue in Symbol.GetChildren())
            {
                output.WriteLine(indentation, "{0} = {1},", enumValue.name, enumValue.value);
            }

            // Class end
            output.WriteLine(--indentation, @"}}");

            if (DeclaredInType == null && !string.IsNullOrEmpty(Namespace))
            {
                output.WriteLine(--indentation, "}}");
            }
        }
    }
}
