using System;
using System.Collections.Generic;

namespace SharpDebug.CodeGen.UserTypes
{
    /// <summary>
    /// Class represents constructor for UserType
    /// </summary>
    internal class UserTypeConstructor
    {
        /// <summary>
        /// <code>
        /// static Constructor()
        /// {
        /// }
        /// </code>
        /// </summary>
        public static readonly UserTypeConstructor Static = new UserTypeConstructor()
        {
            IsStatic = true,
            ContainsFieldDefinitions = true,
            AccessLevel = AccessLevel.Default,
        };

        /// <summary>
        /// <code>
        /// public Constructor(Variable variable)
        ///     : base(variable)
        /// {
        /// }
        /// </code>
        /// </summary>
        public static readonly UserTypeConstructor Simple = new UserTypeConstructor()
        {
            Arguments = new Tuple<Type, string>[]
            {
                Tuple.Create(typeof(Variable), "variable"),
            },
            ContainsFieldDefinitions = true,
            IsStatic = false,
            AccessLevel = AccessLevel.Public,
        };

        /// <summary>
        /// <code>
        /// public Constructor(Variable variable)
        ///     : this(variable.GetBaseClass(baseClassString), SharpDebug.Debugger.ReadMemory(variable.GetCodeType().Module.Process, variable.GetBaseClass(baseClassString).GetPointerAddress(), variable.GetBaseClass(baseClassString).GetCodeType().Size), 0, variable.GetBaseClass(baseClassString).GetPointerAddress())
        /// {
        /// }
        /// </code>
        /// </summary>
        public static readonly UserTypeConstructor SimplePhysical = new UserTypeConstructor()
        {
            Arguments = new Tuple<Type, string>[]
            {
                Tuple.Create(typeof(Variable), "variable"),
            },
            ContainsFieldDefinitions = true,
            IsStatic = false,
            AccessLevel = AccessLevel.Public,
        };

        /// <summary>
        /// <code>
        /// public Constructor(Variable variable, SharpUtilities.MemoryBuffer buffer, int offset, ulong bufferAddress)
        ///     : base(variable, buffer, offset, bufferAddress)
        /// {
        /// }
        /// </code>
        /// </summary>
        public static readonly UserTypeConstructor RegularPhysical = new UserTypeConstructor()
        {
            Arguments = new Tuple<Type, string>[]
            {
                Tuple.Create(typeof(Variable), "variable"),
                Tuple.Create(typeof(SharpUtilities.MemoryBuffer), "buffer"),
                Tuple.Create(typeof(int), "offset"),
                Tuple.Create(typeof(ulong), "bufferAddress"),
            },
            ContainsFieldDefinitions = true,
            IsStatic = false,
            AccessLevel = AccessLevel.Public,
        };

        /// <summary>
        /// <code>
        /// public Constructor(SharpUtilities.MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name = Variable.ComputedName, string path = Variable.UnknownPath)
        ///     : base(buffer, offset, bufferAddress, codeType, address, name, path)
        /// {
        /// }
        /// </code>
        /// </summary>
        public static readonly UserTypeConstructor ComplexPhysical = new UserTypeConstructor()
        {
            Arguments = new Tuple<Type, string>[]
            {
                Tuple.Create(typeof(SharpUtilities.MemoryBuffer), "buffer"),
                Tuple.Create(typeof(int), "offset"),
                Tuple.Create(typeof(ulong), "bufferAddress"),
                Tuple.Create(typeof(CodeType), "codeType"),
                Tuple.Create(typeof(ulong), "address"),
                Tuple.Create(typeof(string), "name"),
                Tuple.Create(typeof(string), "path"),
            },
            DefaultValues = new Dictionary<string, object>()
            {
                { "name", Variable.ComputedName },
                { "path", Variable.UnknownPath },
            },
            ContainsFieldDefinitions = true,
            IsStatic = false,
            AccessLevel = AccessLevel.Public,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeConstructor"/> class.
        /// </summary>
        private UserTypeConstructor()
        {
        }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public Tuple<Type, string>[] Arguments { get; private set; }

        /// <summary>
        /// Gets dictionary of default values for arguments.
        /// </summary>
        public Dictionary<string, object> DefaultValues { get; private set; }

        /// <summary>
        /// Gets the access level.
        /// </summary>
        public AccessLevel AccessLevel { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this constructor contains field definitions in the body.
        /// </summary>
        /// <value>
        /// <c>true</c> if this constructor contains field definitions in the body; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsFieldDefinitions { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this constructor is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if static constructor; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatic { get; private set; }
    }
}
