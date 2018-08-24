using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record specifies the overloaded member functions of a class. This type record can also be
    /// used to specify a non-overloaded method, but is inefficient. The <see cref="OneMethodRecord"/> should be used for non-overloaded methods.
    /// </summary>
    public class OverloadedMethodRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_METHOD
        };

        /// <summary>
        /// Gets the number of occurrences of function within the class. If the function is
        /// overloaded, there will be multiple entries in the method list.
        /// </summary>
        public ushort OverloadsCount { get; private set; }

        /// <summary>
        /// Gets the type index of method list.
        /// </summary>
        public TypeIndex MethodList { get; private set; }

        /// <summary>
        /// Gets the name of method.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="OverloadedMethodRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        public static OverloadedMethodRecord Read(IBinaryReader reader, TypeLeafKind kind)
        {
            return new OverloadedMethodRecord
            {
                Kind = kind,
                OverloadsCount = reader.ReadUshort(),
                MethodList = TypeIndex.Read(reader),
                Name = reader.ReadCString(),
            };
        }
    }
}
