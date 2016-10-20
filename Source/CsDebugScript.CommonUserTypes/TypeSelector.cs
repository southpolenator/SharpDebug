using CsDebugScript.Engine;
using System;
using System.Collections.Concurrent;

namespace CsDebugScript.CommonUserTypes
{
    /// <summary>
    /// Helper class that allows easier implementation of different user types.
    /// This class provides selector for different implementations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TypeSelector<T>
    {
        /// <summary>
        /// The verified types
        /// </summary>
        private ConcurrentDictionary<CodeType, IUserTypeDelegates> verifiedTypes = new ConcurrentDictionary<CodeType, IUserTypeDelegates>();

        /// <summary>
        /// The user defined types with selector function
        /// </summary>
        private Tuple<Type, Func<CodeType, bool>>[] types;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSelector{T}"/> class.
        /// </summary>
        /// <param name="types">The user defined types with selector function.</param>
        public TypeSelector(params Tuple<Type, Func<CodeType, bool>>[] types)
        {
            this.types = types;
        }

        /// <summary>
        /// Selects the type based on the specified variables code type.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>New instance of user type that knows how to read variable data.</returns>
        public T SelectType(Variable variable)
        {
            CodeType codeType = variable.GetCodeType();
            IUserTypeDelegates delegates = GetUserTypeDelegates(codeType);

            return (T)delegates?.SymbolicConstructor(variable);
        }

        /// <summary>
        /// Verifies that type selector can work with the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if type selector can work with the specified code type; <c>false</c> otherwise</returns>
        public bool VerifyCodeType(CodeType codeType)
        {
            return GetUserTypeDelegates(codeType) != null;
        }

        /// <summary>
        /// Gets the user type delegates for the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns>User type delegates</returns>
        private IUserTypeDelegates GetUserTypeDelegates(CodeType codeType)
        {
            IUserTypeDelegates delegates;

            if (!verifiedTypes.TryGetValue(codeType, out delegates))
            {
                foreach (var t in types)
                {
                    if (t.Item2(codeType))
                    {
                        delegates = UserTypeDelegates.Delegates[t.Item1];
                        break;
                    }
                }

                verifiedTypes.TryAdd(codeType, delegates);
            }

            return delegates;
        }
    }
}
