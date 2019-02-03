using SharpUtilities;

namespace CsDebugScript
{
    /// <summary>
    /// Helper class that provides <see cref="CsDebugScript.InteractiveExecution"/> delayed initialization.
    /// </summary>
    public class InteractiveExecutionInitialization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExecutionInitialization" /> class.
        /// </summary>
        /// <param name="cacheInvalidator">Cache invalidator that will be used to create simple cache for creating interactive execution.</param>
        public InteractiveExecutionInitialization(CacheInvalidator cacheInvalidator = null)
            : this(new InteractiveExecutionBehavior(), cacheInvalidator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExecutionInitialization" /> class.
        /// </summary>
        /// <param name="interactiveExecutionBehavior">Customization of interactive execution.</param>
        /// <param name="cacheInvalidator">Cache invalidator that will be used to create simple cache for creating interactive execution.</param>
        public InteractiveExecutionInitialization(InteractiveExecutionBehavior interactiveExecutionBehavior, CacheInvalidator cacheInvalidator = null)
        {
            InteractiveExecutionBehavior = interactiveExecutionBehavior;
            if (cacheInvalidator != null)
                InteractiveExecutionCache = cacheInvalidator.CreateSimpleCache(CreateInteractiveExecution);
            else
                InteractiveExecutionCache = SimpleCache.Create(CreateInteractiveExecution);
        }

        /// <summary>
        /// Gets the interactive execution cache.
        /// </summary>
        public SimpleCache<InteractiveExecution> InteractiveExecutionCache { get; private set; }

        /// <summary>
        /// Gets the customization of interactive execution.
        /// </summary>
        public InteractiveExecutionBehavior InteractiveExecutionBehavior { get; private set; }

        /// <summary>
        /// Gets the interactive execution from the cache.
        /// </summary>
        public InteractiveExecution InteractiveExecution => InteractiveExecutionCache.Value;

        /// <summary>
        /// Creates interactive execution for the cache.
        /// </summary>
        public virtual InteractiveExecution CreateInteractiveExecution()
        {
            InteractiveExecution interactiveExecution = new InteractiveExecution(InteractiveExecutionBehavior);

            interactiveExecution.Reset();
            return interactiveExecution;
        }
    }
}
