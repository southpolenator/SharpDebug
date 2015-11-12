using System;
using System.Linq;
using CsScripts;
import helper.cs;

Console.Error.WriteLine("This is sample error");
HelpMe("It works!");

writeln("Current thread {0}:{1}", Thread.Current.Id, Thread.Current.SystemId);
writeln("Current call stack {0}", Thread.Current.StackTrace);
writeln("Locals: {0}", string.Join(", ", Thread.Current.StackTrace.CurrentFrame.Locals.Select(v => v.GetName())));
writeln("Current source file: {0}:{1}", Thread.Current.StackTrace.CurrentFrame.SourceFileName, Thread.Current.StackTrace.CurrentFrame.SourceFileLine);
