using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// Represents function ID type record.
    /// </summary>
    public class FunctionIdRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_FUNC_ID
        };

        /// <summary>
        /// Gets the function parent scoep.
        /// </summary>
        public TypeIndex ParentScope { get; private set; }

        /// <summary>
        /// Gets the function type.
        /// </summary>
        public TypeIndex FunctionType { get; private set; }

        /// <summary>
        /// Gets the function name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="FunctionIdRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static FunctionIdRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new FunctionIdRecord
            {
                Kind = kind,
                ParentScope = TypeIndex.Read(reader),
                FunctionType = TypeIndex.Read(reader),
                Name = reader.ReadCString(),
            };
        }
    }
}
