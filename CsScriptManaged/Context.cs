using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbgEngManaged;

namespace CsScriptManaged
{
    public class Context
    {
        public static IDebugAdvanced3 Advanced;
        public static IDebugBreakpoint3 Breakpoint;
        public static IDebugClient7 Client;
        public static IDebugControl7 Control;
        public static IDebugDataSpaces4 DataSpaces;
        public static IDebugRegisters2 Registers;
        public static IDebugSymbolGroup2 SymbolGroup;
        public static IDebugSymbols5 Symbols;
        public static IDebugSystemObjects4 SystemObjects;
        private static ScriptManager ScriptManager = new ScriptManager();

        public static void Initalize(IDebugClient client)
        {
            Advanced = client as IDebugAdvanced3;
            Breakpoint = client as IDebugBreakpoint3;
            Client = client as IDebugClient7;
            Control = client as IDebugControl7;
            DataSpaces = client as IDebugDataSpaces4;
            Registers = client as IDebugRegisters2;
            SymbolGroup = client as IDebugSymbolGroup2;
            Symbols = client as IDebugSymbols5;
            SystemObjects = client as IDebugSystemObjects4;
        }

        public static void Execute(string path, params string[] args)
        {
            // TODO: Execute script with arguments
            ScriptManager.Execute(path, args);
        }
    }
}
