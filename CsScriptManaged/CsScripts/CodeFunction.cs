using CsScriptManaged;
using CsScriptManaged.Native;
using CsScriptManaged.Utility;
using System;

namespace CsScripts
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
        /// Initializes a new instance of the <see cref="CodeFunction" /> class.
        /// </summary>
        /// <param name="address">The function address.</param>
        /// <param name="process">The process.</param>
        public CodeFunction(ulong address, Process process = null)
        {
            Address = address;
            Process = process ?? Process.Current;
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
                throw new Exception("Wrong code type of passed variable " + variable.GetCodeType().Name);
            }
        }

        /// <summary>
        /// The function pointer address.
        /// </summary>
        public ulong Address { get; private set; }

        /// <summary>
        /// The process where this function is located.
        /// </summary>
        public Process Process { get; private set; }

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
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private static bool VerifyCodeType(CodeType codeType)
        {
            return codeType.IsFunction;
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
                throw new AggregateException("Couldn't read source file name. Check if symbols are present.", ex);
            }
        }

        /// <summary>
        /// Reads the function name and displacement.
        /// </summary>
        /// <returns></returns>
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
                throw new AggregateException("Couldn't read function name. Check if symbols are present.", ex);
            }
        }
    }
}
