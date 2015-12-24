using CsScriptManaged;
using CsScripts;
using DbgEngManaged;
using System;
using System.Runtime.InteropServices;

namespace CsScriptManagedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = OpenDumpFile(@"C:\Users\vujova\Downloads\defect4327276\SQLDUMP0001.MDMP", @"srv*;C:\Users\vujova\Downloads\defect4327276");

            Context.Initalize(client);
            //Context.Execute(@"..\..\..\..\samples\script.cs", new string[] { });

            Console.WriteLine("Threads: {0}", Thread.All.Length);
            Console.WriteLine("Current thread: {0}", Thread.Current.Id);
            var frames = Thread.Current.StackTrace.Frames;
            //Console.WriteLine("Call stack:");
            //foreach (var frame in frames)
            //    Console.WriteLine("  {0,3:x} {1}+0x{2:x}", frame.FrameNumber, frame.FunctionName, frame.FunctionDisplacement);
            var test = Process.Current.GetGlobal("sqldk!SOS_Task::NoPoolTracking");
            Console.WriteLine(test);
            var f = frames[34];
            var locals = f.Locals;
            dynamic pTask = locals["pTask"];
            var aa = pTask.GetBaseClass("SOSListElem").GetBaseClass("SEListElem");
            Console.WriteLine(string.Join(", ", aa.GetClassFieldNames()));
            var bb = pTask.GetClassField("m_Params").GetClassField("m_flags");
            Console.WriteLine(bb);
            var fieldNames = pTask.GetCodeType().FieldNames;
            var m_prev = pTask.m_prev;
            Console.WriteLine(m_prev);
            Variable list = pTask.m_pList;
            var ct = list.GetCodeType();
            var fieldOffsets = ct.GetFieldOffsets();
            var fields = ct.FieldNames;
        }

        public static IDebugClient OpenDumpFile(string dumpFile, string symbolPath)
        {
            IDebugClient client;
            int hresult = DebugCreate(Marshal.GenerateGuidForType(typeof(IDebugClient)), out client);

            if (hresult > 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }

            client.OpenDumpFile(dumpFile);
            ((IDebugControl7)client).WaitForEvent(0, uint.MaxValue);
            ((IDebugSymbols5)client).SetSymbolPathWide(symbolPath);
            return client;
        }

        [DllImport("dbgeng.dll", EntryPoint = "DebugCreate", SetLastError = false)]
        public static extern int DebugCreate(Guid iid, out IDebugClient client);
    }
}
