using System;
using System.Threading;

namespace Examine.Lucene
{
    /// <summary>
    /// A thread-safe lazy initializer that, unlike <see cref="Lazy{T}"/> using the default
    /// <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> mode, does NOT cache exceptions
    /// thrown by the value factory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The standard <see cref="Lazy{T}"/> caches the first exception thrown by its factory and
    /// re-throws that same exception on every subsequent access for the lifetime of the instance.
    /// For index directory creation that means a transient failure (e.g. a momentarily locked
    /// index file during a host overlap/recycle) permanently poisons the index until the process
    /// is restarted.
    /// </para>
    /// <para>
    /// This implementation only caches a successfully created value. If the factory throws, the
    /// exception propagates to the caller but is not stored, so a later access will retry the
    /// factory and can recover once the transient condition clears.
    /// </para>
    /// </remarks>
    internal sealed class ResettableLazy<T>
        where T : class
    {
        private readonly Func<T> _valueFactory;
        private readonly object _locker = new object();
        private T? _value;

        // A dedicated flag (rather than a null check on _value) is used to track creation so that a
        // factory which legitimately returns null is treated as "created" instead of being re-invoked
        // on every subsequent access. Declared volatile so that a true read also publishes _value.
        private volatile bool _isValueCreated;

        public ResettableLazy(Func<T> valueFactory)
            => _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));

        /// <summary>
        /// Returns true once a value has been successfully created.
        /// </summary>
        public bool IsValueCreated => _isValueCreated;

        /// <summary>
        /// Gets the lazily created value, creating it on first successful access.
        /// </summary>
        /// <remarks>
        /// Exceptions thrown by the value factory are intentionally not cached so that a subsequent
        /// access can retry.
        /// </remarks>
        public T Value
        {
            get
            {
                if (_isValueCreated)
                {
                    return _value!;
                }

                lock (_locker)
                {
                    if (_isValueCreated)
                    {
                        return _value!;
                    }

                    // If this throws, the exception is not cached - a later access will retry.
                    var created = _valueFactory();
                    _value = created;

                    // Volatile write publishes _value to other threads and marks creation complete.
                    _isValueCreated = true;
                    return created;
                }
            }
        }
    }
}
