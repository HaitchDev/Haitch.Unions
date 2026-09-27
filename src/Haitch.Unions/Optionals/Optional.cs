namespace Haitch.Unions;

/// <summary>
/// Helpers for creating <see cref="Optional{T}"/> values.
/// </summary>
public static class Optional
{
    /// <summary>
    /// Creates an optional holding <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static Optional<T> Some<T>(T value)
        where T : notnull =>
        Optional<T>.Some(value);

    /// <summary>
    /// Creates an optional holding <paramref name="value"/>, or none if it is null.
    /// </summary>
    public static Optional<T> From<T>(T? value)
        where T : notnull =>
        Optional<T>.From(value);

    /// <summary>
    /// Creates an optional holding <paramref name="value"/>, or none if it is null.
    /// </summary>
    public static Optional<T> From<T>(T? value)
        where T : struct =>
        value.HasValue
            ? Optional<T>.Some(value.Value)
            : Optional<T>.None();

    /// <summary>
    /// Creates an empty optional.
    /// </summary>
    public static Optional<T> None<T>()
        where T : notnull =>
        Optional<T>.None();
}
