namespace Haitch.Unions;

/// <summary>
/// Marks the absence of a value in an <see cref="Optional{T}"/>.
/// </summary>
public readonly record struct None
{
    /// <summary>
    /// Gets the default <see cref="None"/> value.
    /// </summary>
    public static None Default => default;
}
