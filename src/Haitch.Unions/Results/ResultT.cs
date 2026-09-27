using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Haitch.Unions;

/// <summary>
/// Either Ok holding a <typeparamref name="T"/> or Error holding an <see cref="Haitch.Unions.Error"/>.
/// </summary>
[Union]
public readonly struct Result<T> : IUnion, IEquatable<Result<T>>
    where T : notnull
{
    private readonly T _value;
    private readonly Error? _error;

    /// <summary>
    /// Creates an Error result holding <see cref="Result.UninitializedError"/>.
    /// </summary>
    public Result()
    {
        _value = default!;
        _error = null;
        IsOk = false;
    }

    /// <summary>
    /// Creates an Ok result holding <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public Result(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
        _error = null;
        IsOk = true;
    }

    /// <summary>
    /// Creates an Error result holding <paramref name="error"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _value = default!;
        _error = error;
        IsOk = false;
    }

    /// <summary>
    /// Gets whether this result is Ok.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// Gets the value when Ok, otherwise the error.
    /// </summary>
    public object Value =>
        IsOk
            ? _value
            : ErrorOrUninitialized;

    /// <summary>
    /// Gets the value if this is Ok.
    /// </summary>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsOk;
    }

    /// <summary>
    /// Gets the error if this is Error.
    /// </summary>
    public bool TryGetValue([NotNullWhen(true)] out Error? error)
    {
        error = IsOk
            ? null
            : ErrorOrUninitialized;
        return !IsOk;
    }

    /// <summary>
    /// Calls <paramref name="onOk"/> with the value, or <paramref name="onError"/> with the error.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onOk, Func<Error, TResult> onError) =>
        IsOk
            ? onOk(_value)
            : onError(ErrorOrUninitialized);

    /// <summary>
    /// Creates an Ok result holding <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static Result<T> Ok(T value) => new(value);

    /// <summary>
    /// Creates an Error result holding <paramref name="error"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public static Result<T> Error(Error error) => new(error);

    /// <summary>
    /// Converts a value to an Ok result.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static implicit operator Result<T>(T value) => new(value);

    /// <summary>
    /// Converts an error to an Error result.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public static implicit operator Result<T>(Error error) => new(error);

    /// <summary>
    /// Whether both results are Ok with equal values, or Error with equal errors.
    /// </summary>
    public bool Equals(Result<T> other) =>
        IsOk == other.IsOk
        && (
            IsOk
                ? EqualityComparer<T>.Default.Equals(_value, other._value)
                : EqualityComparer<Error>.Default.Equals(
                    ErrorOrUninitialized,
                    other.ErrorOrUninitialized
                )
        );

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        IsOk
            ? HashCode.Combine(true, _value)
            : HashCode.Combine(false, ErrorOrUninitialized);

    /// <summary>
    /// Returns <c>Ok(value)</c> or <c>Error(code)</c>.
    /// </summary>
    public override string ToString() =>
        IsOk
            ? $"Ok({_value})"
            : $"Error({ErrorOrUninitialized.Code})";

    /// <summary>
    /// Whether two results are equal.
    /// </summary>
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

    /// <summary>
    /// Whether two results are not equal.
    /// </summary>
    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);

    // A default or parameterless result has no stored error but behaves as holding UninitializedError.
    private Error ErrorOrUninitialized => _error ?? Result.UninitializedError;
}
