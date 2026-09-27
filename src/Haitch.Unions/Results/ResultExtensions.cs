namespace Haitch.Unions;

/// <summary>
/// Transformations and side effects for <see cref="Result{T}"/> values.
/// </summary>
public static class ResultExtensions
{
    extension<T>(Result<T> result)
        where T : notnull
    {
        /// <summary>
        /// Transforms the value when Ok; an Error passes through unchanged.
        /// </summary>
        public Result<TOut> Map<TOut>(Func<T, TOut> map)
            where TOut : notnull =>
            result.TryGetValue(out T? value)
                ? Result<TOut>.Ok(map(value))
                : Result<TOut>.Error(Result.ErrorOf(result));

        /// <summary>
        /// Chains an operation that can return Error; an Error passes through unchanged.
        /// </summary>
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
            where TOut : notnull =>
            result.TryGetValue(out T? value)
                ? bind(value)
                : Result<TOut>.Error(Result.ErrorOf(result));

        /// <summary>
        /// Transforms the error when Error; Ok passes through unchanged.
        /// </summary>
        public Result<T> MapError(Func<Error, Error> map) =>
            result.TryGetValue(out Error? error)
                ? Result<T>.Error(map(error))
                : result;

        /// <summary>
        /// Turns Ok into Error holding <paramref name="error"/> when <paramref name="predicate"/> is false.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public Result<T> Ensure(Func<T, bool> predicate, Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return result.TryGetValue(out T? value) && !predicate(value)
                ? Result<T>.Error(error)
                : result;
        }

        /// <summary>
        /// Runs <paramref name="action"/> with the value when Ok and returns the result unchanged.
        /// </summary>
        public Result<T> Tap(Action<T> action)
        {
            if (result.TryGetValue(out T? value))
            {
                action(value);
            }

            return result;
        }

        /// <summary>
        /// Runs <paramref name="action"/> with the error when Error and returns the result unchanged.
        /// </summary>
        public Result<T> TapError(Action<Error> action)
        {
            if (result.TryGetValue(out Error? error))
            {
                action(error);
            }

            return result;
        }

        /// <summary>
        /// Gets the value when Ok, otherwise <paramref name="fallback"/>.
        /// </summary>
        public T GetValueOrDefault(T fallback) =>
            result.TryGetValue(out T? value)
                ? value
                : fallback;

        /// <summary>
        /// Returns this result if it is Ok, otherwise the one from <paramref name="alternative"/>.
        /// </summary>
        public Result<T> OrElse(Func<Error, Result<T>> alternative) =>
            result.TryGetValue(out Error? error)
                ? alternative(error)
                : result;

        /// <summary>
        /// Converts to an optional holding the value when Ok, or an empty optional when Error.
        /// </summary>
        public Optional<T> ToOptional() =>
            result.TryGetValue(out T? value)
                ? Optional<T>.Some(value)
                : Optional<T>.None();

        /// <summary>
        /// Discards the value when Ok, keeping only whether it is Ok; an Error passes through unchanged.
        /// </summary>
        public Result<Unit> ToUnit() =>
            result.IsOk
                ? Result<Unit>.Ok(Unit.Default)
                : Result<Unit>.Error(Result.ErrorOf(result));
    }
}
