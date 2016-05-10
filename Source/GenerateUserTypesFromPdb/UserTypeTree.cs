using Dia2Lib;
using GenerateUserTypesFromPdb.UserTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        UseHungarianNotation = 512,
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
            if (userType == null)
                throw new ArgumentNullException("userType");

            var type = userType;

            while (type != null)
            {
                var templateType = type as TemplateUserType;

                if (templateType != null)
                    return new UserTypeTreeGenericsType(userType, factory);
                type = type.DeclaredInType;
            }

            var enumType = userType as EnumUserType;

            if (enumType != null)
                return new UserTypeTreeEnum(enumType);

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

    /// <summary>
    /// Class represent Generic Argument Type.  
    /// User when we know that the type is representing Generic Specialization.
    /// </summary>
    class UserTypeTreeArgumentGenericsType : UserTypeTreeGenericsType
    {
        private int argumentNumber;

        public UserTypeTreeArgumentGenericsType(int genericArgumentNumber, UserType originalUserType, UserTypeFactory factory)
            : base(originalUserType, factory)
        {
            argumentNumber = genericArgumentNumber;
        }

        public override string GetUserTypeString()
        {
            return "T" + (argumentNumber > 0 ? argumentNumber.ToString() : string.Empty);
        }
    }

    class UserTypeTreeGenericsType : UserTypeTreeUserType
    {
        public bool CanInstatiate;

        public UserTypeTreeGenericsType(UserType originalUserType, UserTypeFactory factory)
            : base(originalUserType)
        {
            // Get all types
            var type = originalUserType;
            var declaredInList = new List<UserType>();

            while (type != null)
            {
                declaredInList.Add(type);
                type = type.DeclaredInType;
            }

            declaredInList.Reverse();
            DeclaredInArray = declaredInList.ToArray();
            SpecializedArguments = new UserTypeTree[DeclaredInArray.Length][];

            // Extract all template types
            CanInstatiate = true;
            for (int j = 0; j < DeclaredInArray.Length; j++)
            {
                TemplateUserType templateType = DeclaredInArray[j] as TemplateUserType;

                if (templateType == null)
                    continue;

                Symbol[] arguments = templateType.ExtractSpecializedSymbols();
                var specializedArguments = new UserTypeTree[arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                {
                    UserType userType;
                    factory.GetUserType(arguments[i], out userType);

                    if (userType != null)
                    {
                        specializedArguments[i] = UserTypeTreeUserType.Create(userType, factory);
                        var genericsTree = specializedArguments[i] as UserTypeTreeGenericsType;

                        if (genericsTree != null && !genericsTree.CanInstatiate)
                            CanInstatiate = false;
                    }
                    else
                    {
                        Symbol symbol = originalUserType.Symbol.Module.GetTypeSymbol(arguments[i].Name);

                        if (symbol.Tag != SymTagEnum.SymTagBaseType)
                        {
                            // Base Types (Primitive Types) can be used for specialization
                            CanInstatiate = false;
                        }
                        
                        // #fixme can't deal with it
                        specializedArguments[i] = templateType.GetTypeString(arguments[i], factory);
                    }
                }

                SpecializedArguments[j] = specializedArguments;
            }
        }

        public UserType[] DeclaredInArray { get; private set; }

        public UserTypeTree[][] SpecializedArguments { get; private set; }

        public override string GetUserTypeString()
        {
            StringBuilder sb = new StringBuilder();

            if (DeclaredInArray[0].Namespace != null)
            {
                sb.Append(DeclaredInArray[0].Namespace);
                sb.Append('.');
            }

            for (int j = 0; j < DeclaredInArray.Length; j++)
            {
                UserType userType = DeclaredInArray[j];
                TemplateUserType templateType = userType as TemplateUserType;
                NamespaceUserType namespaceType = userType as NamespaceUserType;

                if (templateType != null)
                    sb.Append(templateType.GetSpecializedType(SpecializedArguments[j].Select(t => t.GetUserTypeString()).ToArray()));
                else if (namespaceType != null)
                {
                    if (j == 0)
                        continue;
                    sb.Append(namespaceType.Namespace);
                }
                else
                    sb.Append(userType.ClassName);
                sb.Append('.');
            }

            sb.Length--;
            return sb.ToString();
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

    internal class UserTypeTreeSingleClassInheritanceWithInterfaces : UserTypeTree
    {
        public UserTypeTreeSingleClassInheritanceWithInterfaces(UserType userType, UserTypeFactory factory)
        {
            UserType = UserTypeTreeUserType.Create(userType, factory);
        }

        public UserTypeTree UserType { get; private set; }

        public override string GetUserTypeString()
        {
            return UserType.GetUserTypeString();
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

    class UserTypeFunction : UserTypeField
    {
        public override void WriteConstructorCode(IndentedWriter output, int indentation)
        {
            output.WriteLine(indentation, "{0}();", FieldName);
        }

        public override void WriteFieldCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options)
        {
            output.WriteLine(indentation, "{0} {1}();", FieldType, FieldName);
        }

        public override void WritePropertyCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options, ref bool firstField)
        {
            // Do nothing
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

        public string Access { get; set; } = "private";

        public bool OverrideWithNew { get; set; } = false;

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

        public virtual void WriteFieldCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options)
        {
            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
                output.WriteLine(indentation, FieldTypeInfoComment);
            if (!string.IsNullOrEmpty(ConstantValue))
                output.WriteLine(indentation, "public static {0} {1} = ({0}){2};", FieldType, FieldName, GetConstantValue());
            else if (Static && !UseUserMember)
                output.WriteLine(indentation, "{3} static {0} {1} = {2};", FieldType, FieldName, ConstructorText, Access);
            else if (UseUserMember && CacheResult)
                output.WriteLine(indentation, "{3}{4} {0}UserMember<{1}> {2};", Static ? "static " : "", FieldType, FieldName, Access, OverrideWithNew ? " new" : "");
            else if (CacheResult)
                output.WriteLine(indentation, "{3}{4} {0}{1} {2};", Static ? "static " : "", FieldType, FieldName, Access, OverrideWithNew ? " new" : "");
            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                output.WriteLine();
        }

        public virtual void WriteConstructorCode(IndentedWriter output, int indentation)
        {
            if (string.IsNullOrEmpty(ConstantValue))
                if (UseUserMember && CacheResult)
                    output.WriteLine(indentation, "{0} = UserMember.Create(() => {1});", FieldName, ConstructorText);
                else if (CacheResult)
                    output.WriteLine(indentation, "{2}{0} = {1};", FieldName, ConstructorText, !Static ? "this." : "");
        }

        public virtual void WritePropertyCode(IndentedWriter output, int indentation, UserTypeGenerationFlags options, ref bool firstField)
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
        public static string GetPropertyName(string fieldName, UserType userType)
        {
            if (fieldName == userType.Symbol.Name)
            {
                // property name cannot be equal to class name
                return string.Format("_{0}", fieldName);
            }

            foreach (string className in userType.InnerTypes.Select(r => r.ClassName))
            {
                if (fieldName == className)
                {
                    // property name cannot be equal to class name
                    return string.Format("_{0}", fieldName);
                }
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
                case "fixed":
                case "internal":
                case "out":
                case "override":
                case "virtual":
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
                // Do nothing. We are initializing static variables in declaration statement.
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
