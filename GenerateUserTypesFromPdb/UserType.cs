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
        SingleFileExport = 256,
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

    internal abstract class UserTypeTree
    {
        public abstract string GetUserTypeString();

        public override string ToString()
        {
            return GetUserTypeString();
        }
    }

    internal class UserTypeTreeUserType : UserTypeTree
    {
        protected UserTypeTreeUserType(UserType userType)
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
                return new UserTypeTreeGenericsType(templateType, factory);
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

    class UserTypeTreeGenericsType : UserTypeTreeUserType
    {
        public bool CanInstatiate;

        public UserTypeTreeGenericsType(TemplateUserType genericsType, UserTypeFactory factory)
            : base(genericsType)
        {
            GenericsType = genericsType;

            Symbol[] arguments = genericsType.ExtractSpecializedSymbols();

            CanInstatiate = true;

            SpecializedArguments = new UserTypeTree[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                UserType userType;
                factory.GetUserType(arguments[i], out userType);

                if (userType != null)
                {
                    SpecializedArguments[i] = UserTypeTreeUserType.Create(userType, factory);
                }
                else
                {
                    CanInstatiate = false;
                    // #fixme can't deal with it
                    SpecializedArguments[i] = genericsType.GetTypeString(arguments[i], factory);
                }
            }
        }

        public TemplateUserType GenericsType { get; private set; }
        public UserTypeTree[] SpecializedArguments { get; private set; }

        public override string GetUserTypeString()
        {
            return GenericsType.GetSpecializedType(SpecializedArguments.Select(t => t.GetUserTypeString()).ToArray());
        }
    }

    internal class UserTypeTreeEnum : UserTypeTreeUserType
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

    internal class UserTypeTreeCodePointer : UserTypeTree
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

    internal class UserTypeTreeCodeArray : UserTypeTree
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

    internal class UserTypeTreeCodeFunction : UserTypeTree
    {
        public override string GetUserTypeString()
        {
            return "CodeFunction";
        }
    }

    internal class UserTypeTreeVariable : UserTypeTree
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

    internal class UserTypeStaticClass : UserTypeTree
    {
        public UserTypeStaticClass()
        {
        }

        public override string GetUserTypeString()
        {
            return string.Empty;
        }
    }

    internal class UserTypeTreeMultiClassInheritance : UserTypeTreeVariable
    {
        public UserTypeTreeMultiClassInheritance()
            : base(false)
        {
        }
    }

    internal class UserTypeTreeTransformation : UserTypeTree
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

        private string GetConstantValue()
        {
            if (ConstantValue.StartsWith("-"))
                switch (FieldType)
                {
                    case "ulong":
                        return ((ulong)long.Parse(ConstantValue)).ToString();
                    case "uint":
                        return ((uint)int.Parse(ConstantValue)).ToString();
                    case "ushort":
                        return ((ushort)short.Parse(ConstantValue)).ToString();
                    case "byte":
                        return ((byte)sbyte.Parse(ConstantValue)).ToString();
                }

            if (FieldType == "bool")
                return (int.Parse(ConstantValue) != 0).ToString().ToLower();
            return "(" + ConstantValue + ")";
        }

        public void WriteFieldCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options)
        {
            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
                output.WriteLine(indentation, FieldTypeInfoComment);
            if (!string.IsNullOrEmpty(ConstantValue))
                output.WriteLine(indentation, "public static {0} {1} = ({0}){2};", FieldType, FieldName, GetConstantValue());
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


        /// <summary>
        /// Gets property name based on the fieldName
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetPropertyName(string fieldName, string className)
        {
            if (fieldName == className)
            {
                // property name cannot be equal to class name
                return string.Format("_{0}", fieldName);
            }

            fieldName = fieldName.Replace("::", "_");

            switch (fieldName)
            {
                case "lock":
                case "base":
                case "params":
                case "enum":
                case "in":
                case "object":
                case "event":
                case "string":
                    return string.Format("_{0}", fieldName);
                default:
                    break;
            }

            return fieldName;
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
        private Func<string, string> typeConverter;
        private UserType ownerUserType;
        private Symbol type;

        public UserTypeTransformation(XmlTypeTransformation transformation, Func<string, string> typeConverter, UserType ownerUserType, Symbol type)
        {
            this.Transformation = transformation;
            this.typeConverter = typeConverter;
            this.ownerUserType = ownerUserType;
            this.type = type;
        }

        internal string TransformType()
        {
            string originalFieldTypeString = type.Name;

            return Transformation.TransformType(originalFieldTypeString, ownerUserType.ClassName, typeConverter);
        }

        internal string TransformConstructor(string field, string fieldOffset)
        {
            string originalFieldTypeString = type.Name;

            return Transformation.TransformConstructor(originalFieldTypeString, field, fieldOffset, ownerUserType.ClassName, typeConverter);
        }

        public XmlTypeTransformation Transformation { get; private set; }
    }
}
