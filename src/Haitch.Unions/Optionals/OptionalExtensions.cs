namespace Haitch.Unions;

/// <summary>
/// Transformations and side effects for <see cref="Optional{T}"/> values.
/// </summary>
public static class OptionalExtensions
{
    extension<T>(Optional<T> optional)
        where T : notnull
    {
        /// <summary>
        /// Transforms the value if present; an empty optional stays empty.
        /// </summary>
        public Optional<TOut> Map<TOut>(Func<T, TOut> map)
            where TOut : notnull =>
            optional.TryGetValue(out T? value)
                ? Optional<TOut>.Some(map(value))
                : Optional<TOut>.None();

        /// <summary>
        /// Chains an operation that may return nothing; an empty optional stays empty.
        /// </summary>
        public Optional<TOut> Bind<TOut>(Func<T, Optional<TOut>> bind)
            where TOut : notnull =>
            optional.TryGetValue(out T? value)
                ? bind(value)
                : Optional<TOut>.None();

        /// <summary>
        /// Keeps the value only when <paramref name="predicate"/> is true.
        /// </summary>
        public Optional<T> Ensure(Func<T, bool> predicate) =>
            optional.TryGetValue(out T? value) && !predicate(value)
                ? Optional<T>.None()
                : optional;

        /// <summary>
        /// Runs <paramref name="action"/> with the value if present and returns the optional unchanged.
        /// </summary>
        public Optional<T> Tap(Action<T> action)
        {
            if (optional.TryGetValue(out T? value))
            {
                action(value);
            }

            return optional;
        }

        /// <summary>
        /// Runs <paramref name="action"/> when empty and returns the optional unchanged.
        /// </summary>
        public Optional<T> TapNone(Action action)
        {
            if (!optional.IsSome)
            {
                action();
            }

            return optional;
        }

        /// <summary>
        /// Gets the value if present, otherwise <paramref name="fallback"/>.
        /// </summary>
        public T GetValueOrDefault(T fallback) =>
            optional.TryGetValue(out T? value)
                ? value
                : fallback;

        /// <summary>
        /// Returns this optional if it holds a value, otherwise the one from <paramref name="alternative"/>.
        /// </summary>
        public Optional<T> OrElse(Func<Optional<T>> alternative) =>
            optional.IsSome
                ? optional
                : alternative();

        /// <summary>
        /// Converts to an Ok result holding the value, or an Error result holding <paramref name="error"/> if empty.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public Result<T> ToResult(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return optional.TryGetValue(out T? value)
                ? Result<T>.Ok(value)
                : Result<T>.Error(error);
        }
    }

    extension<T>(Optional<T> optional)
        where T : struct
    {
        /// <summary>
        /// Converts to the value if present, otherwise null.
        /// </summary>
        public T? ToNullable() =>
            optional.TryGetValue(out T value)
                ? value
                : null;
    }
}

// C# cannot overload a same-named generic member on constraints alone (CS0111).
/// <summary>
/// Reference-type <c>ToNullable</c> for <see cref="Optional{T}"/>.
/// </summary>
public static class OptionalClassExtensions
{
    extension<T>(Optional<T> optional)
        where T : class
    {
        /// <summary>
        /// Converts to the value if present, otherwise null.
        /// </summary>
        public T? ToNullable() =>
            optional.TryGetValue(out T? value)
                ? value
                : null;
    }
}
