namespace Haitch.Unions;

/// <summary>
/// A value that carries no information, for results with nothing to return.
/// </summary>
public readonly record struct Unit
{
    /// <summary>
    /// Gets the default <see cref="Unit"/> value.
    /// </summary>
    public static Unit Default => default;
}
