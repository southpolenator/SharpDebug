using CsDebugScript.CodeGen.TypeTrees;
using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsDebugScript.CodeGen.UserTypes
{
    /// <summary>
    /// Base class for any exported user type
    /// </summary>
    internal class UserType
    {
        /// <summary>
        /// Flag that saves if thisClass variable was used during generation and should be exported.
        /// </summary>
        protected bool usedThisClass = false;

        /// <summary>
        /// The "parent" user type where this user type is declared in.
        /// </summary>
        private UserType declaredInType;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="symbol">The symbol we are generating this user type from.</param>
        /// <param name="xmlType">The XML description of the type.</param>
        /// <param name="nameSpace">The namespace it belongs to.</param>
        public UserType(Symbol symbol, XmlType xmlType, string nameSpace)
        {
            Symbol = symbol;
            XmlType = xmlType;
            InnerTypes = new List<UserType>();
            Usings = new HashSet<string>(new string[] { "CsDebugScript" });
            NamespaceSymbol = nameSpace;
        }

        /// <summary>
        /// Gets the symbol we are generating this user type from..
        /// </summary>
        public Symbol Symbol { get; private set; }

        /// <summary>
        /// Gets the original name of this user type.
        /// </summary>
        protected string TypeName
        {
            get
            {
                return XmlType != null ? XmlType.Name : Symbol.Name;
            }
        }

        /// <summary>
        /// Gets or sets the namespace in symbol name.
        /// </summary>
        protected string NamespaceSymbol { get; set; }

        /// <summary>
        /// Gets the normalized Namespace. If this user type is defined as nested type it does include parent type name.
        /// </summary>
        public string Namespace
        {
            get
            {
                if (NamespaceSymbol != null)
                {
                    return NormalizeSymbolName(NamespaceSymbol.Replace("::", "."));
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the "parent" user type where this user type is declared in.
        /// </summary>
        public virtual UserType DeclaredInType
        {
            get
            {
                return declaredInType;
            }

            private set
            {
                declaredInType = value;
            }
        }

        /// <summary>
        /// Gets the list of types declared inside this type.
        /// </summary>
        public List<UserType> InnerTypes { get; private set; }

        /// <summary>
        /// Gets the list of using commands needed for this user type.
        /// </summary>
        public HashSet<string> Usings { get; private set; }

        /// <summary>
        /// Gets the XML type description.
        /// </summary>
        public XmlType XmlType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether user type should export static fields.
        /// </summary>
        /// <value>
        ///   <c>true</c> if user type should export static fields; otherwise, <c>false</c>.
        /// </value>
        internal bool ExportStaticFields { get; set; } = true;

        /// <summary>
        /// Gets the list of derived classes.
        /// </summary>
        internal HashSet<UserType> DerivedClasses { get; private set; } = new HashSet<UserType>();

        /// <summary>
        /// Gets or sets a value indicating whether user type should export dynamic fields.
        /// </summary>
        /// <value>
        ///   <c>true</c> if user type should export dynamic fields; otherwise, <c>false</c>.
        /// </value>
        internal bool ExportDynamicFields { get; set; } = true;

        /// <summary>
        /// Gets the normalized symbol name.
        /// </summary>
        public string NormalizedSymbolName
        {
            get
            {
                return NormalizeSymbolName(Symbol.Name);
            }
        }

        /// <summary>
        /// Gets the class name for this user type. Class name doesn't contain namespace.
        /// </summary>
        public virtual string ClassName
        {
            get
            {
                string symbolName = Symbol.Name;

                if (DeclaredInType != null)
                {
                    symbolName = Symbol.Namespaces.Last();
                }

                symbolName = NormalizeSymbolName(symbolName);

                switch (symbolName)
                {
                    case "lock":
                    case "base":
                    case "params":
                    case "enum":
                    case "in":
                    case "object":
                    case "event":
                    case "string":
                        return string.Format("@{0}", symbolName);
                    default:
                        break;
                }

                return symbolName;
            }
        }

        /// <summary>
        /// Gets the name of the constructor for this user type.
        /// </summary>
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

        /// <summary>
        /// Gets the full name of the class, including namespace and "parent" type it is declared into.
        /// </summary>
        public virtual string FullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.FullClassName, ClassName);
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    return string.Format("{0}.{1}", Namespace, ClassName);
                }

                return string.Format("{0}", ClassName);
            }
        }

        /// <summary>
        /// Gets the full name of the class (specialized version), including namespace and "parent" type it is declared into.
        /// This specialized version of FullClassName returns it with original specialization.
        /// </summary>
        internal virtual string SpecializedFullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.SpecializedFullClassName, ClassName);
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    return string.Format("{0}.{1}", Namespace, ClassName);
                }

                return string.Format("{0}", ClassName);
            }
        }

        /// <summary>
        /// Gets the full name of the class (non-specialized version), including namespace and "parent" type it is declared into.
        /// This non-specialized version of FullClassName returns it with template being trimmed to just &lt;&gt;.
        /// </summary>
        internal virtual string NonSpecializedFullClassName
        {
            get
            {
                if (DeclaredInType != null)
                {
                    return string.Format("{0}.{1}", DeclaredInType.NonSpecializedFullClassName, ClassName);
                }

                if (!string.IsNullOrEmpty(Namespace))
                {
                    return string.Format("{0}.{1}", Namespace, ClassName);
                }

                return string.Format("{0}", ClassName);
            }
        }

        /// <summary>
        /// Normalizes the symbol name by removing special characters.
        /// </summary>
        /// <param name="symbolName">The symbol name.</param>
        /// <returns>Normalized symbol name.</returns>
        /// <remarks>
        /// Do not trim right, some of the classes start with '_'.
        /// We cannot replace __ with _ , it will generate class name collisions.
        /// </remarks>
        public static string NormalizeSymbolName(string symbolName)
        {
            return symbolName.Replace("::", "_").Replace("*", "").Replace("&", "").Replace('-', '_').Replace('<', '_').Replace('>', '_').Replace(' ', '_').Replace(',', '_').Replace('(', '_').Replace(')', '_').TrimEnd('_');
        }

        /// <summary>
        /// Gets the list of generic type constraints.
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        protected virtual IEnumerable<string> GetGenericTypeConstraints(UserTypeFactory factory)
        {
            // No generic type contains
            yield break;
        }

        /// <summary>
        /// Generates user type field based on the specified symbol field.
        /// </summary>
        /// <param name="field">The symbol field.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <param name="extractingBaseClass">if set to <c>true</c> user type field is being generated for getting base class.</param>
        /// <param name="forceIsStatic">if set to <c>true</c> user type field is generated as static.</param>
        protected virtual UserTypeField ExtractField(SymbolField field, UserTypeFactory factory, UserTypeGenerationFlags generationFlags, bool extractingBaseClass = false, bool forceIsStatic = false)
        {
            // Prepare data for ExtractFieldInternal
            bool useThisClass = generationFlags.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool isStatic = field.DataKind == DataKind.StaticMember || forceIsStatic;
            TypeTree fieldType = GetFieldTypeTree(field, factory, extractingBaseClass, field.Size);
            string fieldName = field.Name;
            string gettingField = "variable.GetField";
            string simpleFieldValue;
            bool usesThisClass = false;

            if (isStatic)
            {
                if (string.IsNullOrEmpty(Symbol.Name))
                {
                    simpleFieldValue = string.Format("Process.Current.GetGlobal(\"{0}!{1}\")", field.ParentType.Module.Name, fieldName);
                }
                else
                {
                    simpleFieldValue = string.Format("Process.Current.GetGlobal(\"{0}!{1}::{2}\")", field.ParentType.Module.Name, Symbol.Name, fieldName);
                }
            }
            else
            {
                if (useThisClass)
                {
                    gettingField = "thisClass.Value.GetClassField";
                    usesThisClass = true;
                }

                simpleFieldValue = string.Format("{0}(\"{1}\")", gettingField, fieldName);
            }

            // Do generate user type field
            UserTypeField userTypeField = ExtractFieldInternal(field, fieldType, factory, simpleFieldValue, gettingField, isStatic, generationFlags, extractingBaseClass);

            // Check if transformation should be applied
            UserTypeTransformation transformation = factory.FindTransformation(field.Type, this);

            if (transformation != null)
            {
                string newFieldTypeString = transformation.TransformType();
                string fieldOffset = string.Format("{0}.GetFieldOffset(\"{1}\")", useThisClass ? "variable" : "thisClass.Value", field.Name);

                usedThisClass = true;
                if (isStatic)
                {
                    fieldOffset = "<Field offset was used on a static member>";
                }

                userTypeField.ConstructorText = transformation.TransformConstructor(userTypeField.SimpleFieldValue, fieldOffset);
                userTypeField.FieldType = newFieldTypeString;
            }
            else if (usesThisClass && userTypeField.ConstructorText.Contains(gettingField))
            {
                usedThisClass = true;
            }

            // If we are generating field for getting base class, we need to "transform" code to do so.
            if (extractingBaseClass)
            {
                if (useThisClass)
                {
                    userTypeField.ConstructorText = userTypeField.ConstructorText.Replace("thisClass.Value.GetClassField", "thisClass.Value.GetBaseClass");
                    usedThisClass = true;
                }
                else
                    userTypeField.ConstructorText = userTypeField.ConstructorText.Replace("variable.GetField", "GetBaseClass");
            }

            return userTypeField;
        }

        /// <summary>
        /// Generates user type field based on the specified symbol field and all other fields that are prepared for this function.
        /// Do not use this function directly, unless you are calling it from overridden function.
        /// </summary>
        /// <param name="field">The symbol field.</param>
        /// <param name="fieldType">The field tree type.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="simpleFieldValue">The code foe "simple field value" used when creating transformation.</param>
        /// <param name="gettingField">The code for getting field variable.</param>
        /// <param name="isStatic">if set to <c>true</c> generated field should be static.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <param name="extractingBaseClass">if set to <c>true</c> user type field is being generated for getting base class.</param>
        protected virtual UserTypeField ExtractFieldInternal(SymbolField field, TypeTree fieldType, UserTypeFactory factory, string simpleFieldValue, string gettingField, bool isStatic, UserTypeGenerationFlags generationFlags, bool extractingBaseClass)
        {
            // Non-template user type must use template that can be instantiated.
            TemplateTypeTree fieldTypeAsTemplate = fieldType as TemplateTypeTree;

            if (!(this is TemplateUserType) && fieldTypeAsTemplate != null && !fieldTypeAsTemplate.CanInstantiate)
            {
                throw new Exception("Generics type cannot be instantiated");
            }

            // Prepare variables for generating user type field
            bool forceUserTypesToNewInsteadOfCasting = generationFlags.HasFlag(UserTypeGenerationFlags.ForceUserTypesToNewInsteadOfCasting);
            bool cacheUserTypeFields = generationFlags.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields);
            bool cacheStaticUserTypeFields = generationFlags.HasFlag(UserTypeGenerationFlags.CacheStaticUserTypeFields);
            bool lazyCacheUserTypeFields = generationFlags.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields);
            string fieldName = field.Name;
            string castingTypeString = GetCastingString(fieldType);
            string constructorText;
            bool useThisClass = generationFlags.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool castWithNewInsteadOfCasting = forceUserTypesToNewInsteadOfCasting && factory.ContainsSymbol(Symbol.Module, castingTypeString);
            var fieldTypeString = fieldType.GetTypeString();
            bool isConstant = field.LocationType == LocationType.Constant;
            string constantString = "";

            if (isConstant && generationFlags.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
            {
                constantString = field.Value.ToString();
            }

            // Generate constructor text based on different tree types
            if (string.IsNullOrEmpty(castingTypeString))
            {
                constructorText = simpleFieldValue;
            }
            else if (fieldType is EnumTreeType)
            {
                constructorText = string.Format("({0})(ulong){1}", castingTypeString, simpleFieldValue);
            }
            else if (castingTypeString == "string")
            {
                constructorText = string.Format("{0}.ToString()", simpleFieldValue);
            }
            else if (fieldType is BasicTypeTree && castingTypeString != "NakedPointer")
            {
                constructorText = string.Format("({0}){1}", castingTypeString, simpleFieldValue);
            }
            else if (fieldType is BasicTypeTree || fieldType is FunctionTypeTree || fieldType is ArrayTypeTree || fieldType is PointerTypeTree || castWithNewInsteadOfCasting)
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

            // TODO: More extensive checks are needed for property name. We must not create duplicate after adding '_'. For example: class has 'in' and '_in' fields.
            fieldName = UserTypeField.GetPropertyName(fieldName, this);

            // When creating property for BaseClass and current class is generic type
            // we need to rename BaseClass property name not to include specialization type.
            if (extractingBaseClass && this is TemplateUserType)
            {
                if (fieldTypeAsTemplate != null)
                {
                    fieldName = fieldTypeAsTemplate.UserType.ClassName;
                }
                else if (fieldType is TemplateArgumentTreeType || (fieldType is UserTypeTree && ((UserTypeTree)fieldType).UserType is TemplateArgumentUserType))
                {
                    fieldName = fieldType.GetTypeString();
                }
            }

            // Do create user type field
            return new UserTypeField
            {
                ConstructorText = constructorText,
                FieldName = "_" + fieldName,
                FieldType = fieldTypeString,
                FieldTypeInfoComment = string.Format("// {0} {1};", field.Type.Name, fieldName),
                PropertyName = fieldName,
                Static = isStatic,
                UseUserMember = lazyCacheUserTypeFields,
                CacheResult = cacheUserTypeFields || (isStatic && cacheStaticUserTypeFields),
                SimpleFieldValue = simpleFieldValue,
                ConstantValue = constantString,
            };
        }

        /// <summary>
        /// Adds the derived class to the list of derived classes.
        /// </summary>
        /// <param name="derivedClass">The derived class.</param>
        internal void AddDerivedClass(UserType derivedClass)
        {
            DerivedClasses.Add(derivedClass);
        }

        /// <summary>
        /// Extracts all fields from the user type.
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        protected virtual IEnumerable<UserTypeField> ExtractFields(UserTypeFactory factory, UserTypeGenerationFlags generationFlags)
        {
            bool hasNonStatic = false;
            bool useThisClass = generationFlags.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);

            if (ExportDynamicFields)
            {
                // Extract non-static fields
                foreach (var field in Symbol.Fields)
                {
                    if (IsFieldFiltered(field) || field.DataKind == DataKind.StaticMember)
                        continue;

                    var userField = ExtractField(field, factory, generationFlags);

                    yield return userField;
                    hasNonStatic = hasNonStatic || !userField.Static;
                }

                // Extract static fields
                if (ExportStaticFields)
                    foreach (var field in GlobalCache.GetSymbolStaticFields(Symbol))
                    {
                        if (IsFieldFiltered(field))
                            continue;

                        var userField = ExtractField(field, factory, generationFlags);

                        yield return userField;
                    }

                // Should we try to incorporate Hungarian notation into field decomposition
                if (generationFlags.HasFlag(UserTypeGenerationFlags.UseHungarianNotation))
                    foreach (var field in GenerateHungarianNotationFields(factory, generationFlags))
                        yield return field;
            }
            else
            {
                if (!ExportStaticFields)
                    throw new NotImplementedException();

                // Extract static fields
                foreach (var field in Symbol.Fields)
                {
                    if (IsFieldFiltered(field) || field.DataKind != DataKind.StaticMember || !field.IsValidStatic)
                        continue;

                    var userField = ExtractField(field, factory, generationFlags);

                    yield return userField;
                }
            }

            // Get auto generated fields
            foreach (var field in GetAutoGeneratedFields(hasNonStatic, useThisClass && usedThisClass))
                yield return field;
        }

        /// <summary>
        /// Try to generate fields based on the Hungarian notation used in class fields.
        /// </summary>
        /// <param name="factory">The user type factory.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        protected virtual IEnumerable<UserTypeField> GenerateHungarianNotationFields(UserTypeFactory factory, UserTypeGenerationFlags generationFlags)
        {
            // TODO: Add comments to this function and expand XML documentation comment
            const string CounterPrefix = "m_c";
            const string PointerPrefix = "m_p";
            const string ArrayPrefix = "m_rg";
            SymbolField[] fields = Symbol.Fields;
            IEnumerable<SymbolField> counterFields = fields.Where(r => r.Name.StartsWith(CounterPrefix));
            Dictionary<SymbolField, SymbolField> userTypesArrays = new Dictionary<SymbolField, SymbolField>();

            foreach (SymbolField counterField in counterFields)
            {
                if (counterField.Type.BasicType != BasicType.UInt &&
                    counterField.Type.BasicType != BasicType.Int &&
                    counterField.Type.BasicType != BasicType.Long &&
                    counterField.Type.BasicType != BasicType.ULong)
                {
                    continue;
                }

                if (counterField.Type.Tag == SymTagEnum.SymTagEnum)
                    continue;

                string counterNameSurfix = counterField.Name.Substring(CounterPrefix.Length);

                if (string.IsNullOrEmpty(counterNameSurfix))
                    continue;

                foreach (SymbolField pointerField in fields.Where(r => (r.Name.StartsWith(PointerPrefix) || r.Name.StartsWith(ArrayPrefix)) && r.Name.EndsWith(counterNameSurfix)))
                {
                    if ((counterField.IsStatic) != (pointerField.IsStatic))
                        continue;

                    if (pointerField.Type.Tag != SymTagEnum.SymTagPointerType)
                        continue;

                    if (userTypesArrays.ContainsKey(pointerField))
                    {
                        if (userTypesArrays[pointerField].Name.Length > counterField.Name.Length)
                        {
                            continue;
                        }
                    }

                    userTypesArrays[pointerField] = counterField;
                }
            }

            foreach (var userTypeArray in userTypesArrays)
            {
                var pointerField = userTypeArray.Key;
                var counterField = userTypeArray.Value;

                TypeTree fieldType = GetSymbolTypeTree(pointerField.Type, factory, pointerField.Size);

                if (fieldType is ArrayTypeTree)
                    continue;

                PointerTypeTree fieldTypeCodePointer = fieldType as PointerTypeTree;

                if (fieldTypeCodePointer != null)
                {
                    fieldType = fieldTypeCodePointer.ElementType;
                }

                fieldType = new ArrayTypeTree(fieldType);
                string fieldName = pointerField.Name + "Array";
                string constructorText = string.Format("new {0}({1}, {2})", fieldType, pointerField.Name, counterField.Name);
                string fieldTypeString = fieldType.GetTypeString();
                bool isStatic = pointerField.IsStatic;
                bool cacheUserTypeFields = generationFlags.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields);
                bool cacheStaticUserTypeFields = generationFlags.HasFlag(UserTypeGenerationFlags.CacheStaticUserTypeFields);
                bool lazyCacheUserTypeFields = generationFlags.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields);

                yield return new UserTypeField
                {
                    ConstructorText = constructorText,
                    FieldName = "_" + fieldName,
                    FieldType = fieldTypeString,
                    FieldTypeInfoComment = string.Format("// From Hungarian notation: {0} {1};", fieldTypeString, fieldName),
                    PropertyName = fieldName,
                    Static = isStatic,
                    CacheResult = cacheUserTypeFields || (isStatic && cacheStaticUserTypeFields),
                    SimpleFieldValue = string.Empty,
                    ConstantValue = string.Empty,
                };
            }
        }

        /// <summary>
        /// Gets the automatically generated fields.
        /// </summary>
        /// <param name="hasNonStatic">if set to <c>true</c> this class has dynamic fields.</param>
        /// <param name="useThisClass">if set to <c>true</c> this class is using thisClass variable.</param>
        /// <returns>The automatically generated fields.</returns>
        protected virtual IEnumerable<UserTypeField> GetAutoGeneratedFields(bool hasNonStatic, bool useThisClass)
        {
            if (ExportDynamicFields)
            {
                if (Symbol.Tag != SymTagEnum.SymTagExe && useThisClass)
                {
                    yield return new UserTypeField
                    {
                        ConstructorText = string.Format("GetBaseClass(baseClassString)"),
                        FieldName = "thisClass",
                        FieldType = "Variable",
                        FieldTypeInfoComment = null,
                        PropertyName = null,
                        Static = false,
                        UseUserMember = true,
                        CacheResult = true,
                    };
                }

                if (Symbol.Tag != SymTagEnum.SymTagExe)
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

                yield return new UserTypeFunction
                {
                    FieldName = "PartialInitialize",
                    FieldType = "partial void",
                    CacheResult = true,
                };
            }
        }

        /// <summary>
        /// Writes the class comment on the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="indentation">The current indentation.</param>
        protected virtual void WriteClassComment(IndentedWriter output, int indentation)
        {
            Symbol[] baseClasses = Symbol.BaseClasses;

            if (baseClasses.Length > 0)
                output.WriteLine(indentation, "// {0} (original name: {1}) is inherited from:", FullClassName, Symbol.Name);
            else
                output.WriteLine(indentation, "// {0} (original name: {1})", FullClassName, Symbol.Name);
            foreach (var type in baseClasses)
                output.WriteLine(indentation, "//   {0}", type.Name);
        }

        /// <summary>
        /// Writes the code for this user type to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="error">The error text writer.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="generationFlags">The user type generation flags.</param>
        /// <param name="indentation">The current indentation.</param>
        public virtual void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags generationFlags, int indentation = 0)
        {
            int baseClassOffset = 0;
            string nameSpace = null;
            TypeTree baseType = ExportDynamicFields ? GetBaseClassTypeTree(error, Symbol, factory, out baseClassOffset) : null;
            Symbol[] baseClasses = Symbol.BaseClasses;
            Symbol[] baseClassesForProperties = baseType is SingleClassInheritanceWithInterfacesTypeTree ? baseClasses.Where(b => b.IsEmpty).ToArray() : baseClasses;
            UserTypeField[] baseClassesForPropertiesAsFields = baseClassesForProperties.Select(type => ExtractField(type.CastAsSymbolField(), factory, generationFlags, true)).ToArray();
            var fields = ExtractFields(factory, generationFlags).OrderBy(f => !f.Static).ThenBy(f => f.FieldName != "ClassCodeType").ThenBy(f => f.GetType().Name).ThenBy(f => f.FieldName).ToArray();
            bool hasStatic = fields.Any(f => f.Static), hasNonStatic = fields.Any(f => !f.Static);

            // Check if we need to write usings and namespace
            if (DeclaredInType == null || (!generationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType))
            {
                if (Usings.Count > 0 && !generationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport))
                {
                    foreach (var u in Usings.OrderBy(s => s))
                        output.WriteLine(indentation, "using {0};", u);
                    output.WriteLine();
                }

                nameSpace = (DeclaredInType as NamespaceUserType)?.FullClassName ?? Namespace;
                if (!string.IsNullOrEmpty(nameSpace))
                {
                    output.WriteLine(indentation, "namespace {0}", nameSpace);
                    output.WriteLine(indentation++, "{{");
                }
            }

            // Write beginning of the class
            if (generationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                WriteClassComment(output, indentation);
            if (ExportDynamicFields)
            {
                // If symbol has vtable, we would like to add DerivedClassAttribute to it
                if (Symbol.HasVTable())
                    foreach (var derivedClass in DerivedClasses)
                    {
                        string fullClassName;

                        try
                        {
                            fullClassName = derivedClass.SpecializedFullClassName;
                        }
                        catch
                        {
                            fullClassName = derivedClass.NonSpecializedFullClassName;
                        }

                        output.WriteLine(indentation, @"[DerivedClass(Type = typeof({0}), Priority = {1}, TypeName = ""{2}"")]", fullClassName, derivedClass.DerivedClasses.Count, derivedClass.Symbol.Name);
                    }

                // Write all UserTypeAttributes and class header
                foreach (var moduleName in GlobalCache.GetSymbolModuleNames(Symbol))
                    output.WriteLine(indentation, @"[UserType(ModuleName = ""{0}"", TypeName = ""{1}"")]", moduleName, TypeName);
                output.WriteLine(indentation, @"public partial class {0} {1} {2}", ClassName, !string.IsNullOrEmpty(baseType.GetTypeString()) ? ":" : "", baseType);
            }
            else
                output.WriteLine(indentation, @"public static class {0}", ClassName);

            foreach (var genericTypeConstraint in GetGenericTypeConstraints(factory))
                output.WriteLine(indentation+1, genericTypeConstraint);
            output.WriteLine(indentation++, @"{{");

            // Write code for field declaration
            bool firstField = true;
            foreach (var field in fields)
            {
                if (((field.Static && !ExportStaticFields && field.FieldTypeInfoComment != null) || (!field.CacheResult && !field.UseUserMember)) && string.IsNullOrEmpty(field.ConstantValue))
                    continue;
                field.WriteFieldCode(output, indentation, generationFlags);
            }

            // Write code for constructors
            var constructors = GenerateConstructors(generationFlags);

            foreach (var constructor in constructors)
                constructor.WriteCode(output, indentation, fields, ConstructorName);

            // Write all properties
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.PropertyName) || (field.Static && !ExportStaticFields && field.FieldTypeInfoComment != null))
                    continue;
                field.WritePropertyCode(output, indentation, generationFlags, ref firstField);
            }

            // Write all inner types
            foreach (var innerType in InnerTypes)
            {
                output.WriteLine();
                innerType.WriteCode(output, error, factory, generationFlags, indentation);
            }

            // Check for multi class inheritance
            if (baseType is MultiClassInheritanceTypeTree || baseType is SingleClassInheritanceWithInterfacesTypeTree)
            {
                // Write all properties for getting base classes
                for (int i = 0; i < baseClassesForProperties.Length; i++)
                {
                    Symbol type = baseClassesForProperties[i];
                    UserTypeField field = baseClassesForPropertiesAsFields[i];
                    string singleLineDefinition = string.Empty;

                    // Change getting base class to use index instead of name.
                    // It works better with generics.
                    foreach (var baseClass in Symbol.BaseClasses)
                    {
                        string baseClassString = string.Format("\"{0}\"", baseClass.CastAsSymbolField().Name);
                        int index = field.ConstructorText.IndexOf(baseClassString);

                        if (index >= 0)
                        {
                            int baseClassIndex = Symbol.BaseClasses.OrderBy(s => s.Offset).ToList().IndexOf(baseClass);

                            if (field.ConstructorText.StartsWith("thisClass."))
                                field.ConstructorText = field.ConstructorText.Replace(baseClassString, baseClassIndex.ToString());
                            break;
                        }
                    }

                    // Generate single line definition
                    if (generationFlags.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                        singleLineDefinition = string.Format(" {{ get {{ return {0}; }} }}", field.ConstructorText);

                    // TODO: Verify getting base class for nested type
                    field.PropertyName = UserType.NormalizeSymbolName(field.PropertyName);

                    field.PropertyName = field.PropertyName.Replace(" ", "").Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace("__", "_").TrimEnd('_');
                    output.WriteLine();
                    if (generationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(field.FieldTypeInfoComment))
                        output.WriteLine(indentation, "// Property for getting base class: {0}", type.Name);
                    if (baseClasses.Length == 1)
                        output.WriteLine(indentation, "public {0} BaseClass{1}", field.FieldType, singleLineDefinition);
                    else
                        output.WriteLine(indentation, "public {0} BaseClass_{1}{2}", field.FieldType, field.PropertyName, singleLineDefinition);
                    if (!generationFlags.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                    {
                        output.WriteLine(indentation++, "{{");
                        output.WriteLine(indentation, "get");
                        output.WriteLine(indentation++, "{{");
                        output.WriteLine(indentation, "return {0};", field.ConstructorText);
                        output.WriteLine(--indentation, "}}");
                        output.WriteLine(--indentation, "}}");
                    }
                }
            }

            // Class end
            output.WriteLine(--indentation, @"}}");

            if ((DeclaredInType == null || (!generationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
            {
                output.WriteLine(--indentation, "}}");
            }
        }

        /// <summary>
        /// Generates the constructors.
        /// </summary>
        /// <param name="generationFlags">The user type generation flags.</param>
        protected virtual IEnumerable<UserTypeConstructor> GenerateConstructors(UserTypeGenerationFlags generationFlags)
        {
            yield return new UserTypeConstructor()
            {
                ContainsFieldDefinitions = true,
                Static = true,
            };

            if (ExportDynamicFields)
            {
                if (generationFlags.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
                {
                    yield return new UserTypeConstructor()
                    {
                        Arguments = "Variable variable",
                        BaseClassInitialization = "this(variable.GetBaseClass(baseClassString), Debugger.ReadMemory(variable.GetCodeType().Module.Process, variable.GetBaseClass(baseClassString).GetPointerAddress(), variable.GetBaseClass(baseClassString).GetCodeType().Size), 0, variable.GetBaseClass(baseClassString).GetPointerAddress())",
                        ContainsFieldDefinitions = true,
                        Static = false,
                    };

                    yield return new UserTypeConstructor()
                    {
                        Arguments = "Variable variable, CsDebugScript.Engine.Utility.MemoryBuffer buffer, int offset, ulong bufferAddress",
                        BaseClassInitialization = "base(variable, buffer, offset, bufferAddress)",
                        ContainsFieldDefinitions = true,
                        Static = false,
                    };

                    yield return new UserTypeConstructor()
                    {
                        Arguments = "CsDebugScript.Engine.Utility.MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name = Variable.ComputedName, string path = Variable.UnknownPath",
                        BaseClassInitialization = "base(buffer, offset, bufferAddress, codeType, address, name, path)",
                        ContainsFieldDefinitions = true,
                        Static = false,
                    };
                }
                else
                {
                    yield return new UserTypeConstructor()
                    {
                        Arguments = "Variable variable",
                        BaseClassInitialization = "base(variable)",
                        ContainsFieldDefinitions = true,
                        Static = false,
                    };
                }
            }
        }

        /// <summary>
        /// Determines whether the specified field should be filtered.
        /// </summary>
        /// <param name="field">The field.</param>
        protected bool IsFieldFiltered(SymbolField field)
        {
            return XmlType != null && ((XmlType.IncludedFields.Count > 0 && !XmlType.IncludedFields.Contains(field.Name))
                || XmlType.ExcludedFields.Contains(field.Name));
        }

        /// <summary>
        /// Updates the "parent" type.
        /// </summary>
        /// <param name="declaredInType">The "parent" type.</param>
        public void UpdateDeclaredInType(UserType declaredInType)
        {
            // Ignore it if it is null
            if (declaredInType == null)
                return;

            DeclaredInType = declaredInType;
            declaredInType.InnerTypes.Add(this);
            foreach (var u in Usings)
            {
                declaredInType.Usings.Add(u);
            }
        }

        /// <summary>
        /// Gets the casting string for the specified type tree.
        /// </summary>
        /// <param name="typeTree">The type tree.</param>
        private static string GetCastingString(TypeTree typeTree)
        {
            if (typeTree is VariableTypeTree)
                return "";
            return typeTree.GetTypeString();
        }

        /// <summary>
        /// Gets the type tree for the base class.
        /// If class has multi inheritance, it can return MultiClassInheritanceTypeTree or SingleClassInheritanceWithInterfacesTypeTree.
        /// </summary>
        /// <param name="error">The error text writer.</param>
        /// <param name="type">The type for which we are getting base class.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="baseClassOffset">The base class offset.</param>
        protected virtual TypeTree GetBaseClassTypeTree(TextWriter error, Symbol type, UserTypeFactory factory, out int baseClassOffset)
        {
            // Check if it is multi class inheritance
            var baseClasses = type.BaseClasses;

            if (baseClasses.Length > 1)
            {
                int emptyTypes = baseClasses.Count(t => t.IsEmpty);

                if (emptyTypes == baseClasses.Length - 1)
                {
                    UserType userType;
                    Symbol baseClassSymbol = baseClasses.First(t => !t.IsEmpty);

                    if (factory.GetUserType(baseClassSymbol, out userType) && !(userType is TemplateArgumentUserType))
                    {
                        baseClassOffset = baseClassSymbol.Offset;
                        return new SingleClassInheritanceWithInterfacesTypeTree(userType, factory);
                    }
                }

                baseClassOffset = 0;
                return new MultiClassInheritanceTypeTree();
            }

            // Single class inheritance
            if (baseClasses.Length == 1)
            {
                // Check if base class type should be transformed
                Symbol baseClassType = baseClasses[0];
                UserTypeTransformation transformation = factory.FindTransformation(baseClassType, this);

                if (transformation != null)
                {
                    baseClassOffset = 0;
                    return new TransformationTypeTree(transformation);
                }

                // Try to find base class user type
                UserType baseUserType;

                if (factory.GetUserType(baseClassType, out baseUserType))
                {
                    TypeTree tree = UserTypeTree.Create(baseUserType, factory);
                    TemplateTypeTree genericsTree = tree as TemplateTypeTree;

                    if (genericsTree != null && !genericsTree.CanInstantiate)
                    {
                        // We cannot instantiate the base class, so we must use UserType as the base class.
                        baseClassOffset = 0;
                        return new VariableTypeTree(false);
                    }

                    baseClassOffset = baseClassType.Offset;
                    return tree;
                }

                // We weren't able to find base class user type. Continue the search.
                return GetBaseClassTypeTree(error, baseClassType, factory, out baseClassOffset);
            }

            // Class doesn't inherit any type
            baseClassOffset = 0;
            return new VariableTypeTree(false);
        }

        /// <summary>
        /// Gets the type tree for the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="extractingBaseClass">if set to <c>true</c> user type field is being generated for getting base class.</param>
        /// <param name="bitLength">Number of bits used for this symbol.</param>
        protected virtual TypeTree GetFieldTypeTree(SymbolField field, UserTypeFactory factory, bool extractingBaseClass, int bitLength = 0)
        {
            return GetSymbolTypeTree(field.Type, factory, bitLength);
        }

        /// <summary>
        /// Gets the type tree for the specified type (symbol).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The user type factory.</param>
        /// <param name="bitLength">Number of bits used for this symbol.</param>
        internal virtual TypeTree GetSymbolTypeTree(Symbol type, UserTypeFactory factory, int bitLength = 0)
        {
            switch (type.Tag)
            {
                case SymTagEnum.SymTagBaseType:
                    if (bitLength == 1)
                        return new BasicTypeTree("bool");
                    switch (type.BasicType)
                    {
                        case BasicType.Bit:
                        case BasicType.Bool:
                            return new BasicTypeTree("bool");
                        case BasicType.Char:
                        case BasicType.WChar:
                            return new BasicTypeTree("char");
                        case BasicType.BSTR:
                            return new BasicTypeTree("string");
                        case BasicType.Void:
                            return new BasicTypeTree("VoidType");
                        case BasicType.Float:
                            return new BasicTypeTree(type.Size <= 4 ? "float" : "double");
                        case BasicType.Int:
                        case BasicType.Long:
                            switch (type.Size)
                            {
                                case 0:
                                    return new BasicTypeTree("VoidType");
                                case 1:
                                    return new BasicTypeTree("sbyte");
                                case 2:
                                    return new BasicTypeTree("short");
                                case 4:
                                    return new BasicTypeTree("int");
                                case 8:
                                    return new BasicTypeTree("long");
                                default:
                                    throw new Exception("Unexpected type length " + type.Size);
                            }

                        case BasicType.UInt:
                        case BasicType.ULong:
                            switch (type.Size)
                            {
                                case 0:
                                    return new BasicTypeTree("VoidType");
                                case 1:
                                    return new BasicTypeTree("byte");
                                case 2:
                                    return new BasicTypeTree("ushort");
                                case 4:
                                    return new BasicTypeTree("uint");
                                case 8:
                                    return new BasicTypeTree("ulong");
                                default:
                                    throw new Exception("Unexpected type length " + type.Size);
                            }

                        case BasicType.Hresult:
                            return new BasicTypeTree("uint"); // TODO: Create Hresult type
                        default:
                            throw new Exception("Unexpected basic type " + type.BasicType);
                    }

                case SymTagEnum.SymTagPointerType:
                    {
                        Symbol pointerType = type.ElementType;
                        UserType pointerUserType;

                        factory.GetUserType(pointerType, out pointerUserType);

                        // When exporting pointer from Global Modules, always export types as code pointer.
                        if (this is GlobalsUserType && pointerUserType != null)
                        {
                            return new PointerTypeTree(UserTypeTree.Create(pointerUserType, factory));
                        }

                        // TODO: Describe the condition.
                        if (pointerUserType is TemplateArgumentUserType)
                        {
                            return new PointerTypeTree(UserTypeTree.Create(pointerUserType, factory));
                        }

                        switch (pointerType.Tag)
                        {
                            case SymTagEnum.SymTagBaseType:
                            case SymTagEnum.SymTagEnum:
                                {
                                    string innerType = GetSymbolTypeTree(pointerType, factory).GetTypeString();

                                    if (innerType == "void")
                                        return new BasicTypeTree("NakedPointer");
                                    return new PointerTypeTree(GetSymbolTypeTree(pointerType, factory));
                                }

                            case SymTagEnum.SymTagUDT:
                                return GetSymbolTypeTree(pointerType, factory);
                            default:
                                return new PointerTypeTree(GetSymbolTypeTree(pointerType, factory));
                        }
                    }

                case SymTagEnum.SymTagUDT:
                case SymTagEnum.SymTagEnum:
                    {
                        // Try to apply transformation on the type
                        UserTypeTransformation transformation = factory.FindTransformation(type, this);

                        if (transformation != null)
                        {
                            return new TransformationTypeTree(transformation);
                        }

                        // Try to find user type that represents current type
                        UserType userType;

                        if (factory.GetUserType(type, out userType))
                        {
                            TypeTree tree = UserTypeTree.Create(userType, factory);
                            TemplateTypeTree genericsTree = tree as TemplateTypeTree;

                            if (genericsTree != null && !genericsTree.CanInstantiate)
                                return new VariableTypeTree();
                            return tree;
                        }

                        // We were unable to find user type. If it is enum, use its basic type
                        if (type.Tag == SymTagEnum.SymTagEnum)
                        {
                            return new BasicTypeTree(EnumUserType.GetEnumBasicType(type));
                        }

                        // Fall-back to Variable
                        return new VariableTypeTree();
                    }

                case SymTagEnum.SymTagArrayType:
                    return new ArrayTypeTree(GetSymbolTypeTree(type.ElementType, factory));

                case SymTagEnum.SymTagFunctionType:
                    return new FunctionTypeTree();

                default:
                    throw new Exception("Unexpected type tag " + type.Tag);
            }
        }
    }
}
