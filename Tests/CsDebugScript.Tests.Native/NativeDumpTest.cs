using std = CsDebugScript.CommonUserTypes.NativeTypes.std;
using System;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Debuggers.DbgEngDllHelpers;
using CsDebugScript.CommonUserTypes;

namespace CsDebugScript.Tests
{
    public abstract class NativeDumpTest : DumpTestBase
    {
        private const string MainSourceFileName = "nativedumptest.cpp";

        public NativeDumpTest(DumpInitialization dumpInitialization, bool executeCodeGen = true)
            : base(dumpInitialization)
        {
            ExecuteCodeGen = executeCodeGen;
            if (ExecuteCodeGen && !DumpInitialization.CodeGenExecuted)
            {
                InterpretInteractive($@"
var options = new ImportUserTypeOptions();
options.Modules.Add(""{DefaultModuleName}"");
options.UseILCodeWriter = {DumpInitialization.UseILCodeGen.ToString().ToLower()};
ImportUserTypes(options, true);
                    ");
                DumpInitialization.CodeGenExecuted = true;
            }
        }

        public bool ExecuteCodeGen { get; private set; }

        public bool ReleaseDump { get; set; }

        public bool LinuxDump { get; set; }

        public bool NoRtti { get; set; }

        public bool UsingDbgEngSymbolProvider
        {
            get
            {
                return Context.SymbolProvider is DbgEngSymbolProvider;
            }
        }

        [Fact]
        public void CurrentThreadContainsMainSourceFileName()
        {
            foreach (var frame in Thread.Current.StackTrace.Frames)
            {
                try
                {
                    if (frame.SourceFileName.ToLower().EndsWith(MainSourceFileName))
                    {
                        return;
                    }
                }
                catch
                {
                    // Ignore exception for getting source file name for frames where we don't have PDBs
                }
            }

            throw new Exception($"{MainSourceFileName} not found on the current thread stack trace");
        }

        [Fact]
        public void CurrentThreadContainsDefaultTestCaseFunction()
        {
            Assert.NotNull(GetFrame($"{DefaultModuleName}!DefaultTestCase"));
        }

        [Fact]
        public void CurrentProcessContainsDefaultModule()
        {
            Assert.Contains(Module.All, module => module.Name == DefaultModuleName);
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(LinuxDump))]
        public void CheckProcess()
        {
            Process process = Process.Current;

            Assert.NotEqual(ArchitectureType.Unknown, process.ArchitectureType);
            Assert.NotEqual(0U, process.SystemId);
            Assert.NotEmpty(Process.All);
            Assert.NotEqual(-1, process.FindMemoryRegion(DefaultModule.Address));
            Assert.Equal(DefaultModule.ImageName, process.ExecutableName);
            Assert.NotNull(process.PEB);
            Assert.Null(process.CurrentCLRAppDomain);
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(LinuxDump))]
        public void CheckThread()
        {
            Thread thread = Thread.Current;

            Assert.NotEmpty(Thread.All);
            Assert.NotNull(thread.Locals);
            Assert.NotNull(thread.TEB);
            Assert.NotNull(thread.ThreadContext);
        }

        [Fact]
        public void CheckCodeFunction()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!DefaultTestCase");
            CodeFunction defaultTestCaseFunction = new CodeFunction(defaultTestCaseFrame.InstructionOffset);

            Assert.NotEqual(0U, defaultTestCaseFunction.Address);
            Assert.NotEqual(0U, defaultTestCaseFunction.FunctionDisplacement);
            Assert.Equal($"{DefaultModuleName}!DefaultTestCase", defaultTestCaseFunction.FunctionName);
            Assert.Equal($"DefaultTestCase", defaultTestCaseFunction.FunctionNameWithoutModule);
            Assert.Equal(Process.Current, defaultTestCaseFunction.Process);
            Assert.Contains(MainSourceFileName, defaultTestCaseFunction.SourceFileName.ToLower());
            Assert.NotEqual(0U, defaultTestCaseFunction.SourceFileLine);
            Assert.True(defaultTestCaseFunction.SourceFileDisplacement < int.MaxValue);

            Variable codeFunctionVariable = DefaultModule.GetVariable($"{DefaultModuleName}!defaultTestCaseAddress");

            Assert.True(codeFunctionVariable.GetCodeType().IsPointer);
            Assert.True(!codeFunctionVariable.IsNull());

            CodeFunction codeFunction = new CodePointer<CodeFunction>(new NakedPointer(codeFunctionVariable)).Element;

            Assert.Equal($"{DefaultModuleName}!DefaultTestCase", codeFunction.FunctionName);
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(LinuxDump))]
        public void CheckDebugger()
        {
            Assert.NotEmpty(Debugger.FindAllPatternInMemory(0x1212121212121212));
            Assert.NotEmpty(Debugger.FindAllBytePatternInMemory(new byte[] { 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12 }));
            Assert.NotEmpty(Debugger.FindAllTextPatternInMemory("qwerty"));
        }

        [Fact]
        public void ReadingFloatPointTypes()
        {
            Variable doubleTest = DefaultModule.GetVariable("doubleTest");

            Assert.Equal(3.5, (double)doubleTest.GetField("d"));
            Assert.Equal(2.5, (float)doubleTest.GetField("f"));
            Assert.Equal(5, (int)doubleTest.GetField("i"));

            Variable doubleTest2 = Process.Current.GetGlobal($"{DefaultModuleName}!doubleTest");

            Assert.Equal(doubleTest.GetPointerAddress(), doubleTest2.GetPointerAddress());
            Assert.Equal(doubleTest.GetCodeType(), doubleTest2.GetCodeType());
        }

        [Fact]
        public void VariableConstants()
        {
            Assert.True((bool)Variable.CreateConstant(true));
            Assert.Equal((byte)42, (byte)Variable.CreateConstant((byte)42));
            Assert.Equal((sbyte)-42, (sbyte)Variable.CreateConstant((sbyte)-42));
            Assert.Equal((short)-42, (short)Variable.CreateConstant((short)-42));
            Assert.Equal((ushort)42, (ushort)Variable.CreateConstant((ushort)42));
            Assert.Equal((int)-42, (int)Variable.CreateConstant((int)-42));
            Assert.Equal((uint)42, (uint)Variable.CreateConstant((uint)42));
            Assert.Equal((long)-42, (long)Variable.CreateConstant((long)-42));
            Assert.Equal((ulong)42, (ulong)Variable.CreateConstant((ulong)42));
            Assert.Equal((float)0.42, (float)Variable.CreateConstant((float)0.42));
            Assert.Equal((double)-0.42, (double)Variable.CreateConstant((double)-0.42));
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(ReleaseDump))]
        public void GettingClassStaticMember()
        {
            Variable staticVariable = DefaultModule.GetVariable("MyTestClass::staticVariable");

            Assert.Equal(1212121212, (int)staticVariable);
        }

        [Fact]
        public void CheckMainArguments()
        {
            StackFrame mainFrame = GetFrame($"{DefaultModuleName}!main");
            VariableCollection arguments = mainFrame.Arguments;

            Assert.True(arguments.ContainsName("argc"));
            Assert.True(arguments.ContainsName("argv"));
            Assert.Equal(1, (int)arguments["argc"]);
            Assert.Equal(2, arguments.Count);

            for (int i = 0; i < arguments.Count; i++)
            {
                Variable argument = arguments[i];

                Assert.False(argument.IsNull());
            }

            string command = arguments["argv"].GetArrayElement(0).ToString();
            Assert.Contains("NativeDumpTest", command);

            Variable p;
            Assert.False(arguments.TryGetValue("p", out p));
            Assert.Empty(arguments.Names.Where(n => n == "p"));
            Assert.Empty(arguments.Where(a => a.GetName() == "p"));
        }

        [Fact]
        public void CheckDefaultTestCase()
        {
            StackFrame frame = GetFrame($"{DefaultModuleName}!DefaultTestCase");
            VariableCollection locals = frame.Locals;
            dynamic p = locals["p"];
            std.wstring string1 = new std.wstring(p.string1);
            Assert.Equal("qwerty", string1.Text);
            std.list<std.wstring> strings = new std.list<std.wstring>(p.strings);
            std.vector<std.@string> ansiStrings = new std.vector<std.@string>(p.ansiStrings);
            std.map<std.wstring, std.@string> stringMap = new std.map<std.wstring, std.@string>(p.stringMap);
            std.unordered_map<std.wstring, std.@string> stringUMap = new std.unordered_map<std.wstring, std.@string>(p.stringUMap);

            string[] stringsConverted = strings.Select(s => s.Text).ToArray();
            string[] ansiStringsConverted = ansiStrings.Select(s => s.Text).ToArray();

            CompareArrays(new[] { "Foo", "Bar" }, stringsConverted);
            CompareArrays(new[] { "AnsiFoo", "AnsiBar" }, ansiStringsConverted);

            foreach (std.wstring s in strings)
            {
                Assert.True(s.Length <= s.Reserved);
            }
            for (int i = 0; i < ansiStrings.Count; i++)
            {
                Assert.True(ansiStrings[i].Length <= ansiStrings[i].Reserved);
            }

            VerifyMap(stringMap);
            VerifyMap(stringUMap);

            // Verify enum value
            dynamic e = locals["e"];

            Assert.Equal("enumEntry3", e.ToString());
            Assert.Equal(3, (int)e);

            dynamic pEnumeration = p.enumeration;
            dynamic pInnerEnumeration = p.innerEnumeration;
            Assert.Equal("enumEntry2", pEnumeration.ToString());
            Assert.Equal(2, (int)pEnumeration);
            Assert.Equal("simple4", pInnerEnumeration.ToString());
            Assert.Equal(4, (int)pInnerEnumeration);

            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
MyTestClass global = ModuleGlobals.globalVariable;
AreEqual(1212121212, MyTestClass.staticVariable);
AreEqual(""qwerty"", global.string1.Text);
AreEqual(2, global.strings.Count);
AreEqual(""Foo"", global.strings.ElementAt(0).Text);
AreEqual(""Bar"", global.strings.ElementAt(1).Text);
AreEqual(2, global.ansiStrings.Count);
AreEqual(""AnsiFoo"", global.ansiStrings[0].Text);
AreEqual(""AnsiBar"", global.ansiStrings[1].Text);
AreEqual(MyEnum.enumEntry2, global.enumeration);
AreEqual(MyTestClass.MyEnumInner.simple4, global.innerEnumeration);
                    ");
            }
        }

        [Fact]
        public void CheckSharedWeakPointers()
        {
            StackFrame frame = GetFrame($"{DefaultModuleName}!TestSharedWeakPointers");
            VariableCollection locals = frame.Locals;

            // Verify shared/weak pointers
            std.shared_ptr<int> sptr1 = new std.shared_ptr<int>(locals["sptr1"]);
            std.shared_ptr<int> esptr1 = new std.shared_ptr<int>(locals["esptr1"]);
            std.shared_ptr<int> esptr2 = new std.shared_ptr<int>(locals["esptr2"]);
            std.weak_ptr<int> wptr1 = new std.weak_ptr<int>(locals["wptr1"]);
            std.weak_ptr<int> ewptr1 = new std.weak_ptr<int>(locals["ewptr1"]);
            std.weak_ptr<int> ewptr2 = new std.weak_ptr<int>(locals["ewptr2"]);

            Assert.False(sptr1.IsEmpty);
            Assert.Equal(1, sptr1.SharedCount);
            Assert.Equal(2, sptr1.WeakCount);
            Assert.Equal(5, sptr1.Element);
            Assert.True(sptr1.IsCreatedWithMakeShared);

            Assert.False(wptr1.IsEmpty);
            Assert.Equal(1, wptr1.SharedCount);
            Assert.Equal(2, wptr1.WeakCount);
            Assert.Equal(5, wptr1.Element);
            Assert.True(wptr1.IsCreatedWithMakeShared);

            Assert.True(esptr1.IsEmpty);

            Assert.True(ewptr1.IsEmpty);
            Assert.Equal(0, ewptr1.SharedCount);
            Assert.Equal(1, ewptr1.WeakCount);
            if (!LinuxDump || !NoRtti)
            {
                Assert.Equal(42, ewptr1.UnsafeElement);
            }
            Assert.True(ewptr1.IsCreatedWithMakeShared);

            Assert.True(esptr2.IsEmpty);

            Assert.True(ewptr2.IsEmpty);
            Assert.Equal(0, ewptr2.SharedCount);
            Assert.Equal(1, ewptr2.WeakCount);
            Assert.False(ewptr2.IsCreatedWithMakeShared);
        }

        [Fact]
        public void CheckCodeArray()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestArray");
            VariableCollection locals = defaultTestCaseFrame.Locals;
            Variable testArrayVariable = locals["testArray"];
            CodeArray<int> testArray = new CodeArray<int>(testArrayVariable);

            Assert.Equal(10000, testArray.Length);
            foreach (int value in testArray)
                Assert.Equal(0x12121212, value);

            Variable testStdArrayVariable = locals["testStdArray"];
            std.array<int> testStdArray = new std.array<int>(testStdArrayVariable);

            Assert.Equal(10000, testStdArray.Count);
            foreach (int value in testStdArray)
                Assert.Equal(0x12121212, value);
            Assert.Equal(0x12121212, testStdArray[testStdArray.Count - 1]);

            int[] a = testStdArray.ToArray();

            Assert.Equal(10000, a.Length);
            foreach (int value in a)
                Assert.Equal(0x12121212, value);
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(UsingDbgEngSymbolProvider))]
        public void SimpleMultiClassInheritance()
        {
            Variable c = DefaultModule.GetVariable("simpleMultiClassInheritanceTest");
            Assert.Equal(42.0f, (float)c.GetField("c"));
            Assert.Equal(42.0, (double)c.GetField("b"));
            Assert.Equal(42, (byte)c.GetField("a"));
            Variable a = c.GetBaseClass("SimpleMultiClassInheritanceA");
            Assert.Equal(42, (byte)a.GetField("a"));
            Variable b = c.GetBaseClass("SimpleMultiClassInheritanceB");
            Assert.Equal(42.0, (double)b.GetField("b"));

            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
Variable global = Process.Current.GetGlobal(""{DefaultModuleName}!simpleMultiClassInheritanceTest"");
var c = new SimpleMultiClassInheritanceC(global);
AreEqual(42.0f, c.c);
AreEqual(42, c.BaseClass_SimpleMultiClassInheritanceA.a);
AreEqual(42.0, c.BaseClass_SimpleMultiClassInheritanceB.b);
                    ");
            }
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(UsingDbgEngSymbolProvider))]
        public void MultiClassInheritance()
        {
            Variable c = DefaultModule.GetVariable("multiClassInheritanceTest");
            Assert.Equal(42, (double)c.GetField("c"));
            Assert.Equal(42, (int)c.GetField("b"));
            Assert.Equal(42, (float)c.GetField("a"));
            Variable a = c.GetBaseClass("MultiClassInheritanceA");
            Assert.Equal(42, (float)a.GetField("a"));
            Variable b = c.GetBaseClass("MultiClassInheritanceB");
            Assert.Equal(42, (int)b.GetField("b"));
            Assert.Equal(c.GetPointerAddress(), a.DowncastInterface().GetPointerAddress());
            Assert.Equal(c.GetPointerAddress(), b.DowncastInterface().GetPointerAddress());

            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
Variable global = Process.Current.GetGlobal(""{DefaultModuleName}!multiClassInheritanceTest"");
var c = new MultiClassInheritanceC(global);
var a = c.BaseClass_MultiClassInheritanceA;
var b = c.BaseClass_MultiClassInheritanceB;
AreEqual(42, c.c);
AreEqual(42, a.a);
AreEqual(42, b.b);
IsTrue(a.GetDowncast() is MultiClassInheritanceC);
IsTrue(b.GetDowncast() is MultiClassInheritanceC);
AreEqual(42, a.As<MultiClassInheritanceB>().b);
                    ");
            }
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(UsingDbgEngSymbolProvider))]
        public void MultiClassInheritance2()
        {
            Variable d = DefaultModule.GetVariable("multiClassInheritanceTest2");
            Assert.Equal(42, (double)d.GetField("d"));
            Assert.Equal(42, (int)d.GetField("b"));
            Assert.Equal(42, (float)d.GetField("a"));
            Variable a = d.GetBaseClass("MultiClassInheritanceA");
            Assert.Equal(42, (float)a.GetField("a"));
            Variable b = d.GetBaseClass("MultiClassInheritanceB");
            Assert.Equal(42, (int)b.GetField("b"));
            Assert.Equal(d.GetPointerAddress(), a.DowncastInterface().GetPointerAddress());
            Assert.Equal(d.GetPointerAddress(), b.DowncastInterface().GetPointerAddress());

            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
Variable global = Process.Current.GetGlobal(""{DefaultModuleName}!multiClassInheritanceTest2"");
var d = new MultiClassInheritanceD(global);
var a = d.BaseClass_MultiClassInheritanceA;
var b = d.BaseClass_MultiClassInheritanceB;
AreEqual(42, d.d);
AreEqual(42, a.a);
AreEqual(42, b.b);
IsTrue(a.GetDowncast() is MultiClassInheritanceD);
IsTrue(b.GetDowncast() is MultiClassInheritanceD);
AreEqual(42, a.As<MultiClassInheritanceB>().b);
                    ");
            }
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(UsingDbgEngSymbolProvider))]
        public void VirtualMultiClassInheritance()
        {
            Variable d = DefaultModule.GetVariable("virtualMultiClassInheritanceTest");
            Assert.Equal(42, (int)d.GetField("d"));
            Assert.Equal(42, (double)d.GetField("b"));
            Assert.Equal(42, (float)d.GetField("c"));
            Assert.Equal(42, (sbyte)d.GetField("a"));
            Variable b = d.GetBaseClass("VirtualMultiClassInheritanceB");
            Assert.Equal(42, (double)b.GetField("b"));
            Variable ba = b.GetBaseClass("VirtualMultiClassInheritanceA");
            Assert.Equal(42, (sbyte)ba.GetField("a"));
            Variable c = d.GetBaseClass("VirtualMultiClassInheritanceC");
            Assert.Equal(42, (float)c.GetField("c"));
            Variable ca = c.GetBaseClass("VirtualMultiClassInheritanceA");
            Assert.Equal(42, (sbyte)ca.GetField("a"));
            Assert.Equal(ca.GetPointerAddress(), ba.GetPointerAddress());
            Assert.Equal(d.GetPointerAddress(), b.DowncastInterface().GetPointerAddress());
            Assert.Equal(ba.GetPointerAddress(), ba.DowncastInterface().GetPointerAddress());
            Assert.Equal(d.GetPointerAddress(), c.DowncastInterface().GetPointerAddress());
            Assert.Equal(ca.GetPointerAddress(), ca.DowncastInterface().GetPointerAddress());

            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
Variable global = Process.Current.GetGlobal(""{DefaultModuleName}!virtualMultiClassInheritanceTest"");
var d = new VirtualMultiClassInheritanceD(global);
var b = d.BaseClass_VirtualMultiClassInheritanceB;
var ba = b.BaseClass;
var c = d.BaseClass_VirtualMultiClassInheritanceC;
var ca = c.BaseClass;
AreEqual(42, d.d);
AreEqual(42, b.b);
AreEqual(42, ba.a);
AreEqual(42, c.c);
AreEqual(42, ca.a);
IsTrue(b.GetDowncast() is VirtualMultiClassInheritanceD);
IsTrue(ba.GetDowncast() is VirtualMultiClassInheritanceD);
IsTrue(c.GetDowncast() is VirtualMultiClassInheritanceD);
IsTrue(ca.GetDowncast() is VirtualMultiClassInheritanceD);
AreEqual(42, b.As<VirtualMultiClassInheritanceC>().c);
AreEqual(42, ba.As<VirtualMultiClassInheritanceC>().c);
AreEqual(42, ca.As<VirtualMultiClassInheritanceB>().b);
                    ");
            }
        }

        [SkippableFact(SkipOnFailurePropertyName = nameof(UsingDbgEngSymbolProvider))]
        public void VirtualMultiClassInheritance2()
        {
            Variable e = DefaultModule.GetVariable("virtualMultiClassInheritanceTest2");
            Assert.Equal(42, (int)e.GetField("e"));
            Assert.Equal(42, (double)e.GetField("b"));
            Assert.Equal(42, (float)e.GetField("c"));
            Assert.Equal(42, (sbyte)e.GetField("a"));
            Variable a = e.GetBaseClass("VirtualMultiClassInheritanceA");
            Assert.Equal(42, (sbyte)a.GetField("a"));
            Variable b = e.GetBaseClass("VirtualMultiClassInheritanceB");
            Assert.Equal(42, (double)b.GetField("b"));
            Variable ba = b.GetBaseClass("VirtualMultiClassInheritanceA");
            Assert.Equal(42, (sbyte)ba.GetField("a"));
            Variable c = e.GetBaseClass("VirtualMultiClassInheritanceC");
            Assert.Equal(42, (float)c.GetField("c"));
            Variable ca = c.GetBaseClass("VirtualMultiClassInheritanceA");
            Assert.Equal(42, (sbyte)ca.GetField("a"));
            Assert.Equal(a.GetPointerAddress(), ba.GetPointerAddress());
            Assert.Equal(ca.GetPointerAddress(), ba.GetPointerAddress());
            Assert.Equal(e.GetPointerAddress(), b.DowncastInterface().GetPointerAddress());
            Assert.Equal(ba.GetPointerAddress(), ba.DowncastInterface().GetPointerAddress());
            Assert.Equal(e.GetPointerAddress(), c.DowncastInterface().GetPointerAddress());
            Assert.Equal(ca.GetPointerAddress(), ca.DowncastInterface().GetPointerAddress());

            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
Variable global = Process.Current.GetGlobal(""{DefaultModuleName}!virtualMultiClassInheritanceTest2"");
var e = new VirtualMultiClassInheritanceE(global);
var a = e.BaseClass_VirtualMultiClassInheritanceA;
var b = e.BaseClass_VirtualMultiClassInheritanceB;
var ba = b.BaseClass;
var c = e.BaseClass_VirtualMultiClassInheritanceC;
var ca = c.BaseClass;
AreEqual(42, e.e);
AreEqual(42, a.a);
AreEqual(42, b.b);
AreEqual(42, ba.a);
AreEqual(42, c.c);
AreEqual(42, ca.a);
IsTrue(a.GetDowncast() is VirtualMultiClassInheritanceE);
IsTrue(b.GetDowncast() is VirtualMultiClassInheritanceE);
IsTrue(ba.GetDowncast() is VirtualMultiClassInheritanceE);
IsTrue(c.GetDowncast() is VirtualMultiClassInheritanceE);
IsTrue(ca.GetDowncast() is VirtualMultiClassInheritanceE);
AreEqual(42, b.As<VirtualMultiClassInheritanceC>().c);
AreEqual(42, a.As<VirtualMultiClassInheritanceB>().b);
AreEqual(42, ba.As<VirtualMultiClassInheritanceC>().c);
AreEqual(42, ca.As<VirtualMultiClassInheritanceB>().b);
                    ");
            }
        }

        [Fact]
        public void TestBasicTemplateType()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestBasicTemplateType");
            VariableCollection locals = defaultTestCaseFrame.Locals;
            Variable floatTemplate = locals["floatTemplate"];
            Variable doubleTemplate = locals["doubleTemplate"];
            Variable intTemplate = locals["intTemplate"];
            Variable[] templateVariables = new Variable[] { floatTemplate, doubleTemplate, intTemplate };

            foreach (Variable variable in templateVariables)
            {
                Variable value = variable.GetField("value");
                Variable values = variable.GetField("values");
                Assert.Equal("42", value.ToString());
                for (int i = 0, n = values.GetArrayLength(); i < n; i++)
                {
                    Assert.Equal(i.ToString(), values.GetArrayElement(i).ToString());
                }
            }

            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
StackFrame frame = GetFrame(""{DefaultModuleName}!TestBasicTemplateType"");
VariableCollection locals = frame.Locals;

var floatTemplate = new BasicTemplateType<float>(locals[""floatTemplate""]);
AreEqual(42, floatTemplate.value);
for (int i = 0; i < floatTemplate.values.Length; i++)
    AreEqual(i, floatTemplate.values[i]);

var doubleTemplate = new BasicTemplateType<double>(locals[""doubleTemplate""]);
AreEqual(42, doubleTemplate.value);
for (int i = 0; i < doubleTemplate.values.Length; i++)
    AreEqual(i, doubleTemplate.values[i]);

var intTemplate = new BasicTemplateType<int>(locals[""intTemplate""]);
AreEqual(42, intTemplate.value);
for (int i = 0; i < intTemplate.values.Length; i++)
    AreEqual(i, intTemplate.values[i]);
                   ");
            }
        }

        [Fact]
        public void TestBuiltinTypes()
        {
            CodeType codeType = CodeType.Create("BuiltinTypesTest", DefaultModule);

            VerifyFieldBuiltinType(codeType, "b", BuiltinType.Bool);
            VerifyFieldBuiltinType(codeType, "c1", BuiltinType.Char8, BuiltinType.Int8);
            VerifyFieldBuiltinType(codeType, "c2", BuiltinType.Char16, BuiltinType.Char32);
            VerifyFieldBuiltinType(codeType, "i8", BuiltinType.Int8, BuiltinType.Char8);
            VerifyFieldBuiltinType(codeType, "i16", BuiltinType.Int16);
            VerifyFieldBuiltinType(codeType, "i32", BuiltinType.Int32);
            VerifyFieldBuiltinType(codeType, "i64", BuiltinType.Int64);
            VerifyFieldBuiltinType(codeType, "u8", BuiltinType.UInt8);
            VerifyFieldBuiltinType(codeType, "u16", BuiltinType.UInt16);
            VerifyFieldBuiltinType(codeType, "u32", BuiltinType.UInt32);
            VerifyFieldBuiltinType(codeType, "u64", BuiltinType.UInt64);
            VerifyFieldBuiltinType(codeType, "f32", BuiltinType.Float32);
            VerifyFieldBuiltinType(codeType, "f64", BuiltinType.Float64);
            VerifyFieldBuiltinType(codeType, "f80", BuiltinType.Float80, BuiltinType.Float64);
        }

        private void VerifyFieldBuiltinType(CodeType codeType, string fieldName, params BuiltinType[] expected)
        {
            CodeType fieldCodeType = codeType.GetFieldType(fieldName);
            NativeCodeType nativeCodeType = fieldCodeType as NativeCodeType;

            Assert.NotNull(nativeCodeType);
            Assert.True(nativeCodeType.Tag == CodeTypeTag.BuiltinType || nativeCodeType.Tag == CodeTypeTag.Enum);
            VerifyBuiltinType(nativeCodeType, expected);
        }

        [Fact]
        public void TestReadingBuiltinTypes()
        {
            Variable var = DefaultModule.GetVariable("builtinTypesTest");
            TestReading<bool>(var.GetField("b"), false);
            TestReading<sbyte>(var.GetField("i8"), 42);
            TestReading<short>(var.GetField("i16"), 42);
            TestReading<int>(var.GetField("i32"), 42);
            TestReading<long>(var.GetField("i64"), 42);
            TestReading<byte>(var.GetField("u8"), 42);
            TestReading<ushort>(var.GetField("u16"), 42);
            TestReading<uint>(var.GetField("u32"), 42);
            TestReading<ulong>(var.GetField("u64"), 42);
            TestReading<float>(var.GetField("f32"), 42);
            TestReading<double>(var.GetField("f64"), 42);
        }

        private static void TestReading<T>(Variable var, T expectedValue)
        {
            Assert.Equal(expectedValue, var.CastAs<T>());
            Assert.Equal(expectedValue, (T)Convert.ChangeType(var, typeof(T)));
            NakedPointer pointer = new NakedPointer(var.GetPointerAddress());
            CodePointer<T> codePointer = new CodePointer<T>(pointer);
            Assert.Equal(expectedValue, codePointer.Element);
            codePointer = new CodePointer<T>(pointer.GetPointerAddress());
            Assert.Equal(expectedValue, codePointer.Element);
            codePointer = new CodePointer<T>(var.GetPointer());
            Assert.Equal(expectedValue, codePointer.Element);
            CodeArray<T> codeArray = new CodeArray<T>(pointer, 1);
            Assert.Equal(expectedValue, codeArray[0]);
        }

        [UserType(TypeName = "DoubleTest")]
        public class DoubleTest : DynamicSelfUserType
        {
            public DoubleTest(Variable variable)
                : base(variable)
            {
            }

            public double d => (double)self.d;

            public float f => (float)self.f;

            public int i => (int)self.i;
        }

        [Fact]
        public void UserTypeAutoCast()
        {
            Execute_AutoCast(() =>
            {
                Variable doubleTestVariable = DefaultModule.GetVariable("doubleTest");

                Assert.IsType<DoubleTest>(doubleTestVariable);

                DoubleTest doubleTest = (DoubleTest)doubleTestVariable;

                Assert.Equal(3.5, doubleTest.d);
                Assert.Equal(2.5, doubleTest.f);
                Assert.Equal(5, doubleTest.i);
            });
        }

        private void VerifyBuiltinType(NativeCodeType codeType, params BuiltinType[] expected)
        {
            BuiltinType actual = codeType.BuiltinType;

            foreach (BuiltinType builtinType in expected)
            {
                if (actual == builtinType)
                {
                    return;
                }
            }

            Assert.Equal(expected[0], actual);
        }

        private static void VerifyMap(IReadOnlyDictionary<std.wstring, std.@string> stringMap)
        {
            string[] mapKeys = stringMap.Keys.Select(s => s.Text).ToArray();
            string[] mapValues = stringMap.Values.Select(s => s.Text).ToArray();
            Dictionary<string, string> stringMapExpected = new Dictionary<string, string>()
            {
                { "foo", "ansiFoo" },
                { "bar", "ansiBar" },
            };

            Assert.Equal(2, stringMap.Count);
            CompareArrays(new[] { "foo", "bar" }, mapKeys);
            CompareArrays(new[] { "ansiFoo", "ansiBar" }, mapValues);
            foreach (KeyValuePair<std.wstring, std.@string> kvp in stringMap)
            {
                std.@string value;

                Assert.True(stringMap.ContainsKey(kvp.Key));
                Assert.True(stringMap.TryGetValue(kvp.Key, out value));
                Assert.Equal(kvp.Value.Text, value.Text);
                value = stringMap[kvp.Key];
                Assert.Equal(kvp.Value.Text, value.Text);
                Assert.Equal(stringMapExpected[kvp.Key.Text], kvp.Value.Text);
            }

            Assert.Equal(2, stringMap.ToStringDictionary().ToStringStringDictionary().Count);
        }

        private void InterpretInteractive(string code)
        {
            code = @"
StackFrame GetFrame(string functionName)
{
    foreach (var frame in Thread.Current.StackTrace.Frames)
    {
        try
        {
            if (frame.FunctionName == functionName)
            {
                return frame;
            }
        }
        catch (Exception)
        {
            // Ignore exception for getting source file name for frames where we don't have PDBs
        }
    }

    throw new Exception($""Frame not found '{functionName}'"");
}

void AreEqual<T>(T value1, T value2)
{
    if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(value1, value2))
    {
        throw new Exception($""Not equal. value1 = {value1}, value2 = {value2}"");
    }
}

void IsTrue(bool value)
{
    if (!value)
        throw new Exception(""Expected value to be true"");
}
                " + code;

            DumpInitialization.InteractiveExecution.UnsafeInterpret(code);
        }
    }

    #region Test configurations
    [Collection("NativeDumpTest.x64.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64 : NativeDumpTest
    {
        public NativeDumpTest_x64(NativeDumpTest_x64_dmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("NativeDumpTest.x64.mdmp NoDia")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_NoDia : NativeDumpTest
    {
        public NativeDumpTest_x64_NoDia(NativeDumpTest_x64_dmp_NoDia_Initialization initialization)
            : base(initialization, executeCodeGen: false)
        {
        }
    }

    [Collection("NativeDumpTest.x64.mdmp ILCodeGen")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_IL : NativeDumpTest
    {
        public NativeDumpTest_x64_IL(NativeDumpTest_x64_dmp_IL_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("NativeDumpTest.x64.Release.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_Release : NativeDumpTest
    {
        public NativeDumpTest_x64_Release(NativeDumpTest_x64_Release_dmp_Initialization initialization)
            : base(initialization)
        {
            ReleaseDump = true;
        }
    }

    [Collection("NativeDumpTest.x86.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x86 : NativeDumpTest
    {
        public NativeDumpTest_x86(NativeDumpTest_x86_dmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("NativeDumpTest.x86.Release.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x86_Release : NativeDumpTest
    {
        public NativeDumpTest_x86_Release(NativeDumpTest_x86_Release_dmp_Initialization initialization)
            : base(initialization)
        {
            ReleaseDump = true;
        }
    }

    [Collection("NativeDumpTest.x64.VS2013.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_VS2013 : NativeDumpTest
    {
        public NativeDumpTest_x64_VS2013(NativeDumpTest_x64_VS2013_mdmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("NativeDumpTest.x64.VS2015.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_VS2015 : NativeDumpTest
    {
        public NativeDumpTest_x64_VS2015(NativeDumpTest_x64_VS2015_mdmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("NativeDumpTest.gcc.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x86_gcc : NativeDumpTest
    {
        public NativeDumpTest_x86_gcc(NativeDumpTest_gcc_dmp_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("NativeDumpTest.x64.gcc.mdmp")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_gcc : NativeDumpTest
    {
        public NativeDumpTest_x64_gcc(NativeDumpTest_x64_gcc_Initialization initialization)
            : base(initialization)
        {
        }
    }

    [Collection("NativeDumpTest.linux.x86.gcc.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x86_Linux_gcc : NativeDumpTest
    {
        public NativeDumpTest_x86_Linux_gcc(NativeDumpTest_linux_x86_gcc_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
        }
    }

    [Collection("NativeDumpTest.linux.x64.gcc.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_Linux_gcc : NativeDumpTest
    {
        public NativeDumpTest_x64_Linux_gcc(NativeDumpTest_linux_x64_gcc_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
        }
    }

    [Collection("NativeDumpTest.linux.x64.clang.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_Linux_clang : NativeDumpTest
    {
        public NativeDumpTest_x64_Linux_clang(NativeDumpTest_linux_x64_clang_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
        }
    }

    [Collection("NativeDumpTest.linux.x86.gcc.nortti.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x86_Linux_gcc_nortti : NativeDumpTest
    {
        public NativeDumpTest_x86_Linux_gcc_nortti(NativeDumpTest_linux_x86_gcc_nortti_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
            NoRtti = true;
        }
    }

    [Collection("NativeDumpTest.linux.x64.gcc.nortti.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_Linux_gcc_nortti : NativeDumpTest
    {
        public NativeDumpTest_x64_Linux_gcc_nortti(NativeDumpTest_linux_x64_gcc_nortti_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
            NoRtti = true;
        }
    }

    [Collection("NativeDumpTest.linux.x64.clang.nortti.coredump")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class NativeDumpTest_x64_Linux_clang_nortti : NativeDumpTest
    {
        public NativeDumpTest_x64_Linux_clang_nortti(NativeDumpTest_linux_x64_clang_nortti_Initialization initialization)
            : base(initialization)
        {
            LinuxDump = true;
            NoRtti = true;
        }
    }
    #endregion
}
