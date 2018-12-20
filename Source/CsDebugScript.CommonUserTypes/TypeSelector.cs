using CsDebugScript.Engine;
using CsDebugScript.Engine.Utility;
using System;

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
        private DictionaryCache<CodeType, Tuple<IUserTypeDelegates, object>> verifiedTypes;

        /// <summary>
        /// The user defined types with selector function
        /// </summary>
        private Tuple<Type, Func<CodeType, object>>[] types;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSelector{T}"/> class.
        /// </summary>
        /// <param name="types">The user defined types with selector function.</param>
        public TypeSelector(params Tuple<Type, Func<CodeType, object>>[] types)
        {
            this.types = types;
            verifiedTypes = GlobalCache.Caches.CreateDictionaryCache<CodeType, Tuple<IUserTypeDelegates, object>>(GetVerifiedTypeData);
        }

        /// <summary>
        /// Selects the type based on the specified variables code type.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>New instance of user type that knows how to read variable data.</returns>
        public T SelectType(Variable variable)
        {
            CodeType codeType = variable.GetCodeType();
            Tuple<IUserTypeDelegates, object> data = verifiedTypes[codeType];

            return (T)data?.Item1.SymbolicConstructorWithData(variable, data.Item2);
        }

        /// <summary>
        /// Verifies that type selector can work with the specified code type.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        /// <returns><c>true</c> if type selector can work with the specified code type; <c>false</c> otherwise</returns>
        public bool VerifyCodeType(CodeType codeType)
        {
            return verifiedTypes[codeType] != null;
        }

        /// <summary>
        /// Finds verified type data for the specified code type.
        /// </summary>
        /// <param name="codeType">The code type to be verified.</param>
        private Tuple<IUserTypeDelegates, object> GetVerifiedTypeData(CodeType codeType)
        {
            foreach (var t in types)
            {
                object data = t.Item2(codeType);

                if (data != null)
                    return Tuple.Create(UserTypeDelegates.Delegates[t.Item1], data);
            }
            return null;
        }
    }
}
