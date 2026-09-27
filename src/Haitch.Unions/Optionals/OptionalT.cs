using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Haitch.Unions;

/// <summary>
/// Either holds <typeparamref name="T"/> or <see cref="Haitch.Unions.None"/>.
/// </summary>
[Union]
public readonly struct Optional<T> : IUnion, IEquatable<Optional<T>>
    where T : notnull
{
    private readonly T _value;

    /// <summary>
    /// Creates an empty optional.
    /// </summary>
    public Optional()
    {
        _value = default!;
        IsSome = false;
    }

    /// <summary>
    /// Creates an optional holding <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public Optional(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
        IsSome = true;
    }

    /// <summary>
    /// Creates an empty optional.
    /// </summary>
    // ReSharper disable once UnusedParameter.Local
    public Optional(None _)
        : this() { }

    /// <summary>
    /// Gets whether this optional holds a value.
    /// </summary>
    public bool IsSome { get; }

    /// <summary>
    /// Gets the value if present, otherwise <see cref="Haitch.Unions.None"/>.
    /// </summary>
    public object Value =>
        IsSome
            ? _value
            : default(None);

    /// <summary>
    /// Gets the value if present.
    /// </summary>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsSome;
    }

    /// <summary>
    /// Gets <see cref="Haitch.Unions.None"/> if the optional is empty.
    /// </summary>
    public bool TryGetValue(out None none)
    {
        none = default;
        return !IsSome;
    }

    /// <summary>
    /// Calls <paramref name="onSome"/> with the value, or <paramref name="onNone"/> if empty.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) =>
        IsSome
            ? onSome(_value)
            : onNone();

    /// <summary>
    /// Creates an optional holding <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static Optional<T> Some(T value) =>
        new(value);

    /// <summary>
    /// Creates an empty optional.
    /// </summary>
    public static Optional<T> None() => new();

    /// <summary>
    /// Creates an optional holding <paramref name="value"/>, or none if it is null.
    /// </summary>
    public static Optional<T> From(T? value) =>
        value is null
            ? new()
            : new(value);

    /// <summary>
    /// Converts a value to an optional, or none if it is null.
    /// </summary>
    public static implicit operator Optional<T>(T? value) => From(value);

    /// <summary>
    /// Converts <see cref="Haitch.Unions.None"/> to an empty optional.
    /// </summary>
    public static implicit operator Optional<T>(None none) => None();

    /// <summary>
    /// Whether both optionals are empty, or both hold equal values.
    /// </summary>
    public bool Equals(Optional<T> other) =>
        IsSome == other.IsSome
        && (!IsSome || EqualityComparer<T>.Default.Equals(_value, other._value));

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        IsSome
            ? HashCode.Combine(true, _value)
            : 0;

    /// <summary>
    /// Returns <c>Some(value)</c> or <c>None</c>.
    /// </summary>
    public override string ToString() =>
        IsSome
            ? $"Some({_value})"
            : "None";

    /// <summary>
    /// Whether two optionals are equal.
    /// </summary>
    public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

    /// <summary>
    /// Whether two optionals are not equal.
    /// </summary>
    public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);
}
