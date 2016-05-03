namespace CsDebugScript
{
    /// <summary>
    /// Wrapper class that helps reading basic types that are native pointers and can be null-able
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasicType<T>
        where T : struct
    {
        /// <summary>
        /// Gets the value of basic type pointer from the variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public static T? GetValue(Variable variable)
        {
            if (variable.IsNullPointer())
            {
                return null;
            }

            return variable.CastAs<T>();
        }
    }
}
