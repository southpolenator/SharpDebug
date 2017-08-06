using CsDebugScript.CodeGen.SymbolProviders;
using System.Linq;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Class represents field in user type
    /// </summary>
    internal class UserTypeField
    {
        /// <summary>
        /// Gets or sets the simple field value. It represents code that gets field variable and is used when creating UserTypeTransformation.
        /// </summary>
        public string SimpleFieldValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the constructor code.
        /// </summary>
        public string ConstructorText { get; set; }

        /// <summary>
        /// Gets or sets the field type information comment.
        /// </summary>
        public string FieldTypeInfoComment { get; set; }

        /// <summary>
        /// Gets or sets the constant value (if field is a constant).
        /// </summary>
        public string ConstantValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if field is static; otherwise, <c>false</c>.
        /// </value>
        public bool Static { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether field should use UserMember caching.
        /// </summary>
        /// <value>
        ///   <c>true</c> if field should use UserMember caching; otherwise, <c>false</c>.
        /// </value>
        public bool UseUserMember { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether field value should be cached inside the user type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if field value should be cached inside the user type; otherwise, <c>false</c>.
        /// </value>
        public bool CacheResult { get; set; }

        /// <summary>
        /// Gets or sets the access level (public/private/internal/protected).
        /// </summary>
        public string Access { get; set; } = "private";

        /// <summary>
        /// Gets or sets a value indicating whether this field overrides base class field.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this field overrides base class field; otherwise, <c>false</c>.
        /// </value>
        public bool OverrideWithNew { get; set; } = false;

        /// <summary>
        /// Gets the constant value if this field is a constant.
        /// </summary>
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

        /// <summary>
        /// Writes the field code to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The current indentation.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        public virtual void WriteFieldCode(IndentedWriter output, int indentation, UserTypeGenerationFlags generationFlags)
        {
            if (generationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
                output.WriteLine(indentation, FieldTypeInfoComment);
            if (!string.IsNullOrEmpty(ConstantValue))
                output.WriteLine(indentation, "public static {0} {1} = ({0}){2};", FieldType, FieldName, GetConstantValue());
            else if (Static && !UseUserMember)
                output.WriteLine(indentation, "{3} static {0} {1} = {2};", FieldType, FieldName, ConstructorText, Access);
            else if (UseUserMember && CacheResult)
                output.WriteLine(indentation, "{3}{4} {0}UserMember<{1}> {2};", Static ? "static " : "", FieldType, FieldName, Access, OverrideWithNew ? " new" : "");
            else if (CacheResult)
                output.WriteLine(indentation, "{3}{4} {0}{1} {2};", Static ? "static " : "", FieldType, FieldName, Access, OverrideWithNew ? " new" : "");
            if (generationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                output.WriteLine();
        }

        /// <summary>
        /// Writes the constructor code to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The current indentation.</param>
        public virtual void WriteConstructorCode(IndentedWriter output, int indentation)
        {
            if (string.IsNullOrEmpty(ConstantValue))
                if (UseUserMember && CacheResult)
                    output.WriteLine(indentation, "{0} = UserMember.Create(() => {1});", FieldName, ConstructorText);
                else if (CacheResult)
                    output.WriteLine(indentation, "{2}{0} = {1};", FieldName, ConstructorText, !Static ? "this." : "");
        }

        /// <summary>
        /// Writes the property code to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The current indentation.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <param name="firstField">if set to <c>true</c> this is the first field in the user type.</param>
        public virtual void WritePropertyCode(IndentedWriter output, int indentation, UserTypeGenerationFlags generationFlags, ref bool firstField)
        {
            if (string.IsNullOrEmpty(ConstantValue))
                if (generationFlags.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                {
                    if (firstField)
                    {
                        output.WriteLine();
                        firstField = false;
                    }

                    if (!UseUserMember && !CacheResult && generationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
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
                    if (!UseUserMember && !CacheResult && generationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(FieldTypeInfoComment))
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
        /// <param name="field">The field.</param>
        /// <param name="userType">The user type owning the field.</param>
        /// <returns>The name of the property.</returns>
        public static string GetPropertyName(ISymbolField field, UserType userType)
        {
            if (!string.IsNullOrEmpty(field.PropertyName))
            {
                return field.PropertyName;
            }

            string fieldName = field.Name;

            // Check if field name is the same as owner type name
            if (fieldName == userType.Symbol.Name) // TODO: Check if this should be userType.ConstructorName
            {
                // Property name cannot be equal to the class name
                return string.Format("_{0}", fieldName);
            }

            // Check if field name is the same as any inner type name
            foreach (string className in userType.InnerTypes.Select(r => r.ClassName)) // TODO: Check if this should be r.ConstructorName
            {
                if (fieldName == className)
                {
                    // Property name cannot be equal to the class name
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
                case "namespace":
                case "public":
                case "private":
                case "decimal":
                    return string.Format("_{0}", fieldName);
                default:
                    break;
            }

            return fieldName;
        }
    }
}
