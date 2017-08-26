using CsDebugScript;
using CsDebugScript.Engine;
using std = CsDebugScript.CommonUserTypes.NativeTypes.std;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using CsDebugScript.DwarfSymbolProvider;

namespace DbgEngTest
{
    /// <summary>
    /// E2E tests for verifying various functionalities of CsScript against NativeDumpTest.exe.
    /// </summary>
    public class NativeDumpTest : TestBase
    {
        private const string MainSourceFileName = "nativedumptest.cpp";

        private readonly string DefaultDumpFile;
        private readonly string DefaultModuleName;
        private readonly string DefaultSymbolPath;

        public NativeDumpTest(string defaultDumpFile, string defaultModuleName, string defaultSymbolPath)
        {
            DefaultDumpFile = defaultDumpFile;
            DefaultModuleName = defaultModuleName;
            DefaultSymbolPath = defaultSymbolPath;
        }

        private Module DefaultModule
        {
            get
            {
                return Module.All.Single(module => module.Name == DefaultModuleName);
            }
        }

        private bool ExecuteCodeGen { get; set; }

        public void TestSetup(bool useDia = true, bool useDwarf = false, bool executeCodeGen = true, bool useElfCoreDumps = false)
        {
            InitializeDump(DefaultDumpFile, DefaultSymbolPath, useElfCoreDumps: useElfCoreDumps);
            if (!useDia)
            {
                if (!useDwarf)
                {
                    Context.InitializeDebugger(Context.Debugger, Context.Debugger.CreateDefaultSymbolProvider());
                }
                else
                {
                    Context.InitializeDebugger(Context.Debugger, new DwarfSymbolProvider());
                }
            }
            ExecuteCodeGen = executeCodeGen;
            if (ExecuteCodeGen)
            {
                InterpretInteractive($@"
var options = new ImportUserTypeOptions();
options.Modules.Add(""{DefaultModuleName}"");
ImportUserTypes(options, true);
                    ");
            }
        }

        public void CurrentThreadContainsNativeDumpTestCpp()
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
                catch (Exception)
                {
                    // Ignore exception for getting source file name for frames where we don't have PDBs
                }
            }

            Assert.Fail($"{MainSourceFileName} not found on the current thread stack trace");
        }

        public void CurrentThreadContainsNativeDumpTestMainFunction()
        {
            Assert.IsNotNull(GetFrame($"{DefaultModuleName}!DefaultTestCase"));
        }

        public void TestModuleExtraction()
        {
            Assert.IsTrue(Module.All.Any(module => module.Name == DefaultModuleName));
        }

        public void CheckProcess()
        {
            Process process = Process.Current;

            Console.WriteLine("Actual processor type: {0}", process.ActualProcessorType);
            Console.WriteLine("SystemId: {0}", process.SystemId);
            Assert.AreNotSame(0, process.SystemId);
            Assert.AreNotSame(0, Process.All.Length);
            Assert.AreNotEqual(-1, process.FindMemoryRegion(DefaultModule.Address));
            Assert.AreEqual(DefaultModule.ImageName, process.ExecutableName);
            Assert.IsNotNull(process.PEB);
            Assert.IsNull(process.CurrentCLRAppDomain);
        }

        public void CheckThread()
        {
            Thread thread = Thread.Current;

            Assert.AreNotSame(0, Thread.All.Length);
            Assert.IsNotNull(thread.Locals);
            Assert.IsNotNull(thread.TEB);
            Assert.IsNotNull(thread.ThreadContext);
        }

        public void CheckCodeFunction()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!DefaultTestCase");
            CodeFunction defaultTestCaseFunction = new CodeFunction(defaultTestCaseFrame.InstructionOffset);

            Assert.AreNotEqual(0, defaultTestCaseFunction.Address);
            Assert.AreNotEqual(0, defaultTestCaseFunction.FunctionDisplacement);
            Assert.AreEqual($"{DefaultModuleName}!DefaultTestCase", defaultTestCaseFunction.FunctionName);
            Assert.AreEqual($"DefaultTestCase", defaultTestCaseFunction.FunctionNameWithoutModule);
            Assert.AreEqual(Process.Current, defaultTestCaseFunction.Process);
            Assert.IsTrue(defaultTestCaseFunction.SourceFileName.ToLower().Contains(MainSourceFileName));
            Assert.AreNotEqual(0, defaultTestCaseFunction.SourceFileLine);
            Console.WriteLine("SourceFileDisplacement: {0}", defaultTestCaseFunction.SourceFileDisplacement);

            Variable codeFunctionVariable = DefaultModule.GetVariable($"{DefaultModuleName}!defaultTestCaseAddress");

            Assert.IsTrue(codeFunctionVariable.GetCodeType().IsPointer);

            CodeFunction codeFunction = new CodePointer<CodeFunction>(new NakedPointer(codeFunctionVariable)).Element;

            Assert.AreEqual($"{DefaultModuleName}!DefaultTestCase", codeFunction.FunctionName);
        }

        public void CheckDebugger()
        {
            string version = Debugger.ExecuteAndCapture("version");

            Console.WriteLine("Debugger version: {0}", version);
            Assert.IsTrue(Debugger.FindAllPatternInMemory(0x1212121212121212).Any());
            Assert.IsTrue(Debugger.FindAllBytePatternInMemory(new byte[] { 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12 }).Any());
            Assert.IsTrue(Debugger.FindAllTextPatternInMemory("qwerty").Any());
        }

        public void ReadingFloatPointTypes()
        {
            Variable doubleTest = DefaultModule.GetVariable("doubleTest");

            Assert.AreEqual(3.5, (double)doubleTest.GetField("d"));
            Assert.AreEqual(2.5, (float)doubleTest.GetField("f"));
            Assert.AreEqual(5, (int)doubleTest.GetField("i"));

            Variable doubleTest2 = Process.Current.GetGlobal($"{DefaultModuleName}!doubleTest");

            Assert.AreEqual(doubleTest.GetPointerAddress(), doubleTest2.GetPointerAddress());
            Assert.AreEqual(doubleTest.GetCodeType(), doubleTest2.GetCodeType());
        }

        public void GettingClassStaticMember()
        {
            Variable staticVariable = DefaultModule.GetVariable("MyTestClass::staticVariable");

            Assert.AreEqual((int)staticVariable, 1212121212);
        }

        public void CheckMainArguments()
        {
            StackFrame mainFrame = GetFrame($"{DefaultModuleName}!main");
            VariableCollection arguments = mainFrame.Arguments.Count > 0 ? mainFrame.Arguments : mainFrame.Locals;

            Assert.IsTrue(arguments.ContainsName("argc"));
            Assert.IsTrue(arguments.ContainsName("argv"));
            Assert.AreEqual(1, (int)arguments["argc"]);

            if (mainFrame.Arguments.Count > 0)
            {
                Assert.AreEqual(2, arguments.Count);
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                Variable argument = arguments[i];

                Assert.IsFalse(argument.IsNullPointer());
            }

            Variable p;
            Assert.IsFalse(arguments.TryGetValue("p", out p));
            Assert.IsNull(arguments.Names.FirstOrDefault(n => n == "p"));
            Assert.IsNull(arguments.FirstOrDefault(a => a.GetName() == "p"));
        }

        public void CheckDefaultTestCaseLocals()
        {
            StackFrame frame = GetFrame($"{DefaultModuleName}!DefaultTestCase");
            VariableCollection locals = frame.Locals;
            dynamic p = locals["p"];
            std.wstring string1 = new std.wstring(p.string1);
            Assert.AreEqual("qwerty", string1.Text);
            std.list<std.wstring> strings = new std.list<std.wstring>(p.strings);
            std.vector<std.@string> ansiStrings = new std.vector<std.@string>(p.ansiStrings);
            std.map<std.wstring, std.@string> stringMap = new std.map<std.wstring, std.@string>(p.stringMap);
            std.unordered_map<std.wstring, std.@string> stringUMap = new std.unordered_map<std.wstring, std.@string>(p.stringUMap);

            string[] stringsConverted = strings.Select(s => s.Text).ToArray();
            string[] ansiStringsConverted = ansiStrings.Select(s => s.Text).ToArray();

            CompareArrays(new[] { "Foo", "Bar" }, stringsConverted);
            CompareArrays(new[] { "AnsiFoo", "AnsiBar" }, ansiStringsConverted);

            foreach (std.wstring s in strings)
                Assert.IsTrue(s.Length <= s.Reserved);
            for (int i = 0; i < ansiStrings.Count; i++)
                Assert.IsTrue(ansiStrings[i].Length <= ansiStrings[i].Reserved);

            VerifyMap(stringMap);
            VerifyMap(stringUMap);

            // Verify enum value
            dynamic e = locals["e"];

            Assert.AreEqual("enumEntry3", e.ToString());
            Assert.AreEqual(3, (int)e);

            dynamic pEnumeration = p.enumeration;
            dynamic pInnerEnumeration = p.innerEnumeration;
            Assert.AreEqual("enumEntry2", pEnumeration.ToString());
            Assert.AreEqual(2, (int)pEnumeration);
            Assert.AreEqual("simple4", pInnerEnumeration.ToString());
            Assert.AreEqual(4, (int)pInnerEnumeration);
        }

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

            Assert.IsFalse(sptr1.IsEmpty);
            Assert.AreEqual(1, sptr1.SharedCount);
            Assert.AreEqual(2, sptr1.WeakCount);
            Assert.AreEqual(5, sptr1.Element);
            Assert.IsTrue(sptr1.IsCreatedWithMakeShared);

            Assert.IsFalse(wptr1.IsEmpty);
            Assert.AreEqual(1, wptr1.SharedCount);
            Assert.AreEqual(2, wptr1.WeakCount);
            Assert.AreEqual(5, wptr1.Element);
            Assert.IsTrue(wptr1.IsCreatedWithMakeShared);

            Assert.IsTrue(esptr1.IsEmpty);

            Assert.IsTrue(ewptr1.IsEmpty);
            Assert.AreEqual(0, ewptr1.SharedCount);
            Assert.AreEqual(1, ewptr1.WeakCount);
            Assert.AreEqual(42, ewptr1.UnsafeElement);
            Assert.IsTrue(ewptr1.IsCreatedWithMakeShared);

            Assert.IsTrue(esptr2.IsEmpty);

            Assert.IsTrue(ewptr2.IsEmpty);
            Assert.AreEqual(0, ewptr2.SharedCount);
            Assert.AreEqual(1, ewptr2.WeakCount);
            Assert.IsFalse(ewptr2.IsCreatedWithMakeShared);
        }

        public void CheckCodeArray()
        {
            StackFrame defaultTestCaseFrame = GetFrame($"{DefaultModuleName}!TestArray");
            VariableCollection locals = defaultTestCaseFrame.Locals;
            Variable testArrayVariable = locals["testArray"];
            CodeArray<int> testArray = new CodeArray<int>(testArrayVariable);

            Assert.AreEqual(10000, testArray.Length);
            foreach (int value in testArray)
                Assert.AreEqual(0x12121212, value);
        }

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
                Assert.AreEqual("42", value.ToString());
                for (int i = 0, n = values.GetArrayLength(); i < n; i++)
                {
                    Assert.AreEqual(i.ToString(), values.GetArrayElement(i).ToString());
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

            Assert.IsNotNull(nativeCodeType);
            Assert.IsTrue(nativeCodeType.Tag == CodeTypeTag.BuiltinType || nativeCodeType.Tag == CodeTypeTag.Enum);
            VerifyBuiltinType(nativeCodeType, expected);
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

            Assert.AreEqual(expected[0], actual);
        }

        private void VerifyMap(IReadOnlyDictionary<std.wstring, std.@string> stringMap)
        {
            string[] mapKeys = stringMap.Keys.Select(s => s.Text).ToArray();
            string[] mapValues = stringMap.Values.Select(s => s.Text).ToArray();
            Dictionary<string, string> stringMapExpected = new Dictionary<string, string>()
            {
                { "foo", "ansiFoo" },
                { "bar", "ansiBar" },
            };

            Assert.AreEqual(2, stringMap.Count);
            CompareArrays(new[] { "foo", "bar" }, mapKeys);
            CompareArrays(new[] { "ansiFoo", "ansiBar" }, mapValues);
            foreach (KeyValuePair<std.wstring, std.@string> kvp in stringMap)
            {
                std.@string value;

                Assert.IsTrue(stringMap.ContainsKey(kvp.Key));
                Assert.IsTrue(stringMap.TryGetValue(kvp.Key, out value));
                Assert.AreEqual(kvp.Value.Text, value.Text);
                value = stringMap[kvp.Key];
                Assert.AreEqual(kvp.Value.Text, value.Text);
                Assert.AreEqual(stringMapExpected[kvp.Key.Text], kvp.Value.Text);
            }

            Assert.AreEqual(2, stringMap.ToStringDictionary().ToStringStringDictionary().Count);
        }
    }
}
