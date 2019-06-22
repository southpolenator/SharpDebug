using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.CommonUserTypes.NativeTypes.Windows
{
    /// <summary>
    /// Class that represents PEB (Process environment block).
    /// </summary>
    [UserType(ModuleName = "ntdll", TypeName = "_HEAP")]
    [UserType(ModuleName = "nt", TypeName = "_HEAP")]
    [UserType(ModuleName = "wow64", TypeName = "_HEAP")]
    public class Heap : DynamicSelfUserType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Heap"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public Heap(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Gets the code type for Heap for the specified process.
        /// </summary>
        /// <param name="process">The process.</param>
        public static CodeType GetCodeType(Process process)
        {
            return CodeType.Create(process, "ntdll!_HEAP", "nt!_HEAP", "wow64!_HEAP");
        }
    }
}
