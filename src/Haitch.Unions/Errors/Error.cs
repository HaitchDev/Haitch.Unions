namespace Haitch.Unions;

/// <summary>
/// Describes why an operation returned an error.
/// </summary>
/// <param name="Code">A stable, machine-readable identifier.</param>
/// <param name="Description">A human-readable explanation.</param>
/// <exception cref="ArgumentNullException"><paramref name="Code"/> or <paramref name="Description"/> is null.</exception>
public record Error(string Code, string Description)
{
    /// <summary>
    /// A stable, machine-readable identifier.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    public string Code
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(Code));
    } = Code ?? throw new ArgumentNullException(nameof(Code));

    /// <summary>
    /// A human-readable explanation.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    public string Description
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(Description));
    } = Description ?? throw new ArgumentNullException(nameof(Description));
}
