using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateUserTypesFromPdb.UserTypes
{
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


        /// <summary>
        /// Namespace in DIA Symbols name
        /// </summary>
        public string NamespaceSymbol { get; set; }

        public string Namespace
        {
            get
            {
                if (NamespaceSymbol != null)
                {
                    string namespaceName = NamespaceSymbol.Replace("::", "."); ;
                    return namespaceName;
                }
                else
                {
                    return null;
                }
            }
        }

        public UserType DeclaredInType { get; private set; }

        public List<UserType> InnerTypes { get; private set; }

        public HashSet<string> Usings { get; private set; }

        public XmlType XmlType { get; private set; }

        protected virtual bool ExportStaticFields { get { return true; } }

        public string NormalizedSymbolName
        {
            get
            {
                return NormalizeSymbolName(Symbol.name);
            }
        }

        public static string NormalizeSymbolName(string symbolName)
        {
            return symbolName.Replace("::", ".").Replace('<', '_').Replace('>', '_').Replace(' ', '_').Replace(',', '_').Replace("__", "_").Replace("__", "_").TrimEnd('_');
        }

        public virtual string ClassName
        {
            get
            {
                string symbolName = NormalizedSymbolName;

                if (symbolName.Contains("_l29"))
                {

                }

                if (DeclaredInType != null)
                {
                    if (Namespace != null)
                    {
                        symbolName = Namespace + "." + symbolName;
                    }

                    return symbolName.Substring(DeclaredInType.FullClassName.Length + 1);
                }

                if (Namespace != null)
                {
                    return symbolName.Substring(NormalizeSymbolName(NamespaceSymbol).Length + 1);
                }

                symbolName = symbolName.Replace('.', '_');

                return symbolName;
            }
        }
        /*
        public string ClassNameWithoutNamespace
        {
            get
            {
                string symbolName = NormalizedSymbolName;

                if (symbolName.Contains("l29"))
                {

                }

                if (DeclaredInType != null)
                {
                    return FullClassName.Substring(DeclaredInType.FullClassName.Length + 1);
                }

                //#fixme, better way
                int index = symbolName.IndexOf('.');
                if (index >0 )
                {
                    symbolName = symbolName.Substring(index + 1);
                    return symbolName;
                }

                return symbolName;
            }
        }
        */

        public string ParentClassSymbolName
        {
            get
            {
                string parentClassSymbolName = Symbol.name;

                int index = parentClassSymbolName.LastIndexOf("::");
                if (index >= 0)
                {
                    parentClassSymbolName = parentClassSymbolName.Substring(0, index);

                    index = parentClassSymbolName.LastIndexOf("::");
                    if (index > 0)
                    {
                    }

                    return parentClassSymbolName;
                }

                return string.Empty;
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

                return string.Format("{0}", ClassName);
            }
        }

        protected virtual UserTypeField ExtractField(IDiaSymbol field, UserTypeFactory factory, UserTypeGenerationFlags options, bool extractingBaseClass = false, bool forceIsStatic = false)
        {
            if (field.type.name == "CAutoRefc<IClassFactory>")
            {

            }

            var symbol = Symbol;
            bool useThisClass = options.HasFlag(UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider);
            bool isStatic = (DataKind)field.dataKind == DataKind.StaticMember || forceIsStatic;
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
            bool isConstant = (LocationType)field.locationType == LocationType.Constant;
            string constantString = "";

            if (isConstant && options.HasFlag(UserTypeGenerationFlags.GeneratePhysicalMappingOfUserTypes))
            {
                constantString = string.Format("({0})", field.value.ToString());
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

            if (field.name == "m_evtWaitFor")
            {

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
                ConstantValue = constantString,
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

        internal virtual IEnumerable<UserTypeField> ExtractFields(UserTypeFactory factory, UserTypeGenerationFlags options)
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

            if (baseType.ToString() == "CAutoBase<unsigned short>")
            {

            }

            if (DeclaredInType == null)
            {
                if (Usings.Count > 0)
                {
                    foreach (var u in Usings.OrderBy(s => s))
                        output.WriteLine(indentation, "using {0};", u);
                    output.WriteLine();
                }

                //#fixme always put in namespace
                if (!string.IsNullOrEmpty(Namespace))
                {
                    output.WriteLine(indentation, "namespace {0}.{1}", ModuleName, Namespace);
                }
                else
                {
                    output.WriteLine(indentation, "namespace {0}", ModuleName);
                }

                output.WriteLine(indentation++, "{{");
            }

            if (options.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
            {
                if (baseClasses.Length > 0)
                    output.WriteLine(indentation, "// {0} is inherited from:", ClassName);
                foreach (var type in baseClasses)
                    output.WriteLine(indentation, "//   {0}", type.name);
            }
            output.WriteLine(indentation, @"[UserType(ModuleName = ""{0}"", TypeName = ""{1}"")]", moduleName, XmlType.Name);

            output.WriteLine(indentation, @"public partial class {0} {1} {2}", ClassName, !string.IsNullOrEmpty(baseType.GetUserTypeString()) ? ":" : "", baseType);

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

            if (DeclaredInType == null)
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

        protected bool IsFieldFiltered(IDiaSymbol field)
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
                    if (userType is TemplateUserType)
                    {
                        return new UserTypeTreeGenericsType(userType as TemplateUserType, factory);
                    }

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
                            return new UserTypeTreeBaseType("uint"); // TODO: Create Hresult type
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
                            {
                                return new UserTypeTreeEnum((EnumUserType)userType);
                            }

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
}
