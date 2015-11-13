using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbgEngManaged;
using System.IO;

namespace CsScriptManaged
{
    class DebuggerTextWriter : TextWriter
    {
        public DebuggerTextWriter(DebugOutput outputType)
        {
            OutputType = outputType;
        }

        public DebugOutput OutputType { get; set; }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.Default;
            }
        }

        public override void Write(char value)
        {
            Context.Control.Output((uint)OutputType, value == '%' ? "%%" : value.ToString());
        }
    }

    public class Context
    {
        public static IDebugAdvanced3 Advanced;
        public static IDebugClient7 Client;
        public static IDebugControl7 Control;
        public static IDebugDataSpaces4 DataSpaces;
        public static IDebugRegisters2 Registers;
        public static IDebugSymbols5 Symbols;
        public static IDebugSystemObjects4 SystemObjects;
        private static ScriptManager ScriptManager = new ScriptManager();

        public bool IsLiveDebugging
        {
            get
            {
                try
                {
                    return Client.GetNumberDumpFiles() == 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static void Initalize(IDebugClient client)
        {
            Advanced = client as IDebugAdvanced3;
            Client = client as IDebugClient7;
            Control = client as IDebugControl7;
            DataSpaces = client as IDebugDataSpaces4;
            Registers = client as IDebugRegisters2;
            Symbols = client as IDebugSymbols5;
            SystemObjects = client as IDebugSystemObjects4;
        }

        public static void Execute(string path, params string[] args)
        {
            TextWriter originalConsoleOut = Console.Out;
            TextWriter originalConsoleError = Console.Error;

            Console.SetOut(new DebuggerTextWriter(DebugOutput.Normal));
            Console.SetError(new DebuggerTextWriter(DebugOutput.Error));
            try
            {
                ScriptManager.Execute(path, args);
            }
            finally
            {
                Console.SetOut(originalConsoleOut);
                Console.SetError(originalConsoleError);
            }
        }
    }
}
