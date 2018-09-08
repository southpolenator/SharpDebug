using CsDebugScript.Exceptions;

namespace CsDebugScript.CommonUserTypes.NativeTypes.std
{
    /// <summary>
    /// Microsoft implementation of std::pair
    /// </summary>
    /// <typeparam name="TFirst">The type of the first field.</typeparam>
    /// <typeparam name="TSecond">The type of the second field.</typeparam>
    public class pair<TFirst, TSecond> : UserType
    {
        /// <summary>
        /// The first field
        /// </summary>
        private UserMember<TFirst> first;

        /// <summary>
        /// The secondField
        /// </summary>
        private UserMember<TSecond> second;

        /// <summary>
        /// Initializes a new instance of the <see cref="pair{TFirst, TSecond}"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <exception cref="WrongCodeTypeException">std::pair</exception>
        public pair(Variable variable)
            : base(variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "std::pair");
            }

            // Initialize members
            first = UserMember.Create(() => variable.GetField("first").CastAs<TFirst>());
            second = UserMember.Create(() => variable.GetField("second").CastAs<TSecond>());
        }

        /// <summary>
        /// Gets the first field.
        /// </summary>
        [ForceDefaultVisualizerAtttribute]
        public TFirst First
        {
            get
            {
                return first.Value;
            }
        }

        /// <summary>
        /// Gets the second field.
        /// </summary>
        [ForceDefaultVisualizerAtttribute]
        public TSecond Second
        {
            get
            {
                return second.Value;
            }
        }

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        public static bool VerifyCodeType(CodeType codeType)
        {
            // We want to have this kind of hierarchy
            // first
            // second
            CodeType first, second;

            if (!codeType.GetFieldTypes().TryGetValue("first", out first))
                return false;
            if (!codeType.GetFieldTypes().TryGetValue("second", out second))
                return false;
            return true;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"({First}, {Second})";
        }
    }

    /// <summary>
    /// Simplification class for creating <see cref="pair{TFirst, TSecond}"/> with TFirst and TSecond being <see cref="Variable"/>.
    /// </summary>
    [UserType(TypeName = "std::pair<>", CodeTypeVerification = nameof(pair.VerifyCodeType))]
    public class pair : pair<Variable, Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="pair"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public pair(Variable variable)
            : base(variable)
        {
        }
    }
}
