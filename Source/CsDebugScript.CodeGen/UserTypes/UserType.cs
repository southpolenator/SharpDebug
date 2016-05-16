using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenerateUserTypesFromPdb.UserTypes
{
    class UserType
    {
        protected bool usedThisClass = false;

        public UserType(Symbol symbol, XmlType xmlType, string nameSpace)
        {
            Symbol = symbol;
            XmlType = xmlType;
            InnerTypes = new List<UserType>();
            Usings = new HashSet<string>(new string[] { "CsDebugScript" });
            NamespaceSymbol = nameSpace;
        }

        public Symbol Symbol { get; private set; }

        protected string TypeName
        {
            get
            {
                return XmlType != null ? XmlType.Name : Symbol.Name;
            }
        }

        /// <summary>
        /// Namespace in DIA Symbols name
        /// </summary>
        protected string NamespaceSymbol { get; set; }

        /// <summary>
        /// Normalized Namespace.
        /// If defined as nested does to include Parent Type.
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

        private UserType declaredInType;

        public virtual UserType DeclaredInType
        {
            get
            {
                return declaredInType;
            }

            set
            {
                declaredInType = value;
            }
        }

        public List<UserType> InnerTypes { get; private set; }

        public HashSet<string> Usings { get; private set; }

        public XmlType XmlType { get; private set; }

        internal bool ExportStaticFields { get; set; } = true;

        internal bool ExportDynamicFields { get; set; } = true;

        public string NormalizedSymbolName
        {
            get
            {
                return NormalizeSymbolName(Symbol.Name);
            }
        }

        public static string NormalizeSymbolName(string symbolName)
        {
            // Notes:
            // Do not trim right, some of the classes start with '_'
            // Cannot replace __ with _ , it will generate class name collisions
            // 
            return symbolName.Replace("::", "_").Replace("*", "").Replace("&", "").Replace('-', '_').Replace('<', '_').Replace('>', '_').Replace(' ', '_').Replace(',', '_').Replace('(', '_').Replace(')', '_').TrimEnd('_');
        }

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

        protected virtual string GetInheritanceTypeConstraint(UserTypeFactory factory)
        {
            return string.Empty;
        }

        protected virtual UserTypeField ExtractField(SymbolField field, UserTypeFactory factory, UserTypeGenerationFlags options, bool extractingBaseClass = false, bool forceIsStatic = false)
        {
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool isStatic = field.DataKind == DataKind.StaticMember || forceIsStatic;
            UserTypeTree fieldType = GetFieldType(field, factory, extractingBaseClass, field.Size);
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

            UserTypeField userTypeField = ExtractField(field, fieldType, factory, simpleFieldValue, gettingField, isStatic, options, extractingBaseClass);
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

            if (extractingBaseClass)
            {
                if (fieldType is UserTypeTreeUserType && ((UserTypeTreeUserType)fieldType).UserType is PrimitiveUserType)
                    userTypeField.ConstructorText = string.Format("CastAs<{0}>()", fieldType.GetUserTypeString());
                else if (useThisClass)
                {
                    userTypeField.ConstructorText = userTypeField.ConstructorText.Replace("thisClass.Value.GetClassField", "GetBaseClass");
                    usedThisClass = true;
                }
                else
                    userTypeField.ConstructorText = userTypeField.ConstructorText.Replace("variable.GetField", "GetBaseClass");
            }

            return userTypeField;
        }

        protected virtual UserTypeField ExtractField(SymbolField field, UserTypeTree fieldType, UserTypeFactory factory, string simpleFieldValue, string gettingField, bool isStatic, UserTypeGenerationFlags options, bool extractingBaseClass)
        {
            //  Non Template User Type must use Instantiable Generics.
            //
            if (!(this is TemplateUserType) && fieldType is UserTypeTreeGenericsType && !((UserTypeTreeGenericsType)fieldType).CanInstatiate)
            {
                throw new Exception("Generics type cannot be instantiated");
            }

            bool forceUserTypesToNewInsteadOfCasting = options.HasFlag(UserTypeGenerationFlags.ForceUserTypesToNewInsteadOfCasting);
            bool cacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields);
            bool cacheStaticUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheStaticUserTypeFields);
            bool lazyCacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields);
            string fieldName = field.Name;
            string castingTypeString = GetCastingString(fieldType);
            string constructorText;
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool castWithNewInsteadOfCasting = forceUserTypesToNewInsteadOfCasting && factory.ContainsSymbol(Symbol.Module, castingTypeString);
            var fieldTypeString = fieldType.GetUserTypeString();
            bool isConstant = field.LocationType == LocationType.Constant;
            string constantString = "";

            if (isConstant && options.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
            {
                constantString = field.Value.ToString();
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
                if (isStatic || !useThisClass)
                {
                    constructorText = string.Format("{1}.CastAs<{0}>()", castingTypeString, simpleFieldValue);
                }
                else
                {
                    constructorText = string.Format("{0}<{1}>(\"{2}\")", gettingField, castingTypeString, fieldName);
                }
            }

            //
            // TODO
            // needs more extensive check for property name
            // do not duplicate after adding '_'
            // ex. class has 'in' and '_in' fields.
            // 
            fieldName = UserTypeField.GetPropertyName(fieldName, this);

            //
            //  When Creating Property for BaseClass, current class is generic type
            //  Rename baseclass property name not to include specialization type.
            //
            if (extractingBaseClass && this is TemplateUserType && fieldType is UserTypeTreeGenericsType)
            {
                fieldName = ((UserTypeTreeGenericsType)(fieldType)).UserType.ClassName;
            }

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

        internal virtual IEnumerable<UserTypeField> ExtractFields(UserTypeFactory factory, UserTypeGenerationFlags options)
        {
            bool hasNonStatic = false;
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);

            if (ExportDynamicFields)
            {
                // Extract non-static fields
                foreach (var field in Symbol.Fields)
                {
                    if (IsFieldFiltered(field) || field.DataKind == DataKind.StaticMember)
                        continue;

                    var userField = ExtractField(field, factory, options);

                    yield return userField;
                    hasNonStatic = hasNonStatic || !userField.Static;
                }

                // Extract static fields
                if (ExportStaticFields)
                    foreach (var field in GlobalCache.GetSymbolStaticFields(Symbol))
                    {
                        if (IsFieldFiltered(field))
                            continue;

                        var userField = ExtractField(field, factory, options);

                        yield return userField;
                    }

                // Should we try to incorporate Hungarian notation into field decomposition
                if (options.HasFlag(UserTypeGenerationFlags.UseHungarianNotation))
                    foreach (var field in GenerateHungarianNotationFields(factory, options))
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

                    var userField = ExtractField(field, factory, options);

                    yield return userField;
                }
            }

            // Get auto generated fields
            foreach (var field in GetAutoGeneratedFields(hasNonStatic, useThisClass && usedThisClass))
                yield return field;
        }

        protected virtual IEnumerable<UserTypeField> GenerateHungarianNotationFields(UserTypeFactory factory, UserTypeGenerationFlags options)
        {
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
                    if ((counterField.LocationType == LocationType.Static) != (pointerField.LocationType == LocationType.Static))
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

                UserTypeTree fieldType = GetTypeString(pointerField.Type, factory, pointerField.Size);

                if (fieldType is UserTypeTreeCodeArray)
                    continue;

                UserTypeTreeCodePointer fieldTypeCodePointer = fieldType as UserTypeTreeCodePointer;

                if (fieldTypeCodePointer != null)
                {
                    fieldType = fieldTypeCodePointer.InnerType;
                }

                fieldType = new UserTypeTreeCodeArray(fieldType);
                string fieldName = pointerField.Name + "Array";
                string constructorText = string.Format("new {0}({1}, {2})", fieldType, pointerField.Name, counterField.Name);
                string fieldTypeString = fieldType.GetUserTypeString();
                bool isStatic = pointerField.LocationType == LocationType.Static;
                bool cacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields);
                bool cacheStaticUserTypeFields = options.HasFlag(UserTypeGenerationFlags.CacheStaticUserTypeFields);
                bool lazyCacheUserTypeFields = options.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields);

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

        protected virtual IEnumerable<UserTypeField> GetAutoGeneratedFields(bool hasNonStatic, bool useThisClass)
        {
            if (ExportDynamicFields)
            {
                if (hasNonStatic && useThisClass)
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

                yield return new UserTypeFunction
                {
                    FieldName = "PartialInitialize",
                    FieldType = "partial void",
                    CacheResult = true,
                };
            }
        }

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

        protected string GenerateClassCodeTypeInfo()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var module in GlobalCache.GetSymbolModuleNames(Symbol))
            {
                sb.Append(string.Format("\"{0}!{1}\", ", module, TypeName));
            }

            sb.Length -= 2;

            return sb.ToString();
        }

        public virtual void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            int baseClassOffset = 0;
            UserTypeTree baseType = ExportDynamicFields ? GetBaseTypeString(error, Symbol, factory, out baseClassOffset) : null;
            var fields = ExtractFields(factory, options).OrderBy(f => !f.Static).ThenBy(f => f.FieldName != "ClassCodeType").ThenBy(f => f.GetType().Name).ThenBy(f => f.FieldName).ToArray();
            bool hasStatic = fields.Any(f => f.Static), hasNonStatic = fields.Any(f => !f.Static);
            Symbol[] baseClasses = Symbol.BaseClasses;
            string nameSpace = null;

            if (DeclaredInType == null || (!options.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType))
            {
                if (Usings.Count > 0 && !options.HasFlag(UserTypeGenerationFlags.SingleFileExport))
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

            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                WriteClassComment(output, indentation);
            if (ExportDynamicFields)
            {
                foreach (var moduleName in GlobalCache.GetSymbolModuleNames(Symbol))
                    output.WriteLine(indentation, @"[UserType(ModuleName = ""{0}"", TypeName = ""{1}"")]", moduleName, TypeName);
                output.WriteLine(indentation, @"public partial class {0} {1} {2}", ClassName, !string.IsNullOrEmpty(baseType.GetUserTypeString()) ? ":" : "", baseType);
            }
            else
                output.WriteLine(indentation, @"public static class {0}", ClassName);

            var inheritanceTypeConstrains = GetInheritanceTypeConstraint(factory);
            if (!string.IsNullOrEmpty(inheritanceTypeConstrains))
                output.WriteLine(indentation, inheritanceTypeConstrains);

            output.WriteLine(indentation++, @"{{");

            bool firstField = true;
            foreach (var field in fields)
            {
                if (((field.Static && !ExportStaticFields && field.FieldTypeInfoComment != null) || (!field.CacheResult && !field.UseUserMember)) && string.IsNullOrEmpty(field.ConstantValue))
                    continue;
                field.WriteFieldCode(output, indentation, options);
            }

            var constructors = GenerateConstructors();

            foreach (var constructor in constructors)
                constructor.WriteCode(output, indentation, fields, ConstructorName, ExportStaticFields);

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

            if (baseType is UserTypeTreeMultiClassInheritance || baseType is UserTypeTreeSingleClassInheritanceWithInterfaces)
            {
                IEnumerable<Symbol> baseClassesForProperties = baseClasses;

                if (baseType is UserTypeTreeSingleClassInheritanceWithInterfaces)
                {
                    baseClassesForProperties = baseClasses.Where(b => b.IsEmpty);
                }

                // Write all properties for getting base classes
                foreach (var type in baseClassesForProperties)
                {
                    var field = ExtractField(type.CastAsSymbolField(), factory, options, true);
                    string singleLineDefinition = string.Empty;

                    if (options.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                        singleLineDefinition = string.Format(" {{ get {{ return {0}; }} }}", field.ConstructorText);

                    // TODO: Verify getting base class for nested type
                    field.PropertyName = UserType.NormalizeSymbolName(field.PropertyName);

                    field.PropertyName = field.PropertyName.Replace(" ", "").Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace("__", "_").TrimEnd('_');
                    output.WriteLine();
                    if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(field.FieldTypeInfoComment))
                        output.WriteLine(indentation, "// Property for getting base class: {0}", type.Name);
                    if (baseClasses.Length == 1)
                        output.WriteLine(indentation, "public {0} BaseClass{1}", field.FieldType, singleLineDefinition);
                    else
                        output.WriteLine(indentation, "public {0} BaseClass_{1}{2}", field.FieldType, field.PropertyName, singleLineDefinition);
                    if (!options.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
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

            if ((DeclaredInType == null || (!options.HasFlag(UserTypeGenerationFlags.SingleFileExport) && DeclaredInType is NamespaceUserType)) && !string.IsNullOrEmpty(nameSpace))
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

            if (ExportDynamicFields)
            {
                yield return new UserTypeConstructor()
                {
                    Arguments = "Variable variable",
                    BaseClassInitialization = "base(variable)",
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
        }

        protected bool IsFieldFiltered(SymbolField field)
        {
            return XmlType != null && ((XmlType.IncludedFields.Count > 0 && !XmlType.IncludedFields.Contains(field.Name))
                || XmlType.ExcludedFields.Contains(field.Name));
        }

        public void SetDeclaredInType(UserType declaredInType)
        {
            if (declaredInType == null)
            {
                // ignore it
                return;
            }

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

        protected virtual UserTypeTree GetBaseTypeString(TextWriter error, Symbol type, UserTypeFactory factory, out int baseClassOffset)
        {
            var baseClasses = type.BaseClasses;

            if (baseClasses.Length > 1)
            {
                int emptyTypes = baseClasses.Count(t => t.IsEmpty);

                if (emptyTypes == baseClasses.Length - 1)
                {
                    UserType userType;
                    Symbol baseClassSymbol = baseClasses.First(t => !t.IsEmpty);

                    if (factory.GetUserType(baseClassSymbol, out userType) && !(userType is PrimitiveUserType))
                    {
                        baseClassOffset = baseClassSymbol.Offset;
                        return new UserTypeTreeSingleClassInheritanceWithInterfaces(userType, factory);
                    }
                }

                baseClassOffset = 0;
                return new UserTypeTreeMultiClassInheritance();
            }

            if (baseClasses.Length == 1)
            {
                Symbol baseClassType = baseClasses[0];

                // Apply transformation first
                var transformation = factory.FindTransformation(baseClassType, this);

                if (transformation != null)
                {
                    baseClassOffset = 0;
                    return new UserTypeTreeTransformation(transformation);
                }

                UserType baseUserType;

                if (factory.GetUserType(baseClassType, out baseUserType))
                {
                    UserTypeTree tree = UserTypeTreeUserType.Create(baseUserType, factory);
                    UserTypeTreeGenericsType genericsTree = tree as UserTypeTreeGenericsType;

                    if (genericsTree != null && !genericsTree.CanInstatiate)
                    {
                        baseClassOffset = 0;
                        return new UserTypeTreeVariable(false);
                    }

                    baseClassOffset = baseClassType.Offset;
                    return tree;
                }

                return GetBaseTypeString(error, baseClassType, factory, out baseClassOffset);
            }

            baseClassOffset = 0;
            return new UserTypeTreeVariable(false);
        }

        public virtual UserTypeTree GetFieldType(SymbolField field, UserTypeFactory factory, bool extractingBaseClass, int bitLength = 0)
        {
            return GetTypeString(field.Type, factory, bitLength);
        }

        public virtual UserTypeTree GetTypeString(Symbol type, UserTypeFactory factory, int bitLength = 0)
        {
            switch (type.Tag)
            {
                case SymTagEnum.SymTagBaseType:
                    if (bitLength == 1)
                        return new UserTypeTreeBaseType("bool");
                    switch (type.BasicType)
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
                            return new UserTypeTreeBaseType("VoidType");
                        case BasicType.Float:
                            return new UserTypeTreeBaseType(type.Size <= 4 ? "float" : "double");
                        case BasicType.Int:
                        case BasicType.Long:
                            switch (type.Size)
                            {
                                case 0:
                                    return new UserTypeTreeBaseType("VoidType");
                                case 1:
                                    return new UserTypeTreeBaseType("sbyte");
                                case 2:
                                    return new UserTypeTreeBaseType("short");
                                case 4:
                                    return new UserTypeTreeBaseType("int");
                                case 8:
                                    return new UserTypeTreeBaseType("long");
                                default:
                                    throw new Exception("Unexpected type length " + type.Size);
                            }

                        case BasicType.UInt:
                        case BasicType.ULong:
                            switch (type.Size)
                            {
                                case 0:
                                    return new UserTypeTreeBaseType("VoidType");
                                case 1:
                                    return new UserTypeTreeBaseType("byte");
                                case 2:
                                    return new UserTypeTreeBaseType("ushort");
                                case 4:
                                    return new UserTypeTreeBaseType("uint");
                                case 8:
                                    return new UserTypeTreeBaseType("ulong");
                                default:
                                    throw new Exception("Unexpected type length " + type.Size);
                            }

                        case BasicType.Hresult:
                            return new UserTypeTreeBaseType("uint"); // TODO: Create Hresult type
                        default:
                            throw new Exception("Unexpected basic type " + type.BasicType);
                    }

                case SymTagEnum.SymTagPointerType:
                    {
                        Symbol pointerType = type.ElementType;

                        UserType fakeUserType;
                        factory.GetUserType(pointerType, out fakeUserType);

                        // When Exporting Pointer from Global Modules, always export types as code pointer.
                        if (this is GlobalsUserType && fakeUserType != null)
                        {
                            return new UserTypeTreeCodePointer(UserTypeTreeUserType.Create(fakeUserType, factory));
                        }

                        // TODO Describe the condition.
                        //
                        if (fakeUserType is PrimitiveUserType)
                        {
                            return new UserTypeTreeCodePointer(UserTypeTreeUserType.Create(fakeUserType, factory));
                        }

                        switch (pointerType.Tag)
                        {
                            case SymTagEnum.SymTagBaseType:
                            case SymTagEnum.SymTagEnum:
                                {
                                    string innerType = GetTypeString(pointerType, factory).GetUserTypeString();

                                    if (innerType == "void")
                                        return new UserTypeTreeBaseType("NakedPointer");
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
                        var transformation = factory.FindTransformation(type, this);

                        if (transformation != null)
                        {
                            return new UserTypeTreeTransformation(transformation);
                        }

                        UserType userType;

                        if (factory.GetUserType(type, out userType))
                        {
                            UserTypeTree tree = UserTypeTreeUserType.Create(userType, factory);
                            UserTypeTreeGenericsType genericsTree = tree as UserTypeTreeGenericsType;

                            if (genericsTree != null && !genericsTree.CanInstatiate)
                                return new UserTypeTreeVariable();
                            return tree;
                        }

                        if (type.Tag == SymTagEnum.SymTagEnum)
                        {
                            return new UserTypeTreeBaseType("uint");
                        }

                        return new UserTypeTreeVariable();
                    }

                case SymTagEnum.SymTagArrayType:
                    return new UserTypeTreeCodeArray(GetTypeString(type.ElementType, factory));

                case SymTagEnum.SymTagFunctionType:
                    return new UserTypeTreeCodeFunction();

                default:
                    throw new Exception("Unexpected type tag " + type.Tag);
            }
        }
    }
}
