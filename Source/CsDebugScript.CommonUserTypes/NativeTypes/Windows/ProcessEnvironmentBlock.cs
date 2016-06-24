using System;
using System.Collections.Generic;

namespace CsDebugScript.CommonUserTypes.NativeTypes.Windows
{
    /// <summary>
    /// Class that represents PEB (Process environment block).
    /// </summary>
    [UserType(ModuleName = "ntdll", TypeName = "_PEB")]
    [UserType(ModuleName = "nt", TypeName = "_PEB")]
    [UserType(ModuleName = "wow64", TypeName = "_PEB")]
    public class ProcessEnvironmentBlock : DynamicSelfUserType
    {
        /// <summary>
        /// Class that represents ProcessParameters field inside PEB.
        /// </summary>
        public class ProcessParametersStructure : DynamicSelfVariable
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProcessParametersStructure" /> class.
            /// </summary>
            /// <param name="variable">The variable.</param>
            public ProcessParametersStructure(Variable variable)
                : base(variable)
            {
            }

            /// <summary>
            /// Gets the command-line string passed to the process.
            /// </summary>
            public string CommandLine
            {
                get
                {
                    return TryExecute(() => self.CommandLine.Buffer.ToString());
                }
            }

            /// <summary>
            /// Gets the path of the image file for the process.
            /// </summary>
            public string ImagePathName
            {
                get
                {
                    return TryExecute(() => self.ImagePathName.Buffer.ToString());
                }
            }

            /// <summary>
            /// Gets the environments variables as list of strings (var=value).
            /// </summary>
            public string[] EnvironmentVariables
            {
                get
                {
                    return TryExecute(() =>
                    {
                        ulong size = (ulong)self.EnvironmentSize;
                        CodePointer<char> s = new CodePointer<char>(self.Environment.GetPointerAddress());

                        return s.ReadUnicodeStringByteLength(size).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    });
                }
            }
        }

        /// <summary>
        /// The heap code type
        /// </summary>
        private CodeType heapCodeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEnvironmentBlock"/> class.
        /// </summary>
        /// <param name="variable">The variable that represents PEB.</param>
        public ProcessEnvironmentBlock(Variable variable)
            : base(variable)
        {
            try
            {
                heapCodeType = Heap.GetCodeType(variable.GetCodeType().Module.Process);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEnvironmentBlock"/> class.
        /// </summary>
        /// <param name="process">The process to get PEB for.</param>
        public ProcessEnvironmentBlock(Process process)
            : this(process.PEB)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessEnvironmentBlock"/> class.
        /// </summary>
        public ProcessEnvironmentBlock()
            : this(Process.Current)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the specified process is currently being debugged.
        /// </summary>
        /// <value>
        ///   <c>true</c> if process is currently being debugged; otherwise, <c>false</c>.
        /// </value>
        public bool BeingDebugged
        {
            get
            {
                return TryExecute(() => (bool)self.BeingDebugged);
            }
        }

        /// <summary>
        /// Gets the process parameters.
        /// </summary>
        public ProcessParametersStructure ProcessParameters
        {
            get
            {
                return TryExecute(() => new ProcessParametersStructure(self.ProcessParameters));
            }
        }

        /// <summary>
        /// Gets the process heap.
        /// </summary>
        public Heap ProcessHeap
        {
            get
            {
                return TryExecute(() => CastHeap(self.ProcessHeap));
            }
        }

        /// <summary>
        /// Gets the process heaps.
        /// </summary>
        public Heap[] ProcessHeaps
        {
            get
            {
                return TryExecute(() =>
                {
                    Variable heaps = self.ProcessHeaps;
                    List<Heap> result = new List<Heap>();

                    for (int i = 0; !heaps.GetArrayElement(i).IsNullPointer(); i++)
                    {
                        result.Add(CastHeap(heaps.GetArrayElement(i)));
                    }

                    return result.ToArray();
                });
            }
        }

        /// <summary>
        /// Casts the variable into the heap user type.
        /// </summary>
        /// <param name="variable">The variable.</param>
        private Heap CastHeap(Variable variable)
        {
            if (heapCodeType != null)
                variable = variable.CastAs(heapCodeType);
            return new Heap(variable);
        }

        /// <summary>
        /// Tries to execute the specified executor. Throws exception if symbols are not available...
        /// </summary>
        /// <typeparam name="T">Return value of the executor.</typeparam>
        /// <param name="executor">The executor.</param>
        /// <returns>Return value of the executor.</returns>
        /// <exception cref="System.Exception">It looks like you don't have symbols for Windows modules.</exception>
        private static T TryExecute<T>(Func<T> executor)
        {
            try
            {
                return executor();
            }
            catch
            {
                throw new InvalidSymbolsException("It looks like you don't have symbols for Windows modules.");
            }
        }
    }
}
