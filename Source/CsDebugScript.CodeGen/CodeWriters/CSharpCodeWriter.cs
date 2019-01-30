using CsDebugScript.CodeGen.SymbolProviders;
using CsDebugScript.CodeGen.TypeInstances;
using CsDebugScript.CodeGen.UserTypes;
using CsDebugScript.CodeGen.UserTypes.Members;
using DIA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsDebugScript.CodeGen.CodeWriters
{
    using UserType = CsDebugScript.CodeGen.UserTypes.UserType;

    /// <summary>
    /// Code writer that outputs code in C#.
    /// </summary>
    internal class CSharpCodeWriter : DotNetCodeWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpCodeWriter"/> class.
        /// </summary>
        /// <param name="generationFlags">The code generation options</param>
        /// <param name="nameLimit">Maximum number of characters that generated name can have.</param>
        public CSharpCodeWriter(UserTypeGenerationFlags generationFlags, int nameLimit)
            : base(generationFlags, nameLimit, fixKeywordsInUserNaming: true)
        {
        }

        /// <summary>
        /// Returns <c>true</c> if code writer supports binary writer.
        /// </summary>
        public override bool HasBinaryWriter => false;

        /// <summary>
        /// Returns <c>true</c> if code writer supports text writer.
        /// </summary>
        public override bool HasTextWriter => true;

        /// <summary>
        /// Generated binary code for user types. This is used only if <see cref="HasBinaryWriter"/> is <c>true</c>.
        /// </summary>
        /// <param name="userTypes">User types for which code should be generated.</param>
        /// <param name="dllFileName">Output DLL file path.</param>
        /// <param name="generatePdb"><c>true</c> if PDB file should be generated.</param>
        /// <param name="additionalAssemblies">Enumeration of additional assemblies that we should load for type lookup - used with transformations.</param>
        public override void GenerateBinary(IEnumerable<UserType> userTypes, string dllFileName, bool generatePdb, IEnumerable<string> additionalAssemblies)
        {
            // This should never be called.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates code for user type and writes it to the specified output.
        /// </summary>
        /// <param name="userType">User type for which code should be generated.</param>
        /// <param name="output">Output text writer.</param>
        public override void WriteUserType(UserType userType, StringBuilder output)
        {
            WriteUserType(userType, new IndentedWriter(output, GenerationFlags.HasFlag(UserTypeGenerationFlags.CompressedOutput)));
        }

        /// <summary>
        /// Generates code for user type and writes it to the specified output.
        /// </summary>
        /// <param name="type">User type for which code should be generated.</param>
        /// <param name="output">Output indented writer.</param>
        public void WriteUserType(UserType type, IndentedWriter output)
        {
            WriteUserType(type, output, recursive: false);
        }

        /// <summary>
        /// Generates code for user type and writes it to the specified output.
        /// </summary>
        /// <param name="type">User type for which code should be generated.</param>
        /// <param name="output">Output indented writer.</param>
        /// <param name="recursive">Is this recursive call from printing inner type?</param>
        private void WriteUserType(UserType type, IndentedWriter output, bool recursive)
        {
            if (type is NamespaceUserType)
            {
                WriteNamespace((NamespaceUserType)type, output);
                return;
            }

            // Write namespace for user type
            bool shouldCloseNamespaceBlock = false;

            if (type.DeclaredInType == null && !string.IsNullOrEmpty(type.Namespace))
            {
                //output.WriteLine($"namespace {type.Namespace}");
                output.StartLine("namespace ");
                output.EndLine(type.Namespace);

                StartBlock(output);
                shouldCloseNamespaceBlock = true;
            }
            else if (!recursive && !GenerationFlags.HasFlag(UserTypeGenerationFlags.SingleFileExport))
            {
                string nameSpace = (type.DeclaredInType as NamespaceUserType)?.FullTypeName ?? type.Namespace;

                if (!string.IsNullOrEmpty(nameSpace))
                {
                    //output.WriteLine($"namespace {nameSpace}");
                    output.StartLine("namespace ");
                    output.EndLine(nameSpace);

                    StartBlock(output);
                    shouldCloseNamespaceBlock = true;
                }
            }

            if (type is EnumUserType enumType)
                WriteEnum(enumType, output);
            else if (type is TemplateArgumentConstantUserType constantType)
                WriteTemplateConstant(constantType, output);
            else
                WriteRegular(type, output);

            // Close namespace for user type
            if (shouldCloseNamespaceBlock)
                EndBlock(output);
        }

        #region Namespace
        /// <summary>
        /// Generates code for namespace user type and writes it to the specified output.
        /// </summary>
        /// <param name="type">User type for which code should be generated.</param>
        /// <param name="output">Output indented writer.</param>
        public void WriteNamespace(NamespaceUserType type, IndentedWriter output)
        {
            bool asStaticClass = GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateNamespaceAsStaticClass);

            // Declared In Type with namespace
            if (type.DeclaredInType != null || asStaticClass)
            {
                foreach (string innerClass in type.Namespaces)
                {
                    //output.WriteLine($"public static partial class {innerClass}");
                    output.StartLine("public static partial class ");
                    output.EndLine(innerClass);

                    StartBlock(output);
                }
            }
            else
            {
                //output.WriteLine($"namespace {type.Namespace}");
                output.StartLine("namespace ");
                output.EndLine(type.Namespace);

                StartBlock(output);
            }

            // Inner types
            WriteInnerTypes(type, output);

            // Declared In Type with namespace
            if (type.DeclaredInType != null || asStaticClass)
                foreach (string innerClass in type.Namespaces)
                    EndBlock(output);
            else
                EndBlock(output);
        }
        #endregion

        #region Enumeration
        /// <summary>
        /// Generates code for enumeration user type and writes it to the specified output.
        /// </summary>
        /// <param name="type">User type for which code should be generated.</param>
        /// <param name="output">Output indented writer.</param>
        public void WriteEnum(EnumUserType type, IndentedWriter output)
        {
            // Write comment
            if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
            {
                //output.WriteLine($"// {type.TypeName} (original name: {type.Symbol.Name})");
                output.StartLine("// ");
                output.Write(type.TypeName);
                output.Write(" (original name: ");
                output.Write(type.Symbol.Name);
                output.EndLine(")");
            }

            // Write type start
            if (type.AreValuesFlags)
                output.WriteLine("[System.Flags]");

            //output.WriteLine($"public enum {type.TypeName} : {ToString(type.BasicType)}");
            output.StartLine("public enum ");
            output.Write(type.TypeName);
            if (type.BasicType != null)
            {
                output.Write(" : ");
                output.Write(ToString(type.BasicType));
            }
            output.EndLine();

            StartBlock(output);

            // Write values
            foreach (var enumValue in type.Symbol.EnumValues)
            {
                string value = enumValue.Item2;

                //output.WriteLine($"{enumValue.Item1} = ({ToString(type.BasicType)}){value},");
                output.StartLine(enumValue.Item1);
                output.Write(" = ");
                if (!EnumValueFitsBasicType(type.BasicType, ref value))
                {
                    output.Write("(");
                    output.Write(ToString(type.BasicType));
                    output.Write(")");
                }
                output.Write(value);
                output.EndLine(",");
            }

            // Write type end
            EndBlock(output);
        }

        /// <summary>
        /// Checks whether value can be stored inside the specified enumeration basic type
        /// </summary>
        /// <param name="enumBasicType">Enumeration basic type</param>
        /// <param name="value">Value of the element</param>
        /// <returns><c>true</c> if no cast is needed to store value; <c>false</c> otherwise.</returns>
        private static bool EnumValueFitsBasicType(Type enumBasicType, ref string value)
        {
            ulong ulongValue;

            if (enumBasicType == null)
                return true;
            else if (enumBasicType == typeof(sbyte))
            {
                if (sbyte.TryParse(value, out sbyte unused))
                    return true;
                if (ulong.TryParse(value, out ulongValue))
                {
                    value = ((sbyte)ulongValue).ToString();
                    return true;
                }
                return false;
            }
            else if (enumBasicType == typeof(byte))
            {
                if (byte.TryParse(value, out byte unused))
                    return true;
                if (ulong.TryParse(value, out ulongValue))
                {
                    value = ((byte)ulongValue).ToString();
                    return true;
                }
                return false;
            }
            else if (enumBasicType == typeof(short))
            {
                if (short.TryParse(value, out short unused))
                    return true;
                if (ulong.TryParse(value, out ulongValue))
                {
                    value = ((short)ulongValue).ToString();
                    return true;
                }
                return false;
            }
            else if (enumBasicType == typeof(ushort))
            {
                if (ushort.TryParse(value, out ushort unused))
                    return true;
                if (ulong.TryParse(value, out ulongValue))
                {
                    value = ((ushort)ulongValue).ToString();
                    return true;
                }
                return false;
            }
            else if (enumBasicType == typeof(int))
            {
                if (int.TryParse(value, out int unused))
                    return true;
                if (ulong.TryParse(value, out ulongValue))
                {
                    value = ((int)ulongValue).ToString();
                    return true;
                }
                return false;
            }
            else if (enumBasicType == typeof(uint))
            {
                if (uint.TryParse(value, out uint unused))
                    return true;
                if (ulong.TryParse(value, out ulongValue))
                {
                    value = ((uint)ulongValue).ToString();
                    return true;
                }
                return false;
            }
            else if (enumBasicType == typeof(long))
            {
                if (long.TryParse(value, out long unused))
                    return true;
                if (ulong.TryParse(value, out ulongValue))
                {
                    value = ((long)ulongValue).ToString();
                    return true;
                }
                return false;
            }
            else if (enumBasicType == typeof(ulong))
                return ulong.TryParse(value, out ulong unused);

            throw new NotImplementedException();
        }
        #endregion

        #region Template argument constant
        /// <summary>
        /// Generates code for constant template argument and writes it to the specified output.
        /// </summary>
        /// <param name="type">Template argument constant user type for which code should be generated.</param>
        /// <param name="output">Output indented writer.</param>
        private void WriteTemplateConstant(TemplateArgumentConstantUserType type, IndentedWriter output)
        {
            IntegralConstantSymbol integralConstant = type.Symbol as IntegralConstantSymbol;
            EnumConstantSymbol enumConstant = type.Symbol as EnumConstantSymbol;
            string constantCode;
            string constantType;

            if (enumConstant != null)
            {
                constantCode = ConstantValue((EnumUserType)enumConstant.EnumSymbol.UserType, enumConstant.Value);
                constantType = enumConstant.EnumSymbol.UserType.FullTypeName;
            }
            else
            {
                constantCode = ConstantValue(integralConstant.Value.GetType(), integralConstant.Value);
                constantType = ToString(integralConstant.Value.GetType());
            }

            //output.WriteLine($"[{ToString(typeof(TemplateConstantAttribute))}(String = \"{type.Symbol.Name}\", Value = {constantCode})]");
            output.StartLine("[");
            output.Write(ToString(typeof(TemplateConstantAttribute)));
            output.Write("(String = \"");
            output.Write(type.Symbol.Name);
            output.Write("\", Value = ");
            output.Write(constantCode);
            output.EndLine(")]");

            //output.WriteLine($"public class {type.TypeName} : {ToString(typeof(ITemplateConstant))}<{constantType}>");
            output.StartLine("public class ");
            output.Write(type.TypeName);
            output.Write(" : ");
            output.Write(ToString(typeof(ITemplateConstant)));
            output.Write("<");
            output.Write(constantType);
            output.EndLine(">");

            StartBlock(output);

            // Output property that will return value of the constant
            string accessLevel = ToString(AccessLevel.Public);
            string propertyName = "Value";
            string comment = null;

            WriteProperty(output, comment, accessLevel, false, constantType, propertyName, constantCode);

            EndBlock(output);
        }
        #endregion

        #region Regular
        /// <summary>
        /// The number of base class arrays created during code generation.
        /// Used as unique number for next base class array name.
        /// </summary>
        private static long baseClassArraysCreated = 0;

        /// <summary>
        /// Generates code for regular user type and writes it to the specified output.
        /// </summary>
        /// <param name="type">User type for which code should be generated.</param>
        /// <param name="output">Output indented writer.</param>
        public void WriteRegular(UserType type, IndentedWriter output)
        {
            Dictionary<string, string> fieldConstructorInitialization = new Dictionary<string, string>();
            bool cacheUserTypeFields = GenerationFlags.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields);
            bool lazyCacheUserTypeFields = GenerationFlags.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields);
            bool useDirectClassAccess = GenerationFlags.HasFlag(UserTypeGenerationFlags.UseDirectClassAccess);
            string baseClassesArrayName = null;

            // Write comment
            if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
            {
                Symbol[] baseClasses = type.Symbol.BaseClasses;

                if (baseClasses.Length > 0)
                {
                    //output.WriteLine($"// {type.FullTypeName} (original name: {type.Symbol.Name}) is inherited from:");
                    output.StartLine("// ");
                    output.Write(type.FullTypeName);
                    output.Write(" (original name: ");
                    output.Write(type.Symbol.Name);
                    output.EndLine(") is inherited from:");

                    foreach (Symbol baseClass in baseClasses)
                    {
                        //output.WriteLine($"//   {baseClass.Name}");
                        output.StartLine("//   ");
                        output.EndLine(baseClass.Name);
                    }
                }
                else
                {
                    //output.WriteLine($"// {type.FullTypeName} (original name: {type.Symbol.Name})");
                    output.StartLine("// ");
                    output.Write(type.FullTypeName);
                    output.Write(" (original name: ");
                    output.Write(type.Symbol.Name);
                    output.EndLine(")");
                }

                if (type is TemplateUserType templateType)
                {
                    output.WriteLine("// Specializations of this class:");
                    foreach (UserType specialization in templateType.Specializations)
                    {
                        //output.WriteLine($"//   {specialization.Symbol.Name}");
                        output.StartLine("//   ");
                        output.EndLine(specialization.Symbol.Name);
                    }
                }
            }

            // Write type start
            if (type.BaseClass is StaticClassTypeInstance)
            {
                //output.WriteLine($"public static class {type.TypeName}");
                output.StartLine("public static class ");
                output.EndLine(type.TypeName);
            }
            else
            {
                // If symbol has vtable, we would like to add DerivedClassAttribute to it
                if (type.Symbol.HasVTable())
                    foreach (UserType derivedClass in type.DerivedClasses)
                    {
                        string fullClassName = derivedClass.FullTypeName;

                        //output.WriteLine($"[{ToString(typeof(DerivedClassAttribute))}(Type = typeof({fullClassName}), Priority = {derivedClass.DerivedClasses.Count}, TypeName = \"{derivedClass.Symbol.Name}\")]");
                        output.StartLine("[");
                        output.Write(ToString(typeof(DerivedClassAttribute)));
                        output.Write("(Type = typeof(");
                        output.Write(fullClassName);
                        output.Write("), Priority = ");
                        output.Write(derivedClass.DerivedClasses.Count);
                        output.Write(", TypeName = \"");
                        output.Write(derivedClass.Symbol.Name);
                        output.EndLine("\")]");
                    }

                // Write all UserTypeAttributes and class header
                if (type is TemplateUserType templateType)
                {
                    foreach (var specialization in templateType.Specializations)
                        foreach (var moduleName in GlobalCache.GetSymbolModuleNames(specialization.Symbol))
                        {
                            //output.WriteLine($"[{ToString(typeof(UserTypeAttribute))}(ModuleName = \"{moduleName}\", TypeName = \"{specialization.Symbol.Name}\")]");
                            output.StartLine("[");
                            output.Write(ToString(typeof(UserTypeAttribute)));
                            output.Write("(ModuleName = \"");
                            output.Write(moduleName);
                            output.Write("\", TypeName = \"");
                            output.Write(specialization.Symbol.Name);
                            output.EndLine("\")]");
                        }
                }
                else
                    foreach (var moduleName in GlobalCache.GetSymbolModuleNames(type.Symbol))
                    {
                        //output.WriteLine($"[{ToString(typeof(UserTypeAttribute))}(ModuleName = \"{moduleName}\", TypeName = \"{type.Symbol.Name}\")]");
                        output.StartLine("[");
                        output.Write(ToString(typeof(UserTypeAttribute)));
                        output.Write("(ModuleName = \"");
                        output.Write(moduleName);
                        output.Write("\", TypeName = \"");
                        output.Write(type.Symbol.Name);
                        output.EndLine("\")]");
                    }

                // If we have multi class inheritance, generate attribute for getting static field with base class C# types
                if (type.BaseClass is MultiClassInheritanceTypeInstance || type.BaseClass is SingleClassInheritanceWithInterfacesTypeInstance)
                {
                    baseClassesArrayName = $"RandomlyNamed_BaseClassesArray{System.Threading.Interlocked.Increment(ref baseClassArraysCreated)}";

                    //output.WriteLine($"[{ToString(typeof(BaseClassesArrayAttribute))}(FieldName = \"{baseClassesArrayName}\")]");
                    output.StartLine("[");
                    output.Write(ToString(typeof(BaseClassesArrayAttribute)));
                    output.Write("(FieldName = \"");
                    output.Write(baseClassesArrayName);
                    output.EndLine("\")]");
                }

                // Write class definition
                string baseTypeString = type.BaseClass.GetTypeString();

                //output.WriteLine($"public partial class {type.TypeName} {baseTypeString}");
                output.StartLine("public partial class ");
                output.Write(type.TypeName);
                if (!string.IsNullOrEmpty(baseTypeString))
                {
                    output.Write(" : ");
                    output.Write(baseTypeString);
                    if (type.Symbol.HasVTable())
                    {
                        output.Write(", ");
                        output.Write(ToString(typeof(ICastableObject)));
                    }
                }
                output.EndLine();
            }

            // TODO: Add constraints to template types
            //foreach (var genericTypeConstraint in type.GetGenericTypeConstraints(factory))
            //    output.WriteLine(1, genericTypeConstraint);
            StartBlock(output);

            // Write ClassCodeType
            if (type is PhysicalUserType || (type is TemplateUserType && type.Members.OfType<DataFieldUserTypeMember>().Any(m => m.IsStatic)))
            {
                if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                    output.WriteLine("// Code type for user type represented by this class.");

                //output.WriteLine($"public static readonly {ToString(typeof(CodeType))} {ClassCodeTypeFieldName} = GetClassCodeType(typeof({type.FullTypeName}));");
                output.StartLine("public static readonly ");
                output.Write(ToString(typeof(CodeType)));
                output.Write(" ");
                output.Write(ClassCodeTypeFieldName);
                output.Write(" = GetClassCodeType(typeof(");
                output.Write(type.FullTypeName);
                output.EndLine("));");
            }

            // Write members that are constants
            foreach (var member in type.Members.OfType<ConstantUserTypeMember>())
                if (!(member.Type is TemplateArgumentTypeInstance))
                {
                    if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                        output.WriteLine(member.Comment);

                    bool shouldBeStaticReadonly;
                    string cvalue = ConstantValue(member, out shouldBeStaticReadonly);

                    //output.WriteLine($"{ToString(member.AccessLevel)} const {member.Type.GetTypeString()} {member.Name} = {ConstantValue(member)};");
                    output.StartLine(ToString(member.AccessLevel));
                    if (!shouldBeStaticReadonly)
                        output.Write("const ");
                    else
                        output.Write("static readonly ");
                    output.Write(member.Type.GetTypeString());
                    output.Write(" ");
                    output.Write(member.Name);
                    output.Write(" = ");
                    output.Write(cvalue);
                    output.EndLine(";");
                }

            // Write cache fields for data fields properties
            bool hasDataFields = type.BaseClass is MultiClassInheritanceTypeInstance
                || type.BaseClass is SingleClassInheritanceWithInterfacesTypeInstance
                || type.Members.OfType<DataFieldUserTypeMember>().Any()
                || type.Constructors.Contains(UserTypeConstructor.SimplePhysical);
            bool usesThisClass = type.BaseClass is MultiClassInheritanceTypeInstance
                || type.BaseClass is SingleClassInheritanceWithInterfacesTypeInstance;

            if (cacheUserTypeFields || lazyCacheUserTypeFields)
                foreach (var dataField in type.Members.OfType<DataFieldUserTypeMember>())
                {
                    if (dataField.IsStatic)
                        continue;
                    hasDataFields = true;

                    string initializationCode = GetDataFieldPropertyCode(type, dataField);
                    string accessLevel = ToString(AccessLevel.Private);
                    string fieldType = dataField.Type.GetTypeString();
                    string propertyName = dataField.Name;
                    string fieldName = GetUserTypeFieldName(propertyName);

                    if (initializationCode.Contains(ThisClassFieldName))
                        usesThisClass = true;

                    if (lazyCacheUserTypeFields)
                    {
                        fieldType = $"{ToString(typeof(UserMember))}<{fieldType}>";
                        initializationCode = $"{ToString(typeof(UserMember))}.Create(() => {initializationCode})";
                    }

                    // Write field code
                    //output.WriteLine($"{accessLevel}{fieldType} {fieldName};");
                    output.StartLine(accessLevel);
                    output.Write(fieldType);
                    output.Write(" ");
                    output.Write(fieldName);
                    output.EndLine(";");

                    fieldConstructorInitialization.Add(fieldName, initializationCode);
                }
            else
                foreach (var dataField in type.Members.OfType<DataFieldUserTypeMember>())
                    if (GetDataFieldPropertyCode(type, dataField).Contains(ThisClassFieldName))
                    {
                        usesThisClass = true;
                        break;
                    }

            // Write private class initialization
            if (!(type.BaseClass is StaticClassTypeInstance))
            {
                if (hasDataFields)
                {
                    if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                        output.WriteLine("// String that will be used to get user type for this class");

                    //output.WriteLine($"private static string {BaseClassStringFieldName} = GetBaseClassString(typeof({type.FullTypeName}));");
                    output.StartLine("private static string ");
                    output.Write(BaseClassStringFieldName);
                    output.Write(" = GetBaseClassString(typeof(");
                    output.Write(type.FullTypeName);
                    output.EndLine("));");

                    if (!GenerationFlags.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                        output.WriteLine();
                    if (usesThisClass)
                    {
                        if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                            output.WriteLine("// Cache of variable casted to user type this class was generated for");

                        //output.WriteLine($"private {ToString(typeof(UserMember))}<{ToString(typeof(Variable))}> {ThisClassFieldName};");
                        output.StartLine("private ");
                        output.Write(ToString(typeof(UserMember)));
                        output.Write("<");
                        output.Write(ToString(typeof(Variable)));
                        output.Write("> ");
                        output.Write(ThisClassFieldName);
                        output.EndLine(";");
                    }
                }
                if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment))
                    output.WriteLine("// Function that does initialization of user part of partial class");
                output.WriteLine("partial void PartialInitialize();");
            }

            // Write code for constructors
            int baseClassOffset = type.BaseClassOffset;

            foreach (var constructor in type.Constructors)
            {
                if (constructor.IsStatic)
                {
                    // Do nothing. We are initializing static variables in declaration statement because of the performance problems with generics.
                }
                else
                {
                    output.WriteLine();

                    //output.WriteLine($"{ToString(constructor.AccessLevel)}{type.ConstructorName}({arguments})");
                    output.StartLine(ToString(constructor.AccessLevel));
                    output.Write(type.ConstructorName);
                    output.Write("(");
                    for (int i = 0; i < constructor.Arguments.Length; i++)
                    {
                        var argument = constructor.Arguments[i];
                        object defaultValue;

                        output.Write(ToString(argument.Item1));
                        output.Write(" ");
                        output.Write(argument.Item2);
                        if (constructor.DefaultValues != null && constructor.DefaultValues.TryGetValue(argument.Item2, out defaultValue))
                        {
                            output.Write(" = ");
                            output.Write(ConstructorDefaultValueToString(defaultValue));
                        }
                        if (i + 1 < constructor.Arguments.Length)
                            output.Write(", ");
                    }
                    output.EndLine(")");

                    // Write constructor initialization
                    if (constructor == UserTypeConstructor.Simple)
                        output.WriteLine(1, ": base(variable)");
                    else if (constructor == UserTypeConstructor.SimplePhysical)
                    {
                        //output.WriteLine(1, $": this(variable.GetBaseClass({BaseClassStringFieldName}), {ToString(typeof(CsDebugScript.Debugger))}.ReadMemory(variable.GetCodeType().Module.Process, variable.GetBaseClass({BaseClassStringFieldName}).GetPointerAddress(), variable.GetBaseClass({BaseClassStringFieldName}).GetCodeType().Size), 0, variable.GetBaseClass({BaseClassStringFieldName}).GetPointerAddress())");
                        output.StartLine(1, ": this(variable.GetBaseClass(");
                        output.Write(BaseClassStringFieldName);
                        output.Write("), ");
                        output.Write(ToString(typeof(CsDebugScript.Debugger)));
                        output.Write(".ReadMemory(variable.GetCodeType().Module.Process, variable.GetBaseClass(");
                        output.Write(BaseClassStringFieldName);
                        output.Write(").GetPointerAddress(), variable.GetBaseClass(");
                        output.Write(BaseClassStringFieldName);
                        output.Write(").GetCodeType().RemovePointer().Size), 0, variable.GetBaseClass(");
                        output.Write(BaseClassStringFieldName);
                        output.EndLine(").GetPointerAddress())");
                    }
                    else if (constructor == UserTypeConstructor.RegularPhysical)
                    {
                        if (baseClassOffset > 0)
                        {
                            //output.WriteLine(1, $": base(variable, buffer, offset + {baseClassOffset}, bufferAddress)");
                            output.StartLine(1, ": base(variable, buffer, offset + ");
                            output.Write(baseClassOffset);
                            output.EndLine(", bufferAddress)");
                        }
                        else
                            output.WriteLine(1, ": base(variable, buffer, offset, bufferAddress)");
                    }
                    else if (constructor == UserTypeConstructor.ComplexPhysical)
                    {
                        if (baseClassOffset > 0)
                        {
                            //output.WriteLine(1, $": base(buffer, offset + {baseClassOffset}, bufferAddress, codeType, address, name, path)");
                            output.StartLine(1, ": base(buffer, offset + ");
                            output.Write(baseClassOffset);
                            output.EndLine(", bufferAddress, codeType, address, name, path)");
                        }
                        else
                            output.WriteLine(1, ": base(buffer, offset, bufferAddress, codeType, address, name, path)");
                    }

                    // Write content
                    StartBlock(output);
                    if (constructor.ContainsFieldDefinitions)
                        foreach (var fieldKvp in fieldConstructorInitialization)
                        {
                            string fieldName = fieldKvp.Key;
                            string fieldInitialization = fieldKvp.Value;

                            //output.WriteLine($"{fieldName} = {fieldInitialization};");
                            output.StartLine(fieldName);
                            output.Write(" = ");
                            output.Write(fieldInitialization);
                            output.EndLine(";");
                        }
                    if (constructor != UserTypeConstructor.SimplePhysical)
                    {
                        if (usesThisClass && hasDataFields && GenerationFlags.HasFlag(UserTypeGenerationFlags.UseDirectClassAccess))
                        {
                            //output.WriteLine($"{ThisClassFieldName} = {ToString(typeof(UserMember))}.Create(() => GetBaseClass({BaseClassStringFieldName}));");
                            output.StartLine(ThisClassFieldName);
                            output.Write(" = ");
                            output.Write(ToString(typeof(UserMember)));
                            output.Write(".Create(() => GetBaseClass(");
                            output.Write(BaseClassStringFieldName);
                            output.EndLine("));");
                        }
                        output.WriteLine("PartialInitialize();");
                    }
                    EndBlock(output);
                }
            }

            // Write properties for data fields
            foreach (var dataField in type.Members.OfType<DataFieldUserTypeMember>())
            {
                string propertyCode = GetDataFieldPropertyCode(type, dataField);
                string accessLevel = ToString(dataField.AccessLevel);
                string fieldType = dataField.Type.GetTypeString();
                string propertyName = dataField.Name;

                // Check if we should use field caching
                if (!dataField.IsStatic && (lazyCacheUserTypeFields || cacheUserTypeFields))
                {
                    string fieldName = GetUserTypeFieldName(propertyName);

                    propertyCode = lazyCacheUserTypeFields ? $"{fieldName}.Value" : fieldName;
                }

                WriteProperty(output, dataField.Comment, accessLevel, dataField.IsStatic, fieldType, propertyName, propertyCode);
            }

            // Write properties for Hungarian notation generated properties
            foreach (var dataField in type.Members.OfType<HungarianArrayUserTypeMember>())
            {
                string fieldType = dataField.Type.GetTypeString();
                string propertyCode = $"new {fieldType}({dataField.PointerFieldName}, {dataField.CounterFieldName})";
                string accessLevel = ToString(dataField.AccessLevel);

                WriteProperty(output, dataField.Comment, accessLevel, false, fieldType, dataField.Name, propertyCode);
            }

            // Write inner types
            WriteInnerTypes(type, output);

            // Write properties for getting base classes
            if (type.BaseClass is MultiClassInheritanceTypeInstance || type.BaseClass is SingleClassInheritanceWithInterfacesTypeInstance)
            {
                BaseClassPropertyUserTypeMember[] baseClassProperties = type.Members.OfType<BaseClassPropertyUserTypeMember>().OrderBy(b => b.Index).ToArray();

                foreach (BaseClassPropertyUserTypeMember baseClassProperty in baseClassProperties)
                {
                    string accessLevel = ToString(baseClassProperty.AccessLevel);
                    string returnType = baseClassProperty.Type.GetTypeString();
                    string propertyName = baseClassProperty.Name;
                    string propertyCode = GetBaseClassPropertyCode(type, baseClassProperty);

                    WriteProperty(output, baseClassProperty.Comment, accessLevel, false, returnType, propertyName, propertyCode);
                }

                // Write array of types for base classes
                string[] baseClassTypeStrings = baseClassProperties.Select(b => b.Type.GetTypeString()).ToArray();

                output.StartLine("public static System.Type[] ");
                output.Write(baseClassesArrayName);
                output.Write(" = new System.Type[] { ");
                for (int i = 0; i < baseClassProperties.Length; i++)
                {
                    if (i > 0)
                        output.Write(", ");
                    output.Write("typeof(");
                    output.Write(baseClassProperties[i].Type.GetTypeString());
                    output.Write(")");
                }
                output.EndLine("};");
            }

            // Write type end
            EndBlock(output);
        }
        #endregion

        /// <summary>
        /// Generates code for inner user types and writes it to the specified output.
        /// </summary>
        /// <param name="type">User type for which inner user types code should be generated.</param>
        /// <param name="output">Output indented writer.</param>
        private void WriteInnerTypes(UserType type, IndentedWriter output)
        {
            List<UserType> innerTypes = (type as TemplateUserType)?.SpecializedRepresentative?.InnerTypes ?? type.InnerTypes;

            foreach (var innerType in innerTypes)
            {
                if (innerType is SpecializedTemplateUserType)
                {
                    // Do nothing...
                    // Printing this type comes from updating specialized template user type with declared in type
                    // so declared in type get instances of specialized template user type and we just shouldn't print them.
                    continue;
                }

                output.WriteLine();
                WriteUserType(innerType, output, recursive: true);
            }
        }

        /// <summary>
        /// Generates physical data field property getter code.
        /// </summary>
        /// <param name="type">User type containing data field.</param>
        /// <param name="dataField">Data field that should be optimized for direct memery buffer access.</param>
        /// <returns>Getter code for the specified data field if it can be optimized; <c>null</c> otherwise.</returns>
        private string GetPhysicalDataFieldPropertyCode(UserType type, DataFieldUserTypeMember dataField)
        {
            // We shouldn't generate physical data field property code if user type is not physical user type.
            // Also, we should ignore static fields.
            PhysicalUserType physicalType = type as PhysicalUserType;

            if (physicalType == null || dataField.IsStatic || type.IsDeclaredInsideTemplate)
                return null;

            SymbolField field = dataField.Symbol;
            int offset = field.Offset - physicalType.MemoryBufferOffset;

            // Specialization for basic type
            if (dataField.Type is BasicTypeInstance basicType)
            {
                if (basicType.BasicType == typeof(string))
                {
                    int charSize = field.Type.ElementType.Size;

                    return $"ReadString(GetCodeType().Module.Process, ReadPointer({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {field.Type.Size}), {charSize})";
                }

                string basicTypeName = ToUserTypeName(basicType.BasicType);

                if (!string.IsNullOrEmpty(basicTypeName))
                {
                    if (field.LocationType == LocationType.BitField)
                        return $"Read{basicTypeName}({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {field.Size}, {field.BitPosition})";
                    else
                        return $"Read{basicTypeName}({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset})";
                }
            }
            // Specialization for arrays
            else if (dataField.Type is ArrayTypeInstance codeArrayType)
            {
                if (codeArrayType.ElementType is BasicTypeInstance basic)
                {
                    string basicTypeName = ToUserTypeName(basic.BasicType);

                    if (!string.IsNullOrEmpty(basicTypeName))
                    {
                        int arraySize = field.Type.Size;
                        int elementSize = field.Type.ElementType.Size;

                        codeArrayType.IsPhysical = true;
                        if (basicTypeName == "Char")
                            return $"Read{basicTypeName}Array({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {arraySize / elementSize}, {elementSize})";
                        else
                            return $"Read{basicTypeName}Array({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {arraySize / elementSize})";
                    }
                }
            }
            // Specialication for enum user type
            else if (dataField.Type is EnumTypeInstance enumType)
            {
                string basicTypeName = ToUserTypeName(enumType.EnumUserType.BasicType);

                if (!string.IsNullOrEmpty(basicTypeName))
                {
                    if (field.LocationType == LocationType.BitField)
                        return $"({dataField.Type.GetTypeString()})Read{basicTypeName}({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {field.Size}, {field.BitPosition})";
                    else
                        return $"({dataField.Type.GetTypeString()})Read{basicTypeName}({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset})";
                }
            }
            // Specialization for user types
            else if (dataField.Type is UserTypeInstance userType)
            {
                if (field.Type.Tag == Engine.CodeTypeTag.Pointer)
                {
                    // We can read pointer directly from memory buffer
                    string propertyCode = $"ReadPointer<{dataField.Type.GetTypeString()}>({ClassCodeTypeFieldName}, \"{dataField.Name}\", {MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {field.Type.Size})";

                    if (userType.UserType.Symbol.HasVTable() && userType.UserType.DerivedClasses.Count > 0)
                        propertyCode = $"{ToString(typeof(VariableCastExtender))}.DowncastObject({propertyCode})";
                    return propertyCode;
                }
                else if (userType.UserType.Constructors.Contains(UserTypeConstructor.ComplexPhysical))
                {
                    // We can create user type by directly calling physical constructor
                    string fieldAddress = $"{MemoryBufferAddressFieldName} + (ulong)({MemoryBufferOffsetFieldName} + {offset})";
                    string fieldCodeType;

                    if (!userType.UserType.IsDeclaredInsideTemplate && userType.UserType is PhysicalUserType)
                        fieldCodeType = $"{dataField.Type.GetTypeString()}.{ClassCodeTypeFieldName}";
                    else
                        fieldCodeType = $"{ClassCodeTypeFieldName}.GetClassFieldType(\"{dataField.Name}\")";
                    return $"new {dataField.Type.GetTypeString()}({MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {MemoryBufferAddressFieldName}, {fieldCodeType}, {fieldAddress}, \"{dataField.Name}\")";
                }
            }
            // Specialization for transformations
            else if (dataField.Type is TransformationTypeInstance transformationType)
            {
                if (field.Type.Tag != Engine.CodeTypeTag.Pointer)
                {
                    string fieldAddress = $"{MemoryBufferAddressFieldName} + (ulong)({MemoryBufferOffsetFieldName} + {offset})";
                    string fieldVariable = $"{ToString(typeof(Variable))}.CreateNoCast({ClassCodeTypeFieldName}.GetClassFieldType(\"{dataField.Name}\"), {fieldAddress}, \"{dataField.Name}\")";

                    if (transformationType.Transformation.Transformation.HasPhysicalConstructor)
                        fieldVariable = $"{fieldVariable}, {MemoryBufferFieldName}, {MemoryBufferOffsetFieldName} + {offset}, {MemoryBufferAddressFieldName}";
                    return $"new {dataField.Type.GetTypeString()}({fieldVariable})";
                }
            }

            // We don't know how to specialize this data field. Fall back to original output.
            return null;
        }

        /// <summary>
        /// Generates data field property getter code. It includes both static and non-static fields, but not constants.
        /// </summary>
        /// <param name="type">User type containing data field.</param>
        /// <param name="dataField">Data field for which we should generate property code.</param>
        private string GetDataFieldPropertyCode(UserType type, DataFieldUserTypeMember dataField)
        {
            // Try to get property code for physical user type
            string propertyCode = GetPhysicalDataFieldPropertyCode(type, dataField);

            if (propertyCode != null)
                return propertyCode;

            // Generate property code
            if (dataField.IsStatic)
            {
                if (type is TemplateUserType)
                    propertyCode = $"{ClassCodeTypeFieldName}.GetStaticField(\"{dataField.Name}\")";
                else if (string.IsNullOrEmpty(type.Symbol.Name))
                    propertyCode = $"{ToString(typeof(Process))}.Current.GetGlobal(\"{type.Module.Name}!{dataField.Name}\")";
                else
                    propertyCode = $"{ToString(typeof(Process))}.Current.GetGlobal(\"{type.Module.Name}!{type.Symbol.Name}::{dataField.Name}\")";
            }
            else
            {
                if (GenerationFlags.HasFlag(UserTypeGenerationFlags.UseDirectClassAccess))
                    propertyCode = $"{ThisClassFieldName}.Value.GetClassField(\"{dataField.Name}\")";
                else if (GenerationFlags.HasFlag(UserTypeGenerationFlags.CacheUserTypeFields)
                    || GenerationFlags.HasFlag(UserTypeGenerationFlags.LazyCacheUserTypeFields))
                    propertyCode = $"variable.GetField(\"{dataField.Name}\")";
                else
                    propertyCode = $"GetField(\"{dataField.Name}\")";
            }

            // Cast property code to correct return type
            if (dataField.Type is VariableTypeInstance)
            {
                // Do nothing, property code should remain the same
            }
            else if (dataField.Type is EnumTypeInstance enumType)
                propertyCode = $"({dataField.Type.GetTypeString()})({ToString(enumType.EnumUserType.BasicType)}){propertyCode}";
            else if (dataField.Type is TemplateTypeInstance templateType && templateType.UserType is EnumUserType enumType2)
                propertyCode = $"({dataField.Type.GetTypeString()})({ToString(enumType2.BasicType)}){propertyCode}";
            else if (dataField.Type is BasicTypeInstance basicType)
            {
                if (basicType.BasicType == typeof(string))
                    propertyCode += ".ToString()";
                else if (basicType.BasicType == typeof(NakedPointer))
                    propertyCode = $"new {ToString(basicType.BasicType)}({propertyCode})";
                else
                    propertyCode = $"({ToString(basicType.BasicType)}){propertyCode}";
            }
            else if ((GenerationFlags.HasFlag(UserTypeGenerationFlags.ForceUserTypesToNewInsteadOfCasting)
                || dataField.Type is ArrayTypeInstance || dataField.Type is PointerTypeInstance) && !(dataField.Type is TemplateArgumentTypeInstance))
                propertyCode = $"new {dataField.Type.GetTypeString()}({propertyCode})";
            else
                propertyCode = $"{propertyCode}.CastAs<{dataField.Type.GetTypeString()}>()";

            // Do downcasting if field is pointer and has vtable
            if (dataField.Symbol.Type.Tag == Engine.CodeTypeTag.Pointer
                && dataField.Type is UserTypeInstance userType)
            {
                if (userType.UserType.Symbol.HasVTable() && userType.UserType.DerivedClasses.Count > 0)
                    propertyCode = $"{ToString(typeof(VariableCastExtender))}.DowncastObject({propertyCode})";
            }

            return propertyCode;
        }

        /// <summary>
        /// Generates base class property getter code. It is being used with multi class inheritance.
        /// </summary>
        /// <param name="type">User type containing data field.</param>
        /// <param name="baseClassProperty">Base class property for which we should generate getter code.</param>
        private string GetBaseClassPropertyCode(UserType type, BaseClassPropertyUserTypeMember baseClassProperty)
        {
            string propertyCode;

            if (GenerationFlags.HasFlag(UserTypeGenerationFlags.UseDirectClassAccess))
                propertyCode = $"{ThisClassFieldName}.Value.GetBaseClass<{baseClassProperty.Type.GetTypeString()}, {type.TypeName}>({baseClassProperty.Index}, this)";
            else
                propertyCode = $"GetBaseClass(\"{baseClassProperty.Symbol.Name}\").CastAs<{baseClassProperty.Type.GetTypeString()}>()";
            return propertyCode;
        }

        /// <summary>
        /// Generates code for property and writes it to the specified output.
        /// </summary>
        /// <param name="output">Output indented writer.</param>
        /// <param name="comment">Comment line.</param>
        /// <param name="accessLevel">Access level.</param>
        /// <param name="isStatic">Is property static.</param>
        /// <param name="type">User type containing this property.</param>
        /// <param name="name">Property name.</param>
        /// <param name="code">Property getter code.</param>
        private void WriteProperty(IndentedWriter output, string comment, string accessLevel, bool isStatic, string type, string name, string code)
        {
            if (!GenerationFlags.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
                output.WriteLine();
            if (GenerationFlags.HasFlag(UserTypeGenerationFlags.GenerateFieldTypeInfoComment) && comment != null)
                output.WriteLine(comment);
            if (!GenerationFlags.HasFlag(UserTypeGenerationFlags.SingleLineProperty))
            {
                //output.WriteLine($"{accessLevel}static {type} {name}");
                output.StartLine(accessLevel);
                if (isStatic)
                    output.Write("static ");
                output.Write(type);
                output.Write(" ");
                output.EndLine(name);

                StartBlock(output);
                output.WriteLine("get");
                StartBlock(output);

                //output.WriteLine($"return {code};");
                output.StartLine("return ");
                output.Write(code);
                output.EndLine(";");

                EndBlock(output);
                EndBlock(output);
            }
            else
            {
                //output.WriteLine($"{accessLevel}static {type} {name} {{ get {{ return {code}; }} }}");
                output.StartLine(accessLevel);
                if (isStatic)
                    output.Write("static ");
                output.Write(type);
                output.Write(" ");
                output.Write(name);
                output.Write(" { get { return ");
                output.Write(code);
                output.EndLine("; } }");
            }
        }

        /// <summary>
        /// Generates code for constructor parameter default value.
        /// </summary>
        /// <param name="defaultValue">Parameter default value.</param>
        private string ConstructorDefaultValueToString(object defaultValue)
        {
            string stringValue = defaultValue as string;

            if (stringValue == Variable.ComputedName)
                return $"{ToString(typeof(Variable))}.{nameof(Variable.ComputedName)}";
            if (stringValue == Variable.UnknownPath)
                return $"{ToString(typeof(Variable))}.{nameof(Variable.UnknownPath)}";
            if (stringValue == Variable.UntrackedPath)
                return $"{ToString(typeof(Variable))}.{nameof(Variable.UntrackedPath)}";
            if (stringValue != null)
                return $"\"{stringValue}\"";
            if (defaultValue == null)
                return "null";
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates constant expression code for the specified constant field.
        /// </summary>
        /// <param name="constant">Constant field.</param>
        /// <param name="shouldBeStaticReadonly">Extracts flag if constant value needs to be evaluated in runtime and hense to be static readonly.</param>
        /// <returns>Constant expression assignment code.</returns>
        private string ConstantValue(ConstantUserTypeMember constant, out bool shouldBeStaticReadonly)
        {
            shouldBeStaticReadonly = false;

            Type constantType = (constant.Type as BasicTypeInstance)?.BasicType;

            if (constantType != null)
                return ConstantValue(constantType, constant.Value, out shouldBeStaticReadonly);

            if (constant.Type is PointerTypeInstance pointerType)
            {
                shouldBeStaticReadonly = true;
                return $"new {constant.Type.GetTypeString()}({ConstantValue(typeof(ulong), constant.Value)})";
            }

            EnumUserType enumUserType = (constant.Type as UserTypeInstance)?.UserType as EnumUserType;

            if (enumUserType != null)
                return ConstantValue(enumUserType, constant.Value, constant.Type.GetTypeString());
            return $"({constant.Type.GetTypeString()})({constant.Value})";
        }

        /// <summary>
        /// Generates constant expression code for the specified enumeration constant.
        /// </summary>
        /// <param name="enumUserType">Enumeration user type.</param>
        /// <param name="value">Constant value.</param>
        /// <param name="fullTypeName">Full type name. If <c>null</c>, full type name of enumeration user type will be used.</param>
        /// <returns>Constant expression assignment code.</returns>
        private string ConstantValue(EnumUserType enumUserType, object value, string fullTypeName = null)
        {
            string svalue = value.ToString();

            if (fullTypeName == null)
                fullTypeName = enumUserType.FullTypeName;

            if (enumUserType.Symbol.EnumValuesByValue.TryGetValue(svalue, out string entry))
                return $"{fullTypeName}.{entry}";

            Type enumBasicType = enumUserType.BasicType;

            if (enumBasicType != null)
            {
                string cvalue = ConstantValue(enumBasicType, svalue);

                if (cvalue[0] == '-')
                    return $"({fullTypeName})({cvalue})";
                return $"({fullTypeName}){cvalue}";
            }
            return $"({fullTypeName})({svalue})";
        }

        /// <summary>
        /// Generates constant expression code of the specified built-in type for the specified constant value.
        /// </summary>
        /// <param name="type">Built-in type.</param>
        /// <param name="value">Constant value.</param>
        private string ConstantValue(Type type, object value)
        {
            bool shouldBeStaticReadonly;

            return ConstantValue(type, value, out shouldBeStaticReadonly);
        }

        /// <summary>
        /// Generates constant expression code of the specified built-in type for the specified constant value.
        /// </summary>
        /// <param name="type">Built-in type.</param>
        /// <param name="value">Constant value.</param>
        /// <param name="shouldBeStaticReadonly">Extracts flag if constant value needs to be evaluated in runtime and hense to be static readonly.</param>
        private string ConstantValue(Type type, object value, out bool shouldBeStaticReadonly)
        {
            shouldBeStaticReadonly = false;
            if (type == value.GetType())
            {
                if (type == typeof(bool))
                    return value.ToString().ToLower();
                return value.ToString();
            }

            string constantValue = value.ToString();

            if (constantValue[0] == '-')
            {
                if (type == typeof(ulong))
                    return ((ulong)long.Parse(constantValue)).ToString();
                if (type == typeof(uint))
                    return ((uint)int.Parse(constantValue)).ToString();
                if (type == typeof(ushort))
                    return ((ushort)short.Parse(constantValue)).ToString();
                if (type == typeof(byte))
                    return ((byte)sbyte.Parse(constantValue)).ToString();
                if (type == typeof(NakedPointer))
                {
                    shouldBeStaticReadonly = true;
                    return $"new {ToString(type)}({((ulong)long.Parse(constantValue)).ToString()})";
                }
            }
            if (type == typeof(byte))
                return byte.Parse(constantValue).ToString();
            if (type == typeof(sbyte))
                if (sbyte.TryParse(constantValue, out sbyte v))
                    return v.ToString();
                else
                    return $"(sbyte){constantValue}";
            if (type == typeof(short))
                return short.Parse(constantValue).ToString();
            if (type == typeof(ushort))
                return ushort.Parse(constantValue).ToString();
            if (type == typeof(int))
                return int.Parse(constantValue).ToString();
            if (type == typeof(uint))
                return uint.Parse(constantValue).ToString();
            if (type == typeof(long))
                return long.Parse(constantValue).ToString();
            if (type == typeof(ulong))
                return ulong.Parse(constantValue).ToString();
            if (type == typeof(float))
                return $"{float.Parse(constantValue):R}f";
            if (type == typeof(double))
                return double.Parse(constantValue).ToString("R");
            if (type == typeof(bool))
                return (int.Parse(constantValue) != 0).ToString().ToLower();
            if (type == typeof(char))
                return $"(char){int.Parse(constantValue)}";

            shouldBeStaticReadonly = true;
            if (type == typeof(NakedPointer))
                return $"new {ToString(type)}({ulong.Parse(constantValue).ToString()})";

            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates code for the specified access level.
        /// </summary>
        /// <param name="accessLevel">Access level.</param>
        private static string ToString(AccessLevel accessLevel)
        {
            switch (accessLevel)
            {
                case AccessLevel.Default:
                    return string.Empty;
                case AccessLevel.Internal:
                    return "internal ";
                case AccessLevel.Private:
                    return "private ";
                case AccessLevel.Protected:
                    return "protected ";
                case AccessLevel.Public:
                    return "public ";
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Starts a new block on the specified output and increments indentation.
        /// </summary>
        /// <param name="output">Output indented writer.</param>
        private static void StartBlock(IndentedWriter output)
        {
            output.WriteLine("{");
            output.CurrentIndent++;
        }

        /// <summary>
        /// Ends last block on the specified output and decremenets indentation.
        /// </summary>
        /// <param name="output">Output indented writer.</param>
        private static void EndBlock(IndentedWriter output)
        {
            output.CurrentIndent--;
            output.WriteLine("}");
        }
    }
}
