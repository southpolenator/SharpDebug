using CsDebugScript.CLR;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Generic;

namespace CsDebugScript.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of the <see cref="IClrStackFrame"/>.
    /// </summary>
    internal class VSClrStackFrame : IClrStackFrame
    {
        /// <summary>
        /// The cache of arguments.
        /// </summary>
        private SimpleCache<VariableCollection> argumentsCache;

        /// <summary>
        /// The cache of locals.
        /// </summary>
        private SimpleCache<VariableCollection> localsCache;

        /// <summary>
        /// The cache of module.
        /// </summary>
        private SimpleCache<VSClrModule> moduleCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VSClrStackFrame"/> class.
        /// </summary>
        /// <param name="thread">The thread containing this stack frame.</param>
        /// <param name="id">The stack frame id.</param>
        /// <param name="instructionPointer">The instruction pointer.</param>
        /// <param name="stackPointer">The stack pointer.</param>
        /// <param name="moduleAddress">The module base address.</param>
        public VSClrStackFrame(VSClrThread thread, int id, ulong instructionPointer, ulong stackPointer, ulong moduleAddress)
        {
            Thread = thread;
            InstructionPointer = instructionPointer;
            StackPointer = stackPointer;
            argumentsCache = SimpleCache.Create(() => CreateVariableCollection(Proxy.GetClrStackFrameArguments(Thread.VSRuntime.Process.Id, Thread.VSRuntime.Id, Thread.SystemId, Id)));
            localsCache = SimpleCache.Create(() => CreateVariableCollection(Proxy.GetClrStackFrameLocals(Thread.VSRuntime.Process.Id, Thread.VSRuntime.Id, Thread.SystemId, Id)));
            moduleCache = SimpleCache.Create(() => Thread.VSRuntime.GetModule(moduleAddress));
        }

        /// <summary>
        /// Gets the Visual Studio debugger proxy.
        /// </summary>
        public VSDebuggerProxy Proxy => Thread.Proxy;

        /// <summary>
        /// Gets the owning thread.
        /// </summary>
        public VSClrThread Thread { get; private set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        internal int Id { get; private set; }

        /// <summary>
        /// Gets the instruction pointer.
        /// </summary>
        internal ulong InstructionPointer { get; private set; }

        /// <summary>
        /// Gets the stack pointer.
        /// </summary>
        internal ulong StackPointer { get; private set; }

        /// <summary>
        /// Gets the module this frame is associated with.
        /// </summary>
        public IClrModule Module => moduleCache.Value;

        /// <summary>
        /// Gets the arguments for this stack frame.
        /// </summary>
        public VariableCollection Arguments => argumentsCache.Value;

        /// <summary>
        /// Gets the locals for this stack frame.
        /// </summary>
        public VariableCollection Locals => localsCache.Value;

        /// <summary>
        /// Reads the function name and displacement.
        /// </summary>
        public Tuple<string, ulong> ReadFunctionNameAndDisplacement()
        {
            return Thread.Runtime.ReadFunctionNameAndDisplacement(InstructionPointer);
        }

        /// <summary>
        /// Reads the name of the source file, line and displacement.
        /// </summary>
        public Tuple<string, uint, ulong> ReadSourceFileNameAndLine()
        {
            return Thread.Runtime.ReadSourceFileNameAndLine(InstructionPointer);
        }

        /// <summary>
        /// Creates the variable collection based on the proxy returned variable tuples.
        /// </summary>
        /// <param name="variableTuples">The array of proxy returned variable tuples.</param>
        /// <returns>The variable collection.</returns>
        private VariableCollection CreateVariableCollection(Tuple<ulong, int, string>[] variableTuples)
        {
            if (variableTuples.Length > 0)
            {
                List<Variable> variables = new List<Variable>();
                Module module = Module.Module;

                foreach (var variableTuple in variableTuples)
                {
                    try
                    {
                        IClrType clrType = Thread.VSRuntime.GetClrType(variableTuple.Item2);
                        if (clrType == null)
                            continue;
                        CodeType codeType = module.FromClrType(clrType);
                        ulong address = variableTuple.Item1;
                        string name = variableTuple.Item3;
                        Variable variable;

                        if (codeType.IsPointer)
                            variable = Variable.CreatePointerNoCast(codeType, address, name);
                        else
                            variable = Variable.CreateNoCast(codeType, address, name);

                        // TODO: Can we get already upcast address and clr type from the remote connection?
                        variables.Add(Variable.UpcastClrVariable(variable));
                    }
                    catch
                    {
                    }
                }
                return new VariableCollection(variables.ToArray());
            }

            return new VariableCollection(new Variable[0]);
        }
    }
}
