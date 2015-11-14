namespace CsScriptManaged
{
    /// <summary>
    /// Helper class for caching results - it is being used as lazy evaluation
    /// </summary>
    /// <typeparam name="T">Type to be cached</typeparam>
    public class SimpleCache<T>
    {
        /// <summary>
        /// Delegate for populating the cached value
        /// </summary>
        public delegate T PopulateAction();

        /// <summary>
        /// The populate action
        /// </summary>
        private PopulateAction populateAction;

        /// <summary>
        /// The value that is cached
        /// </summary>
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCache{T}"/> class.
        /// </summary>
        /// <param name="populateAction">The populate action.</param>
        public SimpleCache(PopulateAction populateAction)
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
                    value = populateAction();
                    Cached = true;
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
