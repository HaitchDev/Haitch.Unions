namespace Haitch.Unions;

/// <summary>
/// Async transformations and side effects for <see cref="Result{T}"/> values and tasks producing them.
/// </summary>
public static class ResultAsyncExtensions
{
    extension<T>(Result<T> result)
        where T : notnull
    {
        /// <summary>
        /// Transforms the value when Ok; an Error passes through unchanged.
        /// </summary>
        public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull =>
            result.TryGetValue(out T? value)
                ? Result<TOut>.Ok(await map(value).ConfigureAwait(false))
                : Result<TOut>.Error(Result.ErrorOf(result));

        /// <summary>
        /// Chains an operation that can return Error; an Error passes through unchanged.
        /// </summary>
        public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> bind)
            where TOut : notnull =>
            result.TryGetValue(out T? value)
                ? await bind(value).ConfigureAwait(false)
                : Result<TOut>.Error(Result.ErrorOf(result));

        /// <summary>
        /// Transforms the error when Error; Ok passes through unchanged.
        /// </summary>
        public async Task<Result<T>> MapErrorAsync(Func<Error, Task<Error>> map) =>
            result.TryGetValue(out Error? error)
                ? Result<T>.Error(await map(error).ConfigureAwait(false))
                : result;

        /// <summary>
        /// Turns Ok into Error holding <paramref name="error"/> when <paramref name="predicate"/> is false.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public Task<Result<T>> EnsureAsync(Func<T, Task<bool>> predicate, Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return EnsureCoreAsync();

            async Task<Result<T>> EnsureCoreAsync() =>
                result.TryGetValue(out T? value) && !await predicate(value).ConfigureAwait(false)
                    ? Result<T>.Error(error)
                    : result;
        }

        /// <summary>
        /// Runs <paramref name="action"/> with the value when Ok and returns the result unchanged.
        /// </summary>
        public async Task<Result<T>> TapAsync(Func<T, Task> action)
        {
            if (result.TryGetValue(out T? value))
            {
                await action(value).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Runs <paramref name="action"/> with the error when Error and returns the result unchanged.
        /// </summary>
        public async Task<Result<T>> TapErrorAsync(Func<Error, Task> action)
        {
            if (result.TryGetValue(out Error? error))
            {
                await action(error).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Returns this result if it is Ok, otherwise the one from <paramref name="alternative"/>.
        /// </summary>
        public async Task<Result<T>> OrElseAsync(Func<Error, Task<Result<T>>> alternative) =>
            result.TryGetValue(out Error? error)
                ? await alternative(error).ConfigureAwait(false)
                : result;

        /// <summary>
        /// Calls <paramref name="onOk"/> with the value, or <paramref name="onError"/> with the error.
        /// </summary>
        public async Task<TResult> MatchAsync<TResult>(
            Func<T, Task<TResult>> onOk,
            Func<Error, Task<TResult>> onError
        ) =>
            result.TryGetValue(out T? value)
                ? await onOk(value).ConfigureAwait(false)
                : await onError(Result.ErrorOf(result)).ConfigureAwait(false);
    }

    extension<T>(Task<Result<T>> resultTask)
        where T : notnull
    {
        /// <summary>
        /// Awaits the result, then transforms the value when Ok.
        /// </summary>
        public async Task<Result<TOut>> Map<TOut>(Func<T, TOut> map)
            where TOut : notnull =>
            (await resultTask.ConfigureAwait(false)).Map(map);

        /// <summary>
        /// Awaits the result, then transforms the value when Ok.
        /// </summary>
        public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> map)
            where TOut : notnull =>
            await (await resultTask.ConfigureAwait(false)).MapAsync(map).ConfigureAwait(false);

        /// <summary>
        /// Awaits the result, then chains an operation that can return Error.
        /// </summary>
        public async Task<Result<TOut>> Bind<TOut>(Func<T, Result<TOut>> bind)
            where TOut : notnull =>
            (await resultTask.ConfigureAwait(false)).Bind(bind);

        /// <summary>
        /// Awaits the result, then chains an operation that can return Error.
        /// </summary>
        public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> bind)
            where TOut : notnull =>
            await (await resultTask.ConfigureAwait(false)).BindAsync(bind).ConfigureAwait(false);

        /// <summary>
        /// Awaits the result, then transforms the error when Error.
        /// </summary>
        public async Task<Result<T>> MapError(Func<Error, Error> map) =>
            (await resultTask.ConfigureAwait(false)).MapError(map);

        /// <summary>
        /// Awaits the result, then transforms the error when Error.
        /// </summary>
        public async Task<Result<T>> MapErrorAsync(Func<Error, Task<Error>> map) =>
            await (await resultTask.ConfigureAwait(false)).MapErrorAsync(map).ConfigureAwait(false);

        /// <summary>
        /// Awaits the result, then turns Ok into Error holding <paramref name="error"/> when
        /// <paramref name="predicate"/> is false.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public Task<Result<T>> Ensure(Func<T, bool> predicate, Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return EnsureCoreAsync();

            async Task<Result<T>> EnsureCoreAsync() =>
                (await resultTask.ConfigureAwait(false)).Ensure(predicate, error);
        }

        /// <summary>
        /// Awaits the result, then turns Ok into Error holding <paramref name="error"/> when
        /// <paramref name="predicate"/> is false.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public Task<Result<T>> EnsureAsync(Func<T, Task<bool>> predicate, Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return EnsureCoreAsync();

            async Task<Result<T>> EnsureCoreAsync() =>
                await (await resultTask.ConfigureAwait(false))
                    .EnsureAsync(predicate, error)
                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the result, then runs <paramref name="action"/> with the value when Ok.
        /// </summary>
        public async Task<Result<T>> TapAsync(Func<T, Task> action) =>
            await (await resultTask.ConfigureAwait(false)).TapAsync(action).ConfigureAwait(false);

        /// <summary>
        /// Awaits the result, then runs <paramref name="action"/> with the error when Error.
        /// </summary>
        public async Task<Result<T>> TapErrorAsync(Func<Error, Task> action) =>
            await (await resultTask.ConfigureAwait(false))
                .TapErrorAsync(action)
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the result, then gets the value when Ok, otherwise <paramref name="fallback"/>.
        /// </summary>
        public async Task<T> GetValueOrDefault(T fallback) =>
            (await resultTask.ConfigureAwait(false)).GetValueOrDefault(fallback);

        /// <summary>
        /// Awaits the result, then falls back to <paramref name="alternative"/> if it is Error.
        /// </summary>
        public async Task<Result<T>> OrElse(Func<Error, Result<T>> alternative) =>
            (await resultTask.ConfigureAwait(false)).OrElse(alternative);

        /// <summary>
        /// Awaits the result, then falls back to <paramref name="alternative"/> if it is Error.
        /// </summary>
        public async Task<Result<T>> OrElseAsync(Func<Error, Task<Result<T>>> alternative) =>
            await (await resultTask.ConfigureAwait(false))
                .OrElseAsync(alternative)
                .ConfigureAwait(false);

        /// <summary>
        /// Awaits the result, then calls <paramref name="onOk"/> with the value, or <paramref name="onError"/> with
        /// the error.
        /// </summary>
        public async Task<TResult> Match<TResult>(Func<T, TResult> onOk, Func<Error, TResult> onError) =>
            (await resultTask.ConfigureAwait(false)).Match(onOk, onError);

        /// <summary>
        /// Awaits the result, then calls <paramref name="onOk"/> with the value, or <paramref name="onError"/> with
        /// the error.
        /// </summary>
        public async Task<TResult> MatchAsync<TResult>(
            Func<T, Task<TResult>> onOk,
            Func<Error, Task<TResult>> onError
        ) =>
            await (await resultTask.ConfigureAwait(false)).MatchAsync(onOk, onError).ConfigureAwait(false);

        /// <summary>
        /// Awaits the result, then converts it to an optional.
        /// </summary>
        public async Task<Optional<T>> ToOptional() =>
            (await resultTask.ConfigureAwait(false)).ToOptional();

        /// <summary>
        /// Awaits the result, then discards the value when Ok.
        /// </summary>
        public async Task<Result<Unit>> ToUnit() =>
            (await resultTask.ConfigureAwait(false)).ToUnit();
    }
}
