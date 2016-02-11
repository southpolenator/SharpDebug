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
        SingleLineProperty,
        GenerateFieldTypeInfoComment,
        UseClassFieldsFromDiaSymbolProvider,
        ForceUserTypesToNewInsteadOfCasting,
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
        public string FieldName { get; set; }

        public string FieldType { get; set; }

        public string PropertyName { get; set; }

        public string ConstructorText { get; set; }

        public string FieldTypeInfoComment { get; set; }

        public bool Static { get; set; }
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
        private List<UserType> userTypes = new List<UserType>();
        private XmlTypeTransformation[] typeTransformations;

        public UserTypeFactory(XmlTypeTransformation[] transformations)
        {
            typeTransformations = transformations;
        }

        public IEnumerable<UserType> Symbols
        {
            get
            {
                return userTypes;
            }
        }

        internal bool GetUserType(string typeString, out UserType userType)
        {
            userType = userTypes.FirstOrDefault(t => t.Symbol.name == typeString);
            return userType != null;
        }

        internal bool GetUserType(IDiaSymbol type, out UserType userType)
        {
            userType = userTypes.FirstOrDefault(t => t.Symbol.symIndexId == type.symIndexId);
            return userType != null;
        }

        internal void AddSymbol(IDiaSymbol symbol, XmlType type, string moduleName)
        {
            if (type == null)
            {
                userTypes.Add(new EnumUserType(symbol, moduleName));
            }
            else
            {
                userTypes.Add(new UserType(symbol, type, moduleName));
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

        public string ClassName
        {
            get
            {
                string symbolName = Symbol.name;
                string newSymbolName = symbolName.Replace("::", ".").Replace('<', '_').Replace('>', '_').Replace(' ', '_').Replace(',', '_').Replace("__", "_").Replace("__", "_").TrimEnd('_');

                if (symbolName != newSymbolName)
                {
                    symbolName = newSymbolName;
                }

                if (DeclaredInType != null)
                {
                    return symbolName.Substring(DeclaredInType.FullClassName.Length + 1);
                }

                return symbolName;
            }
        }

        public string FullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.FullClassName, ClassName);
                }

                if (Namespace != null)
                {
                    return string.Format("{0}.{1}", Namespace, ClassName);
                }

                return ClassName;
            }
        }

        private UserTypeField ExtractField(IDiaSymbol field, UserTypeFactory factory, UserTypeGenerationFlags options)
        {
            var symbol = Symbol;
            var moduleName = ModuleName;
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool forceUserTypesToNewInsteadOfCasting = options.HasFlag(UserTypeGenerationFlags.ForceUserTypesToNewInsteadOfCasting);

            bool isStatic = (DataKind)field.dataKind == DataKind.StaticMember;
            UserTypeTree fieldType = GetTypeString(field.type, factory, field.length);
            string castingTypeString = GetCastingString(fieldType);
            string fieldName = field.name;
            string simpleFieldValue;
            string constructorText;
            bool castWithNewInsteadOfCasting = forceUserTypesToNewInsteadOfCasting && factory.Symbols.Select(t => t.FullClassName).Contains(castingTypeString);

            if (isStatic)
            {
                simpleFieldValue = string.Format("Process.Current.GetGlobal(\"{0}!{1}::{2}\")", moduleName, symbol.name, field.name);
            }
            else
            {
                string gettingField = "variable.GetField";

                if (options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider))
                {
                    gettingField = "thisClass.Value.GetClassField";
                }

                simpleFieldValue = string.Format("{0}(\"{1}\")", gettingField, field.name);
            }

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
                constructorText = string.Format("{1}.CastAs<{0}>()", castingTypeString, simpleFieldValue);
            }

            var fieldTypeString = fieldType.GetUserTypeString();
            UserTypeTransformation transformation = factory.FindTransformation(field.type, this);

            if (transformation != null)
            {
                string newFieldTypeString = transformation.TransformType();
                string fieldOffset = string.Format("{0}.GetFieldOffset(\"{1}\")", useThisClass ? "variable" : "thisClass.Value", field.name);

                if (isStatic)
                {
                    fieldOffset = "<Field offset was used on a static member>";
                }

                constructorText = transformation.TransformConstructor(simpleFieldValue, fieldOffset);
                fieldTypeString = newFieldTypeString;
            }

            return new UserTypeField
            {
                ConstructorText = constructorText,
                FieldName = "_" + fieldName,
                FieldType = fieldTypeString,
                FieldTypeInfoComment = string.Format("// {0} {1};", TypeToString.GetTypeString(field.type), field.name),
                PropertyName = fieldName,
                Static = isStatic,
            };
        }

        internal void UpdateUserTypes(UserTypeFactory factory)
        {
            var fields = Symbol.GetChildren(SymTagEnum.SymTagData);

            foreach (var field in fields)
            {
                var type = field.type;

                if ((SymTagEnum)type.symTag == SymTagEnum.SymTagEnum)
                {
                    if (!factory.ContainsSymbol(type))
                    {
                        factory.AddSymbol(type, null, ModuleName);
                    }
                }
            }
        }

        private IEnumerable<UserTypeField> ExtractFields(UserTypeFactory factory, UserTypeGenerationFlags options)
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

            if (hasNonStatic && useThisClass)
            {
                yield return new UserTypeField
                {
                    ConstructorText = string.Format("variable.GetBaseClass(\"{0}\")", symbol.name),
                    FieldName = "thisClass",
                    FieldType = "Variable",
                    FieldTypeInfoComment = null,
                    PropertyName = null,
                    Static = false,
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
                    {
                        output.WriteLine(indentation, "using {0};", u);
                    }

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
            output.WriteLine(indentation, @"[UserType(ModuleName = ""{0}"", TypeName = ""{1}"")]", moduleName, symbol.name);
            output.WriteLine(indentation, @"public partial class {0} : {1}", ClassName, baseType);
            output.WriteLine(indentation++, @"{{");

            foreach (var field in fields)
            {
                if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(field.FieldTypeInfoComment))
                    output.WriteLine(indentation, field.FieldTypeInfoComment);
                output.WriteLine(indentation, "private {0}UserMember<{1}> {2};", field.Static ? "static " : "", field.FieldType, field.FieldName);
                if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                    output.WriteLine();
            }

            // Static type initialization
            if (hasStatic)
            {
                output.WriteLine();
                output.WriteLine(indentation, "static {0}()", ClassName);
                output.WriteLine(indentation++, "{{");

                foreach (var field in fields)
                {
                    if (field.Static)
                    {
                        output.WriteLine(indentation, "{0} = UserMember.Create(() => {1});", field.FieldName, field.ConstructorText);
                    }
                }

                output.WriteLine(--indentation, "}}");
            }

            // We always want to have constructor because base class expects variable as parameter in its constructor
            //if (hasNonStatic)
            {
                output.WriteLine();
                output.WriteLine(indentation, "public {0}(Variable variable)", ClassName);
                output.WriteLine(indentation + 1, ": base(variable)");
                output.WriteLine(indentation++, "{{");

                foreach (var field in fields)
                {
                    if (!field.Static)
                    {
                        output.WriteLine(indentation, "{0} = UserMember.Create(() => {1});", field.FieldName, field.ConstructorText);
                    }
                }

                output.WriteLine(--indentation, "}}");
            }

            bool firstField = true;
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.PropertyName))
                {
                    continue;
                }

                if (options.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                {
                    if (firstField)
                    {
                        output.WriteLine();
                        firstField = false;
                    }

                    output.WriteLine(indentation, "public {0}{1} {2} {{ get {{ return {3}.Value; }} }}", field.Static ? "static " : "", field.FieldType, field.PropertyName, field.FieldName);
                }
                else
                {
                    output.WriteLine();
                    output.WriteLine(indentation, "public {0}{1} {2}", field.Static ? "static " : "", field.FieldType, field.PropertyName);
                    output.WriteLine(indentation++, "{{");
                    output.WriteLine(indentation, "get");
                    output.WriteLine(indentation++, "{{");
                    output.WriteLine(indentation, "return {0}.Value;", field.FieldName);
                    output.WriteLine(--indentation, "}}");
                    output.WriteLine(--indentation, "}}");
                }
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
                    var field = ExtractField(type, factory, options);

                    field.PropertyName = field.PropertyName.Replace(" ", "").Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace("__", "_").TrimEnd('_');
                    if (options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider))
                        field.ConstructorText = field.ConstructorText.Replace("thisClass.Value.GetClassField", "GetBaseClass");
                    else
                        field.ConstructorText = field.ConstructorText.Replace("variable.GetField", "GetBaseClass");
                    output.WriteLine();
                    if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(field.FieldTypeInfoComment))
                        output.WriteLine(indentation, "// Property for getting base class: {0}", type.name);
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

        private static string GetCastingString(UserTypeTree typeTree)
        {
            if (typeTree is UserTypeTreeVariable)
                return "";
            return typeTree.GetUserTypeString();
        }

        private static UserTypeTree GetBaseTypeString(TextWriter error, IDiaSymbol type, UserTypeFactory factory)
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
                    return new UserTypeTreeUserType(userType);
                }

                return GetBaseTypeString(error, type, factory);
            }

            return new UserTypeTreeVariable(false);
        }

        private UserTypeTree GetTypeString(IDiaSymbol type, UserTypeFactory factory, ulong bitLength = 0)
        {
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
                            return new UserTypeTreeUserType(userType);
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
    }

    class EnumUserType : UserType
    {
        public EnumUserType(IDiaSymbol symbol, string moduleName)
            : base(symbol, new XmlType() { Name = symbol.name, IncludedFields = new HashSet<string>(), ExcludedFields = new HashSet<string>() }, moduleName)
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
