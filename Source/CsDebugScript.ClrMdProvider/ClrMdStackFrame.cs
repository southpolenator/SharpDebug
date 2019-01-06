using System;
using CsDebugScript.CLR;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CsDebugScript.ClrMdProvider
{
    /// <summary>
    /// ClrMD implementation of the <see cref="IClrStackFrame"/>.
    /// </summary>
    internal class ClrMdStackFrame : IClrStackFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdStackFrame"/> class.
        /// </summary>
        /// <param name="provider">The CLR provider.</param>
        /// <param name="clrStackFrame">The CLR stack frame.</param>
        public ClrMdStackFrame(CLR.ClrMdProvider provider, Microsoft.Diagnostics.Runtime.ClrStackFrame clrStackFrame)
        {
            Provider = provider;
            ClrStackFrame = clrStackFrame;
        }

        /// <summary>
        /// Gets the arguments for this stack frame.
        /// </summary>
        public VariableCollection Arguments
        {
            get
            {
                if (ClrStackFrame.Arguments.Count > 0)
                    return ConvertClrToVariableCollection(ClrStackFrame.Arguments, GetClrArgumentsNames());
                else
                    return new VariableCollection(new Variable[0]);
            }
        }

        /// <summary>
        /// Gets the locals for this stack frame.
        /// </summary>
        public VariableCollection Locals
        {
            get
            {
                if (ClrStackFrame.Locals.Count > 0)
                    return ConvertClrToVariableCollection(ClrStackFrame.Locals, GetClrLocalsNames());
                else
                    return new VariableCollection(new Variable[0]);
            }
        }

        /// <summary>
        /// Gets the module this frame is associated with.
        /// </summary>
        public IClrModule Module => Provider.FromClrModule(ClrStackFrame.Module);

        /// <summary>
        /// Gets the CLR provider.
        /// </summary>
        internal CLR.ClrMdProvider Provider { get; private set; }

        /// <summary>
        /// Gets the CLR stack frame.
        /// </summary>
        internal Microsoft.Diagnostics.Runtime.ClrStackFrame ClrStackFrame { get; private set; }

        /// <summary>
        /// Reads the function name and displacement.
        /// </summary>
        public Tuple<string, ulong> ReadFunctionNameAndDisplacement()
        {
            return ReadFunctionNameAndDisplacement(Module.Module, ClrStackFrame.Method, ClrStackFrame.InstructionPointer);
        }

        /// <summary>
        /// Reads the function name and displacement.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="method">The CLR method.</param>
        /// <param name="address">The address.</param>
        internal static Tuple<string, ulong> ReadFunctionNameAndDisplacement(Module module, Microsoft.Diagnostics.Runtime.ClrMethod method, ulong address)
        {
            string moduleName = module?.Name ?? "???";
            string functionName = moduleName + "!" + method;
            ulong displacement = address - method.NativeCode;

            return Tuple.Create(functionName, displacement);
        }

        /// <summary>
        /// Reads the name of the source file, line and displacement.
        /// </summary>
        public Tuple<string, uint, ulong> ReadSourceFileNameAndLine()
        {
            return ReadSourceFileNameAndLine((ClrMdModule)Module, ClrStackFrame.Method, ClrStackFrame.InstructionPointer);
        }

        /// <summary>
        /// Reads the name of the source file, line and displacement.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="method">The CLR method.</param>
        /// <param name="address">The address.</param>
        internal static Tuple<string, uint, ulong> ReadSourceFileNameAndLine(ClrMdModule module, Microsoft.Diagnostics.Runtime.ClrMethod method, ulong address)
        {
            Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader pdbReader = module.ClrPdbReader;
            Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbFunction function = pdbReader.GetFunctionFromToken(method.MetadataToken);
            uint ilOffset = FindIlOffset(method, address);

            ulong distance = ulong.MaxValue;
            string sourceFileName = "";
            uint sourceFileLine = uint.MaxValue;

            foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePointCollection sequenceCollection in function.SequencePoints)
                foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePoint point in sequenceCollection.Lines)
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
            return Tuple.Create(sourceFileName, sourceFileLine, distance);
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
                arguments.Insert(0, "this");
            return arguments.ToArray();
        }

        /// <summary>
        /// Gets the CLR local variable names.
        /// </summary>
        private string[] GetClrLocalsNames()
        {
            var pdb = (Module as ClrMdModule)?.ClrPdbReader;

            if (pdb == null)
            {
                try
                {
                    string pdbPath = ClrStackFrame.Runtime.DataTarget.SymbolLocator.FindPdb(ClrStackFrame.Module.Pdb);

                    if (!string.IsNullOrEmpty(pdbPath))
                        pdb = new Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader(pdbPath);
                }
                catch
                {
                }
            }

            if (pdb == null)
                return Enumerable.Range(0, ClrStackFrame.Locals.Count).Select(id => string.Format("local_{0}", id)).ToArray();
            else
            {
                var function = pdb.GetFunctionFromToken(ClrStackFrame.Method.MetadataToken);
                uint ilOffset = FindIlOffset(ClrStackFrame);
                var scope = function.FindScopeByILOffset(ilOffset);

                return GetRecursiveSlots(scope).Select(s => s.Name).ToArray();
            }
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
            Module module = Module.Module;

            for (int i = 0; i < names.Length; i++)
            {
                if (values[i] != null)
                {
                    try
                    {
                        var value = values[i];
                        ulong address = value.Address;
                        CodeType codeType = module.FromClrType(Provider.FromClrType(value.Type));
                        Variable variable;

                        if (codeType.IsPointer)
                            variable = Variable.CreatePointerNoCast(codeType, address, names[i]);
                        else
                        {
                            // TODO: This address unboxing should be part of ClrMD.
                            if (value.ElementType == Microsoft.Diagnostics.Runtime.ClrElementType.Class)
                                address += module.Process.GetPointerSize();
                            variable = Variable.CreateNoCast(codeType, address, names[i]);
                        }

                        variables.Add(Variable.UpcastClrVariable(variable));
                    }
                    catch (Exception)
                    {
                    }
                }
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
    }
}
