using CsDebugScript.PdbSymbolProvider.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents UDT source line information type record.
    /// </summary>
    public class UdtModuleSourceLineRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_UDT_MOD_SRC_LINE
        };

        /// <summary>
        /// Gets user defined type index.
        /// </summary>
        public TypeIndex UDT { get; private set; }

        /// <summary>
        /// Gets source file type index.
        /// </summary>
        public TypeIndex SourceFile { get; private set; }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public uint LineNumber { get; private set; }

        /// <summary>
        /// Gets the module index.
        /// </summary>
        public ushort Module { get; private set; }

        /// <summary>
        /// Reads <see cref="UdtModuleSourceLineRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static UdtModuleSourceLineRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new UdtModuleSourceLineRecord
            {
                Kind = kind,
                UDT = TypeIndex.Read(reader),
                SourceFile = TypeIndex.Read(reader),
                LineNumber = reader.ReadUint(),
                Module = reader.ReadUshort(),
            };
        }
    }
}
