using System;

namespace CsScripts
{
    /// <summary>
    /// Helper class for caching user members (fields) in auto generated classes
    /// </summary>
    public static class UserMember
    {
        /// <summary>
        /// Creates a new instance of the <see cref="UserMember{T}" /> class.
        /// </summary>
        /// <typeparam name="T">Type to be cached</typeparam>
        /// <param name="populateAction">The function that populates the cache on demand.</param>
        /// <returns><see cref="UserMember{T}" /></returns>
        public static UserMember<T> Create<T>(Func<T> populateAction)
        {
            return new UserMember<T>(populateAction);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UserMember{T}" /> class.
        /// </summary>
        /// <typeparam name="T">Type to be cached</typeparam>
        /// <param name="populateAction">The function that populates the cache on demand.</param>
        /// <returns><see cref="UserMember{T}" /></returns>
        public static UserMember<T> Create<T>(Func<Variable> populateAction)
        {
            return new UserMember<T>(() => (T)Convert.ChangeType(populateAction(), typeof(T)));
        }
    }

    /// <summary>
    /// Helper class for caching user members (fields) in auto generated classes
    /// </summary>
    /// <typeparam name="T">Type to be cached</typeparam>
    public class UserMember<T>
    {
        /// <summary>
        /// The populate action
        /// </summary>
        private Func<T> populateAction;

        /// <summary>
        /// The value that is cached
        /// </summary>
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMember{T}"/> class.
        /// </summary>
        /// <param name="populateAction">The function that populates the cache on demand.</param>
        public UserMember(Func<T> populateAction)
        {
            this.populateAction = populateAction;
        }

        /// <summary>
        /// Gets a value indicating whether value is cached.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cached; otherwise, <c>false</c>.
        /// </value>
        public bool Cached { get; private set; }

        /// <summary>
        /// Gets or sets the value. The value will be populated if it wasn't cached.
        /// </summary>
        public T Value
        {
            get
            {
                if (!Cached)
                {
                    Value = populateAction();
                }

                return value;
            }

            set
            {
                this.value = value;
                Cached = true;
            }
        }
    }
}
