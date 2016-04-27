using System;

namespace CsScripts
{
    /// <summary>
    /// Attribute that gives more info for user defined types in scripts.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UserTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the module.
        /// Note: it can be null, then all modules will be scanned and if multiple modules have same type, exception will be thrown.
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Gets or sets the name of the type.
        /// Note: it can be null, then class name where this attribute was set would be used.
        /// </summary>
        public string TypeName { get; set; }
    }
}
