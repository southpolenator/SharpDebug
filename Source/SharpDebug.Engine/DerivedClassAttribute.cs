using System;

namespace SharpDebug
{
    /// <summary>
    /// Attribute that gives more info for user defined types in scripts.
    /// It allows "automatic" downcasting to C# types.
    /// It is preferred to let CodeGen generate this attribute for you.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DerivedClassAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the priority for ordering in which we should process derived classes when applying automatic downcasting to C# types.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the type which we should consider when applying automatic downcasting to C# types.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the specialization type.
        /// </summary>
        public string TypeName { get; set; }
    }
}
