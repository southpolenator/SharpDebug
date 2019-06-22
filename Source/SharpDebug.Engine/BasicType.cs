namespace CsDebugScript
{
    /// <summary>
    /// Wrapper class that helps reading basic types that are native pointers and can be null-able
    /// </summary>
    /// <typeparam name="T">Basic type</typeparam>
    public class BasicType<T>
        where T : struct
    {
        /// <summary>
        /// Gets the value of basic type pointer from the variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>The value of basic type pointer from the variable</returns>
        public static T? GetValue(Variable variable)
        {
            if (variable.IsNull())
            {
                return null;
            }

            return variable.CastAs<T>();
        }
    }
}
