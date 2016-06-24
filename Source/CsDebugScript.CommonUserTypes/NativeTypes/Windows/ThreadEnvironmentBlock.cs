using System;

namespace CsDebugScript.CommonUserTypes.NativeTypes.Windows
{
    /// <summary>
    /// Class that represents TEB (Thread environment block).
    /// </summary>
    [UserType(ModuleName = "ntdll", TypeName = "_PEB")]
    [UserType(ModuleName = "nt", TypeName = "_PEB")]
    [UserType(ModuleName = "wow64", TypeName = "_PEB")]
    public class ThreadEnvironmentBlock : DynamicSelfUserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadEnvironmentBlock"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public ThreadEnvironmentBlock(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the PEB.
        /// </summary>
        public ProcessEnvironmentBlock PEB
        {
            get
            {
                return TryExecute(() => new ProcessEnvironmentBlock(self.ProcessEnvironmentBlock));
            }
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
