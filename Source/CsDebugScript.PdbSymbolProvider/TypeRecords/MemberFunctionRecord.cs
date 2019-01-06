using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents member function type record.
    /// </summary>
    public class MemberFunctionRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_MFUNCTION
        };

        /// <summary>
        /// Gets the type index of the value returned by the procedure.
        /// </summary>
        public TypeIndex ReturnType { get; private set; }

        /// <summary>
        /// Gets the type index of the containing class of the function.
        /// </summary>
        public TypeIndex ClassType { get; private set; }

        /// <summary>
        /// Type index of the <c>this</c> parameter of the member function. A type of
        /// void indicates that the member function is static and has no <c>this</c> parameter.
        /// </summary>
        public TypeIndex ThisType { get; private set; }

        /// <summary>
        /// Gets the calling convention of the procedure.
        /// </summary>
        public CallingConvention CallingConvention { get; private set; }

        /// <summary>
        /// Gets the member function options.
        /// </summary>
        public FunctionOptions Options { get; private set; }

        /// <summary>
        /// Gets the number of parameters.  This count does not include the <c>this</c> parameter.
        /// </summary>
        public ushort ParameterCount { get; private set; }

        /// <summary>
        /// Gets the list of parameter specifiers. This list does not include the <c>this</c> parameter.
        /// </summary>
        public TypeIndex ArgumentList { get; private set; }

        /// <summary>
        /// Gets the Logical <c>this</c> adjuster for the method.Whenever a class element is
        /// referenced via the <c>this</c> pointer, <see cref="ThisPointerAdjustment"/> will be added to the resultant
        /// offset before referencing the element.
        /// </summary>
        public int ThisPointerAdjustment { get; private set; }

        /// <summary>
        /// Reads <see cref="MemberFunctionRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static MemberFunctionRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new MemberFunctionRecord
            {
                Kind = kind,
                ReturnType = TypeIndex.Read(reader),
                ClassType = TypeIndex.Read(reader),
                ThisType = TypeIndex.Read(reader),
                CallingConvention = (CallingConvention)reader.ReadByte(),
                Options = (FunctionOptions)reader.ReadByte(),
                ParameterCount = reader.ReadUshort(),
                ArgumentList = TypeIndex.Read(reader),
                ThisPointerAdjustment = reader.ReadInt(),
            };
        }
    }
}
