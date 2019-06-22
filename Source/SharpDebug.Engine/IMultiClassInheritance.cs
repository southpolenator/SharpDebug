namespace CsDebugScript
{
    /// <summary>
    /// Interface for allowing crossing to downcast parent.
    /// It allows "automatic" downcasting to C# types.
    /// </summary>
    internal interface IMultiClassInheritance
    {
        /// <summary>
        /// Gets or sets the downcast parent.
        /// </summary>
        UserType DowncastParent { get; set; }
    }
}
