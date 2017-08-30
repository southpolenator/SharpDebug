using CsDebugScript.CLR;
using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using CsDebugScript.Exceptions;
using System;

namespace CsDebugScript
{
    /// <summary>
    /// Wrapper class that represents a function pointer.
    /// </summary>
    public class CodeFunction
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
        /// The function address
        /// </summary>
        private SimpleCache<ulong> functionAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFunction" /> class.
        /// </summary>
        /// <param name="address">The function address.</param>
        /// <param name="process">The process.</param>
        public CodeFunction(ulong address, Process process = null)
        {
            OriginalAddress = address;
            Process = process ?? Process.Current;
            functionAddress = SimpleCache.Create(() => Debugger.ResolveFunctionAddress(Process, OriginalAddress));
            sourceFileNameAndLine = SimpleCache.Create(ReadSourceFileNameAndLine);
            functionNameAndDisplacement = SimpleCache.Create(ReadFunctionNameAndDisplacement);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFunction"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public CodeFunction(Variable variable)
            : this(variable.GetPointerAddress(), variable.GetCodeType().Module.Process)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "function");
            }
        }

        /// <summary>
        /// Gets the original address which was supplied to the constructor.
        /// </summary>
        public ulong OriginalAddress { get; private set; }

        /// <summary>
        /// The process where this function is located.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// The resolved function pointer address.
        /// It is the same as OriginalAddress if it is not public symbol.
        /// </summary>
        public ulong Address
        {
            get
            {
                return functionAddress.Value;
            }
        }

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
        /// Gets the name of the function.
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            try
            {
                return string.Format("{0}+0x{1:x}   ({2}:{3})", FunctionName, FunctionDisplacement, SourceFileName, SourceFileLine);
            }
            catch
            {
                try
                {
                    return string.Format("{0}+0x{1:x}", FunctionName, FunctionDisplacement);
                }
                catch
                {
                    return string.Format("0x{0:x}", Address);
                }
            }
        }

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private static bool VerifyCodeType(CodeType codeType)
        {
            return codeType.IsFunction || (codeType.IsPointer && codeType.ElementType.IsFunction);
        }

        /// <summary>
        /// Reads the name of the source file, line and displacement.
        /// </summary>
        /// <exception cref="System.AggregateException">Couldn't read source file name. Check if symbols are present.</exception>
        private Tuple<string, uint, ulong> ReadSourceFileNameAndLine()
        {
            try
            {
                string sourceFileName;
                uint sourceFileLine;
                ulong displacement;

                Context.SymbolProvider.GetProcessAddressSourceFileNameAndLine(Process, Address, out sourceFileName, out sourceFileLine, out displacement);
                return Tuple.Create(sourceFileName, sourceFileLine, displacement);
            }
            catch (Exception ex)
            {
                // Try to find function among CLR ones
                foreach (IClrRuntime runtime in Process.ClrRuntimes)
                {
                    try
                    {
                        return runtime.ReadSourceFileNameAndLine(Address);
                    }
                    catch
                    {
                    }
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
            try
            {
                ulong displacement;
                string functionName;

                Context.SymbolProvider.GetProcessAddressFunctionName(Process, Address, out functionName, out displacement);
                return Tuple.Create(functionName, displacement);
            }
            catch (Exception ex)
            {
                // Try to find function among CLR ones
                foreach (IClrRuntime runtime in Process.ClrRuntimes)
                {
                    try
                    {
                        return runtime.ReadFunctionNameAndDisplacement(Address);
                    }
                    catch
                    {
                    }
                }

                throw new AggregateException("Couldn't read function name. Check if symbols are present.", ex);
            }
        }
    }
}
