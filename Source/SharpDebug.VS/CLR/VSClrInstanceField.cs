using CsDebugScript.CLR;

namespace CsDebugScript.VS.CLR
{
    /// <summary>
    /// Visual Studio implementation of the <see cref="IClrInstanceField"/>.
    /// </summary>
    internal class VSClrInstanceField : IClrInstanceField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VSClrInstanceField"/> class.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="type">The field type.</param>
        /// <param name="offsetWhenValueClass">The offset when this field belongs to value class.</param>
        /// <param name="offsetNotValueClass">The offset when this field belongs to something that is not value class.</param>
        public VSClrInstanceField(string name, IClrType type, int offsetWhenValueClass, int offsetNotValueClass)
        {
            Name = name;
            Type = type;
            OffsetWhenValueClass = offsetWhenValueClass;
            OffsetNotValueClass = offsetNotValueClass;
        }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public IClrType Type { get; private set; }

        /// <summary>
        /// Gets the offset when this field belongs to value class.
        /// </summary>
        public int OffsetWhenValueClass { get; private set; }

        /// <summary>
        /// Gets the offset when this field belongs to something that is not value class.
        /// </summary>
        public int OffsetNotValueClass { get; private set; }

        /// <summary>
        /// Gets the field offset.
        /// </summary>
        /// <param name="isValueClass">if set to <c>true</c> it will return offset in a structure; otherwise it will return offset in a class.</param>
        public int GetOffset(bool isValueClass)
        {
            return isValueClass ? OffsetWhenValueClass : OffsetNotValueClass;
        }
    }
}
