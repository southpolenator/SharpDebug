﻿using System;

namespace SharpDebug
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

        /// <summary>
        /// Gets or sets name of the function that will be used for code type verification.
        /// If <see cref="TypeName"/> is not unique (for example template that ends with &lt;&gt;) this function will be used to verify that code type is correct for user type.
        /// </summary>
        public string CodeTypeVerification { get; set; }
    }
}
