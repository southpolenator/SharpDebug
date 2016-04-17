using System;
using System.Linq;
using CsScripts;
using std = CsScripts.CommonUserTypes.NativeTypes.std;
import CsScripts.CommonUserTypes.dll;
import helper.cs;

Console.Error.WriteLine("This is sample error");
HelpMe("It works!");

var frame = Thread.Current.StackTrace.Frames[1];
writeln("0x{0:X}", frame.StackOffset);
writeln("0x{0:X}", frame.FrameOffset);
writeln("{0}:{1} {2}+[{3:X}]", frame.SourceFileName, frame.SourceFileLine, frame.FunctionName, frame.FunctionDisplacement);
var locals = frame.Locals;
var p = locals["p"];
var q = locals["q"];
writeln("&p = 0x{0:X}", p.GetPointerAddress());
writeln("&q = 0x{0:X}", q.GetPointerAddress());
var codeType = p.DereferencePointer().GetCodeType();
var a = p.DereferencePointer();
var b = a.CastAs("int");

writeln(q.GetCodeType().Name);
writeln(q.GetCodeType().ElementType.Name);
writeln(q.GetCodeType().ElementType.ElementType.Name);
writeln(string.Join(", ", codeType.FieldNames));
writeln(codeType.Name);
writeln(codeType.Size);
writeln(codeType.GetFieldOffset("ansiStrings"));
writeln(string.Join(", ", codeType.GetFieldOffsets().Select(t => string.Format("{0} [{1}]", t.Key, t.Value))));
writeln(p.GetField("string1"));

var s1 = new std.wstring(p.GetField("string1"));
writeln("std::wstring: {0}", s1);
var ansiStrings = new std.vector<std.@string>(p.GetField("ansiStrings"));
writeln("std::vector.Length = {0} / {1}", ansiStrings.Length, ansiStrings.Reserved);
for (int i = 0; i < ansiStrings.Length; i++)
    writeln("ansiStrings[{0}] = {1}", i, ansiStrings[i]);
var strings = new std.list<Variable>(p.GetField("strings"));
writeln("std::list.Length = {0}", strings.Length);
foreach (var ss in strings)
    writeln("strings[??] = {0}", new std.wstring(ss));

var strings2 = new std.list<std.wstring>(p.GetField("strings"));
writeln("std::list.Length = {0}", strings2.Length);
foreach (var ss in strings2)
    writeln("strings[??] = {0}", ss);

var qqq = p.GetField("stringArray");
var qs = new std.wstring(qqq.GetArrayElement(1));

writeln(frame.Locals);
writeln("Proccesses: {0}", Process.All.Length);

//writeln("All Modules: {0}", string.Join(", ", Module.All.Select(m => m.Name)));

//writeln("Current process exe: {0}", Process.Current.ExecutableName);
writeln("Current thread {0}:{1}", Thread.Current.Id, Thread.Current.SystemId);
//writeln("Current call stack {0}", Thread.Current.StackTrace);
//writeln("Current source file: {0}:{1}", Thread.Current.StackTrace.CurrentFrame.SourceFileName, Thread.Current.StackTrace.CurrentFrame.SourceFileLine);
//writeln("Current function: {0}", Thread.Current.StackTrace.CurrentFrame.FunctionName);
//writeln("Callstack:");
//writeln(string.Join("\n", Thread.Current.StackTrace.Frames.Select(f => f.FunctionName)));
//writeln("Locals: {0}", string.Join(", ", Thread.Current.StackTrace.CurrentFrame.Locals.Select(v => string.Format("{0} ({1})", v.GetName(), v.GetCodeType()))));
//writeln("Arguments: {0}", string.Join(", ", Thread.Current.StackTrace.CurrentFrame.Arguments.Select(v => string.Format("{0} ({1})", v.GetName(), v.GetCodeType()))));

//dynamic l = Thread.Current.Locals[0];

//writeln(l.GetName());
//foreach (var field in l.GetFields())
//    writeln("  {0} ({1}) = {0}", field.GetName(), field.GetCodeType(), field);
