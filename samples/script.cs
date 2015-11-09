using System;
using CsScripts;
import helper.cs;

Console.Error.WriteLine("This is sample error");
HelpMe("It works!");

writeln("Current thread {0}:{1}", Thread.Current.Id, Thread.Current.SystemId);
