using CsDebugScript.CodeGen.UserTypes;
using System;
using System.Linq;

namespace CsDebugScript.CodeGen
{
    internal class UserTypeFunction : UserTypeField
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

    internal class UserTypeField
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
                if (options.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                {
                    if (firstField)
                    {
                        output.WriteLine();
                        firstField = false;
                    }

                    if (!UseUserMember && !CacheResult && options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
                        output.WriteLine(indentation, FieldTypeInfoComment);
                    if (UseUserMember && CacheResult)
                        output.WriteLine(indentation, "public {0}{1} {2} {{ get {{ return {3}.Value; }} }}", Static ? "static " : "", FieldType, PropertyName, FieldName);
                    else if (CacheResult)
                        output.WriteLine(indentation, "public {0}{1} {2} {{ get {{ return {3}; }} }}", Static ? "static " : "", FieldType, PropertyName, FieldName);
                    else
                        output.WriteLine(indentation, "public {0}{1} {2} {{ get {{ return {3}; }} }}", Static ? "static " : "", FieldType, PropertyName, ConstructorText);
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
        /// Gets the name of the property based on the field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="userType">Type of the user.</param>
        /// <returns>The name of the property.</returns>
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

    internal class UserTypeConstructor
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

    internal class UserTypeTransformation
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
