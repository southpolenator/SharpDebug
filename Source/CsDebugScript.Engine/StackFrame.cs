using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsDebugScript
{
    /// <summary>
    /// Stack frame of the process being debugged.
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        /// The source file name, line and displacement
        /// </summary>
        private SimpleCache<Tuple<string, uint, ulong>> sourceFileNameAndLine;

        /// <summary>
        /// The function name and displacement
        /// </summary>
        private SimpleCache<Tuple<string, ulong>> functionNameAndDisplacement;

        /// <summary>
        /// The local variables
        /// </summary>
        private SimpleCache<VariableCollection> locals;

        /// <summary>
        /// The user type converted local variables
        /// </summary>
        private SimpleCache<VariableCollection> userTypeConvertedLocals;

        /// <summary>
        /// The arguments
        /// </summary>
        private SimpleCache<VariableCollection> arguments;

        /// <summary>
        /// The user type converted arguments
        /// </summary>
        private SimpleCache<VariableCollection> userTypeConvertedArguments;

        /// <summary>
        /// The CLR stack frame
        /// </summary>
        private SimpleCache<Microsoft.Diagnostics.Runtime.ClrStackFrame> clrStackFrame;

        /// <summary>
        /// The module where instruction pointer is located.
        /// </summary>
        private SimpleCache<Module> module;

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrame" /> class.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <param name="frameContext">The frame context.</param>
        internal StackFrame(StackTrace stackTrace, ThreadContext frameContext)
        {
            StackTrace = stackTrace;
            FrameContext = frameContext;
            sourceFileNameAndLine = SimpleCache.Create(ReadSourceFileNameAndLine);
            functionNameAndDisplacement = SimpleCache.Create(ReadFunctionNameAndDisplacement);
            locals = SimpleCache.Create(GetLocals);
            arguments = SimpleCache.Create(GetArguments);
            clrStackFrame = SimpleCache.Create(() => Thread.ClrThread?.StackTrace.Where(f => f.InstructionPointer == InstructionOffset).FirstOrDefault());
            userTypeConvertedLocals = SimpleCache.Create(() =>
            {
                VariableCollection collection = Variable.CastVariableCollectionToUserType(locals.Value);

                GlobalCache.UserTypeCastedVariableCollections.Add(userTypeConvertedLocals);
                return collection;
            });
            userTypeConvertedArguments = SimpleCache.Create(() =>
            {
                VariableCollection collection = Variable.CastVariableCollectionToUserType(arguments.Value);

                GlobalCache.UserTypeCastedVariableCollections.Add(userTypeConvertedArguments);
                return collection;
            });
            module = SimpleCache.Create(() =>
            {
                var m = Process.GetModuleByInnerAddress(InstructionOffset);

                if (m == null && ClrStackFrame != null)
                    m = Process.ClrModuleCache[ClrStackFrame.Module];
                return m;
            });
        }

        /// <summary>
        /// Gets or sets the current stack frame in current thread of current process.
        /// </summary>
        public static StackFrame Current
        {
            get
            {
                return Context.Debugger.GetThreadCurrentStackFrame(Thread.Current);
            }

            set
            {
                Context.Debugger.SetCurrentStackFrame(value);
            }
        }

        /// <summary>
        /// Gets the owning stack trace.
        /// </summary>
        public StackTrace StackTrace { get; internal set; }

        /// <summary>
        /// Gets the frame context.
        /// </summary>
        public ThreadContext FrameContext { get; private set; }

        /// <summary>
        /// Gets the owning thread.
        /// </summary>
        public Thread Thread
        {
            get
            {
                return StackTrace.Thread;
            }
        }

        /// <summary>
        /// Gets the owning process.
        /// </summary>
        public Process Process
        {
            get
            {
                return Thread.Process;
            }
        }

        /// <summary>
        /// Gets the module where instruction pointer is located.
        /// </summary>
        public Module Module
        {
            get
            {
                return module.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="StackFrame"/> is virtual.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this stack frame was generated by the debugger by unwinding; otherwise, <c>false</c> if it
        ///   was formed from a thread's current context. Typically, this is <c>true</c> for the frame at the top of the
        ///   stack, where InstructionOffset is the current instruction pointer.
        /// </value>
        public bool Virtual { get; internal set; }

        /// <summary>
        /// Gets the index of the frame. This index counts the number of frames from the top of the call stack.
        /// The frame at the top of the stack, representing the current call, has index zero..
        /// </summary>
        public uint FrameNumber { get; internal set; }

        /// <summary>
        /// Gets the location in the process's virtual address space of the stack frame, if known.
        /// Some processor architectures do not have a frame or have more than one. In these cases,
        /// the engine chooses a value most representative for the given level of the stack.
        /// </summary>
        public ulong FrameOffset { get; internal set; }

        /// <summary>
        /// Gets the location in the process's virtual address space of the related instruction for the stack frame.
        /// This is typically the return address for the next stack frame, or the current instruction pointer if the
        /// frame is at the top of the stack. 
        /// </summary>
        public ulong InstructionOffset { get; internal set; }

        /// <summary>
        /// Gets the location in the process's virtual address space of the return address for the stack frame.
        /// This is typically the related instruction for the previous stack frame.
        /// </summary>
        public ulong ReturnOffset { get; internal set; }

        /// <summary>
        /// Gets the location in the process's virtual address space of the processor stack.
        /// </summary>
        public ulong StackOffset { get; internal set; }

        /// <summary>
        /// Gets the name of the source file.
        /// </summary>
        public string SourceFileName
        {
            get
            {
                return sourceFileNameAndLine.Value.Item1;
            }
        }

        /// <summary>
        /// Gets the source file line.
        /// </summary>
        public uint SourceFileLine
        {
            get
            {
                return sourceFileNameAndLine.Value.Item2;
            }
        }

        /// <summary>
        /// Gets the source file displacement.
        /// </summary>
        public ulong SourceFileDisplacement
        {
            get
            {
                return sourceFileNameAndLine.Value.Item3;
            }
        }

        /// <summary>
        /// Gets the name of the function (including module name).
        /// </summary>
        public string FunctionName
        {
            get
            {
                return functionNameAndDisplacement.Value.Item1;
            }
        }

        /// <summary>
        /// Gets the function name without module name.
        /// </summary>
        public string FunctionNameWithoutModule
        {
            get
            {
                int moduleEnd = FunctionName.IndexOf('!');

                return moduleEnd >= 0 ? FunctionName.Substring(moduleEnd + 1) : FunctionName;
            }
        }

        /// <summary>
        /// Gets the function displacement.
        /// </summary>
        public ulong FunctionDisplacement
        {
            get
            {
                return functionNameAndDisplacement.Value.Item2;
            }
        }

        /// <summary>
        /// Gets the local variables.
        /// </summary>
        public VariableCollection Locals
        {
            get
            {
                return userTypeConvertedLocals.Value;
            }
        }

        /// <summary>
        /// Gets the function arguments.
        /// </summary>
        public VariableCollection Arguments
        {
            get
            {
                return userTypeConvertedArguments.Value;
            }
        }

        /// <summary>
        /// Gets the CLR stack frame.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrStackFrame ClrStackFrame
        {
            get
            {
                return clrStackFrame.Value;
            }

            set
            {
                clrStackFrame.Value = value;
            }
        }

        /// <summary>
        /// Reads the name of the source file, line and displacement.
        /// </summary>
        /// <exception cref="System.AggregateException">Couldn't read source file name. Check if symbols are present.</exception>
        private Tuple<string, uint, ulong> ReadSourceFileNameAndLine()
        {
            if (clrStackFrame.Cached && clrStackFrame.Value != null)
            {
                return ReadClrSourceFileNameAndLine(Module, ClrStackFrame.Method, InstructionOffset);
            }

            try
            {
                string sourceFileName;
                uint sourceFileLine;
                ulong displacement;

                Context.SymbolProvider.GetStackFrameSourceFileNameAndLine(this, out sourceFileName, out sourceFileLine, out displacement);
                return Tuple.Create(sourceFileName, sourceFileLine, displacement);
            }
            catch (Exception ex)
            {
                if (ClrStackFrame != null)
                {
                    return ReadClrSourceFileNameAndLine(Module, ClrStackFrame.Method, InstructionOffset);
                }

                throw new AggregateException("Couldn't read source file name. Check if symbols are present.", ex);
            }
        }

        /// <summary>
        /// Reads the function name and displacement.
        /// </summary>
        /// <exception cref="System.AggregateException">Couldn't read source file name. Check if symbols are present.</exception>
        private Tuple<string, ulong> ReadFunctionNameAndDisplacement()
        {
            if (clrStackFrame.Cached && clrStackFrame.Value != null)
            {
                return ReadClrFunctionNameAndDisplacement(Module, ClrStackFrame.Method, InstructionOffset);
            }

            try
            {
                ulong displacement;
                string functionName;

                Context.SymbolProvider.GetStackFrameFunctionName(this, out functionName, out displacement);
                return Tuple.Create(functionName, displacement);
            }
            catch (Exception ex)
            {
                if (ClrStackFrame != null)
                {
                    return ReadClrFunctionNameAndDisplacement(Module, ClrStackFrame.Method, InstructionOffset);
                }

                throw new AggregateException("Couldn't read function name. Check if symbols are present.", ex);
            }
        }

        /// <summary>
        /// Gets the function arguments.
        /// </summary>
        private VariableCollection GetArguments()
        {
            if (clrStackFrame.Cached && ClrStackFrame != null)
            {
                if (ClrStackFrame.Arguments.Count > 0)
                {
                    return ConvertClrToVariableCollection(ClrStackFrame.Arguments, GetClrArgumentsNames());
                }
                else
                {
                    return new VariableCollection(new Variable[0]);
                }
            }

            VariableCollection arguments = null;

            try
            {
                arguments = Context.SymbolProvider.GetFrameLocals(this, true);
            }
            catch (Exception)
            {
            }

            if ((arguments == null || arguments.Count == 0) && ClrStackFrame != null && ClrStackFrame.Arguments.Count > 0)
            {
                arguments = ConvertClrToVariableCollection(ClrStackFrame.Arguments, GetClrArgumentsNames());
            }

            return arguments;
        }

        #region CLR specific methods
        /// <summary>
        /// Reads the CLR function name and displacement.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="method">The method.</param>
        /// <param name="address">The address.</param>
        internal static Tuple<string, ulong> ReadClrFunctionNameAndDisplacement(Module module, Microsoft.Diagnostics.Runtime.ClrMethod method, ulong address)
        {
            string moduleName = module?.Name ?? "???";
            string functionName = moduleName + "!" + method;
            ulong displacement = address - method.NativeCode;

            return Tuple.Create(functionName, displacement);
        }

        /// <summary>
        /// Reads the CLR source file name, line number and displacement.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="method">The method.</param>
        /// <param name="address">The address.</param>
        internal static Tuple<string, uint, ulong> ReadClrSourceFileNameAndLine(Module module, Microsoft.Diagnostics.Runtime.ClrMethod method, ulong address)
        {
            Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader pdbReader = module.ClrPdbReader;
            Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbFunction function = pdbReader.GetFunctionFromToken(method.MetadataToken);
            uint ilOffset = StackFrame.FindIlOffset(method, address);

            ulong distance = ulong.MaxValue;
            string sourceFileName = "";
            uint sourceFileLine = uint.MaxValue;

            foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePointCollection sequenceCollection in function.SequencePoints)
            {
                foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePoint point in sequenceCollection.Lines)
                {
                    if (point.Offset <= ilOffset)
                    {
                        ulong dist = ilOffset - point.Offset;

                        if (dist < distance)
                        {
                            sourceFileName = sequenceCollection.File.Name;
                            sourceFileLine = point.LineBegin;
                            distance = dist;
                        }
                    }
                }
            }

            return Tuple.Create(sourceFileName, sourceFileLine, distance);
        }

        /// <summary>
        /// Gets the local variables.
        /// </summary>
        private VariableCollection GetLocals()
        {
            if (clrStackFrame.Cached && ClrStackFrame != null)
            {
                if (ClrStackFrame.Locals.Count > 0)
                {
                    return ConvertClrToVariableCollection(ClrStackFrame.Locals, GetClrLocalsNames());
                }
                else
                {
                    return new VariableCollection(new Variable[0]);
                }
            }

            VariableCollection locals = null;

            try
            {
                locals = Context.SymbolProvider.GetFrameLocals(this, false);
            }
            catch (Exception)
            {
            }

            if ((locals == null || locals.Count == 0) && ClrStackFrame != null && ClrStackFrame.Locals.Count > 0)
            {
                locals = ConvertClrToVariableCollection(ClrStackFrame.Locals, GetClrLocalsNames());
            }

            return locals;
        }

        /// <summary>
        /// Gets the CLR local variable names.
        /// </summary>
        private string[] GetClrLocalsNames()
        {
            var pdb = Module?.ClrPdbReader;

            if (Module == null)
            {
                try
                {
                    string pdbPath = ClrStackFrame.Runtime.DataTarget.SymbolLocator.FindPdb(ClrStackFrame.Module.Pdb);

                    if (!string.IsNullOrEmpty(pdbPath))
                    {
                        pdb = new Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader(pdbPath);
                    }
                }
                catch
                {
                }
            }

            if (pdb == null)
            {
                return Enumerable.Range(0, ClrStackFrame.Locals.Count).Select(id => string.Format("local_{0}", id)).ToArray();
            }
            else
            {
                var function = pdb.GetFunctionFromToken(ClrStackFrame.Method.MetadataToken);
                uint ilOffset = FindIlOffset(ClrStackFrame);
                var scope = function.FindScopeByILOffset(ilOffset);

                return GetRecursiveSlots(scope).Select(s => s.Name).ToArray();
            }
        }

        /// <summary>
        /// Gets the CLR function argument names.
        /// </summary>
        private string[] GetClrArgumentsNames()
        {
            var frame = ClrStackFrame;
            var imd = frame.Module.MetadataImport;
            var sb = new StringBuilder(64);
            List<string> arguments = new List<string>(frame.Arguments.Count);
            IntPtr paramEnum = IntPtr.Zero;
            uint fetched = 0;
            int paramDef;

            imd.EnumParams(ref paramEnum, (int)frame.Method.MetadataToken, out paramDef, 1, out fetched);
            while (fetched == 1)
            {
                int pmd;
                uint pulSequence, pchName, pdwAttr, pdwCPlusTypeFlag, pcchValue;
                IntPtr ppValue;

                imd.GetParamProps(paramDef, out pmd, out pulSequence, sb, (uint)sb.Capacity, out pchName, out pdwAttr, out pdwCPlusTypeFlag, out ppValue, out pcchValue);
                arguments.Add(sb.ToString());
                sb.Clear();
                imd.EnumParams(ref paramEnum, (int)frame.Method.MetadataToken, out paramDef, 1, out fetched);
            }

            imd.CloseEnum(paramEnum);
            if (arguments.Count == frame.Arguments.Count - 1)
            {
                arguments.Insert(0, "this");
            }

            return arguments.ToArray();
        }

        /// <summary>
        /// Converts the CLR values to variable collection.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="names">The names.</param>
        private VariableCollection ConvertClrToVariableCollection(IList<Microsoft.Diagnostics.Runtime.ClrValue> values, string[] names)
        {
            if (values.Count != names.Length)
                throw new ArgumentOutOfRangeException(nameof(names));

            List<Variable> variables = new List<Variable>(values.Count);

            for (int i = 0; i < names.Length; i++)
                if (values[i] != null)
                    try
                    {
                        var value = values[i];
                        ulong address = value.Address;
                        CodeType codeType = Module.FromClrType(value.Type);
                        Variable variable;

                        if (codeType.IsPointer)
                            variable = Variable.CreatePointerNoCast(codeType, address, names[i]);
                        else
                        {
                            // TODO: This address unboxing should be part of ClrMD.
                            if (value.ElementType == Microsoft.Diagnostics.Runtime.ClrElementType.Class)
                                address += Process.GetPointerSize();
                            variable = Variable.CreateNoCast(codeType, address, names[i]);
                        }

                        variables.Add(Variable.UpcastClrVariable(variable));
                    }
                    catch (Exception)
                    {
                    }

            return new VariableCollection(variables.ToArray());
        }

        /// <summary>
        /// Gets the recursive slots.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="results">The results.</param>
        private static IEnumerable<Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSlot> GetRecursiveSlots(Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbScope scope, List<Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSlot> results = null)
        {
            if (results == null)
                results = new List<Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSlot>();
            foreach (var slot in scope.Slots)
            {
                while (results.Count <= slot.Slot)
                    results.Add(null);
                results[(int)slot.Slot] = slot;
            }

            foreach (var innerScope in scope.Scopes)
                GetRecursiveSlots(innerScope, results);
            return results;
        }

        /// <summary>
        /// Finds the IL offset for the specified frame.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="instructionPointer">The instruction pointer.</param>
        internal static uint FindIlOffset(Microsoft.Diagnostics.Runtime.ClrMethod method, ulong instructionPointer)
        {
            ulong ip = instructionPointer;
            uint last = uint.MaxValue;

            foreach (var item in method.ILOffsetMap)
            {
                if (item.StartAddress > ip)
                    return last;
                if (ip <= item.EndAddress)
                    return (uint)item.ILOffset;
                last = (uint)item.ILOffset;
            }

            return last;
        }

        /// <summary>
        /// Finds the IL offset for the specified frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        private static uint FindIlOffset(Microsoft.Diagnostics.Runtime.ClrStackFrame frame)
        {
            return FindIlOffset(frame.Method, frame.InstructionPointer);
        }
        #endregion
    }
}
