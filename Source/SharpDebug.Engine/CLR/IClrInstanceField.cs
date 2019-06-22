namespace CsDebugScript.CLR
{
    /// <summary>
    /// CLR code instance field interface. Fundamentally it represents a name and a type.
    /// </summary>
    public interface IClrInstanceField
    {
        /// <summary>
        /// Gets the field name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the field type.
        /// </summary>
        IClrType Type { get; }

        /// <summary>
        /// Gets the field offset.
        /// </summary>
        /// <param name="isValueClass">if set to <c>true</c> it will return offset in a structure; otherwise it will return offset in a class.</param>
        int GetOffset(bool isValueClass);
    }
}
