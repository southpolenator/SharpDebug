using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Usings = new HashSet<string>(new string[] { "CsScripts" });
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
            UserTypeTree fieldType = GetTypeString(field.Type, factory, field.Size);
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
            if (fieldType is UserTypeTreeGenericsType && !((UserTypeTreeGenericsType)fieldType).CanInstatiate)
                throw new Exception("Generics type cannot be instantiated");

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

        protected virtual IEnumerable<UserTypeField> GetAutoGeneratedFields(bool hasNonStatic, bool useThisClass)
        {
            if (ExportDynamicFields)
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

        public virtual void WriteCode(IndentedWriter output, TextWriter error, UserTypeFactory factory, UserTypeGenerationFlags options, int indentation = 0)
        {
            var symbol = Symbol;
            var fields = ExtractFields(factory, options).OrderBy(f => !f.Static).ThenBy(f => f.GetType().Name).ThenBy(f => f.FieldName).ToArray();
            bool hasStatic = fields.Where(f => f.Static).Any(), hasNonStatic = fields.Where(f => !f.Static).Any();
            UserTypeTree baseType = ExportDynamicFields ? GetBaseTypeString(error, symbol, factory) : null;
            Symbol[] baseClasses = symbol.BaseClasses;

            if (DeclaredInType == null)
            {
                if (Usings.Count > 0 && !options.HasFlag(UserTypeGenerationFlags.SingleFileExport))
                {
                    foreach (var u in Usings.OrderBy(s => s))
                        output.WriteLine(indentation, "using {0};", u);
                    output.WriteLine();
                }

                //#fixme always put in namespace
                if (!string.IsNullOrEmpty(Namespace))
                {
                    output.WriteLine(indentation, "namespace {0}", Namespace);
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

            foreach (var field in fields)
            {
                if (((field.Static && !ExportStaticFields && field.FieldTypeInfoComment != null) || (!field.CacheResult && !field.UseUserMember)) && string.IsNullOrEmpty(field.ConstantValue))
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
                    var field = ExtractField(type.CastAsSymbolField(), factory, options, true);

                    //
                    // TODO Verify getting base class for nested type
                    //
                    field.PropertyName = UserType.NormalizeSymbolName(field.PropertyName);

                    field.PropertyName = field.PropertyName.Replace(" ", "").Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace("__", "_").TrimEnd('_');
                    output.WriteLine();
                    if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && !string.IsNullOrEmpty(field.FieldTypeInfoComment))
                        output.WriteLine(indentation, "// Property for getting base class: {0}", type.Name);
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
                    Arguments = "Variable variable, CsScriptManaged.Utility.MemoryBuffer buffer, int offset, ulong bufferAddress",
                    BaseClassInitialization = "base(variable, buffer, offset, bufferAddress)",
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

        protected virtual UserTypeTree GetBaseTypeString(TextWriter error, Symbol type, UserTypeFactory factory)
        {
            var baseClasses = type.BaseClasses;

            if (baseClasses.Length > 1)
                return new UserTypeTreeMultiClassInheritance();

            if (baseClasses.Length == 1)
            {
                Symbol baseClassType = baseClasses[0];

                // Apply transformation first
                var transformation = factory.FindTransformation(baseClassType, this);

                if (transformation != null)
                    return new UserTypeTreeTransformation(transformation);

                UserType baseUserType;

                if (factory.GetUserType(baseClassType, out baseUserType))
                {
                    UserTypeTree tree = UserTypeTreeUserType.Create(baseUserType, factory);
                    UserTypeTreeGenericsType genericsTree = tree as UserTypeTreeGenericsType;

                    if (genericsTree != null && !genericsTree.CanInstatiate)
                        return new UserTypeTreeVariable(false);
                    return tree;
                }

                return GetBaseTypeString(error, baseClassType, factory);
            }

            return new UserTypeTreeVariable(false);
        }

        public virtual UserTypeTree GetTypeString(Symbol type, UserTypeFactory factory, int bitLength = 0)
        {
            UserType fakeUserType;

            if (factory.GetUserType(type, out fakeUserType) && !(fakeUserType is PrimitiveUserType))
            {
                //return UserTypeTreeUserType.Create(fakeUserType, factory);
            }

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

                        if (factory.GetUserType(pointerType, out fakeUserType) && (fakeUserType is PrimitiveUserType))
                        {
                            //return new UserTypeTreeCodePointer(UserTypeTreeUserType.Create(fakeUserType, factory));
                        }

                        switch (pointerType.Tag)
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
