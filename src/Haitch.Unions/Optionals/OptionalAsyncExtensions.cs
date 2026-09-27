namespace Haitch.Unions;

/// <summary>
/// Async transformations and side effects for <see cref="Optional{T}"/> values and tasks producing them.
/// </summary>
public static class OptionalAsyncExtensions
{
    extension<T>(Optional<T> optional)
        where T : notnull
    {
        /// <summary>
        /// Transforms the value if present; an empty optional stays empty.
        /// </summary>
        public async Task<Optional<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull =>
            optional.TryGetValue(out T? value)
                ? Optional<TOut>.Some(await map(value).ConfigureAwait(false))
                : Optional<TOut>.None();

        /// <summary>
        /// Chains an operation that may return nothing; an empty optional stays empty.
        /// </summary>
        public async Task<Optional<TOut>> BindAsync<TOut>(Func<T, Task<Optional<TOut>>> bind)
            where TOut : notnull =>
            optional.TryGetValue(out T? value)
                ? await bind(value).ConfigureAwait(false)
                : Optional<TOut>.None();

        /// <summary>
        /// Keeps the value only when <paramref name="predicate"/> is true.
        /// </summary>
        public async Task<Optional<T>> EnsureAsync(Func<T, Task<bool>> predicate) =>
            optional.TryGetValue(out T? value) && !await predicate(value).ConfigureAwait(false)
                ? Optional<T>.None()
                : optional;

        /// <summary>
        /// Runs <paramref name="action"/> with the value if present and returns the optional unchanged.
        /// </summary>
        public async Task<Optional<T>> TapAsync(Func<T, Task> action)
        {
            if (optional.TryGetValue(out T? value))
            {
                await action(value).ConfigureAwait(false);
            }

            return optional;
        }

        /// <summary>
        /// Runs <paramref name="action"/> when empty and returns the optional unchanged.
        /// </summary>
        public async Task<Optional<T>> TapNoneAsync(Func<Task> action)
        {
            if (!optional.IsSome)
            {
                await action().ConfigureAwait(false);
            }

            return optional;
        }

        /// <summary>
        /// Returns this optional if it holds a value, otherwise the one from <paramref name="alternative"/>.
        /// </summary>
        public async Task<Optional<T>> OrElseAsync(Func<Task<Optional<T>>> alternative) =>
            optional.IsSome
                ? optional
                : await alternative().ConfigureAwait(false);

        /// <summary>
        /// Calls <paramref name="onSome"/> with the value, or <paramref name="onNone"/> if empty.
        /// </summary>
        public async Task<TResult> MatchAsync<TResult>(
            Func<T, Task<TResult>> onSome,
            Func<Task<TResult>> onNone
        ) =>
            optional.TryGetValue(out T? value)
                ? await onSome(value).ConfigureAwait(false)
                : await onNone().ConfigureAwait(false);
    }

    extension<T>(Task<Optional<T>> optionalTask)
        where T : notnull
    {
        /// <summary>
        /// Awaits the optional, then transforms the value if present.
        /// </summary>
        public async Task<Optional<TOut>> Map<TOut>(Func<T, TOut> map)
            where TOut : notnull =>
            (await optionalTask.ConfigureAwait(false)).Map(map);

        /// <summary>
        /// Awaits the optional, then transforms the value if present.
        /// </summary>
        public async Task<Optional<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull =>
            await (await optionalTask.ConfigureAwait(false)).MapAsync(map).ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional, then chains an operation that may return nothing.
        /// </summary>
        public async Task<Optional<TOut>> Bind<TOut>(Func<T, Optional<TOut>> bind)
            where TOut : notnull =>
            (await optionalTask.ConfigureAwait(false)).Bind(bind);

        /// <summary>
        /// Awaits the optional, then chains an operation that may return nothing.
        /// </summary>
        public async Task<Optional<TOut>> BindAsync<TOut>(Func<T, Task<Optional<TOut>>> bind)
            where TOut : notnull =>
            await (await optionalTask.ConfigureAwait(false)).BindAsync(bind).ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional, then keeps the value only when <paramref name="predicate"/> is true.
        /// </summary>
        public async Task<Optional<T>> Ensure(Func<T, bool> predicate) =>
            (await optionalTask.ConfigureAwait(false)).Ensure(predicate);

        /// <summary>
        /// Awaits the optional, then keeps the value only when <paramref name="predicate"/> is true.
        /// </summary>
        public async Task<Optional<T>> EnsureAsync(Func<T, Task<bool>> predicate) =>
            await (await optionalTask.ConfigureAwait(false))
                .EnsureAsync(predicate)
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional, then runs <paramref name="action"/> with the value if present.
        /// </summary>
        public async Task<Optional<T>> TapAsync(Func<T, Task> action) =>
            await (await optionalTask.ConfigureAwait(false)).TapAsync(action).ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional, then runs <paramref name="action"/> when empty.
        /// </summary>
        public async Task<Optional<T>> TapNoneAsync(Func<Task> action) =>
            await (await optionalTask.ConfigureAwait(false))
                .TapNoneAsync(action)
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional, then gets the value if present, otherwise <paramref name="fallback"/>.
        /// </summary>
        public async Task<T> GetValueOrDefault(T fallback) =>
            (await optionalTask.ConfigureAwait(false)).GetValueOrDefault(fallback);

        /// <summary>
        /// Awaits the optional, then falls back to <paramref name="alternative"/> if empty.
        /// </summary>
        public async Task<Optional<T>> OrElse(Func<Optional<T>> alternative) =>
            (await optionalTask.ConfigureAwait(false)).OrElse(alternative);

        /// <summary>
        /// Awaits the optional, then falls back to <paramref name="alternative"/> if empty.
        /// </summary>
        public async Task<Optional<T>> OrElseAsync(Func<Task<Optional<T>>> alternative) =>
            await (await optionalTask.ConfigureAwait(false))
                .OrElseAsync(alternative)
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional, then calls <paramref name="onSome"/> with the value, or <paramref name="onNone"/> if
        /// empty.
        /// </summary>
        public async Task<TResult> Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) =>
            (await optionalTask.ConfigureAwait(false)).Match(onSome, onNone);

        /// <summary>
        /// Awaits the optional, then calls <paramref name="onSome"/> with the value, or <paramref name="onNone"/> if
        /// empty.
        /// </summary>
        public async Task<TResult> MatchAsync<TResult>(
            Func<T, Task<TResult>> onSome,
            Func<Task<TResult>> onNone
        ) =>
            await (await optionalTask.ConfigureAwait(false)).MatchAsync(onSome, onNone).ConfigureAwait(false);

        /// <summary>
        /// Awaits the optional, then converts to an Ok result holding the value, or an Error result holding
        /// <paramref name="error"/> if empty.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public Task<Result<T>> ToResult(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return ToResultCoreAsync();

            async Task<Result<T>> ToResultCoreAsync() =>
                (await optionalTask.ConfigureAwait(false)).ToResult(error);
        }
    }

    extension<T>(Task<Optional<T>> optionalTask)
        where T : struct
    {
        /// <summary>
        /// Awaits the optional, then converts it to the value if present, otherwise null.
        /// </summary>
        public async Task<T?> ToNullable() =>
            (await optionalTask.ConfigureAwait(false)).ToNullable();
    }
}

// C# cannot overload a same-named generic member on constraints alone (CS0111).
/// <summary>
/// Reference-type <c>ToNullable</c> for <see cref="Task{TResult}"/> of <see cref="Optional{T}"/>.
/// </summary>
public static class OptionalAsyncClassExtensions
{
    extension<T>(Task<Optional<T>> optionalTask)
        where T : class
    {
        /// <summary>
        /// Awaits the optional, then converts it to the value if present, otherwise null.
        /// </summary>
        public async Task<T?> ToNullable() =>
            (await optionalTask.ConfigureAwait(false)).ToNullable();
    }
}
