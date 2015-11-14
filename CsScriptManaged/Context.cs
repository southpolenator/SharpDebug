using System;
using System.Text;
using DbgEngManaged;
using System.IO;

namespace CsScriptManaged
{
    public class Context
    {
        /// <summary>
        /// Helper class for redirecting console out/error to debuggers output
        /// </summary>
        private class DebuggerTextWriter : TextWriter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DebuggerTextWriter"/> class.
            /// </summary>
            /// <param name="outputType">Type of the output.</param>
            public DebuggerTextWriter(DebugOutput outputType)
            {
                OutputType = outputType;
            }

            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            public DebugOutput OutputType { get; set; }

            /// <summary>
            /// When overridden in a derived class, returns the character encoding in which the output is written.
            /// </summary>
            public override Encoding Encoding
            {
                get
                {
                    return Encoding.Default;
                }
            }

            /// <summary>
            /// Writes a character to the text string or stream.
            /// </summary>
            /// <param name="value">The character to write to the text stream.</param>
            public override void Write(char value)
            {
                Context.Control.Output((uint)OutputType, value == '%' ? "%%" : value.ToString());
            }
        }

        /// <summary>
        /// The advanced interface
        /// </summary>
        public static IDebugAdvanced3 Advanced;

        /// <summary>
        /// The client interface
        /// </summary>
        public static IDebugClient7 Client;

        /// <summary>
        /// The control interface
        /// </summary>
        public static IDebugControl7 Control;

        /// <summary>
        /// The data spaces interface
        /// </summary>
        public static IDebugDataSpaces4 DataSpaces;

        /// <summary>
        /// The registers interface
        /// </summary>
        public static IDebugRegisters2 Registers;

        /// <summary>
        /// The symbols interface
        /// </summary>
        public static IDebugSymbols5 Symbols;

        /// <summary>
        /// The system objects interface
        /// </summary>
        public static IDebugSystemObjects4 SystemObjects;

        /// <summary>
        /// The script manager
        /// </summary>
        private static ScriptManager ScriptManager = new ScriptManager();

        /// <summary>
        /// Gets a value indicating whether this instance is live debugging.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is live debugging; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Initalizes the Context with the specified client.
        /// </summary>
        /// <param name="client">The client.</param>
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

        /// <summary>
        /// Executes the specified script.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="args">The arguments.</param>
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
