namespace Haitch.Unions;

/// <summary>
/// Helpers for working with <see cref="Result{T}"/> values.
/// </summary>
public static class Result
{
    /// <summary>
    /// The error held by a default or parameterless <see cref="Result{T}"/>.
    /// </summary>
    public static readonly Error UninitializedError = new(
        "Result.Uninitialized",
        "The result was never assigned a value or an error explicitly"
    );

    /// <summary>
    /// Creates an Ok result holding <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static Result<T> Ok<T>(T value)
        where T : notnull =>
        Result<T>.Ok(value);

    /// <summary>
    /// Creates an Ok result with no value.
    /// </summary>
    public static Result<Unit> Ok() => Result<Unit>.Ok(default);

    /// <summary>
    /// Creates an Error result holding <paramref name="error"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public static Result<T> Error<T>(Error error)
        where T : notnull =>
        Result<T>.Error(error);

    /// <summary>
    /// Creates an Error result holding <paramref name="error"/> for an operation with no value.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public static Result<Unit> Error(Error error) => Result<Unit>.Error(error);

    /// <summary>
    /// Runs <paramref name="func"/>, turning a thrown exception into an <see cref="UnexpectedError"/>.
    /// </summary>
    public static Result<T> Try<T>(Func<T> func)
        where T : notnull
    {
        try
        {
            return Result<T>.Ok(func());
        }
        // Cancellation is control flow, not an error, so it keeps propagating.
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Result<T>.Error(ExceptionError(exception));
        }
    }

    /// <summary>
    /// Runs <paramref name="action"/>, turning a thrown exception into an <see cref="UnexpectedError"/>.
    /// </summary>
    public static Result<Unit> Try(Action action) =>
        Try(() =>
        {
            action();
            return Unit.Default;
        });

    /// <summary>
    /// Awaits <paramref name="func"/>, turning a thrown exception into an <see cref="UnexpectedError"/>.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
        where T : notnull
    {
        try
        {
            return Result<T>.Ok(await func().ConfigureAwait(false));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Result<T>.Error(ExceptionError(exception));
        }
    }

    /// <summary>
    /// Awaits <paramref name="func"/>, turning a thrown exception into an <see cref="UnexpectedError"/>.
    /// </summary>
    public static Task<Result<Unit>> TryAsync(Func<Task> func) =>
        TryAsync(async () =>
        {
            await func().ConfigureAwait(false);
            return Unit.Default;
        });

    /// <summary>
    /// Combines <paramref name="results"/> into one: every value when Ok, otherwise the first error.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is null.</exception>
    public static Result<IReadOnlyList<T>> Combine<T>(params IEnumerable<Result<T>> results)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = new List<T>();
        foreach (var result in results)
        {
            if (!result.TryGetValue(out T? value))
            {
                return Result<IReadOnlyList<T>>.Error(ErrorOf(result));
            }

            values.Add(value);
        }

        return Result<IReadOnlyList<T>>.Ok(values);
    }

    /// <summary>
    /// Combines <paramref name="results"/> into one: every value when Ok, otherwise every error merged by
    /// <paramref name="mergeErrors"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="mergeErrors"/> is null.</exception>
    public static Result<IReadOnlyList<T>> Combine<T>(
        IEnumerable<Result<T>> results,
        Func<IReadOnlyList<Error>, Error> mergeErrors
    )
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(mergeErrors);

        var values = new List<T>();
        List<Error>? errors = null;
        foreach (var result in results)
        {
            if (result.TryGetValue(out T? value))
            {
                values.Add(value);
            }
            else
            {
                (errors ??= []).Add(ErrorOf(result));
            }
        }

        return errors is null
            ? Result<IReadOnlyList<T>>.Ok(values)
            : Result<IReadOnlyList<T>>.Error(mergeErrors(errors));
    }

    internal static Error ErrorOf<T>(Result<T> result)
        where T : notnull =>
        result.TryGetValue(out Error? error)
            ? error
            : throw new InvalidOperationException("The result is Ok and holds no error.");

    private static UnexpectedError ExceptionError(Exception exception) =>
        new("Result.Exception", exception.Message, exception);
}
