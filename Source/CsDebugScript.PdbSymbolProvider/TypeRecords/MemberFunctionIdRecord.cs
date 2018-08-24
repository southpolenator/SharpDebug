using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents member function ID type record.
    /// </summary>
    public class MemberFunctionIdRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_MFUNC_ID
        };

        /// <summary>
        /// Gets the type index of the containing class of the function.
        /// </summary>
        public TypeIndex ClassType { get; private set; }

        /// <summary>
        /// Gets the type index of the function type.
        /// </summary>
        public TypeIndex FunctionType { get; private set; }

        /// <summary>
        /// Gets the function name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="MemberFunctionIdRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static MemberFunctionIdRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new MemberFunctionIdRecord
            {
                Kind = kind,
                ClassType = TypeIndex.Read(reader),
                FunctionType = TypeIndex.Read(reader),
                Name = reader.ReadCString(),
            };
        }
    }
}
