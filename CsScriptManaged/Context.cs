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
            Advanced = (IDebugAdvanced3)client;
            Breakpoint = (IDebugBreakpoint3)client;
            Client = (IDebugClient7)client;
            Control = (IDebugControl7)client;
            DataSpaces = (IDebugDataSpaces4)client;
            Registers = (IDebugRegisters2)client;
            SymbolGroup = (IDebugSymbolGroup2)client;
            Symbols = (IDebugSymbols5)client;
            SystemObjects = (IDebugSystemObjects4)client;
        }

        public static void Execute(string path, string[] args)
        {
            // TODO: Execute script with arguments
            ScriptManager.Execute(path, args);
        }
    }
}
