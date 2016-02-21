using Dia2Lib;
using GenerateUserTypesFromPdb.UserTypes;
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
            {
                return new UserTypeSpecializedGenericsType(templateType, factory);
            }

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

    class UserTypeTreeGenericsType : UserTypeSpecializedGenericsType
    {
        public UserTypeTreeGenericsType(TemplateUserType genericsType, UserTypeFactory factory)
            : base(genericsType, factory)
        {
        }
        public override string GetUserTypeString()
        {
            return GenericsType.GetSpecializedType(SpecializedArguments.Select(t => t.GetUserTypeString()).ToArray());
            //return GenericsType.GetSpecializedType(this.GenericsType.ExtractSpecializedTypes());
        }
    }

    class UserTypeSpecializedGenericsType : UserTypeTreeUserType
    {
        public UserTypeSpecializedGenericsType(TemplateUserType genericsType, UserTypeFactory factory)
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
            //#fixem
            //return GenericsType.GetSpecializedType(SpecializedArguments.Select(t => t.GetUserTypeString()).ToArray());
            return GenericsType.GetSpecializedType(this.GenericsType.ExtractSpecializedTypes());
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

    class UserTypeStaticClass : UserTypeTree
    {
        public UserTypeStaticClass()
        {
        }

        public override string GetUserTypeString()
        {
            return string.Empty;
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

        public string ConstantValue { get; set; }

        public bool Static { get; set; }

        public bool UseUserMember { get; set; }

        public bool CacheResult { get; set; }

        public void WriteFieldCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options)
        {
            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
                output.WriteLine(indentation, FieldTypeInfoComment);
            if (!string.IsNullOrEmpty(ConstantValue))
                output.WriteLine(indentation, "public static {0} {1} = ({0}){2};", FieldType, FieldName, ConstantValue);
            else if (UseUserMember && CacheResult)
                output.WriteLine(indentation, "private {0}UserMember<{1}> {2};", Static ? "static " : "", FieldType, FieldName);
            else if (CacheResult)
                output.WriteLine(indentation, "private {0}{1} {2};", Static ? "static " : "", FieldType, FieldName);
            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                output.WriteLine();
        }

        public void WriteConstructorCode(IndentedWriter output, int indentation)
        {
            if (string.IsNullOrEmpty(ConstantValue))
                if (UseUserMember && CacheResult)
                    output.WriteLine(indentation, "{0} = UserMember.Create(() => {1});", FieldName, ConstructorText);
                else if (CacheResult)
                    output.WriteLine(indentation, "{0} = {1};", FieldName, ConstructorText);
        }

        public void WritePropertyCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options, ref bool firstField)
        {
            if (FieldName == "m_evtWaitFor")
            {

            }

            if (string.IsNullOrEmpty(ConstantValue))
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

        public List<UserType> Symbols
        {
            get
            {
                return userTypes;
            }
        }

        internal virtual bool TryGetUserType(string typeString, out UserType userType)
        {
            try
            {
                GlobalCache.UserTypesBySymbolName.TryGetValue(typeString, out userType);

                //#fixme, fast aproach for now
                //userType = userTypes.FirstOrDefault(t => t.Matches(typeString, this));
                return userType != null;
            }
            catch (Exception)
            {
                // unable to match the type
                userType = null;
                return false;
            }
        }

        internal virtual bool GetUserType(IDiaSymbol type, out UserType userType)
        {
            if (type.name != null && type.name.StartsWith("CAutoRefc<WaitStatsSink>"))
            {

            }
            
            string typeString = TypeToString.GetTypeString(type);

            GlobalCache.UserTypesBySymbolName.TryGetValue(typeString, out userType);

            if (userType == null)
            {
                return false;
            }

            //#fixme
            var matchingTypes = userTypes.Where(t => t.Matches(typeString, this)).ToArray(); ;

            userType = matchingTypes.OfType<PhysicalUserType>().FirstOrDefault();

            if (userType != null)
            {
                return true;
            }

            if (matchingTypes.Count() >= 3)
            {

            }

            userType = matchingTypes.FirstOrDefault(r => r.Symbol.name == type.name);

            if (userType != null)
            {
                return true;
            }

            userType = matchingTypes.FirstOrDefault();

            if (userType != null)
            {
                var templateType = userType as TemplateUserType;
                //#fixme
                if (userType.Symbol.name == "CAutoRefc<IClassFactory>")
                {

                }

                string typeName = type.name;

                if (templateType != null)
                {
                    var specializedType = templateType.specializedTypes.FirstOrDefault(r => r.Symbol.name == typeName);

                    userType = specializedType;
                }
            }


            return userType != null;
        }

        internal void AddUserType(UserType userType)
        {
            //#fixme
            userTypes.Add(userType);
        }

        internal void InserUserType(UserType userType)
        {
            userTypes.Insert(0, userType);
        }

        internal void AddSymbol(IDiaSymbol symbol, XmlType type, string moduleName, UserTypeGenerationFlags generationOptions)
        {
            UserType newUserType;

            if (type == null)
            {
                newUserType = new EnumUserType(symbol, moduleName);
            }
            else if (generationOptions.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
            {
                newUserType = new PhysicalUserType(symbol, type, moduleName);
            }
            else
            {
                newUserType = new UserType(symbol, type, moduleName);
            }

            userTypes.Add(newUserType);

            // Store in global cache
            string typeName = newUserType.Symbol.name;
            if (!GlobalCache.UserTypesBySymbolName.TryAdd(typeName, newUserType))
            {
                //#failed
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
                var buckets = new Dictionary<int, TemplateUserType>();

                foreach (IDiaSymbol diaSymbol in symbols)
                {
                    try
                    {
                        // We want to ignore "empty" generic classes (for now)
                        if (diaSymbol.name == null || diaSymbol.length == 0)
                        {
                            continue;
                        }

                        TemplateUserType templateType = new TemplateUserType(session, diaSymbol, type, moduleName, this);

                        int templateArgs = templateType.GenericsArguments;

                        TemplateUserType previousTemplateType;

                        if (!buckets.TryGetValue(templateArgs, out previousTemplateType))
                        {
                            // Add new template type
                            buckets.Add(templateArgs, templateType);
                        }
                        else
                        {
                            previousTemplateType.specializedTypes.Add(templateType);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                // Add newly generated types
                foreach (var template in buckets.Values)
                {
                    userTypes.Add(template);
                }
            }
        }

        internal void ProcessTypes()
        {
            int index = 0;
            foreach (var userType in userTypes)
            {
                string parentClassSymbolName = userType.ParentClassSymbolName;
                UserType parentUserType;

                GlobalCache.UserTypesBySymbolName.TryGetValue(parentClassSymbolName, out parentUserType);

                if (userType.ClassName.Contains("XE_LiveWriter.MetadataBlockEntry"))
                {

                }

                //var fullClassName = userType.FullClassName;
                //int lastIndex = fullClassName.LastIndexOf('.');


                //var parentTypeName = lastIndex > 0 ? fullClassName.Substring(0, lastIndex) : null;
                //if (parentTypeName != userType.Namespace)
                {
                    //var parentType = userTypes.FirstOrDefault(t => t.FullClassName == parentTypeName);

                    if (parentUserType != null)
                    {
                        userType.SetDeclaredInType(parentUserType);
                    }
                    else
                    if (!string.IsNullOrEmpty(parentClassSymbolName))
                    {
                        // if there is not parent class, that set up correct namespace
                        userType.NamespaceSymbol = parentClassSymbolName;

                        if (parentClassSymbolName == "DetourFindFunction::__l29")
                        {

                        }
                       
                    }
                    else
                    {
                        //throw new Exception("Unsupported namespace of class " + userType.Symbol.name);
                    }
                }

                Console.WriteLine("{0}:{1}", index++, userTypes.Count());
            }
        }

        internal bool ContainsSymbol(IDiaSymbol type)
        {
            UserType userType;

            string typeString = TypeToString.GetTypeString(type);

            GlobalCache.UserTypesBySymbolName.TryGetValue(typeString, out userType);

            return (userType != null);
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

                if (TryGetUserType(inputType, out userType))
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

            return TryGetUserType(typeString, out userType);
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
                // always make module namespace
                if (Namespace != null)
                {
                    output.WriteLine(indentation, "namespace {0}.{1}", ModuleName, Namespace);
                }
                else
                {
                    output.WriteLine(indentation, "namespace {0}", ModuleName);
                }
                output.WriteLine(indentation++, "{{");
            }

            output.WriteLine(indentation, @"public enum {0}", ClassName);
            output.WriteLine(indentation++, @"{{");

            foreach (var enumValue in Symbol.GetChildren())
            {
                output.WriteLine(indentation, "{0} = {1},", enumValue.name, enumValue.value);
            }

            // Class end
            output.WriteLine(--indentation, @"}}");

            if (DeclaredInType == null)
            {
                output.WriteLine(--indentation, "}}");
            }
        }
    }

    class GlobalsUserType : UserType
    {
        private IDiaSession session;

        public GlobalsUserType(IDiaSession session, string moduleName)
            : base(session.globalScope, new XmlType() { Name = "Globals" }, moduleName)
        {
            this.session = session;
        }

        public override string ClassName
        {
            get
            {
                return "ModuleGlobals";
            }
        }

        internal override IEnumerable<UserTypeField> ExtractFields(UserTypeFactory factory, UserTypeGenerationFlags options)
        {
            var fields = session.globalScope.GetChildren(SymTagEnum.SymTagData).OrderBy(s => s.name).ToArray();
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            string previousName = "";

            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.type.name))
                {
                    continue;
                }

                if (IsFieldFiltered(field) || field.name == previousName)
                    continue;

                UserType userType;
                factory.TryGetUserType(field.type.name, out userType);

                if (userType == null)
                {
                    continue;
                }

                var userField = ExtractField(field, factory, options, forceIsStatic: true);

                userField.FieldName = userField.FieldName.Replace("?", "_").Replace("$", "_").Replace("@", "_").Replace(":", "_").Replace(" ", "_").Replace("<", "_").Replace(">", "_").Replace("*", "_").Replace(",", "_");
                userField.PropertyName = userField.PropertyName.Replace("?", "_").Replace("$", "_").Replace("@", "_").Replace(":", "_").Replace(" ", "_").Replace("<", "_").Replace(">", "_").Replace("*", "_").Replace(",", "_");

                yield return userField;
                previousName = field.name;
            }

            foreach (var field in GetAutoGeneratedFields(false, useThisClass))
                yield return field;
        }

        internal override bool Matches(IDiaSymbol type, UserTypeFactory factory)
        {
            return false;
        }

        internal override bool Matches(string typeString, UserTypeFactory factory)
        {
            return false;
        }

        protected override UserTypeTree GetBaseTypeString(TextWriter error, IDiaSymbol type, UserTypeFactory factory)
        {
            return new UserTypeStaticClass();
        }

        protected override IEnumerable<UserTypeConstructor> GenerateConstructors()
        {
            yield return new UserTypeConstructor()
            {
                ContainsFieldDefinitions = true,
                Static = true,
            };
        }
    }
}
