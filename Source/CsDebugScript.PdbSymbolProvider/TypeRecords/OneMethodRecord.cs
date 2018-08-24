using CsDebugScript.PdbSymbolProvider.Utility;

namespace CsDebugScript.PdbSymbolProvider.TypeRecords
{
    /// <summary>
    /// This type record is used to specify a method of a class that is not overloaded.
    /// </summary>
    public class OneMethodRecord : TypeRecord
    {
        /// <summary>
        /// Array of <see cref="TypeLeafKind"/> that this class can read.
        /// </summary>
        public static readonly TypeLeafKind[] Kinds = new TypeLeafKind[]
        {
            TypeLeafKind.LF_ONEMETHOD
        };

        /// <summary>
        /// Gets the method attributes.
        /// </summary>
        public MemberAttributes Attributes { get; private set; }

        /// <summary>
        /// Gets the type index of method.
        /// </summary>
        public TypeIndex Type { get; private set; }

        /// <summary>
        /// Gets the offset in virtual function table if virtual method. If the method is not virtual, then value is -1.
        /// </summary>
        public int VFTableOffset { get; private set; }

        /// <summary>
        /// Gets the name of method.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reads <see cref="OneMethodRecord"/> from the stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        /// <param name="kind">Type record kind.</param>
        /// <param name="isFromOverloadedList"><c>true</c> if we are reading this from <see cref="MethodOverloadListRecord"/>.</param>
        public static OneMethodRecord Read(IBinaryReader reader, TypeLeafKind kind, bool isFromOverloadedList = false)
        {
            var record = new OneMethodRecord
            {
                Kind = kind,
                Attributes = MemberAttributes.Read(reader)
            };
            if (isFromOverloadedList)
                reader.ReadFake(2); // 2 = sizeof(ushort)
            record.Type = TypeIndex.Read(reader);
            record.VFTableOffset = record.Attributes.IsIntroducedVirtual ? reader.ReadInt() : -1;
            if (!isFromOverloadedList)
                record.Name = reader.ReadCString();
            return record;
        }
    }
}
