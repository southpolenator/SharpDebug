using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents procedure type record.
    /// </summary>
    public class ProcedureRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_PROCEDURE
        };

        /// <summary>
        /// Gets the type index of the value returned by the procedure.
        /// </summary>
        public TypeIndex ReturnType { get; private set; }

        /// <summary>
        /// Gets the calling convention of the procedure.
        /// </summary>
        public CallingConvention CallingConvention { get; private set; }

        /// <summary>
        /// Gets the procedure options.
        /// </summary>
        public FunctionOptions Options { get; private set; }

        /// <summary>
        /// Gets the number of parameters.
        /// </summary>
        public ushort ParameterCount { get; private set; }

        /// <summary>
        /// Gets the type index of argument list type record.
        /// </summary>
        public TypeIndex ArgumentList { get; private set; }

        /// <summary>
        /// Reads <see cref="ProcedureRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static ProcedureRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new ProcedureRecord
            {
                Kind = kind,
                ReturnType = TypeIndex.Read(reader),
                CallingConvention = (CallingConvention)reader.ReadByte(),
                Options = (FunctionOptions)reader.ReadByte(),
                ParameterCount = reader.ReadUshort(),
                ArgumentList = TypeIndex.Read(reader),
            };
        }
    }
}
