﻿using CsDebugScript.CLR;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using System;

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
        private SimpleCache<IClrStackFrame> clrStackFrame;

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
            clrStackFrame = SimpleCache.Create(() => Thread.ClrThread?.GetClrStackFrame(InstructionOffset));
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
                {
                    m = Process.ClrModuleCache[ClrStackFrame.Module];
                }
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
        internal IClrStackFrame ClrStackFrame
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            try
            {
                return $"{FrameNumber}  {FunctionName}+0x{FunctionDisplacement:x}   ({SourceFileName}:{SourceFileLine})";
            }
            catch
            {
                try
                {
                    return $"{FrameNumber}  {FunctionName}+0x{FunctionDisplacement:x}";
                }
                catch
                {
                    return $"{FrameNumber}  0x{InstructionOffset:x}";
                }
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
                return ClrStackFrame.ReadSourceFileNameAndLine(Module, InstructionOffset);
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
                    return ClrStackFrame.ReadSourceFileNameAndLine(Module, InstructionOffset);
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
            if (clrStackFrame.Cached && ClrStackFrame != null)
            {
                return ClrStackFrame.ReadFunctionNameAndDisplacement(Module, InstructionOffset);
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
                    return ClrStackFrame.ReadFunctionNameAndDisplacement(Module, InstructionOffset);
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
                return ClrStackFrame.Arguments;
            }

            VariableCollection arguments = null;

            try
            {
                arguments = Context.SymbolProvider.GetFrameLocals(this, true);
            }
            catch (Exception)
            {
            }

            if ((arguments == null || arguments.Count == 0) && ClrStackFrame != null)
            {
                arguments = ClrStackFrame.Arguments;
            }

            return arguments;
        }

        /// <summary>
        /// Gets the local variables.
        /// </summary>
        private VariableCollection GetLocals()
        {
            if (clrStackFrame.Cached && ClrStackFrame != null)
            {
                return ClrStackFrame.Locals;
            }

            VariableCollection locals = null;

            try
            {
                locals = Context.SymbolProvider.GetFrameLocals(this, false);
            }
            catch (Exception)
            {
            }

            if ((locals == null || locals.Count == 0) && ClrStackFrame != null)
            {
                locals = ClrStackFrame.Locals;
            }

            return locals;
        }
    }
}
