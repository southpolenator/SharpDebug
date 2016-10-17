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

            return (T)delegates?.SymbolicConstructor(variable);
        }
    }
}
