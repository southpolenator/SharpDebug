using System;

namespace CsDebugScript
{
    /// <summary>
    /// Attribute that gives more info for user defined types in scripts.
    /// It allows "automatic" downcasting to C# types.
    /// It is preferred to let CodeGen generate this attribute for you.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BaseClassesArrayAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the field that contains array with types for base classes.
        /// </summary>
        public string FieldName { get; set; }
    }
}
